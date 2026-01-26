Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

git -C $repoRoot config core.hooksPath .githooks

Write-Host "Git hooks installed (core.hooksPath=.githooks)."
