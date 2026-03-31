# Azure DevOps Workflow (Contract Delivery)

This workflow is optimized for using Azure DevOps as the system of record while delivering with Ryan.MCP-enabled agents.

## 1. Work Item Hierarchy

Use this hierarchy consistently:

- **Epic**: business outcome / client objective
- **Feature**: capability slice under an epic
- **User Story**: implementable user-facing behavior
- **Task**: technical implementation step (optional but recommended)
- **Bug**: defect against accepted behavior
- **Test Case**: validation of user story acceptance criteria

## 2. Definition of Ready (Before Coding)

For each User Story, require:

- clear problem statement
- acceptance criteria (Given/When/Then preferred)
- non-functional constraints (security/perf/observability)
- dependency notes (APIs, environments, approvals)
- test case linkage

## 3. Recommended Board States

Use explicit lifecycle states to reduce ambiguity:

- `New` -> `Refined` -> `Ready` -> `In Progress` -> `PR Review` -> `QA` -> `Done`

Suggested policy:

- Do not move to `Ready` unless acceptance criteria and test cases exist.
- Do not move to `Done` until acceptance evidence is attached.

## 4. Sprint Planning Pattern

- Plan by **User Story**, execute by **Task**.
- Keep stories thin (1-3 days each when possible).
- Cap concurrent in-progress stories to reduce context switching.
- Track spillover causes (scope churn, unclear AC, dependency bottlenecks).

## 5. Traceability Rules

Every PR should reference a single primary User Story and optionally additional linked Tasks/Bugs.

Minimum trace links:

- Story -> Task(s)
- Story -> Test Case(s)
- PR/Commit -> Story
- Bug -> Story/Feature (if regression)

## 6. Test Case Strategy in ADO

For each story:

- add at least one happy-path test case
- add negative/error-path test case(s)
- add regression case when fixing bugs
- map automated test names to ADO test case IDs where practical

## 7. Ryan.MCP + Agent Usage Pattern

Per story execution:

1. `get_context(language, task)` for standards + agent guidance.
2. `recommend_agent(task)` for specialist selection.
3. Implement with focused scope.
4. Validate with tests and readiness checks.
5. Persist durable lessons via `memory_persist` (decision/risk/pattern).

## 8. PR Quality Gate (Contract Standard)

Before merge:

- acceptance criteria met and evidenced
- tests pass (new + regression)
- security checks completed for changed boundaries
- observability updated for non-trivial backend changes
- rollback note included for production-impacting change

## 9. Client Reporting Cadence

At end of sprint/week, provide:

- completed stories with acceptance evidence
- blocked stories with mitigation/ETA
- defect summary (opened/resolved, severity)
- next sprint forecast by feature

## 10. Suggested ADO Queries

Create saved queries:

- `Stories Ready for Dev`
- `Stories in PR Review > 2 days`
- `Bugs by Severity (Current Sprint)`
- `Stories Done Missing Test Case Links`
- `Blocked Work Items`
