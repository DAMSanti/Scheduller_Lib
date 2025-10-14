using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Infrastructure.Validations;

namespace Scheduler_Lib.Core.Services;
public class CalcOneTime {
    public virtual ResultPattern<SolvedDate> CalculateDate(RequestedDate requestedDate) {
        var validation = Validations.ValidateOnce(requestedDate);

        return !validation.IsSuccess ? ResultPattern<SolvedDate>.Failure(validation.Error!) : ResultPattern<SolvedDate>.Success(BuildResultForChangeDate(requestedDate));
    }

    private static SolvedDate BuildResultForChangeDate(RequestedDate requestedDate) {
        var newDateLocal = requestedDate.ChangeDate!.Value.DateTime;
        var newDateConverted = new DateTimeOffset(newDateLocal, requestedDate.TimeZonaId.GetUtcOffset(newDateLocal));

        List<DateTimeOffset>? futureDates = null;
        if (requestedDate.Ocurrence == EnumOcurrence.Weekly && requestedDate.WeeklyPeriod.HasValue && requestedDate.DaysOfWeek != null && requestedDate.DaysOfWeek.Count > 0) {
            futureDates = CalculateWeeklyRecurrence(requestedDate, newDateConverted);
        }

        return new SolvedDate {
            NewDate = newDateConverted,
            Description = BuildDescriptionForChangeDate(requestedDate, newDateConverted),
            FutureDates = futureDates
        };
    }

    private static List<DateTimeOffset> CalculateWeeklyRecurrence(RequestedDate requestedDate, DateTimeOffset startDate) {
        var dates = new List<DateTimeOffset>();
        var endDate = requestedDate.EndDate ?? startDate.AddDays(7 * 3);
        var weeklyPeriod = requestedDate.WeeklyPeriod!.Value;
        var daysOfWeek = requestedDate.DaysOfWeek!;

        var current = startDate.Date;
        while (current <= endDate) {
            foreach (var day in daysOfWeek) {
                var nextDate = NextWeekday(current, day);
                if (nextDate > endDate) continue;
                if (!dates.Contains(nextDate)) {
                    var dateWithOffset = new DateTimeOffset(nextDate, requestedDate.TimeZonaId.GetUtcOffset(nextDate));
                    dates.Add(dateWithOffset);
                }
            }
            current = current.AddDays(7 * weeklyPeriod);
        }
        dates.Sort();
        return dates;
    }

    private static DateTime NextWeekday(DateTime start, DayOfWeek day) {
        var daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
        return start.AddDays(daysToAdd);
    }

    private static string BuildDescriptionForChangeDate(RequestedDate requestedDate, DateTimeOffset newDateConverted) {
        if (requestedDate.Ocurrence == EnumOcurrence.Weekly
            && requestedDate.WeeklyPeriod.HasValue
            && requestedDate.DaysOfWeek != null
            && requestedDate.DaysOfWeek.Count > 0
            && requestedDate.DailyStartTime.HasValue
            && requestedDate.DailyEndTime.HasValue)
        {
            var daysOfWeek = string.Join(", ", requestedDate.DaysOfWeek.Select(d => d.ToString()));
            var period = requestedDate.Period.HasValue ? $"{requestedDate.Period.Value.TotalDays} days" : "1 week";
            var horaInicio = TimeSpanToString(requestedDate.DailyStartTime.Value);
            var horaFin = TimeSpanToString(requestedDate.DailyEndTime.Value);
            var startingDate = requestedDate.StartDate.Date.ToShortDateString();

            return $"Occurs every {requestedDate.WeeklyPeriod} week(s) on {daysOfWeek} every {period} between {horaInicio} and {horaFin} starting on {startingDate}";
        }

        return $"Occurs once: Schedule will be used on {newDateConverted.Date.ToShortDateString()} at {newDateConverted.Date.ToShortTimeString()} starting on {requestedDate.StartDate.Date.ToShortDateString()}";
    }

    private static string TimeSpanToString(TimeSpan ts) {
        return ts.ToString(@"hh\:mm");
    }
}
 