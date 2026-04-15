---
name: reflect-workflow
description: Post-implementation self-review before formal multi-agent review
argument-hint: "Diff, branch, or scoped files"
allowed-tools: Read, Grep, Glob, Bash, Write
---

Run a self-review for:
$ARGUMENTS

Sections:
- Issues Found (Critical, Major, Minor)
- Clean Areas
- Questions for the user
- Recommended Next Action

Evaluate:
- Correctness
- Convention compliance
- Architecture fit
- Testing completeness
- Completeness and cleanup
