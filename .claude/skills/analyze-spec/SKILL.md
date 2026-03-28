---
name: analyze-spec
description: Parse and analyze an OpenAPI/Swagger/AsyncAPI/GraphQL SDL spec — extracts endpoints, models, auth patterns, and produces client architecture recommendations
argument-hint: "<path-to-spec-file> [target language/framework — optional, e.g. typescript, csharp]"
allowed-tools: Read, Grep, Glob, Bash, Agent, Write
---

Use the Read tool to read `D:/Projects/agent-configurations/ai-docs/agents/openapi-client-builder.agent.md` then follow the workflow defined in that file exactly.

Spec: $ARGUMENTS

Steps to take before applying the agent workflow:

1. If the MCP server is running (`ryan-mcp` connected), call `parse_openapi` with the spec path to get a structured summary. Use that summary as input to the agent workflow.
2. If the MCP server is not available, use the Read tool to load the spec file directly, then proceed.

Current directory: !`pwd`
Spec file contents (if local path given): !`cat "$1" 2>/dev/null | head -200 || echo "Spec not found at that path — please provide the full path"`
