using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services.Calculators.Base;
using Scheduler_Lib.Core.Services.Calculators.Weekly;
using Scheduler_Lib.Core.Services.Calculators.Daily;
using Scheduler_Lib.Core.Services.Utilities;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Services.Calculators.Monthly;

public static class MonthlyRecurrenceCalculator {
    public static List<DateTimeOffset> Calculate(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        var dates = new List<DateTimeOffset>();
        var baseLocal = BaseDateTimeCalculator.GetBaseDateTime(schedulerInput, tz);
        var endLocal = schedulerInput.EndDate ?? GetEffectiveEndDate(schedulerInput, tz);

        var currentMonth = new DateTime(baseLocal.Year, baseLocal.Month, 1);
        var endMonth = new DateTime(endLocal.DateTime.Year, endLocal.DateTime.Month, 1);

        var iteration = 0;
        const int maxIterations = Config.MaxIterations;

        var timeOfDay = schedulerInput.TargetDate?.TimeOfDay ?? schedulerInput.StartDate.TimeOfDay;

        while (currentMonth <= endMonth && iteration < maxIterations) {
            DateTimeOffset? nextEligible = null;

            if (schedulerInput.MonthlyDayChk && schedulerInput.MonthlyDay.HasValue) {
                nextEligible = GetEligibleDateByDay(currentMonth, schedulerInput.MonthlyDay.Value, timeOfDay, tz);
            } else if (schedulerInput.MonthlyTheChk && schedulerInput.MonthlyFrequency.HasValue && schedulerInput.MonthlyDateType.HasValue) {
                var targetDate = new DateTimeOffset(
                    new DateTime(currentMonth.Year, currentMonth.Month, 1, timeOfDay.Hours, timeOfDay.Minutes, timeOfDay.Seconds),
                    tz.GetUtcOffset(currentMonth));

                nextEligible = WeeklyRecurrenceCalculator.SelectNextEligibleDate(
                    targetDate, null!, tz, 
                    schedulerInput.MonthlyFrequency, 
                    schedulerInput.MonthlyDateType, 
                    currentMonth);
            }

            if (nextEligible.HasValue) {
                var baseOffset = new DateTimeOffset(baseLocal, tz.GetUtcOffset(baseLocal));
                
                if (schedulerInput.DailyStartTime.HasValue && schedulerInput.DailyEndTime.HasValue) {
                    var slotStep = schedulerInput.DailyPeriod ?? TimeSpan.FromMinutes(30);
                    DailySlotGenerator.GenerateSlotsForDay(
                        nextEligible.Value.DateTime.Date,
                        schedulerInput.DailyStartTime.Value,
                        schedulerInput.DailyEndTime.Value,
                        slotStep,
                        tz,
                        schedulerInput,
                        endLocal,
                        baseOffset,
                        dates);
                } else {
                    if (nextEligible.Value >= baseOffset && nextEligible.Value <= endLocal && !dates.Contains(nextEligible.Value))
                        dates.Add(nextEligible.Value);
                }
            }

            if (dates.Count >= maxIterations)
                break;

            var monthlyPeriod = (schedulerInput.MonthlyDayChk ? schedulerInput.MonthlyDayPeriod : schedulerInput.MonthlyThePeriod) ?? 1;
            currentMonth = currentMonth.AddMonths(monthlyPeriod);
            iteration++;
        }

        dates.Sort();
        return dates;
    }

