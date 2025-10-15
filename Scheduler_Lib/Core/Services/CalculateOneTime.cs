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
        if (requestedDate.Recurrency == EnumRecurrency.Weekly)  futureDates = CalculateWeeklyRecurrence(requestedDate);

        return new SchedulerOutput {
            NextDate = requestedDate.TargetDate!.Value,
            Description = BuildDescriptionForTargetDate(requestedDate),
            FutureDates = futureDates
        };
    }

    private static List<DateTimeOffset> CalculateWeeklyRecurrence(SchedulerInput requestedDate) {
        var dates = new List<DateTimeOffset>();
        var endDate = requestedDate.EndDate ?? requestedDate.StartDate.AddDays(7 * 3);
        var current = requestedDate.TargetDate ?? requestedDate.StartDate;

        while (current <= endDate) {
            foreach (var day in requestedDate.DaysOfWeek!) {
                var nextDate = NextWeekday(current, day);
                if (nextDate > endDate) continue;
                if (!dates.Contains(nextDate)) {
                    dates.Add(nextDate);
                }
            }
            current = current.AddDays(7 * requestedDate.WeeklyPeriod!.Value);
        }
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

        return $"Occurs once: Schedule will be used on {requestedDate.TargetDate!.Value.Date.ToShortDateString()} " +
               $"at {requestedDate.TargetDate!.Value.Date.ToShortTimeString()} starting on " +
               $"{requestedDate.StartDate.Date.ToShortDateString()}";
    }

    private static string TimeSpanToString(TimeSpan timeSpan) {
        return timeSpan.ToString(@"hh\:mm");
    }
    private static DateTimeOffset NextWeekday(DateTimeOffset start, DayOfWeek day)
    {
        var daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
        return start.AddDays(daysToAdd);
    }
}