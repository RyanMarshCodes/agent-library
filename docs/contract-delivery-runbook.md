# Contract Delivery Runbook

This runbook defines a repeatable workflow for using Ryan.MCP + OpenCode to deliver client work safely and consistently.

## 1. Project Intake

- Capture business outcome, scope boundaries, constraints, and timeline.
- Confirm stack and deployment target (language, framework, cloud, database).
- Identify acceptance criteria and non-functional requirements (security, performance, observability).

## 2. MCP Preflight

- Verify MCP service health:
  - `GET /health/live`
  - `GET /health/ready`
- Validate OpenCode effective config:
  - `opencode debug config`
  - `opencode mcp list --print-logs --log-level DEBUG`
- Ensure memory backend is available:
  - `memory_status`

## 3. Discovery and Planning

- Use `get_context(language, task)` as first entry point.
- Use `recommend_agent(task)` to select specialist agents.
- Use `search_documents` and `read_document` for relevant standards.
- Record key decisions with `memory_persist` as durable facts.

## 4. Delivery Execution

- Keep changes atomic and scoped to acceptance criteria.
- Prefer fast feedback loops:
  - compile
  - lint
  - targeted tests
- For debugging workflows, use:
  - `analyze_runtime_logs`
  - `summarize_errors`
  - `incident_timeline`

## 5. Quality Gates Before PR

- All acceptance criteria satisfied.
- No new warnings/errors in build/lint.
- Tests for changed behavior included and passing.
- Security and input validation checks completed.
- Observability added/updated for non-trivial backend behavior.

## 6. Handoff Package

- PR description includes:
  - scope
  - rationale
  - risk notes
  - validation evidence
- Include rollout/rollback notes for production-impacting changes.
- Persist project-specific learnings via `memory_persist`.

## 7. Post-Delivery Review

- Capture what caused delay/rework.
- Add process fixes to standards/docs.
- Store recurring client/domain patterns in memory for future reuse.

## 8. Azure DevOps Alignment (Optional but Recommended)

If Azure DevOps is your primary work-tracking system, apply:

- [Azure DevOps Workflow](azure-devops-workflow.md) as the default hierarchy/process.
- Work item traceability from story to task/test case to PR.
- Acceptance evidence attached to User Stories before closure.
