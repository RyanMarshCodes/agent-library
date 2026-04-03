# ADR: Model Routing & Agent-Model Mappings in Ryan.MCP

**Status:** Implemented (Phase 1)  
**Date:** 2026-04-02  
**Author:** Ryan + OpenCode  

---

## Context

We have 66 agents in `agents/`, each with a `model` frontmatter field assigning a recommended LLM (e.g., `model: gpt-5.4-nano # capable — alt: big-pickle, gemini-3-flash`). These assignments reflect a tiered cost/capability strategy:

| Tier | Primary Model | Count |
|------|--------------|-------|
| Frontier | claude-opus-4-6 | 12 |
| Strong/Coding | gpt-5.3-codex | 15 |
| Strong/Analysis | claude-sonnet-4-6 | 15 |
| Strong/Infra | gemini-3.1-pro | 7 |
| Capable | gpt-5.4-nano | 16 |
| Efficient | gpt-5.4-nano | 1 |

**Problem:** These model assignments are currently advisory-only metadata. No system reads them to actually route work to the right model. The `recommend_agent` tool returns the best agent for a task but says nothing about which model to use. Every time an AI agent (OpenCode, Claude Code, Cursor) is asked to pick a model, it has to reason from scratch — or the user switches manually.

**Additional desires:**
1. Store model mappings in Postgres for fast retrieval (no re-parsing frontmatter on every call)
2. Eventually check provider quotas/budgets to inform fallback recommendations
3. Keep it working across all MCP-compatible coding agents

---

## Decision

### What we will build

Extend Ryan.MCP with a **model advisory layer** — a set of Postgres-backed data and MCP tools that return model recommendations alongside agent recommendations. The layer is **advisory, not interceptive**: it tells the calling agent which model to use, but cannot force it.

### What we will NOT build (and why)

- **LLM API proxy/router** — MCP is a tool-calling protocol; the host (OpenCode, Claude Code, Cursor) controls the model. Ryan.MCP cannot intercept or redirect LLM calls. Building a full LLM proxy would be a separate product, not an MCP extension.
- **Real-time quota checking** — Most providers don't expose pre-checkable quota APIs. Anthropic and OpenAI expose usage history, not remaining budget. Google exposes rate-limit headers reactively. We'll design the schema to support local budget tracking later, but won't build provider API integrations in v1.

---

## Design

### 1. Database: New migration `002_agent_model_mappings.sql`

```sql
-- Agent-to-model mapping with tier and cost metadata
CREATE TABLE IF NOT EXISTS agent_model_mappings (
    agent_name       TEXT PRIMARY KEY,
    tier             TEXT NOT NULL,
    primary_model    TEXT NOT NULL,
    primary_provider TEXT,
    alt_model_1      TEXT,
    alt_provider_1   TEXT,
    alt_model_2      TEXT,
    alt_provider_2   TEXT,
    cost_per_1m_in   NUMERIC(10,4),
    cost_per_1m_out  NUMERIC(10,4),
    notes            TEXT,
    synced_from      TEXT,                  -- 'frontmatter' or 'manual'
    updated_at       TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_agent_model_mappings_tier
    ON agent_model_mappings(tier);
```

**Why a separate table instead of reusing memory_entities?**  
The knowledge graph (memory_entities/observations/relations) is for freeform, evolving context. Model mappings are structured, queryable data with typed columns. Mixing them would make queries awkward and lose the benefit of typed columns for filtering/aggregation.

### 2. Data access: `IModelMappingStore` + `PostgresModelMappingStore`

Follows the same pattern as `IMemoryStore` / `PostgresMemoryStore`:

```
Services/
  ModelMapping/
    IModelMappingStore.cs
    PostgresModelMappingStore.cs
    ModelMappingModels.cs
```

**Interface:**

