---
name: dotnet-pr
description: "Generates a structured GitHub PR description: max 5 bullet summary, risks + rollback plan, paste-ready description, and what reviewers should focus on. Use this command before opening any PR."
---

# dotnet-pr Command

Generates a complete, paste-ready GitHub PR description with:
- **Summary**: Max 5 bullet points of what changed
- **Risks & Rollback**: Risk level, breaking changes, rollback plan
- **Paste-ready description**: Full markdown ready to copy
- **Reviewer focus**: What reviewers should prioritize

## Usage

Type `/dotnet-pr` to invoke this skill in OpenCode.

## How It Works

1. Collect git context:
   - `git diff main...HEAD --stat` — changed files
   - `git diff main...HEAD` — full diff
   - `git log main...HEAD --oneline` — commit history

2. Analyze changes and produce PR description using the format below

## Output Format

```markdown
## Summary (max 5 bullets)
- [Bullet 1: what changed]
- [Bullet 2: what changed]
- ...

## Risks & Rollback
- **Risk Level**: Low / Medium / High
- **Breaking Changes**: [None / list]
- **Rollback Plan**: [Revert sufficient / schema rollback needed / etc]

## What Reviewers Should Focus On
- [Focus area 1]
- [Focus area 2]

## PR Description (paste-ready)

## Summary

[2-4 sentences: what this PR does and why]

## Changes

### [Group 1]
- [Change description]

### [Group 2]  
- [Change description]

## Test Plan

- [ ] [Manual verification step]
- [ ] [Automated test that covers this]

## Related

Closes #[issue]
```

## Requirements

- Use this command BEFORE opening a PR
- Provide context about linked issues if any
- If base branch differs from main, specify it
