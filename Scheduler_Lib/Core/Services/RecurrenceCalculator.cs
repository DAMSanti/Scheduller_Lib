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
        if (requestedDate.EndDate == null)
            Config.MaxIterations = 9999;

        if (requestedDate.DaysOfWeek == null)
            throw new NullReferenceException();

        var dates = new List<DateTimeOffset>();
        var baseLocal = (requestedDate.TargetDate ?? requestedDate.StartDate).DateTime;
        var nextEligible = SelectNextEligibleDate(new DateTimeOffset(baseLocal, tz.GetUtcOffset(baseLocal)),
            requestedDate.DaysOfWeek!, tz);

        var iteration = 0;
        var weekStart = baseLocal.Date;
        var endLocal = requestedDate.EndDate?.DateTime ?? DateTime.MaxValue;

        while (weekStart <= endLocal && iteration < Config.MaxIterations) {
            foreach (var day in requestedDate.DaysOfWeek!) {
                var timeOfDay = requestedDate.TargetDate?.TimeOfDay ?? requestedDate.StartDate.TimeOfDay;
                var candidateLocal = new DateTime(weekStart.Year, weekStart.Month, weekStart.Day, timeOfDay.Hours,
                    timeOfDay.Minutes, timeOfDay.Seconds, DateTimeKind.Unspecified);

                int daysToAdd = ((int)day - (int)candidateLocal.DayOfWeek + 7) % 7;
                if (daysToAdd > 0) {
                    if (candidateLocal > DateTime.MaxValue.AddDays(-daysToAdd))
                        continue;
                    candidateLocal = candidateLocal.AddDays(daysToAdd);
                }

                var offset = tz.GetUtcOffset(candidateLocal);
                var candidate = new DateTimeOffset(candidateLocal, offset);

                if (candidate > (requestedDate.EndDate ?? DateTimeOffset.MaxValue)) continue;
                if (candidate <= nextEligible) continue;
                if (!dates.Contains(candidate)) dates.Add(candidate);

                if (dates.Count >= Config.MaxIterations)
                    return dates;
            }

            weekStart = weekStart.AddDays(7 * requestedDate.WeeklyPeriod!.Value);
            iteration++;
        }

        dates.Sort();
        return dates;
    }

    public static List<DateTimeOffset> CalculateFutureDates(SchedulerInput requestedDate, TimeZoneInfo tz) {
        var dates = new List<DateTimeOffset>();

        var endDate = requestedDate.EndDate ?? requestedDate.CurrentDate.Add(requestedDate.Period ?? TimeSpan.FromDays(1) * 3);

        var slotStep = requestedDate.DailyFrequency ?? requestedDate.Period ?? TimeSpan.FromDays(1);

        if (requestedDate.Periodicity != EnumConfiguration.Recurrent)
            return dates; 

        if (requestedDate.Recurrency == EnumRecurrency.Daily) { 
            if (!requestedDate.DailyStartTime.HasValue || !requestedDate.DailyEndTime.HasValue) {
                var current = requestedDate.CurrentDate;
                while (current <= endDate) {
                    if (current >= requestedDate.StartDate && current <= endDate)
                        dates.Add(current);

                    current = current.Add(slotStep);
                }

                return dates;
            }

            var dayCursor = requestedDate.StartDate.Date;
            var lastDay = endDate.Date;
            while (dayCursor <= lastDay) {
                var startLocal = new DateTime(dayCursor.Year, dayCursor.Month, dayCursor.Day,
                    requestedDate.DailyStartTime.Value.Hours,
                    requestedDate.DailyStartTime.Value.Minutes,
                    requestedDate.DailyStartTime.Value.Seconds,
                    DateTimeKind.Unspecified);

                var endLocal = new DateTime(dayCursor.Year, dayCursor.Month, dayCursor.Day,
                    requestedDate.DailyEndTime.Value.Hours,
                    requestedDate.DailyEndTime.Value.Minutes,
                    requestedDate.DailyEndTime.Value.Seconds,
                    DateTimeKind.Unspecified);

                var slotLocal = startLocal;
                while (slotLocal <= endLocal) {
                    var slotDto = new DateTimeOffset(slotLocal, tz.GetUtcOffset(slotLocal));
                    if (slotDto >= requestedDate.StartDate && slotDto <= endDate)
                        dates.Add(slotDto);

                    slotLocal = slotLocal.Add(slotStep);
                }

                dayCursor = dayCursor.AddDays(1);
            }

            return dates;
        }

        if (requestedDate.Recurrency == EnumRecurrency.Weekly) {
            if (requestedDate.DaysOfWeek == null || requestedDate.DaysOfWeek.Count == 0)
                return dates;

            var weeklyPeriod = requestedDate.WeeklyPeriod ?? 1;
            var baseLocal = (requestedDate.TargetDate ?? requestedDate.StartDate).DateTime;
            var weekStart = baseLocal.Date;

            var lastWeekDay = endDate.Date;
            while (weekStart <= lastWeekDay) {
                foreach (var day in requestedDate.DaysOfWeek) {
                    var timeOfDay = requestedDate.TargetDate?.TimeOfDay ?? requestedDate.StartDate.TimeOfDay;
                    var candidateLocal = new DateTime(weekStart.Year, weekStart.Month, weekStart.Day,
                        timeOfDay.Hours, timeOfDay.Minutes, timeOfDay.Seconds, DateTimeKind.Unspecified);

                    while (candidateLocal.DayOfWeek != day)
                        candidateLocal = candidateLocal.AddDays(1);

                    int daysToAdd = ((int)day - (int)candidateLocal.DayOfWeek + 7) % 7;
                    if (daysToAdd > 0)
                    {
                        if (candidateLocal > DateTime.MaxValue.AddDays(-daysToAdd))
                            continue;
                        candidateLocal = candidateLocal.AddDays(daysToAdd);
                    }

                    var candidateDayDto = new DateTimeOffset(candidateLocal, tz.GetUtcOffset(candidateLocal));
                    if (candidateDayDto > endDate) continue;
                    if (candidateDayDto < requestedDate.StartDate) continue;

                    if (!requestedDate.DailyStartTime.HasValue || !requestedDate.DailyEndTime.HasValue) {
                        dates.Add(candidateDayDto);
                        continue;
                    }

                    var startLocal = new DateTime(candidateLocal.Year, candidateLocal.Month, candidateLocal.Day,
                        requestedDate.DailyStartTime.Value.Hours,
                        requestedDate.DailyStartTime.Value.Minutes,
                        requestedDate.DailyStartTime.Value.Seconds,
                        DateTimeKind.Unspecified);

                    var endLocal = new DateTime(candidateLocal.Year, candidateLocal.Month, candidateLocal.Day,
                        requestedDate.DailyEndTime.Value.Hours,
                        requestedDate.DailyEndTime.Value.Minutes,
                        requestedDate.DailyEndTime.Value.Seconds,
                        DateTimeKind.Unspecified);

                    var slotLocal = startLocal;
                    while (slotLocal <= endLocal) {
                        var slotDto = new DateTimeOffset(slotLocal, tz.GetUtcOffset(slotLocal));
                        if (slotDto >= requestedDate.StartDate && slotDto <= endDate)
                            dates.Add(slotDto);

                        slotLocal = slotLocal.Add(slotStep);
                    }
                }

                weekStart = weekStart.AddDays(7 * weeklyPeriod);
            }

            dates.Sort();
        }

        return dates;
    }

    private static DateTimeOffset? NextWeekday(DateTimeOffset startLocal, DayOfWeek day, TimeZoneInfo tz) {
        var date = startLocal.Date;
        while (date.DayOfWeek != day) 
            date = date.AddDays(1);

        var localWallClock = new DateTime(date.Year, date.Month, date.Day, startLocal.Hour, startLocal.Minute,
            startLocal.Second, DateTimeKind.Unspecified);
        return new DateTimeOffset(localWallClock, tz.GetUtcOffset(localWallClock));
    }
    public static TimeZoneInfo GetTimeZone() {
        return TimeZoneInfo.Local;
    }
}