# ?? Diagramas de Arquitectura - Scheduler_Lib

## ??? Diagrama de Clases Principal

```mermaid
classDiagram
    %% Interfaces principales
    class ISchedulerConfiguration {
        <<interface>>
        +DateTimeOffset CurrentDate
        +EnumRecurrency Recurrency
        +EnumConfiguration Periodicity
        +string Language
        +DateTimeOffset StartDate
        +DateTimeOffset? EndDate
    }

    class IRecurrenceCalculationStrategy {
        <<interface>>
        +CalculateFutureDates(config, tz) List~DateTimeOffset~
        +GetNextExecutionDate(config, tz) DateTimeOffset
        +CanHandle(config) bool
    }

    class IValidator {
        <<interface>>
        +Validate(config) ResultPattern~bool~
        +SetNext(validator) void
    }

    class IDescriptionBuilder {
        <<interface>>
        +SetConfiguration(config) IDescriptionBuilder
        +SetTimeZone(tz) IDescriptionBuilder
        +SetNextDate(date) IDescriptionBuilder
        +Build() string
    }

    class ISchedulerRepository {
        <<interface>>
        +ValidateAndCalculate(config) ResultPattern~SchedulerOutput~
        +Calculate(config) SchedulerOutput
    }

    %% Implementaciones principales
    class SchedulerService {
        -SchedulerServiceFacade _facade$
        +InitialOrchestator(input)$ ResultPattern~SchedulerOutput~
    }

    class SchedulerServiceFacade {
        -ISchedulerRepository _repository
        +CalculateNextExecution(input) ResultPattern~SchedulerOutput~
    }

    class SchedulerRepository {
        -IRecurrenceStrategyFactory _strategyFactory
        -IValidator _validator
        +ValidateAndCalculate(config) ResultPattern~SchedulerOutput~
        +Calculate(config) SchedulerOutput
    }

    class SchedulerInputAdapter {
        -SchedulerInput _schedulerInput
        +DateTimeOffset CurrentDate
        +EnumRecurrency Recurrency
    }

    %% Estrategias
    class RecurrenceCalculationStrategyBase {
        <<abstract>>
        +CalculateFutureDates(config, tz) List~DateTimeOffset~
        +GetNextExecutionDate(config, tz) DateTimeOffset
        #CalculateDatesCore(config, tz, baseDto, endDate, accumulator)*
        #GetEffectiveEndDate(config, tz)* DateTimeOffset
    }

    class DailyRecurrenceStrategy {
        +CanHandle(config) bool
        #CalculateDatesCore(...) void
        #GetEffectiveEndDate(...) DateTimeOffset
    }

    class WeeklyRecurrenceStrategy {
        +CanHandle(config) bool
        #CalculateDatesCore(...) void
        #GetEffectiveEndDate(...) DateTimeOffset
    }

    class MonthlyRecurrenceStrategy {
        +CanHandle(config) bool
        #CalculateDatesCore(...) void
        #GetEffectiveEndDate(...) DateTimeOffset
    }

    class OneTimeExecutionStrategy {
        +CanHandle(config) bool
        #CalculateDatesCore(...) void
        #GetEffectiveEndDate(...) DateTimeOffset
    }

    %% Validadores
    class BaseValidator {
        <<abstract>>
        -IValidator _nextValidator
        +Validate(config) ResultPattern~bool~
        +SetNext(validator) void
        #DoValidate(config)* ResultPattern~bool~
    }

    class BasicConfigurationValidator {
        #DoValidate(config) ResultPattern~bool~
    }

    class WeeklyRecurrenceValidator {
        #DoValidate(config) ResultPattern~bool~
    }

    %% Factory y Builder
    class RecurrenceStrategyFactory {
        -List~IRecurrenceCalculationStrategy~ _strategies
        +CreateStrategy(config) IRecurrenceCalculationStrategy
    }

    class SchedulerDescriptionBuilder {
        -ISchedulerConfiguration _configuration
        -TimeZoneInfo _timeZone
        -DateTimeOffset _nextDate
        -string _language
        +SetConfiguration(config) IDescriptionBuilder
        +Build() string
    }

    %% Command
    class CalculateSchedulerCommand {
        -ISchedulerConfiguration _configuration
        -IRecurrenceCalculationStrategy _strategy
        -IDescriptionBuilder _descriptionBuilder
        -TimeZoneInfo _timeZone
        +Execute() SchedulerOutput
        +CanExecute() bool
    }

    %% Modelos
    class SchedulerInput {
        +DateTimeOffset CurrentDate
        +EnumRecurrency Recurrency
        +string Language
    }

    class SchedulerOutput {
        +DateTimeOffset NextDate
        +string Description
    }

    class ResultPattern~T~ {
        +bool IsSuccess
        +T Value
        +string Error
        +Success(value)$ ResultPattern~T~
        +Failure(error)$ ResultPattern~T~
    }

    %% Relaciones
    SchedulerService --> SchedulerServiceFacade : usa
    SchedulerServiceFacade --> SchedulerInputAdapter : crea
    SchedulerServiceFacade --> ISchedulerRepository : usa
    SchedulerRepository ..|> ISchedulerRepository : implementa
    SchedulerRepository --> IValidator : usa
    SchedulerRepository --> RecurrenceStrategyFactory : usa
    SchedulerRepository --> CalculateSchedulerCommand : crea

    SchedulerInputAdapter ..|> ISchedulerConfiguration : implementa
    SchedulerInputAdapter --> SchedulerInput : adapta

    RecurrenceCalculationStrategyBase ..|> IRecurrenceCalculationStrategy : implementa
    DailyRecurrenceStrategy --|> RecurrenceCalculationStrategyBase : hereda
    WeeklyRecurrenceStrategy --|> RecurrenceCalculationStrategyBase : hereda
    MonthlyRecurrenceStrategy --|> RecurrenceCalculationStrategyBase : hereda
    OneTimeExecutionStrategy --|> RecurrenceCalculationStrategyBase : hereda

    RecurrenceStrategyFactory --> IRecurrenceCalculationStrategy : crea

    BaseValidator ..|> IValidator : implementa
    BasicConfigurationValidator --|> BaseValidator : hereda
    WeeklyRecurrenceValidator --|> BaseValidator : hereda

    SchedulerDescriptionBuilder ..|> IDescriptionBuilder : implementa

    CalculateSchedulerCommand --> ISchedulerConfiguration : usa
    CalculateSchedulerCommand --> IRecurrenceCalculationStrategy : usa
    CalculateSchedulerCommand --> IDescriptionBuilder : usa
    CalculateSchedulerCommand --> SchedulerOutput : crea
```

