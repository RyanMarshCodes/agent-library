# Agent Library Audit

**Date**: 2026-03-30
**Auditor**: AIAgentExpert workflow
**Scope**: agents/, skills/, catalog.json, knowledge/, MCP server tools

## Summary

56 agents, 19 skills, 12 knowledge docs, MCP server with 10+ tool handlers. Found **23 issues** across purpose clarity, overlap, naming consistency, guardrails, delegation, and catalog sync.

---

## Issues Found

### HIGH: Purpose Overlap / Redundancy

| Agents | Problem | Recommendation |
|--------|---------|----------------|
| `test-writer` / `test-generator` / `ac-test-planner` | Three test agents with unclear boundaries. test-writer = "any stack", test-generator = "from scratch", ac-test-planner = "from AC" — subtle distinction, likely confusing | Consolidate into one `test-agent` with clear modes, or document exact triggers |
| `janitor` / `code-simplifier` | janitor = "stack-agnostic cleanup, purely subtractive", code-simplifier = "simplify without changing functionality" — near identical scope | Merge or clearly distinguish: janitor = delete unused, simplifier = refactor |
| `documentation` / `technical-writer` | Two documentation agents. documentation = "any project", technical-writer = "technical writing specialist" — duplicates | Keep one, make the other a delegation target for specific formats |
| `backend-developer` / `fullstack-developer` | fullstack = "spanning DB + API + frontend together" — implies backend already does API-only. Overlap on API work | Clarify: backend = API-only, fullstack = coordinated cross-layer |

### HIGH: Naming Inconsistency in catalog.json

| Entry | Issue |
|-------|-------|
| `OrchestratorAgent` | Has "Agent" suffix |
| `GameDirectorAgent` | Has "Agent" suffix |
| `AccessibilityExpert` | No suffix |
| `DocumentationAgent` | Has "Agent" suffix |
| `TechnicalWriterAgent` | Has "Agent" suffix |
| `CSharpExpertAgent` | Has "Agent" suffix |
| `UnityExpertAgent` | Has "Agent" suffix |

**Recommendation**: Pick one convention (Agent suffix or not) and apply consistently. Suggest: remove "Agent" suffix for all entries.

### HIGH: Missing / Broken Delegation References

| Agent | Issue |
|-------|-------|
| `frontend-developer.agent.md` | References `qa-expert` in line 139, but no such agent exists in catalog |
| `backend-developer.agent.md` | References `database-optimizer`, `devops-engineer`, `performance-engineer`, `security-auditor` — none exist |
| `troubleshooting.agent.md` | References `verify agent` (lowercase, not in catalog) |

**Recommendation**: Audit all agent files for agent references and ensure every referenced agent exists in catalog.

### MEDIUM: Skills vs Agents Redundancy

| Skill | Agent | Redundancy |
|-------|-------|------------|
| `audit-agents` skill | `ai-agent-expert.agent.md` | Skill exists to audit agents, but agent is used for agent design/audit. Both do similar work |
| `simplify` skill | `code-simplifier.agent.md` | Both simplify code |
| `document` skill | `documentation.agent.md`, `technical-writer.agent.md` | Three ways to write docs |
| `troubleshoot` skill | `troubleshooting.agent.md` | Both debug/root cause |
| `security-audit` skill | `security-check.agent.md` | Both do security |
| `review` skill | Delegates to `code-analysis`, `security-check`, `code-simplifier` | Composes agents — less overlap, good pattern |

**Recommendation**: Consider making skills thin wrappers that delegate to agents, rather than duplicating logic. Or clearly document when to use skill vs agent.

### MEDIUM: Hardcoded References to Non-Existent MCP Tools

- Multiple agents reference `context-manager` for project context — need to verify this is actually implemented in MCP server's `ContextTools.cs`
- `frontend-developer` and `backend-developer` send JSON to `context-manager` — verify the tool accepts this format

### MEDIUM: Catalog version mismatch

- Catalog shows `version: "0.12.0"`
- Some agents have inline version comments (e.g., `csharp-expert.agent.md` has `# version: 2026-01-20a`)
- No clear versioning strategy — is catalog version bumped on agent changes?

### LOW: Inconsistent file structure

Some agents lack clear H1:
- Check each agent has `# AgentName` as H1
- Some have YAML frontmatter with `name`, others don't

### LOW: Knowledge files not indexed

