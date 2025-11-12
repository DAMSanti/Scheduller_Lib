using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services.Calculators.Base;
using Scheduler_Lib.Core.Services.Calculators.Monthly;
using Scheduler_Lib.Core.Services.Utilities;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Services.Calculators.Weekly;

public static class WeeklyRecurrenceCalculator {
    private const int DaysInWeek = 7;

    public static List<DateTimeOffset> CalculateFutureDates(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        var dates = new List<DateTimeOffset>();
        var baseLocal = BaseDateTimeCalculator.GetBaseDateTime(schedulerInput, tz);
        var nextEligible = SelectNextEligibleDate(
            new DateTimeOffset(baseLocal, tz.GetUtcOffset(baseLocal)),
            schedulerInput.DaysOfWeek!, 
            tz);

        var iteration = 0;
        var weekStart = baseLocal.Date;
        var endLocal = schedulerInput.EndDate;

        const int maxIterations = Config.MaxIterations;

        while (weekStart <= endLocal && iteration < maxIterations) {
            GenerateSlotsForWeek(weekStart, schedulerInput, tz, nextEligible, dates);

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
            var eligibleDate = MonthlyRecurrenceCalculator.GetEligibleDate(
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
        DateTimeOffset nextEligible, 
        List<DateTimeOffset> accumulator) {
        
        foreach (var day in schedulerInput.DaysOfWeek!) {
            var timeOfDay = schedulerInput.TargetDate?.TimeOfDay ?? schedulerInput.StartDate.TimeOfDay;
            var candidateLocal = GetCandidateForWeekAndDay(weekStart, day, timeOfDay);
            
            if (candidateLocal == null) 
                continue;

            var candidate = TimeZoneConverter.CreateDateTimeOffset(candidateLocal.Value, tz);

            if (candidate <= nextEligible) 
                continue;
                
            if (!accumulator.Contains(candidate)) 
                accumulator.Add(candidate);
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
}
