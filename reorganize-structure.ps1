# Script para reorganizar la estructura de Scheduler_Lib

$basePathLib = "Scheduler_Lib\Core\Services"

Write-Host "=== Iniciando reorganización de archivos ===" -ForegroundColor Green

# Eliminar archivos antiguos en la raíz de Services (excepto los que queremos mantener)
$filesToKeep = @("DescriptionBuilder.cs", "ResultPattern.cs", "ScheduleCalculator.cs", "SchedulerService.cs", "CalculateDate.cs")

Get-ChildItem -Path $basePathLib -File | Where-Object { 
    $_.Name -notin $filesToKeep -and 
    $_.Name -notlike "*.csproj"
} | ForEach-Object {
    Write-Host "Eliminando archivo antiguo: $($_.Name)" -ForegroundColor Yellow
    Remove-Item $_.FullName -Force
}

# Eliminar archivos enum antiguos
$enumFiles = @(
    "Scheduler_Lib\Core\Model\EnumConfiguration.cs",
    "Scheduler_Lib\Core\Model\EnumRecurrency.cs",
    "Scheduler_Lib\Core\Model\EnumMonthlyFrequency.cs",
    "Scheduler_Lib\Core\Model\EnumMonthlyDateType.cs"
)

foreach ($file in $enumFiles) {
    if (Test-Path $file) {
        Write-Host "Eliminando enum antiguo: $file" -ForegroundColor Yellow
        Remove-Item $file -Force
    }
}

Write-Host "`n=== Reorganización completada ===" -ForegroundColor Green
Write-Host "Nueva estructura:" -ForegroundColor Cyan
Write-Host "  - Enums movidos a: Core/Model/Enums/" -ForegroundColor Cyan
Write-Host "  - Calculadores movidos a: Core/Services/Calculators/*/" -ForegroundColor Cyan
Write-Host "  - Utilidades movidas a: Core/Services/Utilities/" -ForegroundColor Cyan
Write-Host "  - Estrategias movidas a: Core/Services/Strategies/" -ForegroundColor Cyan
