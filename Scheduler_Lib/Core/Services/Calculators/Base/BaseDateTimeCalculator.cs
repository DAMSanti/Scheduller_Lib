using Scheduler_Lib.Core.Adapters;
using Scheduler_Lib.Core.Interfaces;
using Scheduler_Lib.Core.Model;

namespace Scheduler_Lib.Core.Services.Calculators.Base;

internal static class BaseDateTimeCalculator {
    internal static DateTime GetBaseDateTime(ISchedulerConfiguration schedulerConfiguration, TimeZoneInfo tz) {
        if (schedulerConfiguration.TargetDate.HasValue)
            return schedulerConfiguration.TargetDate.Value.DateTime;

        if (schedulerConfiguration.CurrentDate != default) {
            var utcTime = schedulerConfiguration.CurrentDate.UtcDateTime;
            var startTime = schedulerConfiguration.StartDate.TimeOfDay;

            var localInTz = Utilities.TimeZoneConverter.ConvertFromUtc(utcTime, tz);
            return new DateTime(localInTz.Year, localInTz.Month, localInTz.Day,
                startTime.Hours, startTime.Minutes, startTime.Seconds, DateTimeKind.Unspecified);
        }

        return schedulerConfiguration.StartDate.DateTime;
    }

    // Renamed method to avoid ambiguity in reflection
    internal static DateTime GetBaseDateTimeFromInput(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        var adapter = new SchedulerInputAdapter(schedulerInput);
        return GetBaseDateTime(adapter, tz);
    }
}
