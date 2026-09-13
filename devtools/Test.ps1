<#!
Runs Cartesia.Tests (MTP mode) and surfaces per-test failures.
Test conventions (see AGENTS.md):
  ShouldBe_*    RED - asserts how the library SHOULD behave. Failing is expected
                until the owner fixes the implementation. Never "fix" by editing these.
  KnownIssue_*  GREEN warning - pins how the library actually behaves today, but
                should not. Listed below on every run. Flip to ShouldBe_* when fixed.
Usage:
  devtools/Test.ps1
  devtools/Test.ps1 -Filter "FullyQualifiedName~BoxTests"
#>
param(
  [string]$Filter = ""
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "Cartesia/Cartesia.Tests/Cartesia.Tests.csproj"
$outFile = Join-Path ([System.IO.Path]::GetTempPath()) "cartesia-testout.txt"

function Invoke-TestRun([string]$runFilter) {
  $runArgs = @("test", "--project", $project)
  if ($runFilter -ne "") { $runArgs += @("--", "--filter", $runFilter) }
  & dotnet @runArgs > $outFile 2>&1
  return $LASTEXITCODE
}

$exitCode = Invoke-TestRun $Filter
$lines = Get-Content $outFile

$failedIdx = @(0..($lines.Count - 1) | Where-Object { $lines[$_] -match "^failed " })
if ($failedIdx.Count -gt 0) {
  Write-Host ""
  Write-Host "FAILURES ($($failedIdx.Count)) - includes RED ShouldBe_* (desired behavior, fix implementation):" -ForegroundColor Red
  foreach ($i in $failedIdx) {
    $lines[$i..[Math]::Min($lines.Count - 1, $i + 6)] | ForEach-Object { Write-Host $_ -ForegroundColor Red }
    Write-Host ""
  }
}

$lines | Select-Object -Last 8 | ForEach-Object { Write-Host $_ }

if ($Filter -eq "") {
  $knownCode = Invoke-TestRun "FullyQualifiedName~KnownIssue"
  $knownLines = Get-Content $outFile
  $totalLine = $knownLines | Where-Object { $_ -match "^\s*total:" } | Select-Object -First 1
  Write-Host ""
  Write-Host "KNOWN ISSUES (green warnings - actual behavior that should not be):$totalLine" -ForegroundColor Yellow
  Write-Host "Run devtools/Test.ps1 -Filter 'FullyQualifiedName~KnownIssue' to list them." -ForegroundColor Yellow
  if ($exitCode -eq 0) { $exitCode = $knownCode }
}

exit $exitCode
