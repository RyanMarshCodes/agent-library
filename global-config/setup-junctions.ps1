# Setup-Junctions.ps1
# Run this script as a NORMAL USER (no admin needed) to create directory
# junctions from global tool config locations back to this repository.
#
# Safe to re-run — removes existing junctions before recreating them.
#
# Usage:
#   cd D:\Projects\agent-configurations\global-config
#   .\setup-junctions.ps1

$RepoRoot = "D:\Projects\agent-configurations\global-config"

function New-Junction {
    param($Link, $Target)
    if (Test-Path $Link) {
        $item = Get-Item -Force $Link
        if ($item.Attributes -band 1024) {
            cmd /c "rmdir `"$Link`"" | Out-Null  # remove existing junction
        } else {
            Remove-Item -Recurse -Force $Link    # remove real directory
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

Write-Host "`nCreating directory junctions...`n" -ForegroundColor Cyan

# Cursor global rules directory
New-Junction `
    -Link "$env:USERPROFILE\.cursor\rules" `
    -Target "$RepoRoot\cursor\rules"

# OpenCode global instructions directory
New-Junction `
    -Link "$env:USERPROFILE\.config\opencode\instructions" `
    -Target "$RepoRoot\opencode\instructions"

# GitHub Copilot global instructions dir (uncomment when Copilot is installed)
# Ensure the target dir exists first:
#   New-Item -ItemType Directory -Force "$RepoRoot\copilot" | Out-Null
# New-Junction `
#     -Link "$env:USERPROFILE\.github" `
#     -Target "$RepoRoot\copilot\github"

Write-Host "`nDone.`n" -ForegroundColor Cyan
