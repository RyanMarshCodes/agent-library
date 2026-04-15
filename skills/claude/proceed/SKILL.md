---
name: proceed
description: End-to-end ADLC-compatible pipeline orchestrator for a feature from context to wrapup
argument-hint: "Feature description or requirement id"
allowed-tools: Read, Grep, Glob, Bash, Write, Edit
---

Use this skill to run the full gated pipeline in one pass.

Target: $ARGUMENTS

Gate sequence:
- /context
- /spec
- /validate
- /architect
- /validate
- /implement
- /test
- /reflect
- /review
- /commit
- /wrapup

If any gate has blocker findings:
- Fix blockers.
- Re-run the gate.
- Stop and report unresolved blockers after 3 loops.
