using Scheduler_Lib.Core.Model;

namespace Scheduler_Lib.Core.Services;

public class RecurrenceCalculator {
    public DateTimeOffset SelectNextEligibleDate(DateTimeOffset targetDate, List<DayOfWeek> daysOfWeek, TimeZoneInfo tz) {
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

    public List<DateTimeOffset>? CalculateWeeklyRecurrence(SchedulerInput requestedDate, TimeZoneInfo tz) {
        if (requestedDate.DaysOfWeek == null || requestedDate.WeeklyPeriod == null) return null;

        var dates = new List<DateTimeOffset>();
        var baseLocal = (requestedDate.TargetDate ?? requestedDate.StartDate).DateTime;
        var nextEligible = SelectNextEligibleDate(new DateTimeOffset(baseLocal, tz.GetUtcOffset(baseLocal)),
            requestedDate.DaysOfWeek, tz);

        var iteration = 0;
        var weekStart = baseLocal.Date;
        var endLocal = requestedDate.EndDate?.DateTime ?? DateTime.MaxValue;

        while (weekStart <= endLocal && iteration < (requestedDate.MaxIterations ?? int.MaxValue)) {
            foreach (var day in requestedDate.DaysOfWeek!) {
                var timeOfDay = requestedDate.TargetDate?.TimeOfDay ?? requestedDate.StartDate.TimeOfDay;
                var candidateLocal = new DateTime(weekStart.Year, weekStart.Month, weekStart.Day, timeOfDay.Hours,
                    timeOfDay.Minutes, timeOfDay.Seconds, DateTimeKind.Unspecified);
                while (candidateLocal.DayOfWeek != day) candidateLocal = candidateLocal.AddDays(1);

                var offset = tz.GetUtcOffset(candidateLocal);
                var candidateDto = new DateTimeOffset(candidateLocal, offset);

                if (candidateDto > (requestedDate.EndDate ?? DateTimeOffset.MaxValue)) continue;
                if (candidateDto <= nextEligible) continue;
                if (!dates.Contains(candidateDto)) dates.Add(candidateDto);

                if (requestedDate.MaxIterations.HasValue && dates.Count >= requestedDate.MaxIterations.Value)
                    return dates;
            }

            weekStart = weekStart.AddDays(7 * requestedDate.WeeklyPeriod!.Value);
            iteration++;
        }

        dates.Sort();
        return dates;
    }

    private static DateTimeOffset? NextWeekday(DateTime startLocal, DayOfWeek day, TimeZoneInfo tz) {
        var date = startLocal.Date;
        while (date.DayOfWeek != day) 
            date = date.AddDays(1);

        var localWallClock = new DateTime(date.Year, date.Month, date.Day, startLocal.Hour, startLocal.Minute,
            startLocal.Second, DateTimeKind.Unspecified);
        return new DateTimeOffset(localWallClock, tz.GetUtcOffset(localWallClock));
    }
}