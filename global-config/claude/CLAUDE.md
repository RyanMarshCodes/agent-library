# Global Instructions (Claude Code)

## Slash Commands

| Command | Description |
|---------|-------------|
| `/dotnet-pr` | Quickly generates a PR summary, risk, rollback, and reviewer focus — see pr-description.agent.md for full details |

# Ryan.MCP Knowledge-Graph Memory

This system uses a canonical, token-efficient memory pattern covering recall and persist. 

## Canonical Guidance

For detailed usage, query construction, and fallback, see:
`global-config/_shared/memory-bridge-instructions.md` (single source of truth).

*Do not duplicate or paraphrase these instructions elsewhere. Only agents or tools requiring memory should link here.*

## Summary
- Use `memory_status`, `memory_recall`, `memory_persist`, `memory_link` as standard
- Memory recall and persist must be proactive, token-conscious, but not user-triggered
- Prefer focused queries, short results, persist only high-value context
- Individual specialist agents **do NOT** interact directly with memory tools; context is injected by the orchestrator/host

*See the canonical doc for details and fallback connector usage.*

---

# Context Window Management

*See memory-bridge-instructions.md for full details and workflow*

1. Keep prompts short and specific
2. Avoid unnecessary MCP servers
3. Prefer linked references over long file pastes
4. Use `context` tool to check usage
5. Compact/clear when context is full
6. Reset between tasks

Symptoms of context overload:
- Forgetting requirements
- Vague responses
- Performance degradation

---

*Everything above not covered here should only be documented in one of: AGENTS.md, memory-bridge-instructions.md, or the canonical agent file as appropriate.*
