# Context Window Management

Context is the single biggest constraint shaping how coding agents think, reason, and respond. Every token competes for attention.

## The Hard Limit

Each LLM has a hard-coded context window limit. When exceeded, you get "context window exceeded" errors or the model stops mid-output.

**Golden rule:** Stay under 70–80% of your total limit.

## The Lost-in-the-Middle Problem

As context grows, performance degrades. Models over-prioritize information from the start and end (primacy/recency bias), while details in the middle fade away.

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

- **compact**: Summarizes existing chat, preserving intent. Use mid-session to reclaim space.
- **clear**: Wipes conversation entirely. Use when starting fresh or context is too cluttered.

## Symptoms of Context Overload

When your model feels "dumber" than usual, it's usually not the model — it's drowning in context:
- Inconsistent responses on mid-conversation details
- Forgetting requirements stated earlier
- Vague or generic suggestions
- Performance degradation on refactoring/debugging tasks
