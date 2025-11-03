# Script para limpiar archivos antiguos después de verificar que la reorganización funciona

Write-Host "=== LIMPIANDO ARCHIVOS ANTIGUOS ===" -ForegroundColor Yellow
Write-Host "ADVERTENCIA: Este script eliminará los archivos antiguos." -ForegroundColor Red
Write-Host "Asegúrate de que la compilación y tests funcionan correctamente antes de continuar.`n" -ForegroundColor Red

$confirmation = Read-Host "¿Estás seguro de que quieres continuar? (S/N)"
if ($confirmation -ne 'S') {
    Write-Host "Operación cancelada." -ForegroundColor Yellow
    exit
}

$filesToDelete = @(
    "Scheduler_Lib\Core\Services\BaseDateTimeCalculator.cs",
    "Scheduler_Lib\Core\Services\TimeZoneConverter.cs",
    "Scheduler_Lib\Core\Services\DateSafetyHelper.cs",
    "Scheduler_Lib\Core\Services\WeeklyRecurrenceCalculator.cs",
    "Scheduler_Lib\Core\Services\MonthlyRecurrenceCalculator.cs",
    "Scheduler_Lib\Core\Services\DailyRecurrenceCalculator.cs",
    "Scheduler_Lib\Core\Services\DailySlotGenerator.cs",
    "Scheduler_Lib\Core\Services\MonthDayCollector.cs",
    "Scheduler_Lib\Core\Services\RecurrenceCalculator.cs",
    "Scheduler_Lib\Core\Services\CalculateOneTime.cs",
    "Scheduler_Lib\Core\Services\CalculateRecurrent.cs",
    "Scheduler_Lib\Core\Model\EnumConfiguration.cs",
    "Scheduler_Lib\Core\Model\EnumRecurrency.cs",
    "Scheduler_Lib\Core\Model\EnumMonthlyFrequency.cs",
    "Scheduler_Lib\Core\Model\EnumMonthlyDateType.cs"
)

foreach ($file in $filesToDelete) {
    if (Test-Path $file) {
        Remove-Item $file -Force
        Write-Host "? Eliminado: $file" -ForegroundColor Green
    } else {
        Write-Host "? No encontrado: $file" -ForegroundColor Gray
    }
}

Write-Host "`n=== LIMPIEZA COMPLETADA ===" -ForegroundColor Green
Write-Host "Los archivos antiguos han sido eliminados." -ForegroundColor White
Write-Host "Ejecuta 'dotnet build' para verificar que todo sigue funcionando." -ForegroundColor White
