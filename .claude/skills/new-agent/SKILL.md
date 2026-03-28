---
name: new-agent
description: Design and write a new agent instruction file using the AIAgentExpert — produces a complete, portable agent definition
argument-hint: "[describe the agent's purpose, inputs, and desired behavior]"
allowed-tools: Read, Grep, Glob, Bash, Write, Edit
---

Use the Read tool to read `D:/Projects/agent-configurations/ai-docs/agents/ai-agent-expert.agent.md` then follow the workflow defined in that file exactly.

New agent brief: $ARGUMENTS

Existing agents in the library: !`ls D:/Projects/agent-configurations/ai-docs/agents/`
Current catalog: !`cat D:/Projects/agent-configurations/ai-docs/catalog.json`

After writing the agent file to `D:/Projects/agent-configurations/ai-docs/agents/`, update the catalog at `D:/Projects/agent-configurations/ai-docs/catalog.json` with the new entry.

File naming: kebab-case + `.agent.md` (e.g., `my-purpose.agent.md`). Use a clear H1 inside the file for the agent’s display name; set the catalog `name` field to a stable PascalCase id (e.g., `MyPurposeAgent`) for tooling.
