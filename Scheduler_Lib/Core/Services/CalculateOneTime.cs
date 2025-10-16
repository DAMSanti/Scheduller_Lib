using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Infrastructure.Validations;

namespace Scheduler_Lib.Core.Services;
public class CalculateOneTime {
    public virtual ResultPattern<SchedulerOutput> CalculateDate(SchedulerInput requestedDate) {
        var validation = ValidationOnce.ValidateOnce(requestedDate);

        return !validation.IsSuccess ? ResultPattern<SchedulerOutput>.Failure(validation.Error!) : ResultPattern<SchedulerOutput>.Success(BuildResultForTargetDate(requestedDate));
    }

    private static SchedulerOutput BuildResultForTargetDate(SchedulerInput requestedDate) {
        List<DateTimeOffset>? futureDates = null;
        if (requestedDate.Recurrency == EnumRecurrency.Weekly) {
            futureDates = CalculateWeeklyRecurrence(requestedDate);
        }

        var tz = GetTimeZone(requestedDate);
        var next = requestedDate.Recurrency == EnumRecurrency.Weekly
            ? SelectNextEligibleDate(requestedDate.TargetDate!.Value, requestedDate.DaysOfWeek!, tz)
            : requestedDate.TargetDate!.Value;

        return new SchedulerOutput {
            NextDate = next,
            Description = BuildDescriptionForTargetDate(requestedDate),
            FutureDates = futureDates
        };
    }

    private static DateTimeOffset SelectNextEligibleDate(DateTimeOffset targetDate, List<DayOfWeek> daysOfWeek, TimeZoneInfo tz) {
        var candidates = daysOfWeek
            .Select(day => NextWeekday(targetDate, day, tz))
            .Where(date => date >= targetDate)
            .OrderBy(date => date)
            .Select(date => date!.Value)
            .ToList();

        return (candidates.Count > 0 ? candidates.First() : targetDate)!;
    }

    private static List<DateTimeOffset>? CalculateWeeklyRecurrence(SchedulerInput requestedDate) {
        var tz = GetTimeZone(requestedDate);

        var dates = new List<DateTimeOffset>();
        var current = requestedDate.TargetDate;
        int iteration = 0;

        var baseDate = requestedDate.TargetDate ?? requestedDate.StartDate;
        var baseInZone = TimeZoneInfo.ConvertTime(baseDate, tz);

        var nextEligible = SelectNextEligibleDate(baseInZone, requestedDate.DaysOfWeek ?? new List<DayOfWeek>(), tz);

        for (var from = current; from <= requestedDate.EndDate && iteration < (requestedDate.MaxIterations ?? int.MaxValue); from = from.Value.AddDays(7 * requestedDate.WeeklyPeriod!.Value), iteration++) {
            foreach (var day in requestedDate.DaysOfWeek!) {
                var nextDate = NextWeekday(from, day, tz);
                if (nextDate == null) continue;
                if (nextDate > requestedDate.EndDate) break;

                if (nextDate.Value <= nextEligible) continue;

                if (!dates.Contains(nextDate.Value)) {
                    dates.Add(nextDate.Value);
                    if (requestedDate.MaxIterations.HasValue && dates.Count >= requestedDate.MaxIterations.Value)
                        return dates;
                }
            }
            if (requestedDate.MaxIterations.HasValue && dates.Count >= requestedDate.MaxIterations.Value)
                break;
        }
        dates.Sort();
        return dates;
    }

    private static string BuildDescriptionForTargetDate(SchedulerInput requestedDate) {
        var tz = GetTimeZone(requestedDate);
        if (requestedDate.Recurrency == EnumRecurrency.Weekly) {
            var daysOfWeek = string.Join(", ", requestedDate.DaysOfWeek!.Select(d => d.ToString()));
            var period = requestedDate.Period.HasValue ? $"{requestedDate.Period.Value.TotalDays} days" : "1 week";

            if (requestedDate.Periodicity == EnumConfiguration.Recurrent)
                return $"Occurs every {requestedDate.WeeklyPeriod} week(s) on {daysOfWeek} every {period} " +
                       $"between {TimeSpanToString(requestedDate.DailyStartTime!.Value)} and " +
                       $"{TimeSpanToString(requestedDate.DailyEndTime!.Value)} starting on " +
                       $"{requestedDate.StartDate.Date.ToShortDateString()}";
            else {
                var next = SelectNextEligibleDate(requestedDate.TargetDate!.Value, requestedDate.DaysOfWeek!, tz);
                return $"Occurs every {daysOfWeek}: Schedule will be used on {next.Date.ToShortDateString()} " +
                       $"at {requestedDate.TargetDate!.Value.Date.ToShortTimeString()} starting on " +
                       $"{requestedDate.StartDate.Date.ToShortDateString()}";
            }
        }
        var targetInZone = TimeZoneInfo.ConvertTime(requestedDate.TargetDate!.Value, tz);
        var startInZone = TimeZoneInfo.ConvertTime(requestedDate.StartDate, tz);

        return $"Occurs once: Schedule will be used on {targetInZone.Date.ToShortDateString()} " +
               $"at {targetInZone.DateTime.ToShortTimeString()} starting on " +
               $"{startInZone.Date.ToShortDateString()}";
    }

    private static string TimeSpanToString(TimeSpan timeSpan) {
        return timeSpan.ToString(@"hh\:mm");
    }

    private static DateTimeOffset? NextWeekday(DateTimeOffset? start, DayOfWeek day, TimeZoneInfo tz) {
        var date = TimeZoneInfo.ConvertTime(start!.Value, tz).DateTime;
        while (date.DayOfWeek != day)
            date = date.AddDays(1);
        return date;
    }

    private static TimeZoneInfo GetTimeZone(SchedulerInput requestedDate) {
        return TimeZoneInfo.FindSystemTimeZoneById(requestedDate.TimeZoneId!);
    }
}