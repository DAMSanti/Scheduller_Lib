using Scheduler_Lib.Core.Interfaces;
using Scheduler_Lib.Core.Services.Calculators.Base;
using Scheduler_Lib.Core.Services.Utilities;

namespace Scheduler_Lib.Core.Strategies.Base;

/// <summary>
/// Abstract base class for recurrence calculation strategies (Template Method Pattern)
/// </summary>
public abstract class RecurrenceCalculationStrategyBase : IRecurrenceCalculationStrategy
{
    public abstract bool CanHandle(ISchedulerConfiguration configuration);

    public List<DateTimeOffset> CalculateFutureDates(ISchedulerConfiguration configuration, TimeZoneInfo timeZone)
    {
        var dates = new List<DateTimeOffset>();
        
        if (!ValidateConfiguration(configuration))
            return dates;

        var baseDateTime = BaseDateTimeCalculator.GetBaseDateTime(configuration, timeZone);
        var baseDto = new DateTimeOffset(baseDateTime, timeZone.GetUtcOffset(baseDateTime));
        var endDate = GetEffectiveEndDate(configuration, timeZone);

        CalculateDatesCore(configuration, timeZone, baseDto, endDate, dates);
        
        dates.Sort();
        return dates;
    }

    public virtual DateTimeOffset GetNextExecutionDate(ISchedulerConfiguration configuration, TimeZoneInfo timeZone)
    {
        var futureDates = CalculateFutureDates(configuration, timeZone);
        
        if (futureDates.Count > 0)
            return futureDates.First();

        var baseLocal = BaseDateTimeCalculator.GetBaseDateTime(configuration, timeZone);
        return new DateTimeOffset(baseLocal, timeZone.GetUtcOffset(baseLocal));
    }

    protected virtual bool ValidateConfiguration(ISchedulerConfiguration configuration)
    {
        return configuration.StartDate != DateTimeOffset.MaxValue && 
               configuration.EndDate != DateTimeOffset.MaxValue;
    }

    protected abstract void CalculateDatesCore(
        ISchedulerConfiguration configuration, 
        TimeZoneInfo timeZone, 
        DateTimeOffset baseDto, 
        DateTimeOffset endDate, 
        List<DateTimeOffset> accumulator);

    protected abstract DateTimeOffset GetEffectiveEndDate(
        ISchedulerConfiguration configuration, 
        TimeZoneInfo timeZone);
}
