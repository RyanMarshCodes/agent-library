# Context Window Management

Context is the single biggest constraint shaping how coding agents think, reason, and respond. Every token competes for attention.

## The Hard Limit

Each LLM has a hard-coded context window limit. When exceeded, you get "context window exceeded" errors or the model stops mid-output.

**Golden rule:** Stay under 70–80% of your total limit.

## The Lost-in-the-Middle Problem

As context grows, performance degrades. Models over-prioritize information from the start and end (primacy/recency bias), while details in the middle fade away. This is the "needle-in-a-haystack" problem.

A lean, focused session often outperforms a massive context.

## Context Hygiene Rules

1. **Keep prompts short and specific** — prefer linked references or summaries over full file pastes
2. **Avoid unnecessary MCP servers** — each adds system prompts, tool definitions, and metadata that bloat your context
3. **Prefer linked references** over pasting entire files when possible
4. **Use tools to check context** — if available, run `context` to see usage breakdown
5. **Clear or compact proactively** — don't wait until the model becomes forgetful
6. **Reset between tasks** — start fresh when shifting focus or after completing significant milestones

## When to Reset

- Starting a new task or file
- Project focus has shifted
- Past 75% of context limit
- Model feels slow, vague, or inconsistent

## Compact vs Clear

- **compact**: Summarizes existing chat, preserving intent. Use when you want to keep the overall context mid-session.
- **clear**: Wipes conversation entirely. Use when starting fresh or when context is too cluttered to be useful.

## Symptoms of Context Overload

When your model feels "dumber" than usual, it's usually not the model — it's drowning in context. Symptoms include:
- Inconsistent responses on mid-conversation details
- Forgetting requirements stated earlier
- Vague or generic suggestions
- Performance degradation on refactoring/debugging tasks

## Model Comparison

When evaluating models, don't just look at context window size. Ask: How well does it retrieve and use information within that window? A smaller, intelligent context often beats a massive one.

## Quota & Budget Management (OpenCode / Zen)

Free-tier quotas are consumed per output token, not per message. Long sessions with large accumulated context burn quota fastest.

**Per-session habits:**
- Start a fresh session when switching tasks — don't carry stale context from a previous task
- Use `/clear` or start a new chat after any major task completes
- Prefer `search_documents(query)` or `read_document(tier, path)` over loading entire knowledge files
- Only call `get_context()` once per task, not before every step
- Avoid `memory_read()` — use `memory_recall(query, maxResults=3)` instead

**Preserve quota across the week/month:**
- Batch related sub-tasks into one session rather than starting many small sessions (each session bootstraps ~2–5K tokens of overhead)
- Use `recommend_agent` + `get_agent` only for the specific agent you need, not `list_agents` for browsing
- If context grows past 70%, `/compact` rather than continuing — compacted sessions cost far fewer tokens per subsequent message
- Avoid loading large scaffolding skills (init-dotnet-api, init-angular, etc.) unless you are actually scaffolding

**Signs you're burning quota unnecessarily:**
- Calling `get_context()` at the start of every single step
- Loading full agent content for agents you won't use
- Keeping a session alive across multiple unrelated tasks
- Asking the model to "summarize everything so far" mid-session (forces a full context re-read)