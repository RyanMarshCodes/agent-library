# Setup-Symlinks.ps1
# Creates file symlinks from global tool config locations back to the
# canonical files in this repository.
#
# Run directly — the script will trigger a UAC prompt to elevate itself:
#   .\setup-symlinks.ps1
#
# Or enable Developer Mode in Windows Settings > System > For Developers
# to allow symlinks without admin privileges.

# Auto-elevate via UAC if not already running as Administrator
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "Requesting elevation..." -ForegroundColor Yellow
    Start-Process powershell.exe -Verb RunAs -ArgumentList "-NoExit -File `"$PSCommandPath`""
    exit
}

$RepoRoot = "D:\Projects\agent-configurations\global-config"

function New-FileSymlink {
    param($Link, $Target)
    if (Test-Path $Link) { Remove-Item -Force $Link }
    try {
        New-Item -ItemType SymbolicLink -Path $Link -Target $Target | Out-Null
        $item = Get-Item -Force $Link
        if ($item.LinkType -eq "SymbolicLink") {
            Write-Host "  [OK] $Link" -ForegroundColor Green
            Write-Host "       -> $Target" -ForegroundColor DarkGray
        } else {
            Write-Host "  [FAIL] $Link" -ForegroundColor Red
        }
    } catch {
        Write-Host "  [ERROR] $Link : $_" -ForegroundColor Red
    }
}

Write-Host "`nCreating file symlinks...`n" -ForegroundColor Cyan

# Claude Code global instructions
New-FileSymlink `
    -Link "$env:USERPROFILE\.claude\CLAUDE.md" `
    -Target "$RepoRoot\claude\CLAUDE.md"

# Gemini CLI global instructions
New-FileSymlink `
    -Link "$env:USERPROFILE\.gemini\GEMINI.md" `
    -Target "$RepoRoot\gemini\GEMINI.md"

# Gemini CLI / Antigravity global AGENTS.md
New-FileSymlink `
    -Link "$env:USERPROFILE\.gemini\AGENTS.md" `
    -Target "$RepoRoot\gemini\AGENTS.md"

# OpenCode global config
New-FileSymlink `
    -Link "$env:USERPROFILE\.config\opencode\opencode.json" `
    -Target "$RepoRoot\opencode\opencode.json"

# GitHub Copilot global instructions
# Uncomment after installing GitHub Copilot. Ensure ~/.github/ exists first.
# New-Item -ItemType Directory -Force "$env:USERPROFILE\.github" | Out-Null
# New-FileSymlink `
#     -Link "$env:USERPROFILE\.github\copilot-instructions.md" `
#     -Target "$RepoRoot\copilot\github\copilot-instructions.md"

Write-Host "`nDone.`n" -ForegroundColor Cyan
Write-Host "Press any key to close..." -ForegroundColor DarkGray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
