---
name: architect-workflow
description: Create architecture decisions and task breakdown from a validated spec
argument-hint: "Validated spec or feature description"
allowed-tools: Read, Grep, Glob, Bash, Write, Edit
---

Design architecture and implementation tasks for:
$ARGUMENTS

Checklist:
- Decisions and rationale
- Impacted components and interfaces
- Sequenced tasks
- Task dependencies form a DAG
- Tests included in task acceptance criteria

After output, recommend /validate.
