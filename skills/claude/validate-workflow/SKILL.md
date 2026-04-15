---
name: validate-workflow
description: Validate phase artifacts (spec, architecture, tasks, implementation) before advancing
argument-hint: "Target artifact or phase"
allowed-tools: Read, Grep, Glob, Bash, Write
---

Validate:
$ARGUMENTS

Produce:
- Pass/fail checklist
- Severity classes (Blocker, Warning, Info)
- Exact fixes for failures
- Recommended next command

Validation targets:
- spec
- architecture
- tasks
- implementation
