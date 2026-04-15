# sync-and-deploy.ps1
# Generates tool-specific config files from shared content blocks,
# then deploys them to global config paths via symlinks or junctions.
#
# Run after any change to _shared/ content or tool-specific templates.
#
# Usage:
#   cd global-config
#   .\sync-and-deploy.ps1           # generate + deploy
#   .\sync-and-deploy.ps1 -GenerateOnly  # generate without deploying
#   .\sync-and-deploy.ps1 -DeployOnly    # deploy without regenerating

param(
    [switch]$GenerateOnly,
    [switch]$DeployOnly,
    [switch]$Force
)

$ErrorActionPreference = "Stop"

# Auto-detect repo root from script location
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$RepoRoot = Split-Path -Parent $ScriptDir
$SharedDir = Join-Path $ScriptDir "_shared"

# --- Helpers ---

function Read-SharedBlock {
    param([string]$FileName)
    $path = Join-Path $SharedDir $FileName
    if (-not (Test-Path $path)) {
        Write-Host "  [WARN] Shared block not found: $FileName" -ForegroundColor Yellow
        return ""
    }
    # Use .NET to read UTF-8 correctly (PS 5.x Get-Content can mangle em-dashes etc.)
    return ([System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)).TrimEnd()
}

function Write-GeneratedFile {
    param([string]$RelativePath, [string]$Content)
    $fullPath = Join-Path $ScriptDir $RelativePath
    $dir = Split-Path -Parent $fullPath
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Force -Path $dir | Out-Null
    }
    # Normalize to CRLF, then write UTF-8 without BOM (works on PS 5.1 and 7+)
    $Content = $Content -replace "`r`n", "`n" -replace "`r", "`n" -replace "`n", "`r`n"
    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($fullPath, $Content, $utf8NoBom)
    Write-Host "  [GEN] $RelativePath" -ForegroundColor Green
}

function New-FileSymlink {
    param([string]$Link, [string]$Target)
    if (-not (Test-Path $Target)) {
        Write-Host "  [SKIP] Target missing: $Target" -ForegroundColor Yellow
        return
    }
    if (Test-Path $Link) {
        $item = Get-Item -Force $Link
        if ($item.LinkType -eq "SymbolicLink" -or $item.LinkType -eq "HardLink") {
            Remove-Item -Force $Link
        } elseif (-not $Force) {
            Write-Host "  [SKIP] Real file exists (use -Force to overwrite): $Link" -ForegroundColor Yellow
            return
        } else {
            Remove-Item -Force $Link
        }
    }
    $dir = Split-Path -Parent $Link
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Force -Path $dir | Out-Null
    }
    try {
        New-Item -ItemType SymbolicLink -Path $Link -Target $Target | Out-Null
        Write-Host "  [OK] $Link -> $Target" -ForegroundColor Green
    } catch {
        Write-Host "  [ERROR] $Link : $_" -ForegroundColor Red
        Write-Host "         Try: Enable Developer Mode (Settings > System > For Developers)" -ForegroundColor DarkGray
    }
}

