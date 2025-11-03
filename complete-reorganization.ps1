# Script completo de reorganización de Scheduler_Lib
# Ejecutar desde la raíz del proyecto

Write-Host "=== INICIANDO REORGANIZACIÓN COMPLETA ===" -ForegroundColor Green

# Función para actualizar namespace en un archivo
function Update-Namespace {
    param(
        [string]$FilePath,
        [string]$NewNamespace,
        [string[]]$AdditionalUsings = @()
    )
    
    if (-not (Test-Path $FilePath)) {
        Write-Host "ADVERTENCIA: No se encontró $FilePath" -ForegroundColor Yellow
        return
    }
    
    $content = Get-Content $FilePath -Raw
    
    # Actualizar namespace
    $content = $content -replace 'namespace Scheduler_Lib\.Core\.Services;', "namespace $NewNamespace;"
    
    # Agregar usings si es necesario
    if ($AdditionalUsings.Count -gt 0) {
        $usingBlock = ($AdditionalUsings | ForEach-Object { "using $_;" }) -join "`n"
        $content = $content -replace '(using Scheduler_Lib\.Core\.Model;)', "`$1`n$usingBlock"
    }
    
    # Actualizar referencias internas
    $content = $content -replace 'BaseDateTimeCalculator', 'Base.BaseDateTimeCalculator'
    $content = $content -replace 'TimeZoneConverter', 'Utilities.TimeZoneConverter'
    $content = $content -replace 'DateSafetyHelper', 'Utilities.DateSafetyHelper'
    $content = $content -replace 'MonthDayCollector', 'Monthly.MonthDayCollector'
    $content = $content -replace 'DailySlotGenerator', 'Daily.DailySlotGenerator'
    
    Set-Content $FilePath -Value $content -NoNewline
    Write-Host "? Actualizado: $FilePath" -ForegroundColor Green
}

# Copiar y actualizar archivos Monthly
Write-Host "`n--- Procesando Monthly ---" -ForegroundColor Cyan
Copy-Item "Scheduler_Lib\Core\Services\MonthlyRecurrenceCalculator.cs" "Scheduler_Lib\Core\Services\Calculators\Monthly\MonthlyRecurrenceCalculator.cs" -Force
Copy-Item "Scheduler_Lib\Core\Services\MonthDayCollector.cs" "Scheduler_Lib\Core\Services\Calculators\Monthly\MonthDayCollector.cs" -Force

Update-Namespace -FilePath "Scheduler_Lib\Core\Services\Calculators\Monthly\MonthlyRecurrenceCalculator.cs" `
    -NewNamespace "Scheduler_Lib.Core.Services.Calculators.Monthly" `
    -AdditionalUsings @(
        "Scheduler_Lib.Core.Services.Calculators.Base",
        "Scheduler_Lib.Core.Services.Calculators.Weekly",
        "Scheduler_Lib.Core.Services.Calculators.Daily",
        "Scheduler_Lib.Core.Services.Utilities"
    )

Update-Namespace -FilePath "Scheduler_Lib\Core\Services\Calculators\Monthly\MonthDayCollector.cs" `
    -NewNamespace "Scheduler_Lib.Core.Services.Calculators.Monthly"

# Copiar y actualizar archivos Daily
Write-Host "`n--- Procesando Daily ---" -ForegroundColor Cyan
Copy-Item "Scheduler_Lib\Core\Services\DailyRecurrenceCalculator.cs" "Scheduler_Lib\Core\Services\Calculators\Daily\DailyRecurrenceCalculator.cs" -Force
Copy-Item "Scheduler_Lib\Core\Services\DailySlotGenerator.cs" "Scheduler_Lib\Core\Services\Calculators\Daily\DailySlotGenerator.cs" -Force

Update-Namespace -FilePath "Scheduler_Lib\Core\Services\Calculators\Daily\DailyRecurrenceCalculator.cs" `
    -NewNamespace "Scheduler_Lib.Core.Services.Calculators.Daily" `
    -AdditionalUsings @(
        "Scheduler_Lib.Core.Services.Calculators.Base",
        "Scheduler_Lib.Core.Services.Calculators.Monthly",
        "Scheduler_Lib.Core.Services.Utilities"
    )

Update-Namespace -FilePath "Scheduler_Lib\Core\Services\Calculators\Daily\DailySlotGenerator.cs" `
    -NewNamespace "Scheduler_Lib.Core.Services.Calculators.Daily" `
    -AdditionalUsings @("Scheduler_Lib.Core.Services.Utilities")

# Copiar y actualizar archivos Strategies
Write-Host "`n--- Procesando Strategies ---" -ForegroundColor Cyan
Copy-Item "Scheduler_Lib\Core\Services\CalculateOneTime.cs" "Scheduler_Lib\Core\Services\Strategies\CalculateOneTime.cs" -Force
Copy-Item "Scheduler_Lib\Core\Services\CalculateRecurrent.cs" "Scheduler_Lib\Core\Services\Strategies\CalculateRecurrent.cs" -Force

Update-Namespace -FilePath "Scheduler_Lib\Core\Services\Strategies\CalculateOneTime.cs" `
    -NewNamespace "Scheduler_Lib.Core.Services.Strategies" `
    -AdditionalUsings @(
        "Scheduler_Lib.Core.Services.Utilities"
    )

Update-Namespace -FilePath "Scheduler_Lib\Core\Services\Strategies\CalculateRecurrent.cs" `
    -NewNamespace "Scheduler_Lib.Core.Services.Strategies" `
    -AdditionalUsings @(
        "Scheduler_Lib.Core.Services.Utilities",
        "Scheduler_Lib.Core.Services.Calculators.Base"
    )

Write-Host "`n=== REORGANIZACIÓN COMPLETADA ===" -ForegroundColor Green
Write-Host "`nPróximos pasos:" -ForegroundColor Yellow
Write-Host "1. Revisar los archivos generados en las nuevas carpetas" -ForegroundColor White
Write-Host "2. Ejecutar dotnet build para verificar compilación" -ForegroundColor White
Write-Host "3. Ejecutar tests para asegurar funcionalidad" -ForegroundColor White
Write-Host "4. Si todo funciona, ejecutar: .\cleanup-old-files.ps1" -ForegroundColor White
