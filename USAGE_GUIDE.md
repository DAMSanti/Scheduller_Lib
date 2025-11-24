# ?? Guía de Uso - Scheduler_Lib (Versión Refactorizada)

## ?? Introducción

Esta guía explica cómo usar la biblioteca Scheduler_Lib después de la refactorización con principios SOLID y patrones de diseño.

---

## ?? Inicio Rápido

### Uso Básico (API Pública)

```csharp
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;

// Crear configuración
var input = new SchedulerInput
{
    EnabledChk = true,
    Periodicity = EnumConfiguration.Recurrent,
    Recurrency = EnumRecurrency.Daily,
    Language = "en_US",
    CurrentDate = DateTimeOffset.Now,
    StartDate = DateTimeOffset.Now,
    EndDate = DateTimeOffset.Now.AddMonths(1),
    OccursOnceChk = true,
    OccursOnceAt = TimeSpan.FromHours(14)
};

// Calcular próxima ejecución
var result = SchedulerService.InitialOrchestator(input);

if (result.IsSuccess)
{
    Console.WriteLine($"Próxima ejecución: {result.Value.NextDate}");
    Console.WriteLine($"Descripción: {result.Value.Description}");
}
else
{
    Console.WriteLine($"Error: {result.Error}");
}
```

---

## ?? Casos de Uso Comunes

### 1. Recurrencia Diaria

#### Ejecutar Todos los Días a las 2:00 PM
```csharp
var input = new SchedulerInput
{
    EnabledChk = true,
    Periodicity = EnumConfiguration.Recurrent,
    Recurrency = EnumRecurrency.Daily,
    Language = "en_US",
    CurrentDate = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero),
    StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
    EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero),
    OccursOnceChk = true,
    OccursOnceAt = TimeSpan.FromHours(14) // 2:00 PM
};

var result = SchedulerService.InitialOrchestator(input);
```

#### Ejecutar Cada 2 Horas entre 9:00 AM y 5:00 PM
```csharp
var input = new SchedulerInput
{
    EnabledChk = true,
    Periodicity = EnumConfiguration.Recurrent,
    Recurrency = EnumRecurrency.Daily,
    Language = "en_US",
    CurrentDate = DateTimeOffset.Now,
    StartDate = DateTimeOffset.Now.Date,
    EndDate = DateTimeOffset.Now.AddMonths(1),
    OccursEveryChk = true,
    DailyPeriod = TimeSpan.FromHours(2),
    DailyStartTime = TimeSpan.FromHours(9),   // 9:00 AM
    DailyEndTime = TimeSpan.FromHours(17)     // 5:00 PM
};

var result = SchedulerService.InitialOrchestator(input);
```

---

### 2. Recurrencia Semanal

#### Ejecutar Lunes, Miércoles y Viernes a las 10:30 AM
```csharp
var input = new SchedulerInput
{
    EnabledChk = true,
    Periodicity = EnumConfiguration.Recurrent,
    Recurrency = EnumRecurrency.Weekly,
    Language = "en_US",
    CurrentDate = DateTimeOffset.Now,
    StartDate = DateTimeOffset.Now.Date,
    EndDate = DateTimeOffset.Now.AddMonths(3),
    WeeklyPeriod = 1, // Cada semana
    DaysOfWeek = new List<DayOfWeek> 
    { 
        DayOfWeek.Monday, 
        DayOfWeek.Wednesday, 
        DayOfWeek.Friday 
    },
    OccursOnceChk = true,
    OccursOnceAt = new TimeSpan(10, 30, 0) // 10:30 AM
};

var result = SchedulerService.InitialOrchestator(input);
```

#### Ejecutar Cada 2 Semanas los Martes, con Múltiples Horarios
```csharp
var input = new SchedulerInput
{
    EnabledChk = true,
    Periodicity = EnumConfiguration.Recurrent,
    Recurrency = EnumRecurrency.Weekly,
    Language = "es_ES",
    CurrentDate = DateTimeOffset.Now,
    StartDate = DateTimeOffset.Now.Date,
    EndDate = DateTimeOffset.Now.AddMonths(6),
    WeeklyPeriod = 2, // Cada 2 semanas
    DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Tuesday },
    OccursEveryChk = true,
    DailyPeriod = TimeSpan.FromHours(3),
    DailyStartTime = TimeSpan.FromHours(8),
    DailyEndTime = TimeSpan.FromHours(17)
};

var result = SchedulerService.InitialOrchestrator(input);
```

---

### 3. Recurrencia Mensual