function New-Junction {
    param([string]$Link, [string]$Target)
    if (-not (Test-Path $Target)) {
        Write-Host "  [SKIP] Target missing: $Target" -ForegroundColor Yellow
        return
    }
    if (Test-Path $Link) {
        $item = Get-Item -Force $Link
        if ($item.Attributes -band 1024) {
            cmd /c "rmdir `"$Link`"" | Out-Null
        } elseif (-not $Force) {
            Write-Host "  [SKIP] Real dir exists (use -Force to overwrite): $Link" -ForegroundColor Yellow
            return
        } else {
            Remove-Item -Recurse -Force $Link
        }
    }
    cmd /c "mklink /J `"$Link`" `"$Target`"" | Out-Null
    $item = Get-Item -Force $Link -ErrorAction SilentlyContinue
    if ($item -and ($item.Attributes -band 1024)) {
        Write-Host "  [OK] $Link -> $Target" -ForegroundColor Green
    } else {
        Write-Host "  [FAIL] $Link" -ForegroundColor Red
    }
}

# --- Content Generation ---
# To add a new tool:
#   1. Add a Write-GeneratedFile block in Invoke-Generate (compose from $memory, $context, etc.)
#   2. Add a New-FileSymlink or New-Junction call in Invoke-Deploy targeting the tool's global config path
#   3. Run: .\sync-and-deploy.ps1 -GenerateOnly   to test, then without flag to deploy

function Get-McpServerConfigs {
    param([string]$Format)

    $serversContent = Read-SharedBlock "mcp-servers.toml"
    $servers = @{}
    $currentSection = $null

    foreach ($line in ($serversContent -split "`n")) {
        $trimmed = $line.Trim()
        if ($trimmed -match '^\[([\w-]+)\]$') {
            $currentSection = $matches[1].Trim()
            $servers[$currentSection] = @{}
        } elseif ($currentSection -and $trimmed -match '^(\w+)\s*=\s*"([^"]+)"$') {
            $servers[$currentSection][$matches[1]] = $matches[2]
        } elseif ($currentSection -and $trimmed -match '^args\s*=\s*\[([^\]]+)\]$') {
            $argsStr = $matches[1]
            $args = @()
            $regexMatches = [regex]::Matches($argsStr, '"([^"]+)"')
            foreach ($m in $regexMatches) {
                $args += $m.Groups[1].Value
            }
            $servers[$currentSection]['args'] = $args
        }
    }

    switch ($Format) {
        'cursor' {
            $json = @{ mcpServers = @{} }
            foreach ($name in $servers.Keys) {
                $s = $servers[$name]
                if ($s.transport -eq 'http') {
                    $json.mcpServers[$name] = @{ url = $s.url }
                } else {
                    $cfg = @{ command = $s.command }
                    if ($s.args) { $cfg.args = $s.args }
                    $json.mcpServers[$name] = $cfg
                }
            }
            return ($json | ConvertTo-Json -Depth 5)
        }
        'opencode' {
            $json = @{}
            foreach ($name in $servers.Keys) {
                $s = $servers[$name]
                $entry = @{
                    type    = if ($s.transport -eq 'http') { 'remote' } else { 'command' }
                    enabled = $true
                }
                if ($s.url) { $entry.url = $s.url }
                if ($s.command) { $entry.command = $s.command }
                if ($s.args) { $entry.args = $s.args }
                if ($s.transport -eq 'http') {
                    $entry.timeout = 120000
                    $entry.oauth = $false
                }
                $json[$name] = $entry
            }
            return ($json | ConvertTo-Json -Depth 5)
        }
        'gemini' {
            $json = @{ mcpServers = @{} }
            foreach ($name in $servers.Keys) {
                $s = $servers[$name]
                if ($s.transport -eq 'http') {
                    $json.mcpServers[$name] = @{ httpUrl = $s.url }
                } else {
                    $cfg = @{ command = $s.command }
                    if ($s.args) { $cfg.args = $s.args }
                    $json.mcpServers[$name] = $cfg
                }
            }
            return ($json | ConvertTo-Json -Depth 5)
        }
        'copilot' {
            $json = @{ mcpServers = @{} }
            foreach ($name in $servers.Keys) {
                $s = $servers[$name]
                if ($s.transport -eq 'http') {
                    $json.mcpServers[$name] = @{ type = 'http'; url = $s.url }
                } else {
                    $cfg = @{
                        type    = 'local'
                        command = $s.command
                        tools   = @('*')
                    }
                    if ($s.args) { $cfg.args = $s.args }
                    $json.mcpServers[$name] = $cfg
                }
            }
            return ($json | ConvertTo-Json -Depth 5)
        }
        'copilot-vscode' {
            $serversObj = @{}
            foreach ($name in $servers.Keys) {
                $s = $servers[$name]
                $cfg = @{
                    command = if ($s.transport -eq 'http') { 'curl' } else { $s.command }
                }
                if ($s.transport -eq 'http') {
                    $cfg.args = @($s.url)
                } elseif ($s.args) {
                    $cfg.args = $s.args
                }
                $serversObj[$name] = $cfg
            }
            return ($serversObj | ConvertTo-Json -Depth 5)
        }
    }
}

function Invoke-Generate {
    Write-Host "`nGenerating tool configs from shared blocks...`n" -ForegroundColor Cyan

    # Read shared blocks — add new _shared/*.md files here as needed
    $memory = Read-SharedBlock "memory-bridge-instructions.md"
    $context = Read-SharedBlock "context-management.md"
    $quota = Read-SharedBlock "opencode-quota-management.md"

    # === Claude Code (global ~/.claude/CLAUDE.md) ===
    Write-GeneratedFile "claude\CLAUDE.md" @"
# Global Instructions (Claude Code)

## Slash Commands

| Command | Description |
|---------|-------------|
| ``/dotnet-pr`` | Generate PR description: 5-bullet summary, risks + rollback, paste-ready description, reviewer focus |

$memory

---

$context
"@

    # === Gemini CLI (global ~/.gemini/GEMINI.md) ===
    Write-GeneratedFile "gemini\GEMINI.md" @"
# Global Instructions (Gemini CLI / Antigravity)

$memory

---

$context
"@

    # === Gemini AGENTS.md (pointer only — avoids double-loading) ===
    Write-GeneratedFile "gemini\AGENTS.md" @"
# Global AGENTS.md (Gemini CLI)

<!-- Gemini CLI reads both GEMINI.md and AGENTS.md. To avoid loading identical content twice,
     all shared instructions are in GEMINI.md. This file is intentionally minimal. -->

See GEMINI.md for global instructions.
"@

    # === Copilot (global ~/.github/copilot-instructions.md) ===
    Write-GeneratedFile "copilot\github\copilot-instructions.md" @"
# Global Copilot Instructions

Follow the standards and conventions defined in the project AGENTS.md if present.
If no project-specific AGENTS.md exists, apply the principles below.

## Code Quality

- >90% test coverage. Zero tolerance for warnings and code smells.
- Follow SOLID, CLEAN, DRY, KISS, SRP.
- No hardcoded secrets. Validate all inputs at system boundaries only.

## Style

- Match the language/framework conventions already present in the project.
- Prefer explicit over implicit. Prefer simple over clever.
- Write code that reads like a clear explanation of intent.

$memory

---

$context
"@

    # === Cursor (global ~/.cursor/rules/ryan-mcp-memory.mdc) ===
    Write-GeneratedFile "cursor\rules\ryan-mcp-memory.mdc" @"
description: Ryan.MCP knowledge-graph memory and context management
alwaysApply: true
---

$memory

---

$context
"@

    # === OpenCode (global ~/.config/opencode/instructions/ryan-mcp-memory.md) ===
    Write-GeneratedFile "opencode\instructions\ryan-mcp-memory.md" @"
# Knowledge-Graph Memory via Ryan.MCP

$memory

---

$context

---

$quota

## OpenCode Note

When using ``call_external_mcp_tool`` in OpenCode, ensure ``argumentsJson`` is a JSON object string (for example ``"{\"query\":\"test-entity-final\"}"``), not null.

When making function calls using tools that accept array or object parameters, ensure those are structured using JSON. For example, when calling ``memory_persist``:

``````json
{
  "entityName": "my-decision",
  "entityType": "decision",
  "observations": ["Chose approach X because of Y"]
}
``````
"@

    # === OpenCode config JSON (global ~/.config/opencode/opencode.json) ===
    Write-GeneratedFile "opencode\opencode.json" @"
{
  "`$schema": "https://opencode.ai/config.json",
  "instructions": [
    "~/.config/opencode/instructions/ryan-mcp-memory.md"
  ],
  "command": {
    "quota": {
      "description": "Check OpenCode usage quota (opencode_usage CLI)",
      "template": "Summarize my OpenCode / Zen usage from this output:\n\n!``python -m opencode_usage run``"
    }
  },
  "mcp": $(Get-McpServerConfigs -Format 'opencode')
}
"@

    # === Cursor MCP (global ~/.cursor/mcp.json) ===
    Write-GeneratedFile "cursor\mcp.json" (Get-McpServerConfigs -Format 'cursor')

    # === Gemini CLI MCP (global ~/.gemini/settings.json) ===
    Write-GeneratedFile "gemini\settings.json" (Get-McpServerConfigs -Format 'gemini')

    # === Copilot CLI MCP (global ~/.copilot/mcp-config.json) ===
    Write-GeneratedFile "copilot\mcp-config.json" (Get-McpServerConfigs -Format 'copilot')

    # === Copilot VS Code Extension MCP (global ~/.copilot/.vscode/mcp.json) ===
    Write-GeneratedFile "copilot\.vscode\mcp.json" @"
{
  "inputs": [],
  "servers": $(Get-McpServerConfigs -Format 'copilot-vscode')
}
"@

    Write-Host "`nGeneration complete.`n" -ForegroundColor Cyan
}

# --- Deployment ---

function Invoke-Deploy {
    Write-Host "`nDeploying configs to global paths...`n" -ForegroundColor Cyan

    # File symlinks (require Developer Mode or elevation)
    Write-Host "  File symlinks:" -ForegroundColor White

    # Claude Code
    New-FileSymlink `
        -Link "$env:USERPROFILE\.claude\CLAUDE.md" `
        -Target (Join-Path $ScriptDir "claude\CLAUDE.md")
    New-FileSymlink `
        -Link "$env:USERPROFILE\.claude\settings.json" `
        -Target (Join-Path $ScriptDir "claude\settings.json")
    New-FileSymlink `
        -Link "$env:USERPROFILE\.claude\mcp.json" `
        -Target (Join-Path $ScriptDir "claude\mcp.json")

    # Gemini CLI
    New-FileSymlink `
        -Link "$env:USERPROFILE\.gemini\GEMINI.md" `
        -Target (Join-Path $ScriptDir "gemini\GEMINI.md")
    New-FileSymlink `
        -Link "$env:USERPROFILE\.gemini\AGENTS.md" `
        -Target (Join-Path $ScriptDir "gemini\AGENTS.md")

    # OpenCode global config
    New-FileSymlink `
        -Link "$env:USERPROFILE\.config\opencode\opencode.json" `
        -Target (Join-Path $ScriptDir "opencode\opencode.json")

    # Copilot global instructions (single file symlink — don't replace the whole ~/.github dir)
    New-FileSymlink `
        -Link "$env:USERPROFILE\.github\copilot-instructions.md" `
        -Target (Join-Path $ScriptDir "copilot\github\copilot-instructions.md")

    # Copilot CLI MCP servers config
    New-FileSymlink `
        -Link "$env:USERPROFILE\.copilot\mcp-config.json" `
        -Target (Join-Path $ScriptDir "copilot\mcp-config.json")

    # Copilot VS Code Extension MCP servers config
    New-FileSymlink `
        -Link "$env:USERPROFILE\.copilot\.vscode\mcp.json" `
        -Target (Join-Path $ScriptDir "copilot\.vscode\mcp.json")

    # VS Code user-level MCP servers (global across all workspaces)
    New-FileSymlink `
        -Link "$env:USERPROFILE\.vscode\mcp.json" `
        -Target (Join-Path $ScriptDir "vscode\mcp.json")

    # VS Code / Copilot custom agents (user level)
    New-Junction `
        -Link "$env:USERPROFILE\.copilot\agents" `
        -Target (Join-Path $ScriptDir "vscode\agents")

    # Gemini CLI MCP servers config
    New-FileSymlink `
        -Link "$env:USERPROFILE\.gemini\settings.json" `
        -Target (Join-Path $ScriptDir "gemini\settings.json")

    # Directory junctions (no elevation needed)
    Write-Host "`n  Directory junctions:" -ForegroundColor White

    # Cursor global rules + settings + keybindings
    # (changes written to ~/.cursor/* go to global-config/cursor/)
    New-Junction `
        -Link "$env:USERPROFILE\.cursor\rules" `
        -Target (Join-Path $ScriptDir "cursor\rules")
    New-FileSymlink `
        -Link "$env:APPDATA\Cursor\User\settings.json" `
        -Target (Join-Path $ScriptDir "cursor\settings.json")
    New-FileSymlink `
        -Link "$env:APPDATA\Cursor\User\keybindings.json" `
        -Target (Join-Path $ScriptDir "cursor\keybindings.json")

    # Cursor MCP servers config
    New-FileSymlink `
        -Link "$env:USERPROFILE\.cursor\mcp.json" `
        -Target (Join-Path $ScriptDir "cursor\mcp.json")

    # Copilot CLI config
    New-FileSymlink `
        -Link "$env:USERPROFILE\.copilot\config.json" `
        -Target (Join-Path $ScriptDir "copilot\config.json")

    # OpenCode global instructions
    New-Junction `
        -Link "$env:USERPROFILE\.config\opencode\instructions" `
        -Target (Join-Path $ScriptDir "opencode\instructions")

    Write-Host "`nDeployment complete.`n" -ForegroundColor Cyan
}

# --- Main ---

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Agent Config Sync & Deploy" -ForegroundColor Cyan
Write-Host "  Repo: $RepoRoot" -ForegroundColor DarkGray
Write-Host "========================================" -ForegroundColor Cyan

if ($DeployOnly) {
    Invoke-Deploy
} elseif ($GenerateOnly) {
    Invoke-Generate
} else {
    Invoke-Generate
    Invoke-Deploy
}

Write-Host "Done." -ForegroundColor Cyan
