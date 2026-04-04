# Quota & Budget Management (OpenCode / Zen)

Free-tier quotas are consumed per output token, not per message. Long sessions with large accumulated context burn quota fastest.

## Per-Session Habits

- Start a fresh session when switching tasks — don't carry stale context
- Use `/clear` or start a new chat after any major task completes
- Prefer `search_documents(query)` or `read_document(tier, path)` over loading entire knowledge files
- Only call `get_context()` once per task, not before every step
- Avoid `memory_read()` — use `memory_recall(query, maxResults=3)` instead

## Preserve Quota Across the Week/Month

- Batch related sub-tasks into one session (each session bootstraps ~2–5K tokens of overhead)
- Use `recommend_agent` + `get_agent` only for the specific agent you need, not `list_agents` for browsing
- If context grows past 70%, `/compact` rather than continuing
- Avoid loading large scaffolding skills unless you are actually scaffolding

## Signs You're Burning Quota Unnecessarily

- Calling `get_context()` at the start of every single step
- Loading full agent content for agents you won't use
- Keeping a session alive across multiple unrelated tasks
- Asking the model to "summarize everything so far" mid-session (forces a full context re-read)