---

## ?? Diagrama de Secuencia - Flujo Principal

```mermaid
sequenceDiagram
    actor Usuario
    participant Service as SchedulerService
    participant Facade as SchedulerServiceFacade
    participant Adapter as SchedulerInputAdapter
    participant Repo as SchedulerRepository
    participant ValChain as ValidationChain
    participant Factory as StrategyFactory
    participant Strategy as RecurrenceStrategy
    participant Command as CalculateCommand
    participant Builder as DescriptionBuilder

    Usuario->>Service: InitialOrchestator(input)
    Service->>Facade: CalculateNextExecution(input)
    Facade->>Adapter: new SchedulerInputAdapter(input)
    Adapter-->>Facade: ISchedulerConfiguration
    
    Facade->>Repo: ValidateAndCalculate(config)
    
    Repo->>ValChain: Validate(config)
    ValChain->>ValChain: BasicValidator
    ValChain->>ValChain: PeriodicityValidator
    ValChain->>ValChain: DateRangeValidator
    ValChain->>ValChain: RecurrenceValidator
    ValChain-->>Repo: ResultPattern<bool>
    
    alt Validación falla
        Repo-->>Facade: ResultPattern<Output>.Failure(error)
        Facade-->>Service: error
        Service-->>Usuario: error
    else Validación exitosa
        Repo->>Factory: CreateStrategy(config)
        Factory-->>Repo: IRecurrenceStrategy
        
        Repo->>Command: new CalculateSchedulerCommand(...)
        Repo->>Command: Execute()
        
        Command->>Strategy: GetNextExecutionDate(config, tz)
        Strategy-->>Command: DateTimeOffset
        
        Command->>Builder: SetConfiguration(config)
        Command->>Builder: SetTimeZone(tz)
        Command->>Builder: SetNextDate(nextDate)
        Command->>Builder: Build()
        Builder-->>Command: description
        
        Command-->>Repo: SchedulerOutput
        Repo-->>Facade: ResultPattern<Output>.Success(output)
        Facade-->>Service: output
        Service-->>Usuario: output
    end
```

---

## ??? Diagrama de Componentes

