using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Services;

public class RecurrenceCalculator {
    public static DateTimeOffset SelectNextEligibleDate(DateTimeOffset targetDate, List<DayOfWeek> daysOfWeek, TimeZoneInfo tz, EnumMonthlyFrequency? monthlyFrequency = null, EnumMonthlyDateType? monthlyDateType = null, DateTime? currentMonth = null) {
        if (targetDate == DateTimeOffset.MinValue)
            return DateTimeOffset.MinValue;

        var targetLocal = targetDate.DateTime;

        if (monthlyFrequency.HasValue && monthlyDateType.HasValue && currentMonth.HasValue) {
            var eligibleDate = GetMonthlyEligibleDate(currentMonth.Value, monthlyFrequency.Value, monthlyDateType.Value, targetLocal.TimeOfDay, tz);
            return eligibleDate ?? new DateTimeOffset(targetLocal, tz.GetUtcOffset(targetLocal));
        }

        var candidates = daysOfWeek
            .Select(day => NextWeekday(targetLocal, day, tz))
            .OrderBy(dateTimeOffset => dateTimeOffset!.Value.DateTime)
            .Select(dateTimeOffset => dateTimeOffset!.Value)
            .ToList();

        return candidates.Count > 0
            ? candidates.First()
            : new DateTimeOffset(targetLocal, tz.GetUtcOffset(targetLocal));
    }

    public static List<DateTimeOffset>? CalculateWeeklyRecurrence(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        var dates = new List<DateTimeOffset>();
        var baseLocal = GetBaseLocal(schedulerInput);
        var nextEligible = SelectNextEligibleDate(new DateTimeOffset(baseLocal, tz.GetUtcOffset(baseLocal)),
            schedulerInput.DaysOfWeek!, tz);

        var iteration = 0;
        var weekStart = baseLocal.Date;
        var endLocal = schedulerInput.EndDate;

        const int maxIterations = Config.MaxIterations;

        while (weekStart <= endLocal && iteration < maxIterations) {
            GenerateWeeklySlotsForWeek(weekStart, schedulerInput, tz, nextEligible, dates);

            if (dates.Count >= maxIterations)
                return dates;

            var stepDays = 7 * schedulerInput.WeeklyPeriod!.Value;
            if (!TryAddDaysSafely(weekStart, stepDays, out var nextWeekStart))
                break;

            weekStart = nextWeekStart;
            iteration++;
        }

        dates.Sort();
        return dates;
    }

