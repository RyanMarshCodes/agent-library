---
description: "Expose and orchestrate the canonical AI/agent workflows as callable skills and slash commands. Enables /spec, /validate, /architect, /implement, /test, /reflect, /review, /commit, /wrapup, /proceed, /status, /bugfix, and /context/init workflows for all agents and tools via MCP server or direct invocation."
name: "Workflow Orchestrator"
model: gpt-4.1 # or your preferred orchestrator model
scope: "global"
tags: ["workflow", "orchestration", "slash-commands", "mcp", "cross-agent", "universal"]
---

# Workflow Orchestrator Skill

This skill exposes the canonical AI/agent workflows as callable slash commands and orchestrated flows, available to all agents and tools (Copilot, Claude, Cursor, OpenCode, Antigravity, etc.) via the MCP server or direct invocation.

## Supported Workflows
- /spec — Write requirements from feature request
- /validate — Validate spec or ARD/task quality
- /architect — Design ARDs and break into tasks
- /implement — Generate implementation tasks/code
- /test — Code coverage and unit test generation
- /reflect — Self-review implementation
- /review — Multi-agent code review
- /commit — Commit and PR description generation
- /wrapup — Finalize artifacts and capture lessons
- /proceed — End-to-end gated pipeline
- /status — Workflow phase/status report
- /bugfix — Streamlined defect workflow
- /context or /init — Project context and memory

## Usage
- Invoke any workflow by slash command (e.g., /spec, /review) or by natural prompt (e.g., "Let's build a new feature: ...").
- The orchestrator will route the request to the appropriate agent/skill and return the result.
- For discoverability, use /workflows or /commands to list all available workflows.

## Implementation Notes
- This skill should be registered globally in your MCP server and referenced in AGENTS.md, CLAUDE.md, and any .cursorrules or agent.md files as the canonical workflow entrypoint.
- For tools that support dynamic skills, auto-register these workflows as slash commands.
- For static tools, ensure the workflow doc is accessible and referenced in agent help.

## References
- Canonical workflow doc: mcp-server/knowledgebase/WORKFLOW_COMMANDS.md
- Global config: AGENTS.md, CLAUDE.md
