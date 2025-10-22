using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Services;

public class RecurrenceCalculator {
    public static DateTimeOffset SelectNextEligibleDate(DateTimeOffset targetDate, List<DayOfWeek> daysOfWeek, TimeZoneInfo tz) {
        if (targetDate == DateTimeOffset.MinValue)
            return DateTimeOffset.MinValue;

        var targetLocal = targetDate.DateTime;
        var candidates = daysOfWeek
            .Select(d => NextWeekday(targetLocal, d, tz))
            .Where(dto => dto != null && dto.Value.DateTime >= targetLocal)
            .OrderBy(dto => dto!.Value.DateTime)
            .Select(dto => dto!.Value)
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

        var maxIterations = Config.MaxIterations;

        while (weekStart <= endLocal && iteration < maxIterations) {
            GenerateWeeklySlotsForWeek(weekStart, schedulerInput, tz, nextEligible, dates);

            if (dates.Count >= maxIterations)
                return dates;

            weekStart = weekStart.AddDays(7 * schedulerInput.WeeklyPeriod!.Value);
            iteration++;
        }

        dates.Sort();
        return dates;
    }

    public static List<DateTimeOffset> CalculateFutureDates(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        var dates = new List<DateTimeOffset>();

        if (schedulerInput.StartDate == DateTimeOffset.MaxValue || schedulerInput.EndDate == DateTimeOffset.MaxValue)
            return dates;

        if (schedulerInput.Periodicity != EnumConfiguration.Recurrent)
            return dates;

        var endDate = GetEffectiveEndDate(schedulerInput);
        var slotStep = GetSlotStep(schedulerInput);

        var baseDto = GetBaseDateTimeOffset(schedulerInput, tz);

        switch (schedulerInput.Recurrency) {
            case EnumRecurrency.Daily:
                if (!schedulerInput.DailyStartTime.HasValue || !schedulerInput.DailyEndTime.HasValue) {
                    AddSimpleDailySlots(baseDto, endDate, slotStep, schedulerInput, dates);
                    break;
                }

                FillDailyWindowSlots(schedulerInput, tz, endDate, slotStep, baseDto, dates);
                break;

            case EnumRecurrency.Weekly:
                if (schedulerInput.DaysOfWeek == null || schedulerInput.DaysOfWeek.Count == 0)
                    return dates;

                FillWeeklySlots(schedulerInput, tz, endDate, slotStep, baseDto, dates);
                dates.Sort();
                break;
        }
        return dates;
    }


    private static void AddSimpleDailySlots(DateTimeOffset startFrom, DateTimeOffset endDate, TimeSpan step, SchedulerInput schedulerInput, List<DateTimeOffset> accumulator) {
        var tz = RecurrenceCalculator.GetTimeZone();

        if (schedulerInput.TargetDate == null) {
            var startTime = schedulerInput.CurrentDate.TimeOfDay;
            startFrom = new DateTimeOffset(
                startFrom.Year, startFrom.Month, startFrom.Day,
                startTime.Hours, startTime.Minutes, startTime.Seconds,
                startFrom.Offset
            );
        }

        while (startFrom <= endDate) {
            var adjustedDate = new DateTimeOffset(startFrom.DateTime, tz.GetUtcOffset(startFrom.DateTime));
            accumulator.Add(adjustedDate);

            startFrom = startFrom.Add(step);
        }
    }

    private static void FillDailyWindowSlots(SchedulerInput schedulerInput, TimeZoneInfo tz, DateTimeOffset endDate, TimeSpan slotStep, DateTimeOffset earliestAllowed, List<DateTimeOffset> accumulator)
    {
        var baseLocal = GetBaseLocal(schedulerInput);
        var dayCursor = schedulerInput.StartDate.Date > baseLocal.Date ? schedulerInput.StartDate.Date : baseLocal.Date;
        var lastDay = endDate.Date;
        while (dayCursor <= lastDay)
        {
            GenerateDailySlotsForDay(dayCursor, schedulerInput.DailyStartTime!.Value, schedulerInput.DailyEndTime!.Value, slotStep, tz, schedulerInput, endDate, earliestAllowed, accumulator);
            dayCursor = dayCursor.AddDays(1);
        }
    }

