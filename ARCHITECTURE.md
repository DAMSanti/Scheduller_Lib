# Scheduler Library - Refactored Architecture Documentation

## ?? Descripción General

Esta biblioteca ha sido completamente refactorizada siguiendo los principios SOLID y múltiples patrones de diseño arquitectónico para mejorar la mantenibilidad, extensibilidad y testabilidad del código.

---

## ?? Principios SOLID Implementados

### 1. **Single Responsibility Principle (SRP)**
Cada clase tiene una única responsabilidad:
- **Validators**: Solo validan configuraciones específicas
- **Strategies**: Solo calculan fechas para su tipo de recurrencia
- **Builders**: Solo construyen descripciones
- **Commands**: Solo ejecutan operaciones específicas

### 2. **Open/Closed Principle (OCP)**
El sistema está abierto a extensión pero cerrado a modificación:
- Nuevas estrategias de recurrencia pueden agregarse sin modificar código existente
- Nuevos validadores pueden añadirse a la cadena sin cambios en validadores existentes
- El Factory selecciona automáticamente la estrategia correcta

### 3. **Liskov Substitution Principle (LSP)**
Las interfaces permiten sustituir implementaciones:
- `IRecurrenceCalculationStrategy` puede tener múltiples implementaciones intercambiables
- `IValidator` permite cualquier implementación de validación
- `IDescriptionBuilder` soporta diferentes builders

### 4. **Interface Segregation Principle (ISP)**
Interfaces específicas y cohesivas:
- `ISchedulerConfiguration`: Solo propiedades de configuración
- `IRecurrenceCalculationStrategy`: Solo métodos de cálculo
- `IValidator`: Solo validación
- `IDescriptionBuilder`: Solo construcción de descripciones

### 5. **Dependency Inversion Principle (DIP)**
Las clases dependen de abstracciones, no de concreciones:
- Todos los componentes dependen de interfaces (`ISchedulerConfiguration`, `IRecurrenceCalculationStrategy`, etc.)
- Inyección de dependencias en constructores
- Uso del Adapter Pattern para convertir `SchedulerInput` a `ISchedulerConfiguration`

---

## ??? Patrones de Diseño Implementados

### 1. **Strategy Pattern** ?
**Ubicación**: `Core/Strategies/Recurrence/`
**Propósito**: Encapsular diferentes algoritmos de cálculo de recurrencia

**Implementación**:
```csharp
public interface IRecurrenceCalculationStrategy
{
    List<DateTimeOffset> CalculateFutureDates(ISchedulerConfiguration config, TimeZoneInfo tz);
    DateTimeOffset GetNextExecutionDate(ISchedulerConfiguration config, TimeZoneInfo tz);
    bool CanHandle(ISchedulerConfiguration config);
}
```

**Estrategias**:
- `DailyRecurrenceStrategy`: Cálculos diarios
- `WeeklyRecurrenceStrategy`: Cálculos semanales
- `MonthlyRecurrenceStrategy`: Cálculos mensuales
- `OneTimeExecutionStrategy`: Ejecuciones únicas

**Beneficios**:
- Fácil agregar nuevos tipos de recurrencia
- Cada estrategia es independiente y testeable
- Elimina switch/case statements

---

### 2. **Factory Pattern** ??
**Ubicación**: `Core/Factories/RecurrenceStrategyFactory.cs`
**Propósito**: Crear la estrategia de cálculo apropiada

**Implementación**:
```csharp
public class RecurrenceStrategyFactory : IRecurrenceStrategyFactory
{
    public IRecurrenceCalculationStrategy CreateStrategy(ISchedulerConfiguration configuration)
    {
        var strategy = _strategies.FirstOrDefault(s => s.CanHandle(configuration));
        return strategy ?? throw new InvalidOperationException(...);
    }
}
```

**Beneficios**:
- Encapsula la lógica de creación
- Desacopla el cliente de las implementaciones concretas
- Fácil extensión con nuevas estrategias

---

### 3. **Template Method Pattern** ??
**Ubicación**: `Core/Strategies/Base/RecurrenceCalculationStrategyBase.cs`
**Propósito**: Define el esqueleto del algoritmo de cálculo

