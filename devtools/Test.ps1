<#!
Runs Cartesia.Tests (MTP mode) and surfaces per-test failures.
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

$args = @("test", "--project", $project)
if ($Filter -ne "") { $args += @("--", "--filter", $Filter) }

& dotnet @args > $outFile 2>&1
$exitCode = $LASTEXITCODE

$lines = Get-Content $outFile

$failedIdx = @(0..($lines.Count - 1) | Where-Object { $lines[$_] -match "^failed " })
if ($failedIdx.Count -gt 0) {
  Write-Host ""
  Write-Host "FAILURES ($($failedIdx.Count)):" -ForegroundColor Red
  foreach ($i in $failedIdx) {
    $lines[$i..[Math]::Min($lines.Count - 1, $i + 6)] | ForEach-Object { Write-Host $_ -ForegroundColor Red }
    Write-Host ""
  }
}

$lines | Select-Object -Last 8 | ForEach-Object { Write-Host $_ }
exit $exitCode
