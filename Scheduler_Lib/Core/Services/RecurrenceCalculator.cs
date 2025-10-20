using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Services;

public class RecurrenceCalculator
{
    public static DateTimeOffset SelectNextEligibleDate(DateTimeOffset targetDate, List<DayOfWeek> daysOfWeek, TimeZoneInfo tz)
    {
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

    public static List<DateTimeOffset>? CalculateWeeklyRecurrence(SchedulerInput requestedDate, TimeZoneInfo tz)
    {
        if (requestedDate.DaysOfWeek == null)
            throw new NullReferenceException();

        var dates = new List<DateTimeOffset>();
        var baseLocal = GetBaseLocal(requestedDate);
        var nextEligible = SelectNextEligibleDate(new DateTimeOffset(baseLocal, tz.GetUtcOffset(baseLocal)),
            requestedDate.DaysOfWeek!, tz);

        var iteration = 0;
        var weekStart = baseLocal.Date;
        var endLocal = GetEndLocal(requestedDate);

        var maxIterations = Config.MaxIterations;

        while (weekStart <= endLocal && iteration < maxIterations)
        {
            GenerateWeeklySlotsForWeek(weekStart, requestedDate, tz, nextEligible, endLocal, dates);

            if (dates.Count >= maxIterations)
                return dates;

            weekStart = weekStart.AddDays(7 * requestedDate.WeeklyPeriod!.Value);
            iteration++;
        }

        dates.Sort();
        return dates;
    }

    public static List<DateTimeOffset> CalculateFutureDates(SchedulerInput requestedDate, TimeZoneInfo tz)
    {
        var dates = new List<DateTimeOffset>();

        var endDate = requestedDate.EndDate ?? requestedDate.CurrentDate.Add(requestedDate.Period ?? TimeSpan.FromDays(1) * 3);

        var slotStep = GetSlotStep(requestedDate);

        if (requestedDate.Periodicity != EnumConfiguration.Recurrent)
            return dates;

        if (requestedDate.Recurrency == EnumRecurrency.Daily)
        {
            if (!requestedDate.DailyStartTime.HasValue || !requestedDate.DailyEndTime.HasValue)
            {
                var current = requestedDate.CurrentDate;
                while (current <= endDate)
                {
                    if (current >= requestedDate.StartDate && current <= endDate)
                        dates.Add(current);

                    current = current.Add(slotStep);
                }

                return dates;
            }

            var dayCursor = requestedDate.StartDate.Date;
            var lastDay = endDate.Date;
            while (dayCursor <= lastDay)
            {
                GenerateDailySlotsForDay(dayCursor, requestedDate.DailyStartTime!.Value, requestedDate.DailyEndTime!.Value, slotStep, tz, requestedDate, endDate, dates);
                dayCursor = dayCursor.AddDays(1);
            }

            return dates;
        }

        if (requestedDate.Recurrency == EnumRecurrency.Weekly)
        {
            if (requestedDate.DaysOfWeek == null || requestedDate.DaysOfWeek.Count == 0)
                return dates;

            var weeklyPeriod = requestedDate.WeeklyPeriod ?? 1;
            var baseLocal = GetBaseLocal(requestedDate);
            var weekStart = baseLocal.Date;

            var lastWeekDay = endDate.Date;
            while (weekStart <= lastWeekDay)
            {
                foreach (var day in requestedDate.DaysOfWeek)
                {
                    var timeOfDay = requestedDate.TargetDate?.TimeOfDay ?? requestedDate.StartDate.TimeOfDay;
                    var candidateLocal = GetCandidateLocalForWeekAndDay(weekStart, day, timeOfDay);
                    if (candidateLocal == null) continue;

                    var candidateDayDto = CreateDateTimeOffset(candidateLocal.Value, tz);
                    if (candidateDayDto > endDate) continue;
                    if (candidateDayDto < requestedDate.StartDate) continue;

                    if (!requestedDate.DailyStartTime.HasValue || !requestedDate.DailyEndTime.HasValue)
                    {
                        dates.Add(candidateDayDto);
                        continue;
                    }

                    GenerateDailySlotsForDay(candidateLocal.Value.Date, requestedDate.DailyStartTime!.Value, requestedDate.DailyEndTime!.Value, slotStep, tz, requestedDate, endDate, dates);
                }

                weekStart = weekStart.AddDays(7 * weeklyPeriod);
            }

            dates.Sort();
        }

        return dates;
    }

    private static DateTime GetBaseLocal(SchedulerInput requestedDate) =>
        (requestedDate.TargetDate ?? requestedDate.StartDate).DateTime;

    private static DateTime GetEndLocal(SchedulerInput requestedDate) =>
        requestedDate.EndDate?.DateTime ?? DateTime.MaxValue;

    private static TimeSpan GetSlotStep(SchedulerInput requestedDate) =>
        requestedDate.DailyFrequency ?? requestedDate.Period ?? TimeSpan.FromDays(1);

    private static DateTimeOffset CreateDateTimeOffset(DateTime localWallClock, TimeZoneInfo tz) =>
        new DateTimeOffset(localWallClock, tz.GetUtcOffset(localWallClock));

    private static DateTime? GetCandidateLocalForWeekAndDay(DateTime weekStart, DayOfWeek day, TimeSpan timeOfDay)
    {
        var candidateLocal = new DateTime(weekStart.Year, weekStart.Month, weekStart.Day,
            timeOfDay.Hours, timeOfDay.Minutes, timeOfDay.Seconds, DateTimeKind.Unspecified);

        int daysToAdd = ((int)day - (int)candidateLocal.DayOfWeek + 7) % 7;
        if (daysToAdd > 0)
        {
            if (!TryAddDaysSafely(candidateLocal, daysToAdd, out var added))
                return null;
            candidateLocal = added;
        }

        return candidateLocal;
    }

    private static bool TryAddDaysSafely(DateTime dt, int days, out DateTime result)
    {
        result = dt;
        if (days == 0) return true;
        if (dt > DateTime.MaxValue.AddDays(-days)) return false;
        result = dt.AddDays(days);
        return true;
    }

    private static void GenerateDailySlotsForDay(DateTime day, TimeSpan start, TimeSpan end, TimeSpan step, TimeZoneInfo tz, SchedulerInput requestedDate, DateTimeOffset endDate, List<DateTimeOffset> accumulator)
    {
        var startLocal = new DateTime(day.Year, day.Month, day.Day,
            start.Hours, start.Minutes, start.Seconds, DateTimeKind.Unspecified);

        var endLocal = new DateTime(day.Year, day.Month, day.Day,
            end.Hours, end.Minutes, end.Seconds, DateTimeKind.Unspecified);

        var slotLocal = startLocal;
        while (slotLocal <= endLocal)
        {
            var slotDto = CreateDateTimeOffset(slotLocal, tz);
            if (slotDto >= requestedDate.StartDate && slotDto <= endDate && !accumulator.Contains(slotDto))
                accumulator.Add(slotDto);

            slotLocal = slotLocal.Add(step);
        }
    }

    private static void GenerateWeeklySlotsForWeek(DateTime weekStart, SchedulerInput requestedDate, TimeZoneInfo tz, DateTimeOffset nextEligible, DateTime endLocal, List<DateTimeOffset> accumulator)
    {
        foreach (var day in requestedDate.DaysOfWeek!)
        {
            var timeOfDay = requestedDate.TargetDate?.TimeOfDay ?? requestedDate.StartDate.TimeOfDay;
            var candidateLocal = GetCandidateLocalForWeekAndDay(weekStart, day, timeOfDay);
            if (candidateLocal == null) continue;

            var offset = tz.GetUtcOffset(candidateLocal.Value);
            var candidate = new DateTimeOffset(candidateLocal.Value, offset);

            if (candidate > (requestedDate.EndDate ?? DateTimeOffset.MaxValue)) continue;
            if (candidate <= nextEligible) continue;
            if (!accumulator.Contains(candidate)) accumulator.Add(candidate);
        }
    }

    private static DateTimeOffset? NextWeekday(DateTimeOffset startLocal, DayOfWeek day, TimeZoneInfo tz)
    {
        var date = startLocal.Date;
        int daysToAdd = ((int)day - (int)date.DayOfWeek + 7) % 7;
        if (daysToAdd > 0)
        {
            if (!TryAddDaysSafely(date, daysToAdd, out var added))
                return null;
            date = added;
        }

        var localWallClock = new DateTime(date.Year, date.Month, date.Day, startLocal.Hour, startLocal.Minute,
            startLocal.Second, DateTimeKind.Unspecified);
        return CreateDateTimeOffset(localWallClock, tz);
    }

    public static TimeZoneInfo GetTimeZone() {
        return TimeZoneInfo.Local;
    }
}