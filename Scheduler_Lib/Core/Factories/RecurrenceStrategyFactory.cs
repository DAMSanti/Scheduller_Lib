using Scheduler_Lib.Core.Interfaces;
using Scheduler_Lib.Core.Strategies.Recurrence;

namespace Scheduler_Lib.Core.Factories;

/// <summary>
/// Factory for creating recurrence calculation strategies (Factory Pattern)
/// </summary>
public class RecurrenceStrategyFactory : IRecurrenceStrategyFactory
{
    private readonly List<IRecurrenceCalculationStrategy> _strategies;

    public RecurrenceStrategyFactory()
    {
        _strategies = new List<IRecurrenceCalculationStrategy>
        {
            new OneTimeExecutionStrategy(),
            new DailyRecurrenceStrategy(),
            new WeeklyRecurrenceStrategy(),
            new MonthlyRecurrenceStrategy()
        };
    }

    public IRecurrenceCalculationStrategy CreateStrategy(ISchedulerConfiguration configuration)
    {
        var strategy = _strategies.FirstOrDefault(s => s.CanHandle(configuration));
        
        if (strategy == null)
            throw new InvalidOperationException(
                $"No strategy found for Periodicity: {configuration.Periodicity}, Recurrency: {configuration.Recurrency}");

        return strategy;
    }
}
