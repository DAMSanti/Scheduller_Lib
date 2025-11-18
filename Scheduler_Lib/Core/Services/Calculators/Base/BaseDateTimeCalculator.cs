using Scheduler_Lib.Core.Model;

namespace Scheduler_Lib.Core.Services.Calculators.Base;

internal static class BaseDateTimeCalculator {
    internal static DateTime GetBaseDateTime(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        if (schedulerInput.TargetDate.HasValue)
            return schedulerInput.TargetDate.Value.DateTime;

        if (schedulerInput.CurrentDate != default) {
            var utcTime = schedulerInput.CurrentDate.UtcDateTime;
            var startTime = schedulerInput.StartDate.TimeOfDay;

            var localInTz = Utilities.TimeZoneConverter.ConvertFromUtc(utcTime, tz);
            return new DateTime(localInTz.Year, localInTz.Month, localInTz.Day,
                startTime.Hours, startTime.Minutes, startTime.Seconds, DateTimeKind.Unspecified);
        }

        return schedulerInput.StartDate.DateTime;
    }
}
