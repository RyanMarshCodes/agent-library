# NAF MCP Server — Pre-Implementation Checklist

## Overview

Before building the MCP server, gather these materials. The plan is in `implementation-plan-naf-mcp-server.md`.

Use this checklist to prepare both:

- **Canonical knowledgebase ingestion** (`kb.*` tables + vectors)
- **Dynamic memory seeding** (`memory.*` tables)

---

## Knowledge Documents to Gather

### Organization Standards (into `knowledge/organization/`)

| Document | Description | Status |
|----------|-------------|--------|
| C# coding standards | Team conventions for .NET/C# | [ ] Gather |
| TypeScript standards | Team conventions for TS | [ ] Gather |
| Angular standards | Team conventions for Angular | [ ] Gather |
| React standards | Team conventions for React | [ ] Gather |
| General code standards | KISS, DRY, SRP, security basics | [ ] Gather |
| SDLC workflow | How features flow through dev → prod | [ ] Gather |
| Code review checklist | What's checked in PRs | [ ] Gather |
| Testing standards | Test frameworks, patterns, coverage | [ ] Gather |

### Architecture Decisions (into `knowledge/organization/`)

| Document | Description | Status |
|----------|-------------|--------|
| Architecture overview | System architecture diagram | [ ] Gather |
| Service dependencies | How services connect | [ ] Gather |
| Data architecture | Databases, stores, caching | [ ] Gather |
| Security policies | Auth, authorization, secrets | [ ] Gather |
| Observability | Logging, monitoring, alerts | [ ] Gather |
| System context map | Product boundaries and integration points | [ ] Gather |
| Workflow runbooks | Common workflows and handoffs across systems | [ ] Gather |

### External Standards (into `knowledge/official/`)

| Document | Source | Status |
|----------|--------|--------|
| OWASP Top 10 | owasp.org | [ ] Gather |
| Team conventions | Existing docs | [ ] Gather |

---

## Agents to Create

### Core Agents (in `src/NafMcp.Mcp/agents/`)

| Agent | Purpose | Priority |
|-------|---------|-----------|
| Planning Agent | Break requirements into tasks | High |
| Implementation Agent | Generate code following standards | High |
| Review Agent | Code review (quality, security) | High |
| Testing Agent | Generate meaningful tests | High |
| Debug Agent | Troubleshoot errors | Medium |
| Security Agent | Security audit | Medium |
| Architect Agent | Design architecture | Medium |
| Explain Agent | Explain code in plain English | Low |

### SDLC Workflow Agents (optional)

| Agent | Purpose |
|-------|---------|
| /init | Bootstrap new repo |
| /spec | Write requirements |
| /validate | Validate SDLC phase |
| /reflect | Self-review before PR |
| /wrapup | Close out feature |

---

## Memory Entities to Seed

Seed these into memory after server is running:

| Entity | Type | Observations |
|--------|------|---------------|
| NAF team | convention | Team size, tech stack |
| Architecture | concept | Main services, dependencies |
| Code review process | convention | What's checked, who approves |
| Deployment process | convention | CI/CD pipeline, environments |
| Known services | service | List of services and owners |
| On-call rotation | convention | Who to contact for issues |

---

## Knowledgebase Data to Ingest (Canonical `kb` Layer)

Prioritize these sources for entity/relationship extraction and document chunking:

| Source Type | Examples | Ingestion Target | Priority |
|------------|----------|------------------|----------|
| Product docs | Product overviews, feature catalogs | `kb.Entities`, `kb.Documents` | High |
| Architecture docs | C4/context diagrams, service maps, ADRs | `kb.Entities`, `kb.Relations` | High |
| Workflow docs | Release flow, incident flow, onboarding flow | `kb.Entities`, `kb.Relations`, `kb.Documents` | High |
| API and integration docs | OpenAPI specs, integration guides | `kb.Entities`, `kb.Relations`, `kb.DocumentChunks` | High |
| Runbooks | Deploy, rollback, troubleshooting | `kb.Documents`, `kb.DocumentChunks` | Medium |
| Ownership metadata | Team owners, escalation paths | `kb.Entities`, `kb.Relations` | Medium |

### Required metadata for each ingested source

| Metadata Field | Why it matters | Status |
|---------------|----------------|--------|
| Canonical source path/URL | Citation + traceability | [ ] Ready |
| Source owner/team | Trust and accountability | [ ] Ready |
| Classification (`public-internal`/`internal`/`restricted`) | Access and safe retrieval | [ ] Ready |
| Last reviewed date | Freshness scoring | [ ] Ready |
| Content hash/version id | Staleness detection | [ ] Ready |

### Entity and relation taxonomy (define before ingest)

| Item | Example values | Status |
|------|----------------|--------|
| Entity kinds | `product`, `service`, `workflow`, `team`, `domain` | [ ] Defined |
| Relation types | `depends_on`, `calls`, `owns`, `supports`, `part_of` | [ ] Defined |
| Workflow schema | `trigger`, `steps`, `systems`, `owner`, `SLA` | [ ] Defined |
| Confidence policy | Source precedence and conflict resolution | [ ] Defined |

### Minimum Viable Ingest Pack (Week 1)

Start with this small, high-value set before broad ingestion. Target 10-15 canonical sources max.