    private static void FillWeeklySlots(SchedulerInput schedulerInput, TimeZoneInfo tz, DateTimeOffset endDate, TimeSpan slotStep, DateTimeOffset earliestAllowed, List<DateTimeOffset> accumulator)
    {
        var weeklyPeriod = schedulerInput.WeeklyPeriod ?? 1;
        var baseLocal = GetBaseLocal(schedulerInput);
        var weekStart = baseLocal.Date;

        var lastWeekDay = endDate.Date;
        while (weekStart <= lastWeekDay) {
            foreach (var day in schedulerInput.DaysOfWeek!) {
                var timeOfDay = schedulerInput.TargetDate?.TimeOfDay ?? schedulerInput.StartDate.TimeOfDay;
                var candidateLocal = GetCandidateLocalForWeekAndDay(weekStart, day, timeOfDay);
                if (candidateLocal == null) continue;

                var candidateDayDto = CreateDateTimeOffset(candidateLocal.Value, tz);
                if (candidateDayDto > endDate) continue;
                if (candidateDayDto < schedulerInput.StartDate) continue;

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

        return beginning.Add(period * 1000);
    }

    private static DateTime GetBaseLocal(SchedulerInput schedulerInput) {
        if (schedulerInput.TargetDate.HasValue)
            return schedulerInput.TargetDate.Value.DateTime;

        if (schedulerInput.CurrentDate != default)
            return schedulerInput.CurrentDate.DateTime;

        return schedulerInput.StartDate.DateTime;
    }
    private static TimeSpan GetSlotStep(SchedulerInput schedulerInput) =>
        schedulerInput.DailyFrequency ?? schedulerInput.DailyPeriod ?? TimeSpan.FromDays(1);

    private static DateTimeOffset CreateDateTimeOffset(DateTime localWallClock, TimeZoneInfo tz) =>
        new(localWallClock, tz.GetUtcOffset(localWallClock));

    private static DateTime? GetCandidateLocalForWeekAndDay(DateTime weekStart, DayOfWeek day, TimeSpan timeOfDay)
    {
        var date = new DateTime(weekStart.Year, weekStart.Month, weekStart.Day,
            timeOfDay.Hours, timeOfDay.Minutes, timeOfDay.Seconds, DateTimeKind.Unspecified);

        for (int i = 0; i < 7; i++)
        {
            if (date.DayOfWeek == day)
                return date;

            if (!TryAddDaysSafely(date, 1, out var next))
                return null;

            date = next;
        }

        return null;
    }

    private static bool TryAddDaysSafely(DateTime dt, int days, out DateTime result)
    {
        result = dt;
        if (days == 0) return true;
        if (dt > DateTime.MaxValue.AddDays(-days)) return false;
        result = dt.AddDays(days);
        return true;
    }

    private static void GenerateDailySlotsForDay(DateTime day, TimeSpan start, TimeSpan end, TimeSpan step, TimeZoneInfo tz, SchedulerInput schedulerInput, DateTimeOffset endDate, DateTimeOffset earliestAllowed, List<DateTimeOffset> accumulator)
    {
        var startLocal = new DateTime(day.Year, day.Month, day.Day,
            start.Hours, start.Minutes, start.Seconds, DateTimeKind.Unspecified);

        var endLocal = new DateTime(day.Year, day.Month, day.Day,
            end.Hours, end.Minutes, end.Seconds, DateTimeKind.Unspecified);

        var slotLocal = startLocal;
        while (slotLocal <= endLocal)
        {
            var slotDto = CreateDateTimeOffset(slotLocal, tz);
            if (slotDto >= schedulerInput.StartDate && slotDto <= endDate && slotDto > earliestAllowed && !accumulator.Contains(slotDto))
                accumulator.Add(slotDto);

            slotLocal = slotLocal.Add(step);
        }
    }

    private static void GenerateWeeklySlotsForWeek(DateTime weekStart, SchedulerInput schedulerInput, TimeZoneInfo tz, DateTimeOffset nextEligible, List<DateTimeOffset> accumulator)
    {
        foreach (var day in schedulerInput.DaysOfWeek!)
        {
            var timeOfDay = schedulerInput.TargetDate?.TimeOfDay ?? schedulerInput.StartDate.TimeOfDay;
            var candidateLocal = GetCandidateLocalForWeekAndDay(weekStart, day, timeOfDay);
            if (candidateLocal == null) continue;

            var offset = tz.GetUtcOffset(candidateLocal.Value);
            var candidate = new DateTimeOffset(candidateLocal.Value, offset);

            if (candidate > (schedulerInput.EndDate ?? DateTimeOffset.MaxValue)) continue;
            if (candidate <= nextEligible) continue;
            if (!accumulator.Contains(candidate)) accumulator.Add(candidate);
        }
    }

    private static DateTimeOffset? NextWeekday(DateTimeOffset startLocal, DayOfWeek day, TimeZoneInfo tz)
    {
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
    private static DateTimeOffset GetBaseDateTimeOffset(SchedulerInput schedulerInput, TimeZoneInfo tz)
    {
        if (schedulerInput.TargetDate.HasValue)
        {
            var td = schedulerInput.TargetDate.Value.DateTime;
            return new DateTimeOffset(td, tz.GetUtcOffset(td));
        }

        if (schedulerInput.CurrentDate != default)
        {
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
}