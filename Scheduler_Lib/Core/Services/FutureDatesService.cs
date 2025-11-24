using Scheduler_Lib.Core.Adapters;
using Scheduler_Lib.Core.Factories;
using Scheduler_Lib.Core.Interfaces;
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services.Utilities;

namespace Scheduler_Lib.Core.Services;

/// <summary>
/// Service for retrieving future execution dates
/// Maintains backward compatibility with existing RecurrenceCalculator functionality
/// </summary>
public static class FutureDatesService
{
    private static readonly IRecurrenceStrategyFactory _strategyFactory = new RecurrenceStrategyFactory();

    /// <summary>
    /// Gets a list of future execution dates for the given configuration
    /// </summary>
    public static List<DateTimeOffset> GetFutureDates(SchedulerInput schedulerInput)
    {
        var configuration = new SchedulerInputAdapter(schedulerInput);
        var timeZone = TimeZoneConverter.GetTimeZone(configuration.TimeZoneId);
        
        var strategy = _strategyFactory.CreateStrategy(configuration);
        var futureDates = strategy.CalculateFutureDates(configuration, timeZone);
        
        // Remove the next execution date from the list (backward compatibility)
        var next = strategy.GetNextExecutionDate(configuration, timeZone);
        
        for (int i = futureDates.Count - 1; i >= 0; i--)
        {
            var d = futureDates[i];
            if (d.UtcDateTime == next.UtcDateTime ||
                (d.DateTime == next.DateTime && d.Offset == next.Offset))
            {
                futureDates.RemoveAt(i);
            }
        }
        
        return futureDates;
    }

    /// <summary>
    /// Gets the next execution date for the given configuration
    /// </summary>
    public static DateTimeOffset GetNextExecutionDate(SchedulerInput schedulerInput)
    {
        var configuration = new SchedulerInputAdapter(schedulerInput);
        var timeZone = TimeZoneConverter.GetTimeZone(configuration.TimeZoneId);
        
        var strategy = _strategyFactory.CreateStrategy(configuration);
        return strategy.GetNextExecutionDate(configuration, timeZone);
    }
}
