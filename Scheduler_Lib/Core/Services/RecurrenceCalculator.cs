using Scheduler_Lib.Core.Model;

namespace Scheduler_Lib.Core.Services;

/// <summary>
/// Legacy RecurrenceCalculator - Maintained for backward compatibility
/// New code should use FutureDatesService instead
/// </summary>
public static class RecurrenceCalculator {
    public static DateTimeOffset GetNextExecutionDate(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        // Delegate to new service
        return FutureDatesService.GetNextExecutionDate(schedulerInput);
    }

    public static List<DateTimeOffset> GetFutureDates(SchedulerInput schedulerInput) {
        // Delegate to new service
        return FutureDatesService.GetFutureDates(schedulerInput);
    }
}