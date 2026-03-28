# Central Agent Library (`ai-docs/`)

Purpose:

- Store reusable custom agent definitions in one portable location (`ai-docs/agents/`).
- Keep a versioned catalog (`ai-docs/catalog.json`) for discovery and MCP clients.

Canonical layout in **this** repository:

```
ai-docs/
├── README.md                 # This file
├── catalog.json               # Agent metadata + file paths
├── MIGRATION-CHECKLIST.md     # Copy / wire / validate checklist
├── issues-resolutions.md      # Shared memory (see repo AGENTS.md)
├── sessions/                  # Session summaries
└── agents/
    └── *.agent.md             # Portable agent instruction files
```

## File naming (agents)

- **On disk**: `kebab-case.agent.md` (e.g. `code-analysis.agent.md`, `tech-debt-analysis.agent.md`).
- **Catalog `name`**: stable PascalCase id for tooling (e.g. `CodeAnalysisAgent`) — see `catalog.json`.
- **Inside the file**: use a clear H1 for the human-facing title (e.g. `# Code Analysis Agent` or `# TechDebtAnalysisAgent`).

Legacy names (`PascalCaseAgent.md`, `topic.md`) are deprecated; MCP ingestion here targets `*.agent.md`.

## How to use in another project

1. Copy `ai-docs/agents/` and `ai-docs/catalog.json` (or the whole `ai-docs/` tree if you want memory + checklist).
2. Point your runtime / MCP `LibraryPath` at the `agents` folder.
3. Keep environment-specific values out of agent markdown.
4. Bump `catalog.json` version when you change agents.

## Notes

- Definitions are plain Markdown instructions (portable across AI tools).
- Designed for local-first workflows; align with your org’s secrets and safety policies.
