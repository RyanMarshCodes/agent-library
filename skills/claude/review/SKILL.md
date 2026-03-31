---
name: review
description: Senior-level code review covering correctness, quality, security, performance, and style for any file or diff
argument-hint: "[file path, or leave blank to review staged/recent changes]"
allowed-tools: Read, Grep, Glob, Bash, Write
---

Perform a thorough senior-level code review following these steps:

1. Use the Read tool to read `D:/Projects/agent-configurations/agents/code-analysis.agent.md` — apply the quality assessment lens from that agent
2. Use the Read tool to read `D:/Projects/agent-configurations/agents/security-check.agent.md` — apply the security checks from that agent
3. Use the Read tool to read `D:/Projects/agent-configurations/agents/code-simplifier.agent.md` — apply the simplification lens from that agent

Then review the target with all three lenses combined.

Target: $ARGUMENTS

Current directory: !`pwd`
Staged changes (if any): !`git diff --cached --stat 2>/dev/null || echo "No staged changes"`
Recent diff: !`git diff HEAD~1 --stat 2>/dev/null || echo "No recent commits"`

## Review Output Format

Produce your review in this structure:

### Summary
Overall assessment in 2-3 sentences.

### Correctness
Issues that would cause incorrect behavior.

### Security
Issues from OWASP Top 10 and general security best practices.

### Performance
Issues that could cause performance problems at scale.

### Code Quality
Complexity, naming, duplication, maintainability concerns.

### Style & Conventions
Deviations from the detected language/framework conventions.

### Positives
What is done well — always include this.

### Recommended Changes
Prioritized list. Mark each: MUST / SHOULD / NICE-TO-HAVE.