```mermaid
graph TB
    subgraph "Capa de Presentación"
        API[Public API<br/>SchedulerService]
    end

    subgraph "Capa de Facade"
        Facade[SchedulerServiceFacade<br/>Facade Pattern]
    end

    subgraph "Capa de Adaptación"
        Adapter[SchedulerInputAdapter<br/>Adapter Pattern]
    end

    subgraph "Capa de Repositorio"
        Repo[SchedulerRepository<br/>Repository Pattern]
    end

    subgraph "Capa de Validación"
        ValFactory[ValidationChainFactory]
        Val1[BasicValidator]
        Val2[PeriodicityValidator]
        Val3[DateRangeValidator]
        Val4[RecurrenceValidators]
        
        ValFactory --> Val1
        Val1 --> Val2
        Val2 --> Val3
        Val3 --> Val4
    end

    subgraph "Capa de Estrategias"
        StratFactory[RecurrenceStrategyFactory<br/>Factory Pattern]
        Daily[DailyStrategy]
        Weekly[WeeklyStrategy]
        Monthly[MonthlyStrategy]
        OneTime[OneTimeStrategy]
        
        StratFactory --> Daily
        StratFactory --> Weekly
        StratFactory --> Monthly
        StratFactory --> OneTime
    end

    subgraph "Capa de Comandos"
        Cmd[CalculateSchedulerCommand<br/>Command Pattern]
    end

    subgraph "Capa de Construcción"
        Builder[SchedulerDescriptionBuilder<br/>Builder Pattern]
    end

    subgraph "Capa de Modelo"
        Input[SchedulerInput]
        Output[SchedulerOutput]
        Config[ISchedulerConfiguration]
    end

    API --> Facade
    Facade --> Adapter
    Adapter --> Config
    Facade --> Repo
    Repo --> ValFactory
    Repo --> StratFactory
    Repo --> Cmd
    Cmd --> Builder
    Cmd --> Output
    Input --> Adapter
    
    style API fill:#e1f5ff
    style Facade fill:#fff3cd
    style Repo fill:#d4edda
    style StratFactory fill:#f8d7da
    style Cmd fill:#d1ecf1
```

---

## ?? Diagrama de Patrones de Diseño

```mermaid
mindmap
  root((Scheduler_Lib<br/>Design Patterns))
    Creational
      Factory Pattern
        RecurrenceStrategyFactory
        ValidationChainFactory
      Builder Pattern
        SchedulerDescriptionBuilder
    Structural
      Adapter Pattern
        SchedulerInputAdapter
      Facade Pattern
        SchedulerServiceFacade
    Behavioral
      Strategy Pattern
        DailyRecurrenceStrategy
        WeeklyRecurrenceStrategy
        MonthlyRecurrenceStrategy
        OneTimeExecutionStrategy
      Template Method
        RecurrenceCalculationStrategyBase
      Chain of Responsibility
        BaseValidator
        BasicConfigurationValidator
        RecurrenceValidators
      Command Pattern
        CalculateSchedulerCommand
    Architectural
      Repository Pattern
        SchedulerRepository
      Dependency Inversion
        All Interfaces
```

---

## ?? Diagrama de Estado - Validación

```mermaid
stateDiagram-v2
    [*] --> BasicValidation: Iniciar validación
    
    BasicValidation --> PeriodicityValidation: ? Básica OK
    BasicValidation --> Failed: ? Error básico
    
    PeriodicityValidation --> DateRangeValidation: ? Periodicidad OK
    PeriodicityValidation --> Failed: ? Error periodicidad
    
    DateRangeValidation --> OneTimeValidation: ? Rango OK
    DateRangeValidation --> Failed: ? Error rango
    
    OneTimeValidation --> RecurrenceValidation: ? OneTime OK
    OneTimeValidation --> Failed: ? Error OneTime
    
    RecurrenceValidation --> Success: ? Todo OK
    RecurrenceValidation --> Failed: ? Error recurrencia
    
    Success --> [*]: Continuar cálculo
    Failed --> [*]: Retornar error
```

---

## ?? Diagrama de Flujo - Selección de Estrategia

```mermaid
flowchart TD
    Start([Inicio]) --> CheckPeriodicity{Periodicity?}
    
    CheckPeriodicity -->|Once| OneTimeStrategy[OneTimeExecutionStrategy]
    CheckPeriodicity -->|Recurrent| CheckRecurrency{Recurrency?}
    
    CheckRecurrency -->|Daily| DailyStrategy[DailyRecurrenceStrategy]
    CheckRecurrency -->|Weekly| WeeklyStrategy[WeeklyRecurrenceStrategy]
    CheckRecurrency -->|Monthly| MonthlyStrategy[MonthlyRecurrenceStrategy]
    
    OneTimeStrategy --> Execute[Execute Strategy]
    DailyStrategy --> Execute
    WeeklyStrategy --> Execute
    MonthlyStrategy --> Execute
    
    Execute --> CalcDates[CalculateFutureDates]
    CalcDates --> GetNext[GetNextExecutionDate]
    GetNext --> BuildDesc[Build Description]
    BuildDesc --> End([Return SchedulerOutput])
    
    style OneTimeStrategy fill:#e3f2fd
    style DailyStrategy fill:#f3e5f5
    style WeeklyStrategy fill:#e8f5e9
    style MonthlyStrategy fill:#fff3e0
```

