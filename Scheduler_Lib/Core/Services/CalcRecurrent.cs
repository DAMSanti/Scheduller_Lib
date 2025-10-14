using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Infrastructure.Validations;

namespace Scheduler_Lib.Core.Services;
public class CalcRecurrent {
    public virtual ResultPattern<SolvedDate> CalculateDate(RequestedDate requestedDate) {
        var validation = Validations.ValidateRecurrent(requestedDate);

        return !validation.IsSuccess ? ResultPattern<SolvedDate>.Failure(validation.Error!) : ResultPattern<SolvedDate>.Success(BuildResultRecurrentDates(requestedDate));
    }

    private static SolvedDate BuildResultRecurrentDates(RequestedDate requestedDate) {
        var futureDates = CalculateFutureDates(requestedDate);

        var nextDateLocal = requestedDate.Date.Add(requestedDate.Period!.Value);

        return new SolvedDate {
            NewDate = nextDateLocal,
            Description = BuildDescription(requestedDate),
            FutureDates = futureDates
        };

    }

    private static List<DateTimeOffset> CalculateFutureDates(RequestedDate requestedDate) {
        var dates = new List<DateTimeOffset>();
        var endDate = requestedDate.EndDate ?? requestedDate.Date.Add(requestedDate.Period!.Value * 3);
        var weeklyPeriod = requestedDate.WeeklyPeriod!.Value;
        var daysOfWeek = requestedDate.DaysOfWeek!;
        var period = requestedDate.Period!.Value;

        var current = requestedDate.Date.Add(requestedDate.Period!.Value*2);

        while (current <= endDate) {
            foreach (var day in daysOfWeek) {
                var dayDate = NextWeekday(current, day);   

                if (dayDate < requestedDate.StartDate || dayDate > endDate)
                    continue;

                if (requestedDate.DailyStartTime.HasValue && requestedDate.DailyEndTime.HasValue) {
                    var slot = (dayDate.Date + requestedDate.DailyStartTime.Value).ToUniversalTime();

                    var lastSlot = (dayDate.Date + requestedDate.DailyEndTime.Value).ToUniversalTime();

                    while (slot <= lastSlot) {
                        if (slot > endDate) break;
                        if (slot >= requestedDate.StartDate && slot <= endDate)
                            dates.Add(slot);

                        slot = slot.Add(period);
                    }
                }
                else {
                    if (dayDate >= requestedDate.StartDate && dayDate <= endDate)
                        dates.Add(dayDate);
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

    private static string BuildDescription(RequestedDate requestedDate) {
        return $"Occurs every {requestedDate.Period!.Value} days. Schedule will be used on {requestedDate.Date.Date.ToShortDateString()}" +
               $" at {requestedDate.Date.Date.ToShortTimeString()} starting on {requestedDate.StartDate.Date.ToShortDateString()}";
    }
}
