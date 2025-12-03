# Script para generar cobertura de código
Write-Host "Generando cobertura de código..." -ForegroundColor Green

# Limpiar resultados anteriores
if (Test-Path "coveragereport") {
    Remove-Item -Recurse -Force "coveragereport"
}
if (Test-Path "Scheduler_Integration/TestResults") {
    Remove-Item -Recurse -Force "Scheduler_Integration/TestResults"
}

# Ejecutar tests con cobertura
Write-Host "`nEjecutando tests..." -ForegroundColor Yellow
dotnet test Scheduler_Integration/Scheduler_Integration.csproj `
    --collect:"XPlat Code Coverage" `
    --results-directory:"./TestResults" `
    --logger "console;verbosity=normal"

# Verificar que se generó el archivo de cobertura
$coverageFile = Get-ChildItem -Path "TestResults" -Filter "coverage.cobertura.xml" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1

if ($null -eq $coverageFile) {
    Write-Host "`nNo se encontró archivo de cobertura" -ForegroundColor Red
    exit 1
}

Write-Host "`nArchivo de cobertura encontrado: $($coverageFile.FullName)" -ForegroundColor Green

# Verificar si reportgenerator está instalado
$reportGenerator = Get-Command reportgenerator -ErrorAction SilentlyContinue
if ($null -eq $reportGenerator) {
    Write-Host "`nInstalando reportgenerator..." -ForegroundColor Yellow
    dotnet tool install -g dotnet-reportgenerator-globaltool
}

# Generar reporte HTML
Write-Host "`nGenerando reporte HTML..." -ForegroundColor Yellow
reportgenerator `
    -reports:"$($coverageFile.FullName)" `
    -targetdir:"coveragereport" `
    -reporttypes:"Html;TextSummary"

# Mostrar resumen en consola
if (Test-Path "coveragereport/Summary.txt") {
    Write-Host "`n=== RESUMEN DE COBERTURA ===" -ForegroundColor Cyan
    Get-Content "coveragereport/Summary.txt"
}

# Abrir reporte en navegador
$reportPath = Resolve-Path "coveragereport/index.html"
Write-Host "`nReporte generado en: $reportPath" -ForegroundColor Green
Write-Host "Abriendo reporte en navegador..." -ForegroundColor Yellow
Start-Process $reportPath

Write-Host "`n¡Proceso completado!" -ForegroundColor Green
