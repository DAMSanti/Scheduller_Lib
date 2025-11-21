using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services.Calculators.Base;
using Scheduler_Lib.Core.Services.Calculators.Daily;
using Scheduler_Lib.Core.Services.Calculators.Monthly;
using Scheduler_Lib.Core.Services.Calculators.Weekly;
using Scheduler_Lib.Core.Services.Utilities;
using System.Runtime.CompilerServices;

namespace Scheduler_Lib.Core.Services;

public static class RecurrenceCalculator {
    public static DateTimeOffset GetNextExecutionDate(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        var timeZone = !string.IsNullOrWhiteSpace(schedulerInput.TimeZoneId)
        ? TimeZoneConverter.GetTimeZone(schedulerInput.TimeZoneId)
        : tz;

        if (schedulerInput.Recurrency == EnumRecurrency.Weekly) {
            var baseLocal = BaseDateTimeCalculator.GetBaseDateTime(schedulerInput, tz);
            var baseDtoForNext = new DateTimeOffset(baseLocal, tz.GetUtcOffset(baseLocal));
            return SelectNextEligibleDate(baseDtoForNext, schedulerInput.DaysOfWeek!, tz);
        }

        if (schedulerInput.TargetDate.HasValue) {
            return schedulerInput.TargetDate.Value;
        }

        if (schedulerInput.CurrentDate != default) {
            var utcTime = schedulerInput.CurrentDate.UtcDateTime;
            var startTime = schedulerInput.StartDate.TimeOfDay;

            var localInTz = TimeZoneConverter.ConvertFromUtc(utcTime, tz);
            var baseLocal = new DateTime(localInTz.Year, localInTz.Month, localInTz.Day,
                startTime.Hours, startTime.Minutes, startTime.Seconds, DateTimeKind.Unspecified);

            return new DateTimeOffset(baseLocal, tz.GetUtcOffset(baseLocal));
        }

        var baseDateTime = BaseDateTimeCalculator.GetBaseDateTime(schedulerInput, tz);
        return new DateTimeOffset(baseDateTime, tz.GetUtcOffset(baseDateTime));
    }

    private static DateTimeOffset SelectNextEligibleDate(
        DateTimeOffset targetDate, 
        List<DayOfWeek> daysOfWeek, 
        TimeZoneInfo tz, 
        EnumMonthlyFrequency? monthlyFrequency = null, 
        EnumMonthlyDateType? monthlyDateType = null, 
        DateTime? currentMonth = null) {
        
        return WeeklyRecurrenceCalculator.SelectNextEligibleDate(targetDate, daysOfWeek, tz, monthlyFrequency, monthlyDateType, currentMonth);
    }

    public static List<DateTimeOffset> GetFutureDates(SchedulerInput schedulerInput) {
        var tz = !string.IsNullOrWhiteSpace(schedulerInput.TimeZoneId)
            ? TimeZoneConverter.GetTimeZone(schedulerInput.TimeZoneId)
            : TimeZoneConverter.GetTimeZone();

        List<DateTimeOffset> futureDates;
        switch (schedulerInput.Recurrency) {
            case EnumRecurrency.Daily:
                futureDates = DailyRecurrenceCalculator.CalculateFutureDates(schedulerInput, tz);
                break;
            case EnumRecurrency.Weekly:
                futureDates = WeeklyRecurrenceCalculator.CalculateFutureDates(schedulerInput, tz);
                break;
            case EnumRecurrency.Monthly:
                futureDates = MonthlyRecurrenceCalculator.CalculateFutureDates(schedulerInput, tz);
                break;
            default:
                futureDates = new List<DateTimeOffset>();
                break;
        }

        var next = GetNextExecutionDate(schedulerInput, tz);

        for (int i = futureDates.Count - 1; i >= 0; i--) {
            var d = futureDates[i];
            if (d.UtcDateTime == next.UtcDateTime ||
                (d.DateTime == next.DateTime && d.Offset == next.Offset)) {
                futureDates.RemoveAt(i);
            }
        }
        return futureDates;
    }
}