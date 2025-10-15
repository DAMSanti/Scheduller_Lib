using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Infrastructure.Validations;

namespace Scheduler_Lib.Core.Services;
public class CalculateOneTime {
    public virtual ResultPattern<SchedulerOutput> CalculateDate(SchedulerInput requestedDate) {
        var validation = Validations.ValidateOnce(requestedDate);

        return !validation.IsSuccess ? ResultPattern<SchedulerOutput>.Failure(validation.Error!) : ResultPattern<SchedulerOutput>.Success(BuildResultForTargetDate(requestedDate));
    }

    private static SchedulerOutput BuildResultForTargetDate(SchedulerInput requestedDate) {
        var newDate = requestedDate.TargetDate!.Value;

        List<DateTimeOffset>? futureDates = null;
        if (requestedDate.Recurrency == EnumRecurrency.Weekly)  futureDates = CalculateWeeklyRecurrence(requestedDate);

        return new SchedulerOutput {
            NextDate = newDate,
            Description = BuildDescriptionForTargetDate(requestedDate, newDate),
            FutureDates = futureDates
        };
    }

    private static List<DateTimeOffset> CalculateWeeklyRecurrence(SchedulerInput requestedDate) {
        var dates = new List<DateTimeOffset>();
        var endDate = requestedDate.EndDate ?? requestedDate.StartDate.AddDays(7 * 3);
        var weeklyPeriod = requestedDate.WeeklyPeriod!.Value;
        var daysOfWeek = requestedDate.DaysOfWeek!;

        DateTimeOffset current = requestedDate.TargetDate.HasValue ? requestedDate.TargetDate.Value : requestedDate.StartDate;

        while (current <= endDate) {
            foreach (var day in daysOfWeek) {
                var nextDate = NextWeekday(current, day);
                if (nextDate > endDate) continue;
                if (!dates.Contains(nextDate)) {
                    dates.Add(nextDate);
                }
            }
            current = current.AddDays(7 * weeklyPeriod);
        }
        return dates;
    }

    private static DateTimeOffset NextWeekday(DateTimeOffset start, DayOfWeek day) {
        var daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
        return start.AddDays(daysToAdd);
    }

    private static string BuildDescriptionForTargetDate(SchedulerInput requestedDate, DateTimeOffset newDateConverted) {
        if (requestedDate.Recurrency == EnumRecurrency.Weekly) {
            var daysOfWeek = string.Join(", ", requestedDate.DaysOfWeek!.Select(d => d.ToString()));
            var period = requestedDate.Period.HasValue ? $"{requestedDate.Period.Value.TotalDays} days" : "1 week";
            var StartTime = TimeSpanToString(requestedDate.DailyStartTime!.Value);
            var EndTime = TimeSpanToString(requestedDate.DailyEndTime!.Value);
            var startingDate = requestedDate.StartDate.Date.ToShortDateString();

            return $"Occurs every {requestedDate.WeeklyPeriod} week(s) on {daysOfWeek} every {period} between {StartTime} and {EndTime} starting on {startingDate}";
        }

        return $"Occurs once: Schedule will be used on {newDateConverted.Date.ToShortDateString()} at {newDateConverted.Date.ToShortTimeString()} starting on {requestedDate.StartDate.Date.ToShortDateString()}";
    }

    private static string TimeSpanToString(TimeSpan ts) {
        return ts.ToString(@"hh\:mm");
    }
}
 