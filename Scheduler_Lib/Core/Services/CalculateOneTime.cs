using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Infrastructure.Validations;

namespace Scheduler_Lib.Core.Services;
public class CalculateOneTime {
    public virtual ResultPattern<SchedulerOutput> CalculateDate(SchedulerInput requestedDate) {
        var validation = Validations.ValidateOnce(requestedDate);

        return !validation.IsSuccess ? ResultPattern<SchedulerOutput>.Failure(validation.Error!) : ResultPattern<SchedulerOutput>.Success(BuildResultForTargetDate(requestedDate));
    }

    private static SchedulerOutput BuildResultForTargetDate(SchedulerInput requestedDate) {
        List<DateTimeOffset>? futureDates = null;
        if (requestedDate.Recurrency == EnumRecurrency.Weekly) {
            futureDates = CalculateWeeklyRecurrence(requestedDate);
        }

        return new SchedulerOutput {
            NextDate = requestedDate.Recurrency == EnumRecurrency.Weekly ? SelectNextEligibleDate(requestedDate.TargetDate!.Value.UtcDateTime, requestedDate.DaysOfWeek!).UtcDateTime : requestedDate.TargetDate!.Value.UtcDateTime,
            Description = BuildDescriptionForTargetDate(requestedDate),
            FutureDates = futureDates
        };
    }
    private static DateTimeOffset SelectNextEligibleDate(DateTimeOffset targetDate, List<DayOfWeek> daysOfWeek) {
        var candidates = daysOfWeek
            .Select(day => NextWeekday(targetDate, day))
            .Where(date => date >= targetDate)
            .OrderBy(date => date)
            .ToList();

        return (DateTimeOffset)(candidates.Count > 0 ? candidates.First() : targetDate)!;
    }

    private static List<DateTimeOffset>? CalculateWeeklyRecurrence(SchedulerInput requestedDate) {
        var dates = new List<DateTimeOffset>();
        var current = requestedDate.TargetDate;
        int iteration = 0;

        for (var from = current; from <= requestedDate.EndDate && iteration < requestedDate.MaxIterations; from = from.Value.UtcDateTime.AddDays(7 * requestedDate.WeeklyPeriod!.Value), iteration++) {
            foreach (var day in requestedDate.DaysOfWeek!) {
                var nextDate = NextWeekday(from, day);
                if (nextDate > requestedDate.EndDate) break;
                if (!dates.Contains(nextDate.Value.UtcDateTime)) {
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
        if (requestedDate.Recurrency == EnumRecurrency.Weekly) {
            var daysOfWeek = string.Join(", ", requestedDate.DaysOfWeek!.Select(d => d.ToString()));
            var period = requestedDate.Period.HasValue ? $"{requestedDate.Period.Value.TotalDays} days" : "1 week";

            return $"Occurs every {requestedDate.WeeklyPeriod} week(s) on {daysOfWeek} every {period} " +
                   $"between {TimeSpanToString(requestedDate.DailyStartTime!.Value)} and " +
                   $"{TimeSpanToString(requestedDate.DailyEndTime!.Value)} starting on " +
                   $"{requestedDate.StartDate.Date.ToShortDateString()}";
        }

        return $"Occurs once: Schedule will be used on {requestedDate.TargetDate!.Value.UtcDateTime.Date.ToShortDateString()} " +
               $"at {requestedDate.TargetDate!.Value.UtcDateTime.Date.ToShortTimeString()} starting on " +
               $"{requestedDate.StartDate.Date.ToShortDateString()}";
    }

    private static string TimeSpanToString(TimeSpan timeSpan) {
        return timeSpan.ToString(@"hh\:mm");
    }

    private static DateTimeOffset? NextWeekday(DateTimeOffset? start, DayOfWeek day) {
        var date = start;
        while (date.Value.DayOfWeek != day)
            date = date.Value.AddDays(1);
        return date;
    }
}