**Implementación**:
```csharp
public abstract class RecurrenceCalculationStrategyBase
{
    public List<DateTimeOffset> CalculateFutureDates(...)
    {
        // Template method que define el flujo
        if (!ValidateConfiguration(config)) return dates;
        var baseDto = GetBaseDateTime(...);
        var endDate = GetEffectiveEndDate(...);
        CalculateDatesCore(...); // Hook method
        dates.Sort();
        return dates;
    }
    
    protected abstract void CalculateDatesCore(...); // Hook
    protected abstract DateTimeOffset GetEffectiveEndDate(...); // Hook
}
```

**Beneficios**:
- Reutilización de código común
- Flexibilidad en pasos específicos
- Mantiene la estructura consistente

---

### 4. **Chain of Responsibility Pattern** ??
**Ubicación**: `Core/Validation/`
**Propósito**: Cadena de validadores que procesan configuraciones

**Implementación**:
```csharp
public abstract class BaseValidator : IValidator
{
    private IValidator? _nextValidator;
    
    public virtual ResultPattern<bool> Validate(ISchedulerConfiguration config)
    {
        var result = DoValidate(config);
        if (!result.IsSuccess) return result;
        return _nextValidator?.Validate(config) ?? ResultPattern<bool>.Success(true);
    }
}
```

**Cadena de validadores**:
1. `BasicConfigurationValidator`
2. `PeriodicityValidator`
3. `DateRangeValidator`
4. `OneTimeExecutionValidator`
5. `WeeklyRecurrenceValidator`
6. `DailyRecurrenceValidator`
7. `MonthlyRecurrenceValidator`

**Beneficios**:
- Desacopla emisor de receptor
- Fácil agregar/remover validadores
- Cada validador tiene una responsabilidad única

---

### 5. **Builder Pattern** ??
**Ubicación**: `Core/Builders/SchedulerDescriptionBuilder.cs`
**Propósito**: Construir descripciones complejas paso a paso

**Implementación**:
```csharp
var description = _descriptionBuilder
    .SetConfiguration(configuration)
    .SetTimeZone(timeZone)
    .SetNextDate(nextDate)
    .SetLanguage(language)
    .Build();
```

**Beneficios**:
- API fluida y legible
- Separación de construcción de representación
- Fácil extensión con nuevos componentes

---

### 6. **Command Pattern** ??
**Ubicación**: `Core/Commands/CalculateSchedulerCommand.cs`
**Propósito**: Encapsular operaciones de cálculo como objetos

**Implementación**:
```csharp
public class CalculateSchedulerCommand : ISchedulerCommand<SchedulerOutput>
{
    public bool CanExecute() { ... }
    public SchedulerOutput Execute() { ... }
}
```

**Beneficios**:
- Desacopla invocación de ejecución
- Permite parametrizar operaciones
- Facilita undo/redo y logging (futuro)

---

### 7. **Repository Pattern** ??
**Ubicación**: `Core/Repositories/SchedulerRepository.cs`
**Propósito**: Abstraer la lógica de acceso y orquestación

**Implementación**:
```csharp
public class SchedulerRepository : ISchedulerRepository
{
    public ResultPattern<SchedulerOutput> ValidateAndCalculate(ISchedulerConfiguration config)
    {
        var validation = _validator.Validate(config);
        if (!validation.IsSuccess) return Failure(validation.Error);
        return Success(Calculate(config));
    }
}
```

**Beneficios**:
- Abstrae la complejidad de orquestación
- Facilita testing con mocks
- Centraliza la lógica de acceso

---

### 8. **Facade Pattern** ??
**Ubicación**: `Core/Facades/SchedulerServiceFacade.cs`
**Propósito**: Proporcionar interfaz simple al subsistema complejo

**Implementación**:
```csharp
public class SchedulerServiceFacade
{
    public ResultPattern<SchedulerOutput> CalculateNextExecution(SchedulerInput input)
    {
        var configuration = new SchedulerInputAdapter(input);
        return _repository.ValidateAndCalculate(configuration);
    }
}
```

**Beneficios**:
- Simplifica la API pública
- Oculta complejidad interna
- Punto único de entrada

