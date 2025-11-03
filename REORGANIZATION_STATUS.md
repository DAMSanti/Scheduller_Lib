# REORGANIZACIÓN COMPLETADA PARCIALMENTE

## ? ARCHIVOS YA MOVIDOS Y FUNCIONANDO

1. **Enums** ? `Core/Model/Enums/` ?
   - EnumConfiguration.cs
   - EnumRecurrency.cs
   - EnumMonthlyFrequency.cs
   - EnumMonthlyDateType.cs

2. **Utilities** ? `Core/Services/Utilities/` ?
   - TimeZoneConverter.cs
   - DateSafetyHelper.cs

3. **Base Calculators** ? `Core/Services/Calculators/Base/` ?
   - BaseDateTimeCalculator.cs

4. **Weekly** ? `Core/Services/Calculators/Weekly/` ?
   - WeeklyRecurrenceCalculator.cs

5. **Monthly (parcial)** ? `Core/Services/Calculators/Monthly/` ??
   - MonthDayCollector.cs ?
   - MonthlyRecurrenceCalculator.cs ? FALTA

---

## ?? ARCHIVOS QUE FALTAN POR MOVER

### Carpeta: `Core/Services/Calculators/Monthly/`
Necesitas copiar manualmente:
```
DESDE: Scheduler_Lib\Core\Services\MonthlyRecurrenceCalculator.cs
HACIA: Scheduler_Lib\Core\Services\Calculators\Monthly\MonthlyRecurrenceCalculator.cs
```

Luego cambiar el namespace a:
```csharp
namespace Scheduler_Lib.Core.Services.Calculators.Monthly;
```

Y agregar estos usings al inicio:
```csharp
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services.Calculators.Base;
using Scheduler_Lib.Core.Services.Calculators.Weekly;
using Scheduler_Lib.Core.Services.Calculators.Daily;
using Scheduler_Lib.Core.Services.Utilities;
using Scheduler_Lib.Resources;
```

### Carpeta: `Core/Services/Calculators/Daily/`
Necesitas copiar manualmente:
```
DESDE: Scheduler_Lib\Core\Services\DailyRecurrenceCalculator.cs
HACIA: Scheduler_Lib\Core\Services\Calculators\Daily\DailyRecurrenceCalculator.cs

DESDE: Scheduler_Lib\Core\Services\DailySlotGenerator.cs
HACIA: Scheduler_Lib\Core\Services\Calculators\Daily\DailySlotGenerator.cs
```

Para **DailyRecurrenceCalculator.cs**, cambiar namespace a:
```csharp
namespace Scheduler_Lib.Core.Services.Calculators.Daily;
```

Y agregar estos usings:
```csharp
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services.Calculators.Base;
using Scheduler_Lib.Core.Services.Calculators.Monthly;
using Scheduler_Lib.Core.Services.Utilities;
using Scheduler_Lib.Resources;
```

Para **DailySlotGenerator.cs**, cambiar namespace a:
```csharp
namespace Scheduler_Lib.Core.Services.Calculators.Daily;
```

Y agregar estos usings:
```csharp
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services.Utilities;
using Scheduler_Lib.Resources;
```

---

## ?? ACTUALIZAR REFERENCIAS EN ARCHIVOS MOVIDOS

En los archivos que muevas, reemplaza estas referencias:

| Antiguo | Nuevo |
|---------|-------|
| `BaseDateTimeCalculator.GetBaseDateTime` | Mantener igual (mismo using) |
| `TimeZoneConverter.CreateDateTimeOffset` | Mantener igual (mismo using) |
| `DateSafetyHelper.TryAddDaysSafely` | Mantener igual (mismo using) |
| `MonthlyRecurrenceCalculator.GetEligibleDate` | Mantener igual |
| `WeeklyRecurrenceCalculator.SelectNextEligibleDate` | Mantener igual |
| `DailySlotGenerator.GenerateSlotsForDay` | Mantener igual |

---

## ??? ARCHIVOS A ELIMINAR DESPUÉS DE VERIFICAR

Una vez que compile correctamente, eliminar:
```
Scheduler_Lib\Core\Services\BaseDateTimeCalculator.cs
Scheduler_Lib\Core\Services\TimeZoneConverter.cs
Scheduler_Lib\Core\Services\DateSafetyHelper.cs
Scheduler_Lib\Core\Services\WeeklyRecurrenceCalculator.cs
Scheduler_Lib\Core\Services\MonthlyRecurrenceCalculator.cs
Scheduler_Lib\Core\Services\DailyRecurrenceCalculator.cs
Scheduler_Lib\Core\Services\DailySlotGenerator.cs
Scheduler_Lib\Core\Services\MonthDayCollector.cs
```

---

## ?? ESTRUCTURA FINAL ESPERADA

```
Scheduler_Lib/Core/Services/
??? Calculators/
?   ??? Base/
?   ?   ??? BaseDateTimeCalculator.cs ?
?   ??? Weekly/
?   ?   ??? WeeklyRecurrenceCalculator.cs ?
?   ??? Monthly/
?   ?   ??? MonthlyRecurrenceCalculator.cs ?
?   ?   ??? MonthDayCollector.cs ?
?   ??? Daily/
?       ??? DailyRecurrenceCalculator.cs ?
?       ??? DailySlotGenerator.cs ?
??? Utilities/
?   ??? TimeZoneConverter.cs ?
?   ??? DateSafetyHelper.cs ?
??? RecurrenceCalculator.cs ? (wrapper/facade)
??? CalculateOneTime.cs
??? CalculateRecurrent.cs
??? DescriptionBuilder.cs
??? ResultPattern.cs
??? ScheduleCalculator.cs
```

---

## ? PASOS FINALES

1. Copiar archivos Monthly y Daily como se indica arriba
2. Actualizar namespaces y usings
3. Ejecutar: `dotnet build`
4. Si compila OK, ejecutar tests
5. Eliminar archivos antiguos
6. Commit!

---

## ?? BENEFICIOS OBTENIDOS

? Código organizado por responsabilidades
? Fácil de navegar y mantener
? Preparado para crecimiento
? Testeable independientemente
? Nombres claros y descriptivos