---

## ??? Diagrama de Capas

```mermaid
graph TD
    subgraph "?? Presentation Layer"
        PL[SchedulerService<br/>Public Entry Point]
    end

    subgraph "?? Facade Layer"
        FL[SchedulerServiceFacade<br/>Simplifies Complexity]
    end

    subgraph "?? Adapter Layer"
        AL[SchedulerInputAdapter<br/>Legacy Compatibility]
    end

    subgraph "?? Business Layer"
        BL1[Repository<br/>Orchestration]
        BL2[Validation Chain<br/>Business Rules]
        BL3[Strategies<br/>Calculation Logic]
        BL4[Commands<br/>Operations]
        BL5[Builders<br/>Description Generation]
    end

    subgraph "?? Domain Layer"
        DL1[Interfaces<br/>Contracts]
        DL2[Models<br/>Domain Objects]
        DL3[Enums<br/>Value Objects]
    end

    subgraph "??? Infrastructure Layer"
        IL1[Utilities<br/>Helpers]
        IL2[Resources<br/>Localization]
        IL3[Legacy Code<br/>Backward Compatibility]
    end

    PL --> FL
    FL --> AL
    AL --> BL1
    BL1 --> BL2
    BL1 --> BL3
    BL1 --> BL4
    BL4 --> BL5
    
    BL1 --> DL1
    BL2 --> DL1
    BL3 --> DL1
    BL4 --> DL1
    BL5 --> DL1
    
    DL1 --> DL2
    DL2 --> DL3
    
    BL3 --> IL1
    BL5 --> IL2
    AL --> IL3
```

---

## ?? Diagrama SOLID

```mermaid
graph LR
    subgraph "Single Responsibility"
        SRP1[BasicValidator:<br/>Solo validación básica]
        SRP2[DailyStrategy:<br/>Solo cálculo diario]
        SRP3[Builder:<br/>Solo descripciones]
    end

    subgraph "Open/Closed"
        OCP1[IRecurrenceStrategy:<br/>Extensible]
        OCP2[Nueva Strategy:<br/>Sin modificar existentes]
    end

    subgraph "Liskov Substitution"
        LSP1[IValidator]
        LSP2[BaseValidator]
        LSP3[Cualquier Validator]
        LSP1 --> LSP2
        LSP2 --> LSP3
    end

    subgraph "Interface Segregation"
        ISP1[IValidator:<br/>Solo validación]
        ISP2[IStrategy:<br/>Solo cálculo]
        ISP3[IBuilder:<br/>Solo construcción]
    end

    subgraph "Dependency Inversion"
        DIP1[SchedulerRepository]
        DIP2[IValidator]
        DIP3[IStrategy]
        DIP1 -.depende de.-> DIP2
        DIP1 -.depende de.-> DIP3
    end

    style SRP1 fill:#e3f2fd
    style SRP2 fill:#e3f2fd
    style SRP3 fill:#e3f2fd
    style OCP1 fill:#f3e5f5
    style OCP2 fill:#f3e5f5
    style LSP1 fill:#e8f5e9
    style LSP2 fill:#e8f5e9
    style LSP3 fill:#e8f5e9
    style ISP1 fill:#fff3e0
    style ISP2 fill:#fff3e0
    style ISP3 fill:#fff3e0
    style DIP1 fill:#fce4ec
    style DIP2 fill:#fce4ec
    style DIP3 fill:#fce4ec
```

---

## ?? Diagrama de Despliegue

```mermaid
C4Deployment
    title Deployment Diagram - Scheduler_Lib

    Deployment_Node(client, "Client Application", ".NET 8 App"){
        Container(app, "Application", "Uses Scheduler_Lib")
    }

    Deployment_Node(lib, "Scheduler_Lib", "NuGet Package"){
        Container(facade, "Facade Layer", "Entry Point")
        Container(business, "Business Layer", "Core Logic")
        Container(domain, "Domain Layer", "Models & Interfaces")
    }

    Deployment_Node(resources, "Resources", "External"){
        ContainerDb(localization, "Localization", "Language Files")
        ContainerDb(config, "Configuration", "Settings")
    }

    Rel(app, facade, "Uses")
    Rel(facade, business, "Delegates to")
    Rel(business, domain, "Uses")
    Rel(business, localization, "Reads")
    Rel(business, config, "Reads")
```

---

**Generado automáticamente por la documentación de Scheduler_Lib**  
**Versión**: 2.0 - Refactorizada  
**Fecha**: Enero 2025