---

### 9. **Adapter Pattern** ??
**Ubicación**: `Core/Adapters/SchedulerInputAdapter.cs`
**Propósito**: Adaptar `SchedulerInput` a `ISchedulerConfiguration`

**Implementación**:
```csharp
public class SchedulerInputAdapter : ISchedulerConfiguration
{
    private readonly SchedulerInput _schedulerInput;
    
    public DateTimeOffset CurrentDate => _schedulerInput.CurrentDate;
    // ... más propiedades
}
```

**Beneficios**:
- Permite trabajar con interfaces sin cambiar clases existentes
- Mantiene compatibilidad hacia atrás
- Facilita migración gradual

---

## ?? Arquitectura en Capas

```
???????????????????????????????????????????????????????
?              PUBLIC API LAYER                       ?
?  SchedulerService (Entry Point) ??? Facade Pattern  ?
???????????????????????????????????????????????????????
                       ?
                       ?
???????????????????????????????????????????????????????
?              FACADE LAYER                           ?
?  SchedulerServiceFacade ??? Simplifies Complexity   ?
???????????????????????????????????????????????????????
                       ?
                       ?
???????????????????????????????????????????????????????
?            ADAPTER LAYER                            ?
?  SchedulerInputAdapter ??? Adapter Pattern          ?
???????????????????????????????????????????????????????
                       ?
                       ?
???????????????????????????????????????????????????????
?           REPOSITORY LAYER                          ?
?  SchedulerRepository ??? Orchestration              ?
?    ??? Validation Chain                             ?
?    ??? Strategy Factory                             ?
?    ??? Command Execution                            ?
???????????????????????????????????????????????????????
                       ?
        ???????????????????????????????
        ?              ?              ?
??????????????? ??????????????? ???????????????
? VALIDATION  ? ?  STRATEGY   ? ?   BUILDER   ?
?   LAYER     ? ?    LAYER    ? ?    LAYER    ?
?             ? ?             ? ?             ?
? Chain of    ? ? Template    ? ?  Builder    ?
? Responsi-   ? ? Method +    ? ?  Pattern    ?
? bility      ? ? Strategy    ? ?             ?
??????????????? ??????????????? ???????????????
```

---

## ?? Flujo de Ejecución

### Cálculo de Próxima Ejecución:

```
1. Cliente llama: SchedulerService.InitialOrchestator(input)
   ?
   ?
2. Facade: SchedulerServiceFacade.CalculateNextExecution(input)
   ?
   ?
3. Adapter: Convierte SchedulerInput ? ISchedulerConfiguration
   ?
   ?
4. Repository: ValidateAndCalculate(configuration)
   ?
   ??? Validation Chain: Valida configuración
   ?   ??? BasicValidator ? PeriodicityValidator ? DateRangeValidator ? ...
   ?
   ??? Strategy Factory: Crea estrategia apropiada
   ?   ??? Selecciona: Daily/Weekly/Monthly/OneTime Strategy
   ?
   ??? Command: CalculateSchedulerCommand.Execute()
   ?   ??? Strategy: CalculateFutureDates / GetNextExecutionDate
   ?   ??? Builder: SchedulerDescriptionBuilder.Build()
   ?
   ?
5. Retorna: ResultPattern<SchedulerOutput>
```

---

## ?? Beneficios para Testing

### Testabilidad Mejorada:
```csharp
// Cada componente puede testearse aisladamente
[Test]
public void DailyStrategy_ShouldCalculateCorrectly()
{
    var mockConfig = new Mock<ISchedulerConfiguration>();
    var strategy = new DailyRecurrenceStrategy();
    
    var result = strategy.CalculateFutureDates(mockConfig.Object, tz);
    
    Assert.NotNull(result);
}

// Validadores pueden testearse independientemente
[Test]
public void BasicValidator_ShouldFailWithNullLanguage()
{
    var validator = new BasicConfigurationValidator();
    var mockConfig = CreateMockWithNullLanguage();
    
    var result = validator.Validate(mockConfig);
    
    Assert.False(result.IsSuccess);
}
```

---

## ?? Extensibilidad

### Agregar Nueva Estrategia de Recurrencia:

