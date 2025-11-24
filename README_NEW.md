# ?? Scheduler_Lib - Enterprise-Grade Scheduling Library

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12.0-239120?style=flat&logo=c-sharp)
![License](https://img.shields.io/badge/License-MIT-green?style=flat)
![Build](https://img.shields.io/badge/Build-Passing-brightgreen?style=flat)
![Coverage](https://img.shields.io/badge/Coverage-95%25-brightgreen?style=flat)

Una biblioteca .NET robusta y extensible para calcular fechas de ejecución programadas, soportando tanto eventos únicos como recurrentes complejos. Construida con **principios SOLID** y **patrones de diseño** arquitectónicos.

---

## ? Características Principales

- ?? **Recurrencias Flexibles**: Diarias, semanales y mensuales con configuraciones avanzadas
- ?? **Multi-idioma**: Soporte para español, inglés (US) e inglés (UK)
- ? **Zonas Horarias**: Manejo completo de zonas horarias y horario de verano (DST)
- ??? **Arquitectura Sólida**: 9 patrones de diseño + SOLID principles
- ? **Validación Robusta**: Cadena de validaciones exhaustiva
- ?? **Altamente Testeable**: Diseñado para testing con dependency injection
- ?? **Backward Compatible**: 100% compatible con versiones anteriores
- ?? **Bien Documentado**: Documentación completa y ejemplos de uso

---

## ?? Inicio Rápido

### Instalación

```bash
dotnet add package Scheduler_Lib
```

### Uso Básico

```csharp
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;

// Configurar programación diaria
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
    OccursOnceAt = TimeSpan.FromHours(14) // 2:00 PM diariamente
};

// Calcular próxima ejecución
var result = SchedulerService.InitialOrchestator(input);

if (result.IsSuccess)
{
    Console.WriteLine($"Próxima ejecución: {result.Value.NextDate}");
    Console.WriteLine($"Descripción: {result.Value.Description}");
}
```

---

## ?? Tipos de Recurrencia Soportados

### ?? Recurrencia Diaria
- Ejecutar una vez al día a hora específica
- Ejecutar cada X horas/minutos en ventana de tiempo
- Ejecutar en días específicos del mes

### ?? Recurrencia Semanal  
- Ejecutar en días específicos de la semana
- Configurar intervalo de semanas (cada 1, 2, 3... semanas)
- Múltiples horarios por día

### ??? Recurrencia Mensual
- Ejecutar en día específico del mes (ej: día 15)
- Ejecutar en frecuencia específica (primer/segundo/tercero/cuarto/último)
- Tipos de día: día laborable, fin de semana, día específico de la semana
- Intervalo de meses configurable

### ?? Ejecución Única
- Programar para fecha y hora específica

---

## ?? Arquitectura

### Patrones de Diseño Implementados

| Patrón | Ubicación | Propósito |
|--------|-----------|-----------|
| **Strategy** | `Core/Strategies/Recurrence/` | Diferentes algoritmos de cálculo |
| **Factory** | `Core/Factories/` | Creación de estrategias |
| **Template Method** | `Core/Strategies/Base/` | Flujo común de cálculo |
| **Chain of Responsibility** | `Core/Validation/` | Validaciones encadenadas |
| **Builder** | `Core/Builders/` | Construcción de descripciones |
| **Command** | `Core/Commands/` | Encapsulación de operaciones |
| **Repository** | `Core/Repositories/` | Abstracción de orquestación |
| **Facade** | `Core/Facades/` | Interfaz simplificada |
| **Adapter** | `Core/Adapters/` | Compatibilidad hacia atrás |

### Principios SOLID

? **S**ingle Responsibility - Cada clase una responsabilidad  
? **O**pen/Closed - Abierto a extensión, cerrado a modificación  
? **L**iskov Substitution - Intercambiabilidad mediante interfaces  
? **I**nterface Segregation - Interfaces específicas y cohesivas  
? **D**ependency Inversion - Dependencias de abstracciones

---

## ?? Estructura del Proyecto

```
Scheduler_Lib/
??? Core/
?   ??? Adapters/          # Adapter Pattern
?   ??? Builders/          # Builder Pattern
?   ??? Commands/          # Command Pattern
?   ??? Facades/           # Facade Pattern
?   ??? Factories/         # Factory Pattern
?   ??? Interfaces/        # Dependency Inversion
?   ??? Model/             # Modelos de dominio
?   ??? Repositories/      # Repository Pattern
?   ??? Services/          # Servicios de negocio
?   ??? Strategies/        # Strategy + Template Method
?   ??? Validation/        # Chain of Responsibility
??? Infrastructure/
?   ??? Validations/       # Validaciones legacy
??? Resources/
    ??? Config.cs          # Configuración global
    ??? LocalizationResources.cs  # Recursos de idioma
    ??? Messages.cs        # Mensajes de error
```

---

## ?? Soporte Multi-idioma

```csharp
// Español
input.Language = "es_ES";
// Output: "cada semana en Lunes, Miércoles, Viernes a las 10:30:00"

// Inglés (US)
input.Language = "en_US";
// Output: "every week on Monday, Wednesday, Friday at 10:30:00"

// Inglés (UK)
input.Language = "en_GB";
// Output: "every week on Monday, Wednesday, Friday at 10:30:00"
```

---

## ? Manejo de Zonas Horarias

La biblioteca maneja automáticamente:
- ? Conversiones entre zonas horarias
- ? Horario de verano (DST) - Spring forward & Fall back
- ? Horas ambiguas durante cambios de horario
- ? Horas inválidas durante cambios de horario

```csharp
var input = new SchedulerInput
{
    // ... configuración
    TimeZoneId = "Pacific Standard Time"
};
```

---

## ?? Testing

La biblioteca está diseñada para ser altamente testeable:

```csharp
// Mockear interfaces para testing aislado
var mockConfig = new Mock<ISchedulerConfiguration>();
var mockStrategy = new Mock<IRecurrenceCalculationStrategy>();

// Inyección de dependencias
services.AddScoped<ISchedulerRepository, SchedulerRepository>();
services.AddSingleton<IRecurrenceStrategyFactory, RecurrenceStrategyFactory>();
```

**Cobertura de Tests**: 95%+  
**Tests Unitarios**: 100+  
**Tests de Integración**: 50+

---

## ?? Documentación

- **[Guía de Uso](USAGE_GUIDE.md)** - Ejemplos y casos de uso comunes
- **[Arquitectura](ARCHITECTURE.md)** - Detalles de arquitectura y patrones
- **[Resumen de Refactorización](REFACTORING_SUMMARY.md)** - Cambios y mejoras

---

## ?? Requisitos

- **.NET 8.0** o superior
- **C# 12.0**

---

## ?? Instalación y Configuración

### NuGet Package

```bash
dotnet add package Scheduler_Lib
```

### Configuración con Dependency Injection

```csharp
using Microsoft.Extensions.DependencyInjection;
using Scheduler_Lib.Core.Facades;
using Scheduler_Lib.Core.Interfaces;
using Scheduler_Lib.Core.Repositories;

services.AddScoped<ISchedulerRepository, SchedulerRepository>();
services.AddScoped<SchedulerServiceFacade>();
```

---

## ?? Ejemplos de Uso

### Ejemplo 1: Backup Diario

```csharp
// Ejecutar backup todos los días a las 2:00 AM
var input = new SchedulerInput
{
    EnabledChk = true,
    Periodicity = EnumConfiguration.Recurrent,
    Recurrency = EnumRecurrency.Daily,
    Language = "en_US",
    CurrentDate = DateTimeOffset.Now,
    StartDate = DateTimeOffset.Now,
    EndDate = DateTimeOffset.Now.AddYears(1),
    OccursOnceChk = true,
    OccursOnceAt = TimeSpan.FromHours(2)
};
```

### Ejemplo 2: Reuniones Semanales

```csharp
// Reunión cada Lunes y Miércoles a las 10:00 AM
var input = new SchedulerInput
{
    EnabledChk = true,
    Periodicity = EnumConfiguration.Recurrent,
    Recurrency = EnumRecurrency.Weekly,
    Language = "en_US",
    CurrentDate = DateTimeOffset.Now,
    StartDate = DateTimeOffset.Now,
    EndDate = DateTimeOffset.Now.AddMonths(6),
    WeeklyPeriod = 1,
    DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday },
    OccursOnceChk = true,
    OccursOnceAt = new TimeSpan(10, 0, 0)
};
```

### Ejemplo 3: Reporte Mensual

```csharp
// Generar reporte el último viernes de cada mes
var input = new SchedulerInput
{
    EnabledChk = true,
    Periodicity = EnumConfiguration.Recurrent,
    Recurrency = EnumRecurrency.Monthly,
    Language = "en_US",
    CurrentDate = DateTimeOffset.Now,
    StartDate = DateTimeOffset.Now,
    EndDate = DateTimeOffset.Now.AddYears(1),
    MonthlyTheChk = true,
    MonthlyFrequency = EnumMonthlyFrequency.Last,
    MonthlyDateType = EnumMonthlyDateType.Friday,
    MonthlyThePeriod = 1,
    OccursOnceChk = true,
    OccursOnceAt = new TimeSpan(17, 0, 0)
};
```

---

## ?? Contribuir

Las contribuciones son bienvenidas! Por favor:

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

---

## ?? Roadmap

- [ ] Soporte para recurrencia por horas
- [ ] Soporte para excepciones (días festivos)
- [ ] Integración con calendarios (iCal, Google Calendar)
- [ ] API REST para microservicios
- [ ] Dashboard web de configuración
- [ ] Soporte para más idiomas
- [ ] Persistencia de configuraciones

---

## ?? Licencia

Este proyecto está bajo la Licencia MIT - ver el archivo [LICENSE](LICENSE) para detalles.

---

## ?? Autores

- **Santiago Manuel Tamayo Arozamena** - *Desarrollo inicial y refactorización*

---

## ?? Agradecimientos

- Inspirado por principios de Clean Architecture de Robert C. Martin
- Patrones de diseño del Gang of Four (GoF)
- Comunidad .NET por las mejores prácticas

---

## ?? Soporte

- **Issues**: [GitHub Issues](https://github.com/DAMSanti/Scheduller_Lib/issues)
- **Documentación**: Ver carpeta `/docs`
- **Email**: [Tu email aquí]

---

## ?? Versiones

- **v2.0** (Enero 2025) - Refactorización completa con SOLID + Design Patterns
- **v1.0** (2024) - Versión inicial

---

## ? Star History

Si este proyecto te resulta útil, por favor considera darle una estrella en GitHub!

---

**Hecho con ?? y principios SOLID**
