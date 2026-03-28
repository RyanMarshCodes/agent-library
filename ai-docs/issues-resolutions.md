# Issues & Resolutions Log

Persistent, append-only record of issues encountered across all sessions. Newest entries first.

Format per entry:
- **Date** | **Agent** | **Category**
- **Issue**: what went wrong or was unclear
- **Resolution**: what was done and why

---

<!-- New entries go here, above the line below -->

- **Date** 2026-03-27 | **Agent** cursor | **Category** tooling  
- **Issue**: Repository-wide search sometimes fails on Windows with `rg: .\nul: Incorrect function` when the workspace contains a reserved/problem path.  
- **Resolution**: Use scoped searches (e.g. `ai-docs/`, `.claude/`) or path-specific greps instead of workspace root until the path issue is resolved.

---

*No entries yet. Agents: add entries here when you encounter and resolve non-trivial issues.*
