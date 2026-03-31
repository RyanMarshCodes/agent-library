# Log Forensics Runbook (Ryan.MCP)

Use this runbook when you only have exported logs and need root cause analysis quickly.

## Inputs

- Log file path
- Approximate UTC incident window
- Environment/service context

## Workflow

1. Run `analyze_runtime_logs(path)` for a high-level error signature overview.
2. Run `summarize_errors(path, groupBy)` to identify dominant error classes.
3. Run `incident_timeline(path, start, end)` to reconstruct causal ordering.
4. Correlate with code and configuration changes (recent commits/deploys).
5. Persist only durable findings to memory (`memory_persist`).

## Durable Memory Policy

Persist:

- confirmed root cause
- remediation decision and rationale
- prevention checklist updates

Do not persist:

- raw stack traces
- one-off noisy lines
- secrets/tokens/credentials

## Example Prompts for Agents

- "Analyze this log file and produce top 3 candidate root causes with evidence."
- "Build incident timeline from 2026-03-31T05:00:00Z to 2026-03-31T06:00:00Z and highlight first trigger event."
- "Summarize repeated failures and map likely failure points to `mcp-server/src`."