- `knowledge/` directory exists with 12 .md files
- No agent references these for standards — agents rely on MCP server's `search_documents`, but the files aren't obviously indexed
- `catalog.json` doesn't mention knowledge docs

---

## Delegation Coverage: Gaps

| Missing capability | Could delegate to |
|--------------------|-------------------|
| Real-time/WebSocket engineering | Referenced but no agent |
| Database optimization (query tuning) | Referenced but no agent |
| Performance engineering | Referenced but no agent |
| QA/Testing coordination | `qa-expert` referenced, doesn't exist |
| IDE extension development | No dedicated agent |
| Browser extension development | No dedicated agent |

---

## Priority Improvements

### Must Fix (High)

1. **Fix broken agent references** — `qa-expert`, `database-optimizer`, `devops-engineer`, `performance-engineer`, `security-auditor`, `verify agent` don't exist
2. **Resolve purpose overlap** — Consolidate test agents, simplify janitor/code-simplifier, dedupe documentation agents
3. **Standardize catalog naming** — Choose Agent suffix convention, apply to all entries

### Should Fix (Medium)

4. **Dedupe skills/agents** — Make skills thin wrappers, or document when to use each
5. **Verify context-manager implementation** — Ensure MCP tool accepts the JSON format agents send
6. **Add knowledge docs to catalog or create index** — Make standards discoverable

### Nice to Have (Low)

7. **Versioning strategy** — Define how catalog/agent versions relate
8. **Consistent H1** — Ensure all agents have proper H1
9. **Add missing agent capabilities** — WebSocket, DB tuning, IDE extensions

---

## MCP Server Usage Assessment

**Current tools exposed** (from file scan):
- `AgentTools.cs` — list_agents, get_agent, search_agents
- `ContextTools.cs` — get_context (likely the context-manager)
- `ConnectorTools.cs` — list_external_connectors, list_external_mcp_tools, call_external_mcp_tool
- `DocumentTools.cs` — list_standards, read_document, search_documents
- `SpecTools.cs` — analyze specs (OpenAPI/GraphQL)
- `FetchTools.cs` — web fetch
- `AgentPrompts.cs`, `DocumentPrompts.cs` — prompts

**Good:**
- Unified entry via `get_context(language, task?)` — polyglot, single call
- External connector proxy (memory, other MCP servers)
- Standards indexed and searchable

**Could improve:**
- No direct tool to list/search skills — only agents and standards
- No tool to execute a skill directly
- No tool to query the knowledge graph (memory) directly at top level — requires `call_external_mcp_tool`

---

## Positives

- Strong delegation infrastructure via `orchestrator.agent.md` routing table
- Good separation: game domain (director, design, shipping, market-intel) in its own track
- MCP server provides external connector proxy — can bridge to other MCP servers
- Skills provide Claude-specific workflows (review, troubleshoot) that compose agents
- Catalog is well-maintained with tags for discovery

---

## Files Needing Attention

| File | Issue |
|------|-------|
| `agents/frontend-developer.agent.md:139` | References non-existent `qa-expert` |
| `agents/backend-developer.agent.md:233-238` | References 4 non-existent agents |
| `agents/troubleshooting.agent.md:192` | References `verify agent` |
| `catalog.json` | Inconsistent naming, no knowledge docs |
| `skills/` | Duplication with agents |

---

# Global Configs Audit

**Scope**: `global-config/` (symlinked to user profile directories)

## Files Present

| Path | Type | Symlink Target |
|------|------|----------------|
| `global-config/claude/CLAUDE.md` | Instructions | `%USERPROFILE%\.claude\CLAUDE.md` |
| `global-config/gemini/GEMINI.md` | Instructions | `%USERPROFILE%\.gemini\GEMINI.md` |
| `global-config/gemini/AGENTS.md` | Instructions | `%USERPROFILE%\.gemini\AGENTS.md` |
| `global-config/opencode/opencode.json` | Config | `%USERPROFILE%\.config\opencode\opencode.json` |
| `global-config/opencode/instructions/ryan-mcp-memory.md` | Instructions | `%USERPROFILE%\.config\opencode\instructions\...` |
| `global-config/cursor/rules/ryan-mcp-knowledge-graph-memory.mdc` | Rules | `%USERPROFILE%\.cursor\rules\...` |
| `global-config/copilot/github/copilot-instructions.md` | Instructions | `%USERPROFILE%\.github\copilot-instructions.md` |

## Issues Found

### HIGH: Duplicate Content Across Tools

The same memory-bridge instructions are copy-pasted into **4 separate files**:

1. `global-config/claude/CLAUDE.md`
2. `global-config/gemini/AGENTS.md`
3. `global-config/gemini/GEMINI.md`
4. `global-config/opencode/instructions/ryan-mcp-memory.md`
5. `global-config/cursor/rules/ryan-mcp-knowledge-graph-memory.mdc`
6. `global-config/copilot/github/copilot-instructions.md`

**All contain**:
- Memory bridge instructions (list_external_mcp_tools, call_external_mcp_tool)
- Same tool names, same examples, same instructions

**Problem**: 
- If you change how memory works, you must update 6 files
- Risk of drift/inconsistency
- Violates DRY

**Recommendation**: 
- Create a single canonical `memory-bridge-instructions.md` in `global-config/`
- Have each tool's config file reference/include that canonical version
- Or use file references if the tool supports it

### MEDIUM: Missing Symlinks in Setup Script

The `setup-symlinks.ps1` script **doesn't create all the symlinks that exist in the directory**:

| File in `global-config/` | Setup script creates symlink? |
|--------------------------|-------------------------------|
| `claude/CLAUDE.md` | ✅ Yes |
| `gemini/GEMINI.md` | ✅ Yes |
| `gemini/AGENTS.md` | ✅ Yes |
| `opencode/opencode.json` | ✅ Yes |
| `opencode/instructions/ryan-mcp-memory.md` | ❌ No (but referenced in opencode.json) |
| `cursor/rules/ryan-mcp-knowledge-graph-memory.mdc` | ❌ No |
| `copilot/github/copilot-instructions.md` | ❌ No (commented out) |

**However**: There's a **better alternative**: `setup-junctions.ps1` creates **directory junctions** (no admin required) that cover:
- `.cursor\rules` → entire `cursor/rules/` directory
- `.config\opencode\instructions` → entire `opencode/instructions/` directory

**This is the smarter approach** — junctions link entire directories, not individual files. The junctions script is more complete than the symlinks script.

**Recommendation**: 
- Document that `setup-junctions.ps1` is the preferred approach (no admin needed)
- Consider deprecating or removing individual file symlink logic in favor of junctions
- Keep symlinks as fallback for tools that don't support directory-level configs

### MEDIUM: Cursor Rules File Extension

- `cursor/rules/ryan-mcp-knowledge-graph-memory.mdc` uses `.mdc` extension
- Verify that Cursor supports this extension and it works as expected
- Check if other Cursor rules use `.cursorrules` or `.mdc` consistently

### LOW: OpenCode Instructions Path

In `global-config/opencode/opencode.json`:
```json
"instructions": [
  "~/.config/opencode/instructions/ryan-mcp-memory.md"
]
```

This uses `~` (home dir) but the actual symlink setup doesn't create this path explicitly — it relies on the `opencode.json` symlink being created. This is fine but could be clearer in the setup script.

### LOW: GitHub Copilot Instructions Disabled

The Copilot symlink is **commented out** in the setup script with a note to "uncomment after installing GitHub Copilot". 
- Consider making this conditional or documenting it

---

## Global Configs: What's Working Well

1. **Centralized source of truth** — All tool configs derive from this single `global-config/` directory
2. **Automated symlink setup** — `setup-symlinks.ps1` handles creation
3. **Consistent memory-bridge pattern** — All tools have the same memory instructions (despite duplication)
4. **Multi-platform support** — Claude, Gemini, OpenCode, Cursor, Copilot all covered

---

## Recommendations Summary

| Priority | Action |
|----------|--------|
| HIGH | Extract memory instructions to single canonical file, reference from each tool |
| MEDIUM | Add missing symlinks to setup script (cursor rules, opencode instructions) |
| MEDIUM | Verify `.mdc` extension works in Cursor |
| LOW | Enable/uncomment Copilot symlink or document requirement |
| LOW | Consider adding junction support for non-admin Windows (in addition to symlinks) |

---

## Integration: Global Configs ↔ Agent Library

The global configs **use** the MCP server (`ryan-mcp`), which exposes **agents** from this repository (`catalog.json`). The flow is:

```
global-config/* → ryan-mcp (MCP server) → list_agents/get_agent → agent definitions in agents/*.md
```

**This is a good pattern** — global configs are thin, the intelligence lives in the agent library.

**However**: The global configs don't reference the agent library's `catalog.json` directly — they rely on the MCP server's `list_agents` tool. This is correct, but worth noting for discoverability.