#### Ejecutar el Día 15 de Cada Mes
```csharp
var input = new SchedulerInput
{
    EnabledChk = true,
    Periodicity = EnumConfiguration.Recurrent,
    Recurrency = EnumRecurrency.Monthly,
    Language = "en_US",
    CurrentDate = DateTimeOffset.Now,
    StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
    EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero),
    MonthlyDayChk = true,
    MonthlyDay = 15,
    MonthlyDayPeriod = 1, // Cada mes
    OccursOnceChk = true,
    OccursOnceAt = TimeSpan.FromHours(10)
};

var result = SchedulerService.InitialOrchestator(input);
```

#### Ejecutar el Último Viernes de Cada Mes
```csharp
var input = new SchedulerInput
{
    EnabledChk = true,
    Periodicity = EnumConfiguration.Recurrent,
    Recurrency = EnumRecurrency.Monthly,
    Language = "en_GB",
    CurrentDate = DateTimeOffset.Now,
    StartDate = DateTimeOffset.Now.Date,
    EndDate = DateTimeOffset.Now.AddYears(1),
    MonthlyTheChk = true,
    MonthlyFrequency = EnumMonthlyFrequency.Last,
    MonthlyDateType = EnumMonthlyDateType.Friday,
    MonthlyThePeriod = 1,
    OccursOnceChk = true,
    OccursOnceAt = new TimeSpan(14, 0, 0)
};

var result = SchedulerService.InitialOrchestator(input);
```

#### Ejecutar el Primer Lunes de Cada Trimestre
```csharp
var input = new SchedulerInput
{
    EnabledChk = true,
    Periodicity = EnumConfiguration.Recurrent,
    Recurrency = EnumRecurrency.Monthly,
    Language = "es_ES",
    CurrentDate = DateTimeOffset.Now,
    StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
    EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero),
    MonthlyTheChk = true,
    MonthlyFrequency = EnumMonthlyFrequency.First,
    MonthlyDateType = EnumMonthlyDateType.Monday,
    MonthlyThePeriod = 3, // Cada 3 meses (trimestre)
    OccursOnceChk = true,
    OccursOnceAt = TimeSpan.FromHours(9)
};

var result = SchedulerService.InitialOrchestator(input);
```

---

### 4. Ejecución Única (One-Time)

```csharp
var targetDate = new DateTimeOffset(2025, 12, 25, 10, 0, 0, TimeSpan.Zero);

var input = new SchedulerInput
{
    EnabledChk = true,
    Periodicity = EnumConfiguration.Once,
    Recurrency = EnumRecurrency.Daily, // Requerido pero ignorado en modo Once
    Language = "en_US",
    CurrentDate = DateTimeOffset.Now,
    StartDate = DateTimeOffset.Now.Date,
    TargetDate = targetDate
};

var result = SchedulerService.InitialOrchestator(input);
```

---

## ?? Uso Avanzado

### Obtener Lista de Fechas Futuras

```csharp
using Scheduler_Lib.Core.Services;

var input = new SchedulerInput
{
    EnabledChk = true,
    Periodicity = EnumConfiguration.Recurrent,
    Recurrency = EnumRecurrency.Weekly,
    Language = "en_US",
    CurrentDate = DateTimeOffset.Now,
    StartDate = DateTimeOffset.Now.Date,
    EndDate = DateTimeOffset.Now.AddMonths(2),
    WeeklyPeriod = 1,
    DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday },
    OccursOnceChk = true,
    OccursOnceAt = TimeSpan.FromHours(10)
};

// Obtener próxima fecha
var nextDate = RecurrenceCalculator.GetNextExecutionDate(input, TimeZoneInfo.Local);

// Obtener todas las fechas futuras
var futureDates = RecurrenceCalculator.GetFutureDates(input);

Console.WriteLine($"Próxima: {nextDate}");
Console.WriteLine($"Total de fechas futuras: {futureDates.Count}");
foreach (var date in futureDates.Take(10))
{
    Console.WriteLine($"  - {date}");
}
```

---

### Uso con Inyección de Dependencias

