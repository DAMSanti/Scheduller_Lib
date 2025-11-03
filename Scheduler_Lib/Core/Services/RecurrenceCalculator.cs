using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services.Calculators.Base;
using Scheduler_Lib.Core.Services.Calculators.Weekly;
using Scheduler_Lib.Core.Services.Calculators.Monthly;
using Scheduler_Lib.Core.Services.Calculators.Daily;
using Scheduler_Lib.Core.Services.Utilities;

namespace Scheduler_Lib.Core.Services;

public static class RecurrenceCalculator {
    public static DateTimeOffset SelectNextEligibleDate(
        DateTimeOffset targetDate, 
        List<DayOfWeek> daysOfWeek, 
        TimeZoneInfo tz, 
        EnumMonthlyFrequency? monthlyFrequency = null, 
        EnumMonthlyDateType? monthlyDateType = null, 
        DateTime? currentMonth = null) {
        
        return WeeklyRecurrenceCalculator.SelectNextEligibleDate(targetDate, daysOfWeek, tz, monthlyFrequency, monthlyDateType, currentMonth);
    }

    public static List<DateTimeOffset>? CalculateWeeklyRecurrence(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        return WeeklyRecurrenceCalculator.Calculate(schedulerInput, tz);
    }

    public static List<DateTimeOffset> CalculateMonthlyRecurrence(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        return MonthlyRecurrenceCalculator.Calculate(schedulerInput, tz);
    }

    public static List<DateTimeOffset> CalculateFutureDates(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        return DailyRecurrenceCalculator.CalculateFutureDates(schedulerInput, tz);
    }

    public static TimeZoneInfo GetTimeZone() {
        return TimeZoneConverter.GetTimeZone();
    }

    public static DateTimeOffset GetNextExecutionDate(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        if (schedulerInput.Recurrency == EnumRecurrency.Weekly) {
            var baseLocal = BaseDateTimeCalculator.GetBaseDateTime(schedulerInput, tz);
            var baseDtoForNext = new DateTimeOffset(baseLocal, tz.GetUtcOffset(baseLocal));
            return SelectNextEligibleDate(baseDtoForNext, schedulerInput.DaysOfWeek!, tz);
        }

        if (schedulerInput.OccursOnceChk && schedulerInput.OccursOnceAt.HasValue) {
            return schedulerInput.OccursOnceAt.Value;
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

    public static List<DateTimeOffset> GetFutureDates(SchedulerInput schedulerInput) {
        if (schedulerInput.Periodicity != EnumConfiguration.Recurrent)
            return [];

        var tz = GetTimeZone();
        var futureDates = CalculateFutureDates(schedulerInput, tz);
        var next = GetNextExecutionDate(schedulerInput, tz);

        futureDates.RemoveAll(d => d.UtcDateTime == next.UtcDateTime || 
                                   (d.DateTime == next.DateTime && d.Offset == next.Offset));
        return futureDates;
    }
}