```csharp
public interface IModelMappingStore
{
    Task<ModelMapping?> GetAsync(string agentName, CancellationToken ct = default);
    Task<IReadOnlyList<ModelMapping>> ListAsync(string? tier = null, CancellationToken ct = default);
    Task UpsertAsync(ModelMapping mapping, CancellationToken ct = default);
    Task BulkUpsertAsync(IReadOnlyList<ModelMapping> mappings, CancellationToken ct = default);
    Task DeleteAsync(string agentName, CancellationToken ct = default);
}
```

**Record:**

```csharp
public record AgentModelMapping(
    string AgentName,
    string Tier,
    string PrimaryModel,
    string? PrimaryProvider = null,
    string? AltModel1 = null,
    string? AltProvider1 = null,
    string? AltModel2 = null,
    string? AltProvider2 = null,
    decimal? CostPer1MIn = null,
    decimal? CostPer1MOut = null,
    string? Notes = null,
    string SyncedFrom = "frontmatter");
```

> **Note:** The record is named `AgentModelMapping` (not `ModelMapping`) to avoid MA0049 — the containing namespace is `Ryan.MCP.Mcp.Services.ModelMapping`.

### 3. Sync: Frontmatter → Postgres

A `ModelMappingSyncService` that:

1. Reads all agents from `AgentIngestionCoordinator.Snapshot`
2. For each agent with a `model` frontmatter field, parses the value and comment:
   - `gpt-5.4-nano # capable — alt: big-pickle, gemini-3-flash`
   - → primary=`gpt-5.4-nano`, tier=`capable`, alt1=`big-pickle`, alt2=`gemini-3-flash`
3. Upserts into `agent_model_mappings` with `synced_from = 'frontmatter'`
4. Does NOT overwrite rows where `synced_from = 'manual'` (manual overrides are preserved)

**Trigger:** Runs automatically after agent ingestion (piggyback on the existing `RebuildIndexAsync` pipeline). Also exposed as a manual MCP tool.

### 4. Frontmatter parsing enhancement

The comment format `model: <primary> # <tier> — alt: <alt1>, <alt2>` needs a small parser. Since `YamlFrontmatterParser` strips comments before storing values, we need to either:

- **Option A:** Change `YamlFrontmatterParser` to preserve comments as a separate field (e.g., `model_comment`)
- **Option B:** Parse the raw line from `RawContent` during sync

**Decision:** Option B. The frontmatter parser is intentionally simple and shared across all fields. Parsing the model comment from raw content during sync keeps it isolated and doesn't risk breaking other frontmatter parsing.

### 5. New MCP Tools: `ModelMappingTools.cs`

| Tool | Signature | Purpose |
|------|-----------|---------|
| `get_model_mapping` | `(agent_name: string)` | Returns the stored mapping for one agent |
| `list_model_mappings` | `(tier?: string)` | Lists all mappings, optionally filtered by tier |
| `update_model_mapping` | `(agent_name, primary_model, tier?, alt1?, alt2?, cost_in?, cost_out?, notes?)` | Manual override (sets `synced_from = 'manual'`) |
| `sync_model_mappings` | `(overwrite_manual?: bool)` | Re-sync all mappings from agent frontmatter |
| `recommend_model` | `(task?: string, agent_name?: string)` | Smart recommendation (see below) |
| `list_llm_providers` | `()` | Lists all configured LLM providers, their enabled status, and available models |

### 6. Enhanced `recommend_agent` response

The existing `recommend_agent` tool (AgentTools.cs:121) returns `Name, Description, Scope, Tags, Score, Reasons`. Extend the response to include model info:

```json
{
  "task": "write unit tests for C# service",
  "recommendation": {
    "name": "test-writer",
    "description": "...",
    "scope": "testing",
    "score": 145,
    "reasons": ["name matches 'test'", "..."],
    "model": {
      "primary": "gpt-5.4-nano",
      "tier": "capable",
      "alternatives": ["big-pickle", "gemini-3-flash"],
      "note": "Simple code generation — capable tier sufficient"
    },
    "fetch": "get_agent(\"test-writer\")"
  }
}
```