    public static DateTimeOffset? GetEligibleDate(
        DateTime month, 
        EnumMonthlyFrequency frequency, 
        EnumMonthlyDateType dateType, 
        TimeSpan timeOfDay, 
        TimeZoneInfo tz) {
        
        var firstDayOfMonth = new DateTime(month.Year, month.Month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        List<DateTime> eligibleDays = dateType switch {
            EnumMonthlyDateType.Day => MonthDayCollector.GetAllDaysInMonth(firstDayOfMonth, lastDayOfMonth),
            EnumMonthlyDateType.Weekday => MonthDayCollector.GetWeekdaysInMonth(firstDayOfMonth, lastDayOfMonth),
            EnumMonthlyDateType.WeekendDay => MonthDayCollector.GetWeekendDaysInMonth(firstDayOfMonth, lastDayOfMonth),
            EnumMonthlyDateType.Monday => MonthDayCollector.GetSpecificDayOfWeekInMonth(firstDayOfMonth, lastDayOfMonth, DayOfWeek.Monday),
            EnumMonthlyDateType.Tuesday => MonthDayCollector.GetSpecificDayOfWeekInMonth(firstDayOfMonth, lastDayOfMonth, DayOfWeek.Tuesday),
            EnumMonthlyDateType.Wednesday => MonthDayCollector.GetSpecificDayOfWeekInMonth(firstDayOfMonth, lastDayOfMonth, DayOfWeek.Wednesday),
            EnumMonthlyDateType.Thursday => MonthDayCollector.GetSpecificDayOfWeekInMonth(firstDayOfMonth, lastDayOfMonth, DayOfWeek.Thursday),
            EnumMonthlyDateType.Friday => MonthDayCollector.GetSpecificDayOfWeekInMonth(firstDayOfMonth, lastDayOfMonth, DayOfWeek.Friday),
            EnumMonthlyDateType.Saturday => MonthDayCollector.GetSpecificDayOfWeekInMonth(firstDayOfMonth, lastDayOfMonth, DayOfWeek.Saturday),
            EnumMonthlyDateType.Sunday => MonthDayCollector.GetSpecificDayOfWeekInMonth(firstDayOfMonth, lastDayOfMonth, DayOfWeek.Sunday),
            _ => []
        };

        if (eligibleDays.Count == 0)
            return null;

        var selectedDay = frequency switch {
            EnumMonthlyFrequency.First => eligibleDays.First(),
            EnumMonthlyFrequency.Second => eligibleDays.Count > 1 ? eligibleDays[1] : eligibleDays.Last(),
            EnumMonthlyFrequency.Third => eligibleDays.Count > 2 ? eligibleDays[2] : eligibleDays.Last(),
            EnumMonthlyFrequency.Fourth => eligibleDays.Count > 3 ? eligibleDays[3] : eligibleDays.Last(),
            EnumMonthlyFrequency.Last => eligibleDays.Last(),
            _ => eligibleDays.First()
        };

        var resultLocal = new DateTime(selectedDay.Year, selectedDay.Month, selectedDay.Day,
            timeOfDay.Hours, timeOfDay.Minutes, timeOfDay.Seconds, DateTimeKind.Unspecified);

        return TimeZoneConverter.CreateDateTimeOffset(resultLocal, tz);
    }

    public static DateTimeOffset? GetEligibleDateByDay(DateTime month, int dayOfMonth, TimeSpan timeOfDay, TimeZoneInfo tz) {
        var lastDayOfMonth = DateTime.DaysInMonth(month.Year, month.Month);

        if (dayOfMonth > lastDayOfMonth)
            return null;
        
        var resultLocal = new DateTime(month.Year, month.Month, dayOfMonth,
            timeOfDay.Hours, timeOfDay.Minutes, timeOfDay.Seconds, DateTimeKind.Unspecified);

        return TimeZoneConverter.CreateDateTimeOffset(resultLocal, tz);
    }

    private static DateTimeOffset GetEffectiveEndDate(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        var period = schedulerInput.DailyPeriod ?? TimeSpan.FromDays(3);
        var beginning = BaseDateTimeCalculator.GetBaseDateTime(schedulerInput, tz);

        const int defaultPeriodMultiplier = 1000;
        var endLocal = beginning.Add(period * defaultPeriodMultiplier);
        return new DateTimeOffset(endLocal, tz.GetUtcOffset(endLocal));
    }
}
