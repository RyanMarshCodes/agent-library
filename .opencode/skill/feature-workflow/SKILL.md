---
name: feature-workflow
description: "Runs a spec-based feature development workflow: /context -> /spec -> /validate -> /architect -> /implement -> /test -> /reflect -> /review -> /commit."
---

# feature-workflow Command

Use this command for new feature delivery using spec-based development.

## Usage

Type `/feature-workflow` and provide:
- Feature request/user story
- Optional stack hint

## Workflow Sequence

1. `/context` - initialize project context
2. `/spec` - write structured requirements
3. `/validate` - validate spec quality
4. `/architect` - create ARD/ADR and tasks
5. `/validate` - validate architecture/task quality
6. `/implement` - implement tasks in small diffs
7. `/test` - generate/extend tests and coverage
8. `/reflect` - self-review
9. `/review` - multi-agent review
10. `/commit` - conventional commit + PR description

## Backed by MCP

Use MCP tools for machine-callable flow:
- `list_workflows`
- `get_workflow`
- `start_feature_workflow`
- `run_workflow_step`