This is the key integration point — one call gives you agent + model in a single response.

### 7. `recommend_model` logic

When called with just a `task` (no `agent_name`):
1. Run existing `RecommendAgents(task)` to get the top agent
2. Look up that agent's model mapping from Postgres
3. Return the model recommendation with context

When called with an `agent_name`:
1. Look up the mapping directly from Postgres
2. Return it

Future enhancement: accept a `budget` parameter and filter to models under a cost threshold.

---

## Migration path

### Phase 1 (implemented)
- [x] `002_agent_model_mappings.sql` migration
- [x] `IModelMappingStore` + `PostgresModelMappingStore`
- [x] `ModelMappingSyncService` (frontmatter → Postgres)
- [x] `ModelMappingTools.cs` (6 new MCP tools including `list_llm_providers`)
- [x] Extend `recommend_agent` response to include model info
- [x] Wire into DI and migration runner
- [x] LLM provider config in `McpOptions` + `appsettings.json`
- [x] Provider-aware model resolution (sync + recommend tools check enabled providers)
- [x] Auto-sync on startup after agent ingestion completes

### Phase 2 (future)
- [ ] Surface `Model` as a first-class property on `AgentEntry`
- [ ] Add model info to `get_context` response
- [ ] Add model info to `search_agents` and `list_agents` responses
- [ ] Model cost aggregation tool (estimate cost for a multi-agent workflow)

### Phase 3 (future, if warranted)
- [ ] Local budget tracking (log token usage per model per day)
- [ ] Provider API integrations for usage/quota checking
- [ ] Budget-aware fallback recommendations

---

## Files to create/modify

### New files
| File | Purpose |
|------|---------|
| `Database/Migrations/002_agent_model_mappings.sql` | Schema migration |
| `Services/ModelMapping/IModelMappingStore.cs` | Interface |
| `Services/ModelMapping/PostgresModelMappingStore.cs` | Postgres implementation |
| `Services/ModelMapping/ModelMappingModels.cs` | Record types |
| `Services/ModelMapping/ModelMappingSyncService.cs` | Frontmatter → Postgres sync |
| `McpTools/ModelMappingTools.cs` | 5 new MCP tools |

### Modified files
| File | Change |
|------|--------|
| `Program.cs` | Register new services in DI, run migration, add startup sync task |
| `McpTools/AgentTools.cs` | Add `IModelMappingStore` injection, make `recommend_agent` async with model info |
| `McpOptions.cs` | Add `LlmProviders` list and `LlmProviderOptions` class |
| `appsettings.json` | Add `LlmProviders` section with 4 configured providers |
| `Services/Memory/MemoryMigrationRunner.cs` | Generalized to run all `*.sql` migrations in alphabetical order (was hardcoded to `001_init_memory.sql`) |

### NOT modified
| File | Why |
|------|-----|
| `AgentEntry.cs` | Phase 2 — keeping model as Frontmatter dict entry for now |
| `YamlFrontmatterParser.cs` | Parsing model comment from raw content during sync instead |
| `AgentIngestionCoordinator.cs` | Sync triggered by background task in Program.cs instead of coupling into coordinator |
| `Ryan.MCP.Mcp.csproj` | Already has wildcard for `Database\Migrations\*.sql` |

---

## Cross-client compatibility

| Coding Agent | How it uses this |
|---|---|
| **OpenCode** | Calls `recommend_agent` or `recommend_model` via MCP → gets model name → user can switch or it's used if agents are in `.opencode/agents/` |
| **Claude Code** | Calls same tools via MCP → model info is advisory (Claude Code controls its own model) → user can switch session model |
| **Cursor** | Same MCP tools → advisory → user sees recommendation in tool output |
| **Any MCP client** | Same pattern — the tools return structured JSON that any client can parse |

