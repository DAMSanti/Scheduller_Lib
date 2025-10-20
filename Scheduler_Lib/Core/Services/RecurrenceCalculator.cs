using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Services;

public class RecurrenceCalculator {
    public static DateTimeOffset SelectNextEligibleDate(DateTimeOffset targetDate, List<DayOfWeek> daysOfWeek, TimeZoneInfo tz) {
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

    public static List<DateTimeOffset>? CalculateWeeklyRecurrence(SchedulerInput requestedDate, TimeZoneInfo tz) {
        if (requestedDate.DaysOfWeek == null)
            throw new NullReferenceException();

        var dates = new List<DateTimeOffset>();
        var baseLocal = GetBaseLocal(requestedDate);
        var nextEligible = SelectNextEligibleDate(new DateTimeOffset(baseLocal, tz.GetUtcOffset(baseLocal)),
            requestedDate.DaysOfWeek!, tz);

        var iteration = 0;
        var weekStart = baseLocal.Date;
        var endLocal = requestedDate.EndDate;

        var maxIterations = Config.MaxIterations;

        while (weekStart <= endLocal && iteration < maxIterations) {
            GenerateWeeklySlotsForWeek(weekStart, requestedDate, tz, nextEligible, dates);

            if (dates.Count >= maxIterations)
                return dates;

            weekStart = weekStart.AddDays(7 * requestedDate.WeeklyPeriod!.Value);
            iteration++;
        }

        dates.Sort();
        return dates;
    }

    public static List<DateTimeOffset> CalculateFutureDates(SchedulerInput requestedDate, TimeZoneInfo tz) {
        var dates = new List<DateTimeOffset>();

        if (requestedDate.Periodicity != EnumConfiguration.Recurrent)
            return dates;

        var endDate = GetEffectiveEndDate(requestedDate);
        var slotStep = GetSlotStep(requestedDate);

        switch (requestedDate.Recurrency) {
            case EnumRecurrency.Daily:
                if (!requestedDate.DailyStartTime.HasValue || !requestedDate.DailyEndTime.HasValue) {
                    AddSimpleDailySlots(requestedDate.CurrentDate, endDate, slotStep, requestedDate, dates);
                    return dates;
                }

                FillDailyWindowSlots(requestedDate, tz, endDate, slotStep, dates);
                return dates;

            case EnumRecurrency.Weekly:
                if (requestedDate.DaysOfWeek == null || requestedDate.DaysOfWeek.Count == 0)
                    return dates;

                FillWeeklySlots(requestedDate, tz, endDate, slotStep, dates);
                dates.Sort();
                return dates;

            default:
                return dates;
        }
    }


    private static void AddSimpleDailySlots(DateTimeOffset currentStart, DateTimeOffset endDate, TimeSpan step, SchedulerInput requestedDate, List<DateTimeOffset> accumulator) {
        var current = currentStart;
        while (current <= endDate) {
            if (current >= requestedDate.StartDate && current <= endDate)
                accumulator.Add(current);

            current = current.Add(step);
        }
    }

    private static void FillDailyWindowSlots(SchedulerInput requestedDate, TimeZoneInfo tz, DateTimeOffset endDate, TimeSpan slotStep, List<DateTimeOffset> accumulator) {
        var dayCursor = requestedDate.StartDate.Date;
        var lastDay = endDate.Date;
        while (dayCursor <= lastDay) {
            GenerateDailySlotsForDay(dayCursor, requestedDate.DailyStartTime!.Value, requestedDate.DailyEndTime!.Value, slotStep, tz, requestedDate, endDate, accumulator);
            dayCursor = dayCursor.AddDays(1);
        }
    }

    private static void FillWeeklySlots(SchedulerInput requestedDate, TimeZoneInfo tz, DateTimeOffset endDate, TimeSpan slotStep, List<DateTimeOffset> accumulator) {
        var weeklyPeriod = requestedDate.WeeklyPeriod ?? 1;
        var baseLocal = GetBaseLocal(requestedDate);
        var weekStart = baseLocal.Date;

        var lastWeekDay = endDate.Date;
        while (weekStart <= lastWeekDay) {
            foreach (var day in requestedDate.DaysOfWeek!) {
                var timeOfDay = requestedDate.TargetDate?.TimeOfDay ?? requestedDate.StartDate.TimeOfDay;
                var candidateLocal = GetCandidateLocalForWeekAndDay(weekStart, day, timeOfDay);
                if (candidateLocal == null) continue;

                var candidateDayDto = CreateDateTimeOffset(candidateLocal.Value, tz);
                if (candidateDayDto > endDate) continue;
                if (candidateDayDto < requestedDate.StartDate) continue;

                if (!requestedDate.DailyStartTime.HasValue || !requestedDate.DailyEndTime.HasValue) {
                    if (!accumulator.Contains(candidateDayDto)) accumulator.Add(candidateDayDto);
                    continue;
                }

                GenerateDailySlotsForDay(candidateLocal.Value.Date, requestedDate.DailyStartTime!.Value, requestedDate.DailyEndTime!.Value, slotStep, tz, requestedDate, endDate, accumulator);
            }

            weekStart = weekStart.AddDays(7 * weeklyPeriod);
        }
    }

    private static DateTimeOffset GetEffectiveEndDate(SchedulerInput requestedDate) =>
        requestedDate.EndDate ?? requestedDate.CurrentDate.Add(requestedDate.Period ?? TimeSpan.FromDays(1) * 3);
    private static DateTime GetBaseLocal(SchedulerInput requestedDate) =>
        (requestedDate.TargetDate ?? requestedDate.StartDate).DateTime;

    private static TimeSpan GetSlotStep(SchedulerInput requestedDate) =>
        requestedDate.DailyFrequency ?? requestedDate.Period ?? TimeSpan.FromDays(1);

    private static DateTimeOffset CreateDateTimeOffset(DateTime localWallClock, TimeZoneInfo tz) =>
        new(localWallClock, tz.GetUtcOffset(localWallClock));
    private static DateTime? GetCandidateLocalForWeekAndDay(DateTime weekStart, DayOfWeek day, TimeSpan timeOfDay) {
        var date = new DateTime(weekStart.Year, weekStart.Month, weekStart.Day,
            timeOfDay.Hours, timeOfDay.Minutes, timeOfDay.Seconds, DateTimeKind.Unspecified);

        for (int i = 0; i < 7; i++) {
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

    private static void GenerateDailySlotsForDay(DateTime day, TimeSpan start, TimeSpan end, TimeSpan step, TimeZoneInfo tz, SchedulerInput requestedDate, DateTimeOffset endDate, List<DateTimeOffset> accumulator) {
        var startLocal = new DateTime(day.Year, day.Month, day.Day,
            start.Hours, start.Minutes, start.Seconds, DateTimeKind.Unspecified);

        var endLocal = new DateTime(day.Year, day.Month, day.Day,
            end.Hours, end.Minutes, end.Seconds, DateTimeKind.Unspecified);

        var slotLocal = startLocal;
        while (slotLocal <= endLocal) {
            var slotDto = CreateDateTimeOffset(slotLocal, tz);
            if (slotDto >= requestedDate.StartDate && slotDto <= endDate && !accumulator.Contains(slotDto))
                accumulator.Add(slotDto);

            slotLocal = slotLocal.Add(step);
        }
    }

    private static void GenerateWeeklySlotsForWeek(DateTime weekStart, SchedulerInput requestedDate, TimeZoneInfo tz, DateTimeOffset nextEligible, List<DateTimeOffset> accumulator) {
        foreach (var day in requestedDate.DaysOfWeek!) {
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

    private static DateTimeOffset? NextWeekday(DateTimeOffset startLocal, DayOfWeek day, TimeZoneInfo tz) {
        var date = startLocal.Date;
        while (date.DayOfWeek != day)
            date = date.AddDays(1);

        var localWallClock = new DateTime(date.Year, date.Month, date.Day, startLocal.Hour, startLocal.Minute,
            startLocal.Second, DateTimeKind.Unspecified);
        return CreateDateTimeOffset(localWallClock, tz);
    }

    public static TimeZoneInfo GetTimeZone() {
        return TimeZoneInfo.Local;
    }
}