---
name: explain
description: Plain-English explanation of code, a file, an architecture, a concept, or an error — adapts depth and framing to the user's context
argument-hint: "<file path, selected code, error message, or concept to explain>"
allowed-tools: Read, Grep, Glob, Bash
---

Explain the following as clearly and concisely as possible.

Target: $ARGUMENTS

Current directory: !`pwd`

## How to explain

1. If a file path is given, read it first with the Read tool before explaining.
2. If it's a code snippet or error, explain it directly.
3. Adapt the depth to what the user likely needs:
   - For a short snippet: explain what it does, why it does it that way, and any non-obvious behavior
   - For a file: summarize the purpose, key abstractions, and how it fits into the surrounding system
   - For an architecture or concept: use an analogy, then the precise definition, then a concrete example
   - For an error: what caused it, what the runtime was trying to do, and how to fix it

4. Lead with the answer — don't build up to it.
5. Use a concrete example if the concept is abstract.
6. Flag anything surprising, dangerous, or easy to misuse.
7. Keep it short. If there's more depth available, offer it — don't dump it unprompted.

## Format

For code:
- What it does (one sentence)
- How it works (key steps, non-obvious patterns)
- Why this approach (design intent, trade-offs if relevant)
- Any gotchas

For errors:
- What went wrong (the actual cause, not a restatement of the message)
- Why it happens
- How to fix it
