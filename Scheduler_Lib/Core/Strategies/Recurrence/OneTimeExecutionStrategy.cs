using Scheduler_Lib.Core.Interfaces;
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services.Utilities;
using Scheduler_Lib.Core.Strategies.Base;

namespace Scheduler_Lib.Core.Strategies.Recurrence;

/// <summary>
/// Strategy for one-time execution calculations
/// </summary>
public class OneTimeExecutionStrategy : RecurrenceCalculationStrategyBase
{
    public override bool CanHandle(ISchedulerConfiguration configuration)
    {
        return configuration.Periodicity == EnumConfiguration.Once;
    }

    public override DateTimeOffset GetNextExecutionDate(ISchedulerConfiguration configuration, TimeZoneInfo timeZone)
    {
        var targetDate = configuration.TargetDate!.Value;
        return new DateTimeOffset(targetDate.DateTime, timeZone.GetUtcOffset(targetDate.DateTime));
    }

    protected override void CalculateDatesCore(
        ISchedulerConfiguration configuration, 
        TimeZoneInfo timeZone, 
        DateTimeOffset baseDto, 
        DateTimeOffset endDate, 
        List<DateTimeOffset> accumulator)
    {
        // One-time execution doesn't generate future dates
        // The next execution is the target date itself
    }

    protected override DateTimeOffset GetEffectiveEndDate(ISchedulerConfiguration configuration, TimeZoneInfo timeZone)
    {
        return configuration.EndDate ?? configuration.TargetDate ?? configuration.StartDate;
    }
}