    public static List<DateTimeOffset> CalculateMonthlyRecurrence(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        var dates = new List<DateTimeOffset>();
        var baseLocal = GetBaseLocal(schedulerInput);
        var endLocal = schedulerInput.EndDate ?? GetEffectiveEndDate(schedulerInput);

        var currentMonth = new DateTime(baseLocal.Year, baseLocal.Month, 1);
        var endMonth = new DateTime(endLocal.DateTime.Year, endLocal.DateTime.Month, 1);

        var iteration = 0;
        const int maxIterations = Config.MaxIterations;

        var timeOfDay = schedulerInput.TargetDate?.TimeOfDay ?? schedulerInput.StartDate.TimeOfDay;

        while (currentMonth <= endMonth && iteration < maxIterations) {
            var targetDate = new DateTimeOffset(
                new DateTime(currentMonth.Year, currentMonth.Month, 1, timeOfDay.Hours, timeOfDay.Minutes, timeOfDay.Seconds),
                tz.GetUtcOffset(currentMonth));

            var nextEligible = SelectNextEligibleDate(targetDate, null!, tz, schedulerInput.MonthlyFrequency, schedulerInput.MonthlyDateType, currentMonth);

            if (nextEligible >= new DateTimeOffset(baseLocal, tz.GetUtcOffset(baseLocal)) && nextEligible <= endLocal && !dates.Contains(nextEligible))
                dates.Add(nextEligible);

            if (dates.Count >= maxIterations)
                break;

            var monthlyPeriod = schedulerInput.MonthlyThePeriod ?? 1;
            currentMonth = currentMonth.AddMonths(monthlyPeriod);
            iteration++;
        }

        dates.Sort();
        return dates;
    }
    private static DateTimeOffset? GetMonthlyEligibleDate(DateTime month, EnumMonthlyFrequency frequency, EnumMonthlyDateType dateType, TimeSpan timeOfDay, TimeZoneInfo tz) {
        var firstDayOfMonth = new DateTime(month.Year, month.Month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        List<DateTime> eligibleDays = dateType switch {
            EnumMonthlyDateType.Day => Enumerable.Range(1, lastDayOfMonth.Day)
                .Select(d => new DateTime(month.Year, month.Month, d))
                .ToList(),

            EnumMonthlyDateType.Weekday => GetWeekdaysInMonth(firstDayOfMonth, lastDayOfMonth),

            EnumMonthlyDateType.WeekendDay => GetWeekendDaysInMonth(firstDayOfMonth, lastDayOfMonth),

            _ => GetSpecificDayOfWeekInMonth(firstDayOfMonth, lastDayOfMonth, (DayOfWeek)(int)dateType)
        };

        if (eligibleDays.Count == 0)
            return null;

        var selectedDay = frequency switch  {
            EnumMonthlyFrequency.First => eligibleDays.First(),
            EnumMonthlyFrequency.Second => eligibleDays.Count > 1 ? eligibleDays[1] : eligibleDays.Last(),
            EnumMonthlyFrequency.Third => eligibleDays.Count > 2 ? eligibleDays[2] : eligibleDays.Last(),
            EnumMonthlyFrequency.Fourth => eligibleDays.Count > 3 ? eligibleDays[3] : eligibleDays.Last(),
            EnumMonthlyFrequency.Last => eligibleDays.Last(),
            _ => eligibleDays.First()
        };

        var resultLocal = new DateTime(selectedDay.Year, selectedDay.Month, selectedDay.Day,
            timeOfDay.Hours, timeOfDay.Minutes, timeOfDay.Seconds, DateTimeKind.Unspecified);

        return CreateDateTimeOffset(resultLocal, tz);
    }

    private static List<DateTime> GetWeekdaysInMonth(DateTime firstDay, DateTime lastDay) {
        var days = new List<DateTime>();
        for (var day = firstDay; day <= lastDay; day = day.AddDays(1)) {
            if (day.DayOfWeek != DayOfWeek.Saturday && day.DayOfWeek != DayOfWeek.Sunday)
                days.Add(day);
        }
        return days;
    }
    private static List<DateTime> GetWeekendDaysInMonth(DateTime firstDay, DateTime lastDay) {
        var days = new List<DateTime>();
        for (var day = firstDay; day <= lastDay; day = day.AddDays(1)) {
            if (day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                days.Add(day);
        }
        return days;
    }
    private static List<DateTime> GetSpecificDayOfWeekInMonth(DateTime firstDay, DateTime lastDay, DayOfWeek targetDayOfWeek) {
        var days = new List<DateTime>();
        for (var day = firstDay; day <= lastDay; day = day.AddDays(1)) {
            if (day.DayOfWeek == targetDayOfWeek)
                days.Add(day);
        }
        return days;
    }
    public static List<DateTimeOffset> CalculateFutureDates(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        var dates = new List<DateTimeOffset>();

        if (schedulerInput.StartDate == DateTimeOffset.MaxValue || schedulerInput.EndDate == DateTimeOffset.MaxValue)
            return dates;

        var endDate = GetEffectiveEndDate(schedulerInput);
        var slotStep = schedulerInput.DailyPeriod ?? TimeSpan.FromDays(1);

        var baseDateTimeOffset = GetBaseDateTimeOffset(schedulerInput, tz);

        switch (schedulerInput.Recurrency) {
            case EnumRecurrency.Daily:
                if (!schedulerInput.DailyStartTime.HasValue || !schedulerInput.DailyEndTime.HasValue) {
                    AddSimpleDailySlots(baseDateTimeOffset, endDate, slotStep, schedulerInput, dates);
                    break;
                }

                FillDailyWindowSlots(schedulerInput, tz, endDate, slotStep, baseDateTimeOffset, dates);
                break;

            case EnumRecurrency.Weekly:
                if (schedulerInput.DaysOfWeek == null || schedulerInput.DaysOfWeek.Count == 0)
                    return dates;

                FillWeeklySlots(schedulerInput, tz, endDate, slotStep, baseDateTimeOffset, dates);
                dates.Sort();
                break;
            case EnumRecurrency.Monthly:
                if (!schedulerInput.MonthlyFrequency.HasValue || !schedulerInput.MonthlyDateType.HasValue)
                    return dates;

                dates = CalculateMonthlyRecurrence(schedulerInput, tz);
                break;
            default:
                break;
        }
        return dates;
    }


    private static void AddSimpleDailySlots(DateTimeOffset startFrom, DateTimeOffset endDate, TimeSpan step, SchedulerInput schedulerInput, List<DateTimeOffset> accumulator) {
        var tz = GetTimeZone();

        if (schedulerInput.TargetDate == null) {
            var startTime = schedulerInput.CurrentDate.TimeOfDay;
            startFrom = new DateTimeOffset(
                startFrom.Year, startFrom.Month, startFrom.Day,
                startTime.Hours, startTime.Minutes, startTime.Seconds,
                startFrom.Offset
            );
        }

        while (startFrom <= endDate) {
            var adjustedDate = CreateDateTimeOffset(startFrom.DateTime, tz);
            accumulator.Add(adjustedDate);

            startFrom = startFrom.Add(step);
        }
    }

    private static void FillDailyWindowSlots(SchedulerInput schedulerInput, TimeZoneInfo tz, DateTimeOffset endDate, TimeSpan slotStep, DateTimeOffset earliestAllowed, List<DateTimeOffset> accumulator) {
        var baseLocal = GetBaseLocal(schedulerInput);
        var dayCursor = schedulerInput.StartDate.Date > baseLocal.Date ? schedulerInput.StartDate.Date : baseLocal.Date;
        var lastDay = endDate.Date;
        while (dayCursor <= lastDay) {
            GenerateDailySlotsForDay(dayCursor, schedulerInput.DailyStartTime!.Value, schedulerInput.DailyEndTime!.Value, slotStep, tz, schedulerInput, endDate, earliestAllowed, accumulator);
            dayCursor = dayCursor.AddDays(1);
        }
    }

    private static void FillWeeklySlots(SchedulerInput schedulerInput, TimeZoneInfo tz, DateTimeOffset endDate, TimeSpan slotStep, DateTimeOffset earliestAllowed, List<DateTimeOffset> accumulator) {
        var weeklyPeriod = schedulerInput.WeeklyPeriod ?? 1;
        var baseLocal = GetBaseLocal(schedulerInput);
        var weekStart = baseLocal.Date;

        var lastWeekDay = endDate.Date;
        while (weekStart <= lastWeekDay) {
            foreach (var day in schedulerInput.DaysOfWeek!) {
                var timeOfDay = schedulerInput.TargetDate?.TimeOfDay ?? schedulerInput.StartDate.TimeOfDay;
                var candidateLocal = GetCandidateLocalForWeekAndDay(weekStart, day, timeOfDay);

                var candidateDayDto = CreateDateTimeOffset(candidateLocal!.Value, tz);
                if (candidateDayDto > endDate) continue;

                if (candidateDayDto <= earliestAllowed) continue;

                if (!schedulerInput.DailyStartTime.HasValue || !schedulerInput.DailyEndTime.HasValue) {
                    if (!accumulator.Contains(candidateDayDto)) accumulator.Add(candidateDayDto);
                    continue;
                }

                GenerateDailySlotsForDay(candidateLocal.Value.Date, schedulerInput.DailyStartTime!.Value, schedulerInput.DailyEndTime!.Value, slotStep, tz, schedulerInput, endDate, earliestAllowed, accumulator);
            }

            weekStart = weekStart.AddDays(7 * weeklyPeriod);
        }
    }

    private static DateTimeOffset GetEffectiveEndDate(SchedulerInput schedulerInput) {
        if (schedulerInput.EndDate.HasValue)
            return schedulerInput.EndDate.Value;

        var period = schedulerInput.DailyPeriod ?? TimeSpan.FromDays(3);
        var beginning = GetBaseLocal(schedulerInput);

        const int defaultPeriodMultiplier = 1000;
        return beginning.Add(period * defaultPeriodMultiplier);
    }

    private static DateTime GetBaseLocal(SchedulerInput schedulerInput) {
        if (schedulerInput.TargetDate.HasValue)
            return schedulerInput.TargetDate!.Value.DateTime;

        return schedulerInput.CurrentDate != default ? schedulerInput.CurrentDate.DateTime : schedulerInput.StartDate.DateTime;
    }

    private static DateTimeOffset CreateDateTimeOffset(DateTime localWallClock, TimeZoneInfo tz) =>
        new(localWallClock, tz.GetUtcOffset(localWallClock));

    private static DateTime? GetCandidateLocalForWeekAndDay(DateTime weekStart, DayOfWeek day, TimeSpan timeOfDay) {
        var date = new DateTime(weekStart.Year, weekStart.Month, weekStart.Day,
            timeOfDay.Hours, timeOfDay.Minutes, timeOfDay.Seconds, DateTimeKind.Unspecified);

        for (var i = 0; i < 7; i++) {
            if (date.DayOfWeek == day)
                return date;

            if (!TryAddDaysSafely(date, 1, out var next))
                return null;

            date = next;
        }

        return null;
    }

    private static bool TryAddDaysSafely(DateTime dt, int days, out DateTime result) {
        result = dt;
        if (days == 0) return true;
        if (dt > DateTime.MaxValue.AddDays(-days)) return false;
        result = dt.AddDays(days);
        return true;
    }

    private static void GenerateDailySlotsForDay(DateTime day, TimeSpan start, TimeSpan end, TimeSpan step, TimeZoneInfo tz, SchedulerInput schedulerInput, DateTimeOffset endDate, DateTimeOffset earliestAllowed, List<DateTimeOffset> accumulator) {
        var startLocal = new DateTime(day.Year, day.Month, day.Day,
            start.Hours, start.Minutes, start.Seconds, DateTimeKind.Unspecified);

        var endLocal = new DateTime(day.Year, day.Month, day.Day,
            end.Hours, end.Minutes, end.Seconds, DateTimeKind.Unspecified);

        var slotLocal = startLocal;
        while (slotLocal <= endLocal) {
            var slotDateTimeOffset = CreateDateTimeOffset(slotLocal, tz);
            if (slotDateTimeOffset >= schedulerInput.StartDate && slotDateTimeOffset <= endDate && slotDateTimeOffset > earliestAllowed && !accumulator.Contains(slotDateTimeOffset))
                accumulator.Add(slotDateTimeOffset);

            slotLocal = slotLocal.Add(step);
        }
    }

    private static void GenerateWeeklySlotsForWeek(DateTime weekStart, SchedulerInput schedulerInput, TimeZoneInfo tz, DateTimeOffset nextEligible, List<DateTimeOffset> accumulator) {
        for (var index = 0; index < schedulerInput.DaysOfWeek!.Count; index++) {
            var day = schedulerInput.DaysOfWeek![index];
            var timeOfDay = schedulerInput.TargetDate?.TimeOfDay ?? schedulerInput.StartDate.TimeOfDay;
            var candidateLocal = GetCandidateLocalForWeekAndDay(weekStart, day, timeOfDay);
            if (candidateLocal == null) continue;

            var candidate = CreateDateTimeOffset(candidateLocal.Value, tz);

            if (candidate <= nextEligible) continue;
            if (!accumulator.Contains(candidate)) accumulator.Add(candidate);
        }
    }

    private static DateTimeOffset? NextWeekday(DateTimeOffset startLocal, DayOfWeek day, TimeZoneInfo tz) {
        var date = startLocal.Date;
        while (date.DayOfWeek != day)
            date = date.AddDays(1);

        var localWallClock = new DateTime(date.Year, date.Month, date.Day, startLocal.Hour, startLocal.Minute,
            startLocal.Second, DateTimeKind.Unspecified);
        return CreateDateTimeOffset(localWallClock, tz);
    }

    public static TimeZoneInfo GetTimeZone()  {
        return TimeZoneInfo.FindSystemTimeZoneById(Config.TimeZoneId);
    }

    private static DateTimeOffset GetBaseDateTimeOffset(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        if (schedulerInput.TargetDate.HasValue) {
            var td = schedulerInput.TargetDate.Value.DateTime;
            return CreateDateTimeOffset(td, tz);
        }

        if (schedulerInput.CurrentDate != default) {
            var cur = schedulerInput.CurrentDate.DateTime;
            var startTime = schedulerInput.StartDate.TimeOfDay;
            var baseLocal = new DateTime(cur.Year, cur.Month, cur.Day,
                startTime.Hours, startTime.Minutes, startTime.Seconds, DateTimeKind.Unspecified);

            var offset = tz.GetUtcOffset(baseLocal);
            return new DateTimeOffset(baseLocal, offset);
        }

        var sd = schedulerInput.StartDate.DateTime;
        return new DateTimeOffset(sd, tz.GetUtcOffset(sd));
    }

    public static DateTime GetBaseLocalTime(SchedulerInput schedulerInput) {
        DateTime baseLocal;
        if (schedulerInput.TargetDate.HasValue) {
            baseLocal = schedulerInput.TargetDate.Value.DateTime;
        } else {
            var cur = schedulerInput.CurrentDate.DateTime;
            var startTime = schedulerInput.StartDate.TimeOfDay;
            baseLocal = new DateTime(cur.Year, cur.Month, cur.Day,
                startTime.Hours, startTime.Minutes, startTime.Seconds, DateTimeKind.Unspecified);
        }

        return baseLocal;
    }
    public static List<DateTimeOffset> GetFutureDates(SchedulerInput schedulerInput) {
        if (schedulerInput.Periodicity != EnumConfiguration.Recurrent)
            return [];

        var tz = GetTimeZone();
        var futureDates = CalculateFutureDates(schedulerInput, tz);

        DateTimeOffset next;
        if (schedulerInput.Recurrency == EnumRecurrency.Weekly) {
            var baseLocal = GetBaseLocalTime(schedulerInput);
            var baseDtoForNext = new DateTimeOffset(baseLocal, tz.GetUtcOffset(baseLocal));
            next = SelectNextEligibleDate(baseDtoForNext, schedulerInput.DaysOfWeek!, tz);
        } else {
            if (schedulerInput.OccursOnceChk) {
                var once = schedulerInput.OccursOnceAt!.Value;
                next = new DateTimeOffset(once.DateTime, tz.GetUtcOffset(once.DateTime));
            } else {
                if (schedulerInput.TargetDate.HasValue) {
                    var td = schedulerInput.TargetDate.Value;
                    next = new DateTimeOffset(td.DateTime, tz.GetUtcOffset(td.DateTime));
                } else {
                    var cur = schedulerInput.CurrentDate;
                    next = new DateTimeOffset(cur.DateTime, tz.GetUtcOffset(cur.DateTime));
                }
            }
        }

        futureDates.RemoveAll(d => d == next);
        return futureDates;
    }
}