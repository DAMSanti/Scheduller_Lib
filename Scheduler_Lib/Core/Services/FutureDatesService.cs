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
        try
        {
            var configuration = new SchedulerInputAdapter(schedulerInput);
            var timeZone = TimeZoneConverter.GetTimeZone(configuration.TimeZoneId);
            
            var strategy = _strategyFactory.CreateStrategy(configuration);
            var futureDates = strategy.CalculateFutureDates(configuration, timeZone);
            
            return futureDates;
        }
        catch (InvalidOperationException)
        {
            // Return empty list if no strategy found for unsupported recurrency
            return new List<DateTimeOffset>();
        }
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
