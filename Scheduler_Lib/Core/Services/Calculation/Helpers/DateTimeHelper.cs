using Scheduler_Lib.Core.Model;

namespace Scheduler_Lib.Core.Services.Calculation.Helpers;

public class DateTimeHelper {
    public DateTime GetBaseLocal(SchedulerInput schedulerInput) {
        if (schedulerInput.TargetDate.HasValue)
            return schedulerInput.TargetDate.Value.DateTime;

        return schedulerInput.CurrentDate != default
            ? schedulerInput.CurrentDate.DateTime
            : schedulerInput.StartDate.DateTime;
    }

    public DateTimeOffset GetBaseDateTimeOffset(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        if (schedulerInput.TargetDate.HasValue) {
            var td = schedulerInput.TargetDate.Value.DateTime;
            return CreateDateTimeOffset(td, tz);
        }

        if (schedulerInput.CurrentDate != default) {
            var cur = schedulerInput.CurrentDate.DateTime;
            var startTime = schedulerInput.StartDate.TimeOfDay;
            var baseLocal = new DateTime(cur.Year, cur.Month, cur.Day,
                startTime.Hours, startTime.Minutes, startTime.Seconds, DateTimeKind.Unspecified);

            return new DateTimeOffset(baseLocal, tz.GetUtcOffset(baseLocal));
        }

        var sd = schedulerInput.StartDate.DateTime;
        return new DateTimeOffset(sd, tz.GetUtcOffset(sd));
    }

    public DateTimeOffset CreateDateTimeOffset(DateTime localWallClock, TimeZoneInfo tz) {
        return new DateTimeOffset(localWallClock, tz.GetUtcOffset(localWallClock));
    }

    public DateTime? GetCandidateLocalForWeekAndDay(DateTime weekStart, DayOfWeek day, TimeSpan timeOfDay) {
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

    public bool TryAddDaysSafely(DateTime dt, int days, out DateTime result) {
        result = dt;
        if (days == 0) return true;
        if (dt > DateTime.MaxValue.AddDays(-days)) return false;
        result = dt.AddDays(days);
        return true;
    }

    public DateTimeOffset NextWeekday(DateTime startLocal, DayOfWeek day, TimeZoneInfo tz) {
        var date = startLocal.Date;
        while (date.DayOfWeek != day)
            date = date.AddDays(1);

        var localWallClock = new DateTime(date.Year, date.Month, date.Day,
            startLocal.Hour, startLocal.Minute, startLocal.Second, DateTimeKind.Unspecified);

        return CreateDateTimeOffset(localWallClock, tz);
    }

    public DateTimeOffset GetEffectiveEndDate(SchedulerInput schedulerInput) {
        if (schedulerInput.EndDate.HasValue)
            return schedulerInput.EndDate.Value;

        var period = schedulerInput.DailyPeriod ?? TimeSpan.FromDays(3);
        var beginning = GetBaseLocal(schedulerInput);

        const int defaultPeriodMultiplier = 1000;
        return beginning.Add(period * defaultPeriodMultiplier);
    }
}