```csharp
using Microsoft.Extensions.DependencyInjection;
using Scheduler_Lib.Core.Facades;
using Scheduler_Lib.Core.Factories;
using Scheduler_Lib.Core.Interfaces;
using Scheduler_Lib.Core.Repositories;
using Scheduler_Lib.Core.Validation;

// Configurar servicios
var services = new ServiceCollection();

// Registrar dependencias
services.AddSingleton<IRecurrenceStrategyFactory, RecurrenceStrategyFactory>();
services.AddSingleton<IValidator>(provider => ValidationChainFactory.CreateValidationChain());
services.AddScoped<ISchedulerRepository, SchedulerRepository>();
services.AddScoped<SchedulerServiceFacade>();

var serviceProvider = services.BuildServiceProvider();

// Usar el facade con DI
var facade = serviceProvider.GetRequiredService<SchedulerServiceFacade>();

var input = new SchedulerInput
{
    EnabledChk = true,
    Periodicity = EnumConfiguration.Recurrent,
    Recurrency = EnumRecurrency.Daily,
    Language = "en_US",
    CurrentDate = DateTimeOffset.Now,
    StartDate = DateTimeOffset.Now.Date,
    EndDate = DateTimeOffset.Now.AddMonths(1),
    OccursOnceChk = true,
    OccursOnceAt = TimeSpan.FromHours(14)
};

var result = facade.CalculateNextExecution(input);
```

---

## ?? Soporte Multi-idioma

### Idiomas Soportados

```csharp
// Español
input.Language = "es_ES";

// Inglés (US)
input.Language = "en_US";

// Inglés (UK)
input.Language = "en_GB";
```

### Ejemplo con Diferentes Idiomas

```csharp
var baseInput = new SchedulerInput
{
    EnabledChk = true,
    Periodicity = EnumConfiguration.Recurrent,
    Recurrency = EnumRecurrency.Weekly,
    CurrentDate = DateTimeOffset.Now,
    StartDate = DateTimeOffset.Now.Date,
    EndDate = DateTimeOffset.Now.AddMonths(1),
    WeeklyPeriod = 1,
    DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday },
    OccursOnceChk = true,
    OccursOnceAt = TimeSpan.FromHours(10)
};

// Español
baseInput.Language = "es_ES";
var resultES = SchedulerService.InitialOrchestator(baseInput);
Console.WriteLine($"Español: {resultES.Value.Description}");

// Inglés (US)
baseInput.Language = "en_US";
var resultEN = SchedulerService.InitialOrchestator(baseInput);
Console.WriteLine($"English (US): {resultEN.Value.Description}");

// Inglés (UK)
baseInput.Language = "en_GB";
var resultGB = SchedulerService.InitialOrchestator(baseInput);
Console.WriteLine($"English (UK): {resultGB.Value.Description}");
```

---

## ? Manejo de Zonas Horarias

```csharp
using Scheduler_Lib.Core.Services.Utilities;

// Usar zona horaria específica
var input = new SchedulerInput
{
    EnabledChk = true,
    Periodicity = EnumConfiguration.Recurrent,
    Recurrency = EnumRecurrency.Daily,
    Language = "en_US",
    CurrentDate = DateTimeOffset.Now,
    StartDate = DateTimeOffset.Now.Date,
    EndDate = DateTimeOffset.Now.AddMonths(1),
    TimeZoneId = "Pacific Standard Time", // PST
    OccursOnceChk = true,
    OccursOnceAt = TimeSpan.FromHours(14)
};

var result = SchedulerService.InitialOrchestator(input);

// Obtener zona horaria
var tz = TimeZoneConverter.GetTimeZone(input.TimeZoneId);
Console.WriteLine($"Zona horaria: {tz.DisplayName}");
```

### Horario de Verano (DST)

La biblioteca maneja automáticamente los cambios de horario de verano:

```csharp
var input = new SchedulerInput
{
    EnabledChk = true,
    Periodicity = EnumConfiguration.Recurrent,
    Recurrency = EnumRecurrency.Daily,
    Language = "en_US",
    // Periodo que cruza cambio de horario de verano
    CurrentDate = new DateTimeOffset(2025, 3, 1, 0, 0, 0, TimeSpan.Zero),
    StartDate = new DateTimeOffset(2025, 3, 1, 0, 0, 0, TimeSpan.Zero),
    EndDate = new DateTimeOffset(2025, 4, 1, 0, 0, 0, TimeSpan.Zero),
    TimeZoneId = "Central European Standard Time",
    OccursEveryChk = true,
    DailyPeriod = TimeSpan.FromMinutes(30),
    DailyStartTime = TimeSpan.FromHours(1),
    DailyEndTime = TimeSpan.FromHours(4)
};

// La biblioteca maneja automáticamente:
// - Horas ambiguas (fall back)
// - Horas inválidas (spring forward)
var futureDates = RecurrenceCalculator.GetFutureDates(input);
```

---

## ? Validación y Manejo de Errores

### Validación Automática

