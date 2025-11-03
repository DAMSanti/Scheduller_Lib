# REORGANIZACIÓN DE ESTRUCTURA - SCHEDULER_LIB

## ? CAMBIOS COMPLETADOS

### 1. Enums movidos a `Core/Model/Enums/`
- ? EnumConfiguration.cs
- ? EnumRecurrency.cs
- ? EnumMonthlyFrequency.cs
- ? EnumMonthlyDateType.cs

### 2. Utilidades movidas a `Core/Services/Utilities/`
- ? TimeZoneConverter.cs
- ? DateSafetyHelper.cs

### 3. Calculadores Base movidos a `Core/Services/Calculators/Base/`
- ? BaseDateTimeCalculator.cs
- ? RecurrenceCalculator.cs (coordinador)

### 4. Calculador Weekly movido a `Core/Services/Calculators/Weekly/`
- ? WeeklyRecurrenceCalculator.cs

---

## ?? ARCHIVOS PENDIENTES DE MOVER

Para completar la reorganización, necesitas mover estos archivos manualmente:

### Monthly ? `Core/Services/Calculators/Monthly/`
```
MonthlyRecurrenceCalculator.cs
MonthDayCollector.cs
```

### Daily ? `Core/Services/Calculators/Daily/`
```
DailyRecurrenceCalculator.cs
DailySlotGenerator.cs
```

### Strategies ? `Core/Services/Strategies/`
```
CalculateOneTime.cs
CalculateRecurrent.cs
```

---

## ?? CAMBIOS DE NAMESPACE REQUERIDOS

### Para archivos en `Calculators/Monthly/`:
```csharp
namespace Scheduler_Lib.Core.Services.Calculators.Monthly;
```

Agregar usings:
```csharp
using Scheduler_Lib.Core.Services.Calculators.Base;
using Scheduler_Lib.Core.Services.Calculators.Weekly;
using Scheduler_Lib.Core.Services.Calculators.Daily;
using Scheduler_Lib.Core.Services.Utilities;
```

### Para archivos en `Calculators/Daily/`:
```csharp
namespace Scheduler_Lib.Core.Services.Calculators.Daily;
```

Agregar usings:
```csharp
using Scheduler_Lib.Core.Services.Calculators.Base;
using Scheduler_Lib.Core.Services.Calculators.Monthly;
using Scheduler_Lib.Core.Services.Utilities;
```

### Para archivos en `Strategies/`:
```csharp
namespace Scheduler_Lib.Core.Services.Strategies;
```

Agregar usings:
```csharp
using Scheduler_Lib.Core.Services.Calculators.Base;
using Scheduler_Lib.Core.Services.Utilities;
```

---

## ?? ESTRUCTURA FINAL

```
Scheduler_Lib/
??? Core/
?   ??? Model/
?   ?   ??? Enums/
?   ?   ?   ??? EnumConfiguration.cs ?
?   ?   ?   ??? EnumRecurrency.cs ?
?   ?   ?   ??? EnumMonthlyFrequency.cs ?
?   ?   ?   ??? EnumMonthlyDateType.cs ?
?   ?   ??? SchedulerInput.cs
?   ?   ??? SchedulerOutput.cs
?   ?
?   ??? Services/
?   ?   ??? Calculators/
?   ?   ?   ??? Base/
?   ?   ?   ?   ??? BaseDateTimeCalculator.cs ?
?   ?   ?   ?   ??? RecurrenceCalculator.cs ?
?   ?   ?   ??? Weekly/
?   ?   ?   ?   ??? WeeklyRecurrenceCalculator.cs ?
?   ?   ?   ??? Monthly/
?   ?   ?   ?   ??? MonthlyRecurrenceCalculator.cs ?
?   ?   ?   ?   ??? MonthDayCollector.cs ?
?   ?   ?   ??? Daily/
?   ?   ?       ??? DailyRecurrenceCalculator.cs ?
?   ?   ?       ??? DailySlotGenerator.cs ?
?   ?   ?
?   ?   ??? Strategies/
?   ?   ?   ??? CalculateOneTime.cs ?
?   ?   ?   ??? CalculateRecurrent.cs ?
?   ?   ?
?   ?   ??? Utilities/
?   ?   ?   ??? TimeZoneConverter.cs ?
?   ?   ?   ??? DateSafetyHelper.cs ?
?   ?   ?
?   ?   ??? DescriptionBuilder.cs
?   ?   ??? ResultPattern.cs
?   ?   ??? ScheduleCalculator.cs
?   ?   ??? SchedulerService.cs
?   ?
?   ??? Factory/
?       ??? ScheduleCalculatorFactory.cs
?
??? Infrastructure/
?   ??? Validations/
?       ??? ValidateOnce.cs
?       ??? ValidateRecurrent.cs
?       ??? Validations.cs
?
??? Resources/
    ??? Config.cs
    ??? Messages.cs
```

---

## ?? ARCHIVOS A ELIMINAR DESPUÉS

Una vez movidos todos los archivos, eliminar:
- `Scheduler_Lib\Core\Services\BaseDateTimeCalculator.cs` (old)
- `Scheduler_Lib\Core\Services\TimeZoneConverter.cs` (old)
- `Scheduler_Lib\Core\Services\DateSafetyHelper.cs` (old)
- `Scheduler_Lib\Core\Services\WeeklyRecurrenceCalculator.cs` (old)
- `Scheduler_Lib\Core\Services\MonthlyRecurrenceCalculator.cs` (old)
- `Scheduler_Lib\Core\Services\DailyRecurrenceCalculator.cs` (old)
- `Scheduler_Lib\Core\Services\DailySlotGenerator.cs` (old)
- `Scheduler_Lib\Core\Services\MonthDayCollector.cs` (old)
- `Scheduler_Lib\Core\Services\RecurrenceCalculator.cs` (old)
- `Scheduler_Lib\Core\Services\CalculateOneTime.cs` (old)
- `Scheduler_Lib\Core\Services\CalculateRecurrent.cs` (old)
- `Scheduler_Lib\Core\Model\EnumConfiguration.cs` (old)
- `Scheduler_Lib\Core\Model\EnumRecurrency.cs` (old)
- `Scheduler_Lib\Core\Model\EnumMonthlyFrequency.cs` (old)
- `Scheduler_Lib\Core\Model\EnumMonthlyDateType.cs` (old)

---

## ?? BENEFICIOS

1. **Separación clara de responsabilidades**
2. **Fácil navegación por el código**
3. **Mantenibilidad mejorada**
4. **Escalabilidad** - fácil agregar nuevos tipos de calculadores
5. **Testing independiente** por módulo
6. **Cohesión** - archivos relacionados juntos

---

## ?? PRÓXIMOS PASOS RECOMENDADOS

1. Mover archivos restantes manualmente
2. Actualizar namespaces y usings
3. Compilar y verificar que no hay errores
4. Ejecutar todos los tests
5. Eliminar archivos antiguos
6. Commit de los cambios
