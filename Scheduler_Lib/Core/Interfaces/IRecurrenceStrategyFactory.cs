namespace Scheduler_Lib.Core.Interfaces;

/// <summary>
/// Factory for creating recurrence calculation strategies (Factory Pattern)
/// </summary>
public interface IRecurrenceStrategyFactory
{
    IRecurrenceCalculationStrategy CreateStrategy(ISchedulerConfiguration configuration);
}
