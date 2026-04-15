---
description: "Global workflow orchestrator agent for surfacing and routing canonical AI/agent workflows as slash commands or recipes."
name: "Workflow Orchestrator Agent"
model: gpt-4.1 # or your preferred orchestrator model
scope: "global"
tags: ["workflow", "orchestration", "slash-commands", "mcp", "cross-agent", "universal"]
---

# Workflow Orchestrator Agent

This agent exposes and routes the canonical AI/agent workflows as slash commands and orchestrated flows, available to all agents and tools (Copilot, Claude, Cursor, OpenCode, Antigravity, etc.) via the MCP server or direct invocation.

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
- The agent will route the request to the appropriate skill or workflow and return the result.
- For discoverability, use /workflows or /commands to list all available workflows.

## References
- Canonical workflow doc: mcp-server/knowledgebase/WORKFLOW_COMMANDS.md
- Global config: AGENTS.md, CLAUDE.md
- Skill: skills/workflow/SKILL.md
