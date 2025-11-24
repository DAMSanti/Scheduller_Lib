namespace Scheduler_Lib.Core.Interfaces;

/// <summary>
/// Defines the contract for recurrence calculation strategies (Strategy Pattern)
/// </summary>
public interface IRecurrenceCalculationStrategy
{
    /// <summary>
    /// Calculates future dates based on the scheduler configuration
    /// </summary>
    List<DateTimeOffset> CalculateFutureDates(ISchedulerConfiguration configuration, TimeZoneInfo timeZone);
    
    /// <summary>
    /// Gets the next execution date
    /// </summary>
    DateTimeOffset GetNextExecutionDate(ISchedulerConfiguration configuration, TimeZoneInfo timeZone);
    
    /// <summary>
    /// Indicates if this strategy can handle the given configuration
    /// </summary>
    bool CanHandle(ISchedulerConfiguration configuration);
}
