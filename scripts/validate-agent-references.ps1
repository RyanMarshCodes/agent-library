param(
    [string]$RepoRoot = "."
)

$ErrorActionPreference = "Stop"

$root = (Resolve-Path $RepoRoot).Path
$catalogPath = Join-Path $root "catalog.json"
$agentsDir = Join-Path $root "agents"
$sharedMemoryPath = Join-Path $root "global-config\_shared\memory-bridge-instructions.md"

if (-not (Test-Path $catalogPath)) {
    throw "catalog.json not found at $catalogPath"
}

if (-not (Test-Path $agentsDir)) {
    throw "agents directory not found at $agentsDir"
}

$catalog = Get-Content -Raw -Path $catalogPath | ConvertFrom-Json
$knownAgents = @{}
foreach ($agent in $catalog.agents) {
    $knownAgents[$agent.name] = $true
}

$knownCanonicalTools = @(
    "memory_status",
    "memory_recall",
    "memory_persist",
    "memory_link",
    "memory_read"
)

$warnings = New-Object System.Collections.Generic.List[string]
$errors = New-Object System.Collections.Generic.List[string]

# Validate that catalog entries point to existing files.
foreach ($agent in $catalog.agents) {
    $filePath = Join-Path $root $agent.file
    if (-not (Test-Path $filePath)) {
        $errors.Add("Catalog entry '$($agent.name)' points to missing file '$($agent.file)'.")
    }
}

# Validate delegation references in agent markdown files.
$agentFiles = Get-ChildItem -Path $agentsDir -Filter "*.agent.md" -File
$referenceRegex = [regex]"\*\*([A-Za-z0-9\.\- ']+)\*\*\s*:"
foreach ($file in $agentFiles) {
    $content = Get-Content -Raw -Path $file.FullName
    $matches = $referenceRegex.Matches($content)
    foreach ($match in $matches) {
        $rawRef = $match.Groups[1].Value.Trim()
        if ($rawRef -match "(?i)\bFallback\b") {
            continue
        }

        $normalized = $rawRef.Replace(" ", "")
        $isLikelyAgentRef = $normalized.EndsWith("Agent") -or $normalized.EndsWith("Expert")
        if (-not $isLikelyAgentRef) {
            continue
        }

        if (-not $knownAgents.ContainsKey($normalized)) {
            $warnings.Add("Unknown delegated agent reference '$rawRef' in '$($file.Name)'.")
        }
    }
}

# Validate memory contract consistency.
if (Test-Path $sharedMemoryPath) {
    $shared = Get-Content -Raw -Path $sharedMemoryPath
    foreach ($tool in $knownCanonicalTools) {
        if (-not $shared.Contains($tool)) {
            $warnings.Add("Shared memory instructions missing canonical tool '$tool'.")
        }
    }

    if (-not $shared.Contains("call_external_mcp_tool")) {
        $warnings.Add("Shared memory instructions should document bridge fallback via call_external_mcp_tool.")
    }
} else {
    $errors.Add("Shared memory instructions not found at '$sharedMemoryPath'.")
}

Write-Host ""
Write-Host "Agent/config validation report" -ForegroundColor Cyan
Write-Host "Root: $root"
Write-Host ""

if ($warnings.Count -gt 0) {
    Write-Host "Warnings ($($warnings.Count)):" -ForegroundColor Yellow
    foreach ($w in $warnings) {
        Write-Host " - $w" -ForegroundColor Yellow
    }
    Write-Host ""
} else {
    Write-Host "Warnings: none" -ForegroundColor Green
}

if ($errors.Count -gt 0) {
    Write-Host "Errors ($($errors.Count)):" -ForegroundColor Red
    foreach ($e in $errors) {
        Write-Host " - $e" -ForegroundColor Red
    }
    exit 1
}

Write-Host "Errors: none" -ForegroundColor Green
exit 0
