---
name: init-mcp-server
description: Scaffold a new .NET Aspire MCP server based on the Ryan.MCP reference implementation — creates AppHost + Mcp projects with agent ingestion, document management, and external connector proxying
argument-hint: "<ProjectName> [output directory — optional, defaults to current directory]"
allowed-tools: Read, Grep, Glob, Bash, Write, Edit
---

Use the Read tool to read `D:/Projects/agent-configurations/ai-docs/agents/csharp-mcp-expert.agent.md` then follow the expertise defined in that file.

Project name and output path: $ARGUMENTS

## Reference implementation

The canonical implementation to base this on is at `D:/Projects/agent-configurations/mcp-server/`.

Read these files from the reference implementation before scaffolding:

1. `D:/Projects/agent-configurations/mcp-server/src/Ryan.MCP.AppHost/AppHost.cs` — Aspire host setup, Docker resource registration
2. `D:/Projects/agent-configurations/mcp-server/src/Ryan.MCP.Mcp/Program.cs` — service registration, MCP setup, REST API routes
3. `D:/Projects/agent-configurations/mcp-server/src/Ryan.MCP.Mcp/Configuration/McpOptions.cs` — configuration model
4. `D:/Projects/agent-configurations/mcp-server/src/Ryan.MCP.Mcp/Ryan.MCP.Mcp.csproj` — NuGet dependencies

## Scaffold steps

1. Create the solution structure:
   ```
   <ProjectName>/
     <ProjectName>.slnx
     Directory.Build.props
     src/
       <ProjectName>.AppHost/
         <ProjectName>.AppHost.csproj
         AppHost.cs
         appsettings.json
         Properties/launchSettings.json
       <ProjectName>.Mcp/
         <ProjectName>.Mcp.csproj
         Program.cs
         appsettings.json
         Configuration/McpOptions.cs
         McpTools/        (AgentTools.cs, DocumentTools.cs, ContextTools.cs)
         McpPrompts/      (AgentPrompts.cs, DocumentPrompts.cs)
         McpResources/    (AgentResources.cs, DocumentResources.cs)
         Services/        (AgentIngestionCoordinator.cs, DocumentIngestionCoordinator.cs, ...)
         wwwroot/index.html
   ```

2. Substitute `Ryan.MCP` → `<ProjectName>` throughout all namespaces, assembly names, and project references.

3. In `appsettings.json`, set:
   - `McpOptions.Knowledge.ProjectSlug` → lowercased kebab-case project name
   - `McpOptions.Agents.LibraryPath` → relative path to the agent library (`../../../ai-docs/agents` for repos using this config repo structure, or ask the user)
   - All external connectors `Enabled: false` by default

4. Update `launchSettings.json` with the correct project name in profile names.

5. Create a `README.md` in the new project root with:
   - How to start the server
   - How to add to Claude Code: `claude mcp add --transport http --scope global <name> http://localhost:<port>/mcp`
   - How to add to other IDEs

## After scaffolding

Tell the user:
- The ports assigned (AppHost dashboard, Mcp service)
- The `claude mcp add` command with the correct port
- Which `appsettings.json` paths to configure for their agent library and standards directories