```csharp
var input = new SchedulerInput
{
    EnabledChk = true,
    Periodicity = EnumConfiguration.Recurrent,
    Recurrency = EnumRecurrency.Weekly,
    Language = "invalid_language", // ? Idioma inválido
    CurrentDate = DateTimeOffset.Now,
    StartDate = DateTimeOffset.Now.Date
    // ? Faltan campos requeridos
};

var result = SchedulerService.InitialOrchestator(input);

if (!result.IsSuccess)
{
    Console.WriteLine($"Errores de validación:");
    Console.WriteLine(result.Error);
}
```

### Validaciones Incluidas

La biblioteca valida automáticamente:

- ? Configuración habilitada (`EnabledChk`)
- ? Idioma soportado
- ? Fechas coherentes (StartDate < EndDate)
- ? CurrentDate dentro del rango
- ? Configuración específica por tipo de recurrencia:
  - **Weekly**: Period > 0, DaysOfWeek no vacío
  - **Monthly**: Configuración de día o frecuencia válida
  - **Daily**: Modo occurs configurado correctamente

---

## ?? Testing

### Test Básico

```csharp
using Xunit;
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;

public class SchedulerTests
{
    [Fact]
    public void DailyRecurrence_ShouldCalculateNextDate()
    {
        // Arrange
        var input = new SchedulerInput
        {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Daily,
            Language = "en_US",
            CurrentDate = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero),
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero),
            OccursOnceChk = true,
            OccursOnceAt = TimeSpan.FromHours(14)
        };

        // Act
        var result = SchedulerService.InitialOrchestator(input);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(14, result.Value.NextDate.Hour);
    }
}
```

### Test con Inyección de Dependencias

```csharp
using Moq;
using Xunit;
using Scheduler_Lib.Core.Interfaces;
using Scheduler_Lib.Core.Commands;

public class CommandTests
{
    [Fact]
    public void CalculateSchedulerCommand_ShouldExecuteSuccessfully()
    {
        // Arrange
        var mockConfig = new Mock<ISchedulerConfiguration>();
        var mockStrategy = new Mock<IRecurrenceCalculationStrategy>();
        var mockBuilder = new Mock<IDescriptionBuilder>();
        
        mockStrategy
            .Setup(s => s.CanHandle(It.IsAny<ISchedulerConfiguration>()))
            .Returns(true);
        
        mockStrategy
            .Setup(s => s.GetNextExecutionDate(It.IsAny<ISchedulerConfiguration>(), It.IsAny<TimeZoneInfo>()))
            .Returns(DateTimeOffset.Now.AddDays(1));
        
        mockBuilder
            .Setup(b => b.SetConfiguration(It.IsAny<ISchedulerConfiguration>()))
            .Returns(mockBuilder.Object);
        
        mockBuilder
            .Setup(b => b.SetTimeZone(It.IsAny<TimeZoneInfo>()))
            .Returns(mockBuilder.Object);
        
        mockBuilder
            .Setup(b => b.SetNextDate(It.IsAny<DateTimeOffset>()))
            .Returns(mockBuilder.Object);
        
        mockBuilder
            .Setup(b => b.SetLanguage(It.IsAny<string>()))
            .Returns(mockBuilder.Object);
        
        mockBuilder
            .Setup(b => b.Build())
            .Returns("Test description");

        var command = new CalculateSchedulerCommand(
            mockConfig.Object,
            mockStrategy.Object,
            mockBuilder.Object,
            TimeZoneInfo.Local);

        // Act
        var result = command.Execute();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test description", result.Description);
    }
}
```

---

## ?? Recursos Adicionales

- **Documentación de Arquitectura**: Ver `ARCHITECTURE.md`
- **Resumen de Refactorización**: Ver `REFACTORING_SUMMARY.md`
- **Código Fuente**: GitHub Repository

---

## ? Preguntas Frecuentes (FAQ)

### ¿Cómo agrego un nuevo tipo de recurrencia?

1. Crear una nueva estrategia que implemente `IRecurrenceCalculationStrategy`
2. Heredar de `RecurrenceCalculationStrategyBase`
3. Implementar `CanHandle()` y `CalculateDatesCore()`
4. La estrategia se registrará automáticamente en el Factory

### ¿Es thread-safe?

Sí, las estrategias y servicios son stateless y pueden usarse concurrentemente.

### ¿Cómo manejo múltiples zonas horarias?

Usa el campo `TimeZoneId` en `SchedulerInput` con IDs de zona horaria válidos de Windows.

### ¿Puedo extender los validadores?

Sí, crea una clase que herede de `BaseValidator` y agrégala a la cadena en `ValidationChainFactory`.

---

**Versión**: 2.0 (Refactorizada)  
**Última actualización**: Enero 2025  
**Soporte**: GitHub Issues