```csharp
// 1. Crear nueva estrategia
public class YearlyRecurrenceStrategy : RecurrenceCalculationStrategyBase
{
    public override bool CanHandle(ISchedulerConfiguration config)
    {
        return config.Recurrency == EnumRecurrency.Yearly; // Nuevo enum
    }
    
    protected override void CalculateDatesCore(...) 
    {
        // Implementación específica
    }
}

// 2. Registrar en factory (automático por reflexión o manual)
_strategies.Add(new YearlyRecurrenceStrategy());

// ¡Listo! Sin modificar código existente
```

### Agregar Nuevo Validador:

```csharp
// 1. Crear validador
public class CustomBusinessRuleValidator : BaseValidator
{
    protected override ResultPattern<bool> DoValidate(ISchedulerConfiguration config)
    {
        // Lógica de validación personalizada
    }
}

// 2. Agregar a la cadena
customValidator.SetNext(existingValidator);
```

---

## ?? Convenciones de Código

### Nomenclatura:
- **Interfaces**: `I{Name}` (e.g., `ISchedulerConfiguration`)
- **Implementaciones base**: `{Name}Base` (e.g., `BaseValidator`)
- **Estrategias**: `{Type}Strategy` (e.g., `DailyRecurrenceStrategy`)
- **Factories**: `{Type}Factory` (e.g., `RecurrenceStrategyFactory`)

### Organización de Carpetas:
```
Core/
??? Adapters/          # Adapter Pattern
??? Builders/          # Builder Pattern
??? Commands/          # Command Pattern
??? Facades/           # Facade Pattern
??? Factories/         # Factory Pattern
??? Interfaces/        # Contratos (SOLID - DIP)
??? Repositories/      # Repository Pattern
??? Strategies/        # Strategy + Template Method
?   ??? Base/
?   ??? Recurrence/
??? Validation/        # Chain of Responsibility
    ??? Base/
    ??? Validators/
```

---

## ?? Compatibilidad Hacia Atrás

El código existente **sigue funcionando** gracias a:

1. **SchedulerService.InitialOrchestator()**: Mantiene la misma firma pública
2. **RecurrenceCalculator**: Delegado a `FutureDatesService`
3. **SchedulerInput**: Aún se usa externamente, convertido internamente por el Adapter

```csharp
// Código antiguo sigue funcionando
var result = SchedulerService.InitialOrchestator(schedulerInput);
var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
```

---

## ?? Métricas de Calidad

### Antes de la Refactorización:
- ? Acoplamiento: Alto
- ? Cohesión: Baja
- ? Testabilidad: Difícil
- ? Extensibilidad: Limitada
- ? Mantenibilidad: Compleja

### Después de la Refactorización:
- ? Acoplamiento: Bajo (interfaces)
- ? Cohesión: Alta (SRP)
- ? Testabilidad: Excelente (mocking fácil)
- ? Extensibilidad: Alta (OCP)
- ? Mantenibilidad: Simplificada

---

## ?? Aprendizajes Clave

1. **SOLID no es opcional**: Facilita todo el desarrollo futuro
2. **Los patrones resuelven problemas reales**: No usar patrones por usar
3. **La refactorización incremental funciona**: Mantener compatibilidad durante migración
4. **Las interfaces son poderosas**: Permiten testing y extensibilidad
5. **El código limpio se paga solo**: Menos bugs, más velocidad de desarrollo

---

## ?? Futuras Mejoras

1. **Logging con Observer Pattern**: Notificar eventos de cálculo
2. **Caching con Decorator**: Cachear resultados frecuentes
3. **Composite Pattern**: Para recurrencias combinadas
4. **Memento Pattern**: Para deshacer/rehacer configuraciones
5. **Unit of Work**: Para operaciones transaccionales múltiples

---

## ?? Referencias

- **SOLID Principles**: Robert C. Martin
- **Design Patterns**: Gang of Four (GoF)
- **Clean Architecture**: Robert C. Martin
- **Domain-Driven Design**: Eric Evans

---

**Desarrollado por**: Santiago Taboada  
**Fecha**: 2025  
**Versión**: 2.0 (Refactorizada)
