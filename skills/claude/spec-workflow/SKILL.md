---
name: spec-workflow
description: Write requirement specifications from feature requests with testable acceptance criteria
argument-hint: "Feature request"
allowed-tools: Read, Grep, Glob, Bash, Write, Edit
---

Generate a requirement spec for:
$ARGUMENTS

Checklist:
- Problem and why
- Acceptance criteria (testable)
- Dependencies
- Assumptions
- Open questions
- Out-of-scope

Do not include implementation details.
After output, recommend /validate.