The advisory approach is the only one that works across all clients. No MCP server can force a host to switch models.

---

## Risks & mitigations

| Risk | Likelihood | Mitigation |
|------|-----------|-----------|
| Frontmatter comment format changes | Low | Sync parser is isolated; fallback to raw `model` value without tier/alts |
| Postgres not running | Medium | Graceful degradation — fall back to in-memory frontmatter lookup |
| Model names become stale | Medium | `sync_model_mappings` tool lets user re-sync anytime; manual overrides preserved |
| Token cost of returning model info on every `recommend_agent` call | Low | ~50 extra tokens per response; negligible |

---

## Open questions

1. ~~**Should `recommend_model` consider the user's configured providers?**~~ **Resolved:** Yes. `LlmProviders` config added to `McpOptions` with `Enabled` flag, `ApiBaseUrl`, `ApiKey` (env var expansion), and `Models` list. The sync service resolves providers during frontmatter parsing. `recommend_model` and `list_llm_providers` surface provider availability. Only enabled providers are considered.

2. **Should we store model pricing in Postgres or hardcode a lookup table?** Pricing changes infrequently but does change. A Postgres table (`model_pricing`) would be more maintainable but adds complexity.

3. ~~**Should the sync run on every agent re-index or only on explicit request?**~~ **Resolved:** Runs automatically on startup via a background task in `Program.cs` that waits for agent ingestion to complete (up to 30s), then syncs. Also available via the `sync_model_mappings` MCP tool for manual re-sync.

---

## Provider Configuration Design

### `LlmProviderOptions` in `McpOptions.cs`

```csharp
public class LlmProviderOptions
{
    public string Name { get; set; } = "";          // e.g. "opencode-zen"
    public bool Enabled { get; set; } = true;
    public string? ApiBaseUrl { get; set; }          // e.g. "https://openrouter.ai/api/v1"
    public string? ApiKey { get; set; }              // "${env:OPENCODE_ZEN_API_KEY}" — env var expansion
    public List<string> Models { get; set; } = [];   // models available via this provider
    public string? Notes { get; set; }               // e.g. "pay-as-you-go, all models"
}
```

### Provider config in `appsettings.json`

Four providers configured:
- **`opencode-zen`** (enabled) — 8 models: `claude-opus-4-6`, `claude-sonnet-4-6`, `gpt-5.3-codex`, `gpt-5.4-nano`, `gemini-3.1-pro`, `gemini-3-flash`, `big-pickle`, `deepseek-v4`
- **`anthropic`** (disabled) — direct API, not currently used
- **`openai`** (disabled) — direct API, not currently used
- **`google`** (disabled) — direct API, not currently used

### How providers are used

1. **During sync:** `ModelMappingSyncService.ResolveProvider()` finds the first enabled provider whose `Models` list contains the model name → stores as `primary_provider` / `alt_provider_1` / `alt_provider_2`
2. **During `recommend_model`:** `FormatRecommendation()` checks provider availability and returns `available: true/false` for each model
3. **Via `list_llm_providers`:** Returns all providers with enabled status, model counts, and whether API key is configured

### Design rationale

- **Config-driven, not hardcoded** — providers and their model lists live in `appsettings.json`, easily updated without code changes
- **`${env:VAR}` expansion for API keys** — same pattern as `ExternalConnectors` headers; keys never stored in plain text
- **Disabled providers still listed** — user can enable them by changing config; `list_llm_providers` shows the full picture

---

## Decision rationale

- **Advisory over interceptive** — MCP protocol limitation; the only honest approach
- **Postgres over in-memory** — Persists across restarts; queryable; extensible for budget tracking later
- **Separate table over knowledge graph** — Structured data needs typed columns, not freeform observations
- **Parse from raw content over changing frontmatter parser** — Isolation; no risk to existing parsing
- **Phase 1 keeps it minimal** — 6 new files, 4 modified files, no config changes, no external API calls
