using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services.Calculators.Base;
using Scheduler_Lib.Core.Services.Calculators.Daily;
using Scheduler_Lib.Core.Services.Calculators.Monthly;
using Scheduler_Lib.Core.Services.Utilities;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Services.Calculators.Weekly;

public static class WeeklyRecurrenceCalculator {
    private const int DaysInWeek = 7;

    public static List<DateTimeOffset> CalculateFutureDates(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        var dates = new List<DateTimeOffset>();
        var baseLocal = BaseDateTimeCalculator.GetBaseDateTime(schedulerInput, tz);
        var baseDto = new DateTimeOffset(baseLocal, tz.GetUtcOffset(baseLocal));

        var endLocal = schedulerInput.EndDate ?? GetEffectiveEndDate(schedulerInput, tz);
        var weekStart = baseLocal.Date;
        var slotStep = schedulerInput.DailyPeriod ?? TimeSpan.FromHours(1);

        const int maxIterations = Config.MaxIterations;
        var iteration = 0;

        while (weekStart <= endLocal.Date && iteration < maxIterations) {
            GenerateSlotsForWeek(weekStart, schedulerInput, tz, baseDto, endLocal, slotStep, dates);

            if (dates.Count >= maxIterations)
                return dates;

            var stepDays = DaysInWeek * schedulerInput.WeeklyPeriod!.Value;
            if (!DateSafetyHelper.TryAddDaysSafely(weekStart, stepDays, out var nextWeekStart))
                break;

            weekStart = nextWeekStart;
            iteration++;
        }

        dates.Sort();
        return dates;
    }

    public static DateTimeOffset SelectNextEligibleDate(
        DateTimeOffset targetDate, 
        List<DayOfWeek> daysOfWeek, 
        TimeZoneInfo tz, 
        EnumMonthlyFrequency? monthlyFrequency = null, 
        EnumMonthlyDateType? monthlyDateType = null, 
        DateTime? currentMonth = null) {
        
        if (targetDate == DateTimeOffset.MinValue)
            return DateTimeOffset.MinValue;

        var targetLocal = targetDate.DateTime;

        if (monthlyFrequency.HasValue && monthlyDateType.HasValue && currentMonth.HasValue) {
            var eligibleDate = GetEligibleDate(
                currentMonth.Value, 
                monthlyFrequency.Value, 
                monthlyDateType.Value, 
                targetLocal.TimeOfDay, 
                tz);
            return eligibleDate ?? new DateTimeOffset(targetLocal, tz.GetUtcOffset(targetLocal));
        }

        var candidates = daysOfWeek
            .Select(day => FindNextWeekday(targetLocal, day, tz))
            .OrderBy(dateTimeOffset => dateTimeOffset!.Value.DateTime)
            .Select(dateTimeOffset => dateTimeOffset!.Value)
            .ToList();

        return candidates.Count > 0
            ? candidates.First()
            : new DateTimeOffset(targetLocal, tz.GetUtcOffset(targetLocal));
    }

    private static DateTimeOffset? FindNextWeekday(DateTimeOffset startLocal, DayOfWeek day, TimeZoneInfo tz) {
        var date = startLocal.Date;
        while (date.DayOfWeek != day)
            date = date.AddDays(1);

        var localWallClock = new DateTime(date.Year, date.Month, date.Day, 
            startLocal.Hour, startLocal.Minute, startLocal.Second, DateTimeKind.Unspecified);
        return TimeZoneConverter.CreateDateTimeOffset(localWallClock, tz);
    }

    private static void GenerateSlotsForWeek(
        DateTime weekStart,
        SchedulerInput schedulerInput,
        TimeZoneInfo tz,
        DateTimeOffset baseDto,
        DateTimeOffset endLocal,
        TimeSpan slotStep,
        List<DateTimeOffset> accumulator) {

        foreach (var day in schedulerInput.DaysOfWeek!) {
            var timeOfDay = schedulerInput.TargetDate?.TimeOfDay ?? schedulerInput.StartDate.TimeOfDay;
            var candidateLocal = GetCandidateForWeekAndDay(weekStart, day, timeOfDay);

            if (candidateLocal == null)
                continue;

            var candidate = TimeZoneConverter.CreateDateTimeOffset(candidateLocal.Value, tz);

            if (candidate < baseDto)
                continue;

            if (candidate > endLocal)
                continue;

            if (schedulerInput.DailyStartTime.HasValue && schedulerInput.DailyEndTime.HasValue) {
                DailySlotGenerator.GenerateSlotsForDay(
                    candidateLocal.Value.Date,
                    schedulerInput.DailyStartTime.Value,
                    schedulerInput.DailyEndTime.Value,
                    slotStep,
                    tz,
                    schedulerInput,
                    endLocal,
                    baseDto,
                    accumulator);
            } else {
                if (!accumulator.Contains(candidate))
                    accumulator.Add(candidate);
            }
        }
    }

    private static DateTime? GetCandidateForWeekAndDay(DateTime weekStart, DayOfWeek day, TimeSpan timeOfDay) {
        var date = new DateTime(weekStart.Year, weekStart.Month, weekStart.Day,
            timeOfDay.Hours, timeOfDay.Minutes, timeOfDay.Seconds, DateTimeKind.Unspecified);

        for (var i = 0; i < DaysInWeek; i++) {
            if (date.DayOfWeek == day)
                return date;

            if (!DateSafetyHelper.TryAddDaysSafely(date, 1, out var next))
                return null; 

            date = next;
        }
        return null;
    }

    private static DateTimeOffset GetEffectiveEndDate(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        var period = schedulerInput.DailyPeriod ?? TimeSpan.FromDays(3);
        var beginning = BaseDateTimeCalculator.GetBaseDateTime(schedulerInput, tz);

        const int defaultPeriodMultiplier = 1000;
        var endLocal = beginning.Add(period * defaultPeriodMultiplier);
        return new DateTimeOffset(endLocal, tz.GetUtcOffset(endLocal));
    }

    private static DateTimeOffset? GetEligibleDate(
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
}
