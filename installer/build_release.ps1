# ============================================================================
# IIT FINCORE — Script de Publication Release & Génération d'Installeur (.exe)
# ============================================================================

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$publishDir = Join-Path $root "dist\publish"
$releaseBin = Join-Path $root "IIT.FinCore.WinForms\bin\Release\net9.0-windows"

Write-Host "1/2 Compilation de IIT FINCORE en mode Release..." -ForegroundColor Cyan
dotnet build (Join-Path $root "IIT.FinCore.WinForms\IIT.FinCore.WinForms.csproj") -c Release
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if (Test-Path $publishDir) { Remove-Item -Recurse -Force $publishDir }
New-Item -ItemType Directory -Force -Path $publishDir | Out-Null
Copy-Item -Path (Join-Path $releaseBin "*") -Destination $publishDir -Recurse -Force

Write-Host "Package Release prêt à livrer dans : $publishDir" -ForegroundColor Green

$iscc = @(
    "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
    "C:\Program Files\Inno Setup 6\ISCC.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1

if ($iscc) {
    Write-Host "2/2 Compilation de l'installeur Windows (.exe) via Inno Setup..." -ForegroundColor Cyan
    & $iscc (Join-Path $PSScriptRoot "IIT_FINCORE_Setup.iss")
    Write-Host "Installeur généré dans : $(Join-Path $root 'dist\installer\IIT_FINCORE_Setup_v2.0.exe')" -ForegroundColor Green
} else {
    Write-Host "2/2 Package autonome prêt dans dist\publish\IIT.FinCore.WinForms.exe" -ForegroundColor Yellow
    Write-Host "Pour compiler l'installeur Setup (.exe), installez Inno Setup 6 et relancez ce script." -ForegroundColor Yellow
}