| # | Document | Why include it first | Maps to |
|---|----------|----------------------|---------|
| 1 | Product portfolio overview | Establishes the canonical list of products and domains | `kb.Entities` (`product`, `domain`) |
| 2 | System context / integration map | Defines how products and platforms connect | `kb.Relations` (`depends_on`, `calls`, `part_of`) |
| 3 | Primary architecture diagram + notes | Captures service boundaries and responsibilities | `kb.Entities`, `kb.Relations` |
| 4 | SDLC workflow standard | Gives common execution flow AI should follow | `kb.Entities` (`workflow`), `kb.Documents` |
| 5 | Deployment + rollback runbook | Critical operational workflow for safe delivery | `kb.Documents`, `kb.DocumentChunks` |
| 6 | Incident response runbook | Enables troubleshooting and escalation context | `kb.Documents`, `kb.DocumentChunks` |
| 7 | API integration guide (top product) | Anchors cross-system behavior with concrete contracts | `kb.Documents`, `kb.Relations` |
| 8 | Ownership/escalation matrix | Adds accountable teams and support routing | `kb.Entities` (`team`), `kb.Relations` (`owns`, `supports`) |
| 9 | Security baseline/policy doc | Enforces secure implementation and handling rules | `kb.Documents` (`source_kind=standards`) |
| 10 | Coding standards + code review checklist | Aligns generated changes with team quality gates | `kb.Documents` (`source_kind=standards`) |

### MVP ingest acceptance criteria

- [ ] All 10 sources are ingested with full metadata (owner, classification, reviewed date, source URL).
- [ ] `kb_search` returns cited results from at least 3 different source types.
- [ ] `kb_get_entity` resolves at least top 5 products and their owning teams.
- [ ] `kb_get_workflow` returns at least SDLC, deployment, and incident workflows.
- [ ] Conflicting facts are traceable to source with clear precedence rules applied.

---

## Embedding and Retrieval Readiness

| Decision | Options | Status |
|---------|---------|--------|
| Embedding provider | Azure OpenAI / OpenAI / local model | [ ] Choose |
| Embedding model dimension | Match `vector(n)` (e.g., 1536) | [ ] Choose |
| Chunking strategy | Token size, overlap, heading-aware split | [ ] Define |
| Hybrid ranking | Keyword + vector + relation expansion weighting | [ ] Define |
| Re-index cadence | On merge / nightly / manual refresh | [ ] Define |

---

## Security & Governance Readiness

| Control | Expectation | Status |
|--------|-------------|--------|
| Secrets management | Connection strings and keys in secret store only | [ ] Ready |
| Ingestion allowlist | Approved repos/folders for indexing | [ ] Ready |
| PII/content exclusions | Explicit denylist for sensitive data | [ ] Ready |
| MCP endpoint auth | Non-local access requires authentication | [ ] Ready |
| Audit logging | Tool usage logged without sensitive payloads | [ ] Ready |
| Vulnerability scanning | NuGet audit in CI | [ ] Ready |

---

## MCP Server Configuration

### Required Config Values

| Config | Value | Who provides |
|--------|-------|--------------|
| Postgres connection | `Host=, Database=, User=, Password=` | DevOps |
| pgvector enabled | `CREATE EXTENSION vector` permissions | DBA/DevOps |
| Embedding provider endpoint | URL/model/deployment id | AI platform owner |
| Embedding API key or identity | Key or managed identity | DevOps/Security |
| Server URL | `http://HOST:PORT/mcp` | Once hosted |
| Agent model mappings | Default models per agent | You decide |

### VS Code Developer List

| Developer | VS Code MCP configured? |
|-----------|------------------------|
| [Name] | [ ] |
| [Name] | [ ] |
| [Name] | [ ] |
| [Name] | [ ] |
| [Name] | [ ] |

---

## Implementation Dependencies

### People

| Person | Role | Responsibility |
|--------|------|----------------|
| [Owner] | MCP owner | Build, deploy, maintain |
| [Backup] | Backup owner | Support when owner unavailable |

### Infrastructure

| Resource | Status |
|----------|--------|
| Dev machine to host | [ ] Identify |
| OR Container hosting | [ ] Arrange |
| Postgres database | [ ] Provision |

---

## Quick Start Template

```markdown
# NAF MCP Server — Setup

## 1. Clone
git clone git@github.com:your-org/naf-mcp.git
cd naf-mcp

## 2. Configure
# Edit appsettings.json with your Postgres connection

## 3. Run locally
dotnet run --project src/NafMcp.AppHost

## 4. Connect VS Code
# Add to .vscode/mcp.json:
{
    "servers": {
        "naf-mcp": {
            "type": "sse",
            "url": "http://localhost:5000/mcp"
        }
    }
}

## 5. Test
# In Copilot chat: "@naf-mcp list_agents"
```

---

## When Complete

- [ ] All knowledge documents gathered
- [ ] KB source metadata complete (owner, classification, reviewed date, source URL)
- [ ] Entity kinds and relation taxonomy finalized
- [ ] Agent definitions created
- [ ] Memory seeded with initial context
- [ ] Canonical knowledgebase ingested and indexed (chunks + embeddings)
- [ ] Server runs locally
- [ ] VS Code connects successfully
- [ ] At least 2 devs can connect
- [ ] Documented setup for team

---

## Questions to Answer First

1. What .NET/C# standards does your team follow?
2. What TypeScript/Angular/React conventions?
3. What's your current SDLC workflow?
4. Where will the MCP server live? (your machine, shared dev, container)
5. Who owns maintaining it?
6. Which devs should be early adopters?
7. Which sources are authoritative when docs conflict?
8. What data should never be ingested (PII/secrets/internal restricted)?