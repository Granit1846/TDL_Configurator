# tdl_push.ps1
param(
  [string]$Message = ""
)

$ErrorActionPreference = "Stop"

# Always run in the folder where the script is located (repo root)
Set-Location -Path $PSScriptRoot

# Basic checks
if (-not (Test-Path ".git")) {
  Write-Host "[TDL_PUSH] ERROR: .git not found. Put this script in the repository root." -ForegroundColor Red
  exit 2
}

# Check git availability and that we are inside a work tree
git --version | Out-Null
$inside = (git rev-parse --is-inside-work-tree) 2>$null
if ($inside -ne "true") {
  Write-Host "[TDL_PUSH] ERROR: Not a git work tree." -ForegroundColor Red
  exit 2
}

# Detect changes
$changes = git status --porcelain
if (-not $changes) {
  Write-Host "[TDL_PUSH] Nothing to commit. Working tree clean."
  exit 0
}

# Choose commit message
if ([string]::IsNullOrWhiteSpace($Message)) {
  $Message = "Update " + (Get-Date -Format "yyyy-MM-dd HH:mm:ss")
}

# Determine branch and remote
$branch = (git rev-parse --abbrev-ref HEAD).Trim()
$remotes = (git remote).Trim()

if (-not $remotes) {
  Write-Host "[TDL_PUSH] ERROR: No git remotes configured." -ForegroundColor Red
  exit 3
}

$remote = "origin"
if ($remotes -notcontains $remote) {
  $remote = $remotes[0]
}

Write-Host "[TDL_PUSH] Repo: $PSScriptRoot"
Write-Host "[TDL_PUSH] Remote: $remote"
Write-Host "[TDL_PUSH] Branch: $branch"
Write-Host "[TDL_PUSH] Commit: $Message"

git add -A
git commit -m $Message
git push $remote $branch

Write-Host "[TDL_PUSH] Done."
exit 0
