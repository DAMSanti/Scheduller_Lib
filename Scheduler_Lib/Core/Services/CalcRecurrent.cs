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

        
            return new SolvedDate
        {
            NewDate = requestedDate.Date.AddDays(requestedDate.Period!.Value),
            Description = BuildDescription(requestedDate),
            FutureDates = futureDates
        };

    }

    private static List<DateTimeOffset> CalculateFutureDates(RequestedDate requestedDate) {
        var dates = new List<DateTimeOffset>();
        var endDate = requestedDate.EndDate ?? requestedDate.Date.AddDays(requestedDate.Period!.Value * 3);
        var current = requestedDate.Date.AddDays(requestedDate.Period!.Value*2);

        while (current <= endDate) {
            dates.Add(current);
            current = current.AddDays(requestedDate.Period.Value);
        }

        return dates;
    }

    private static string BuildDescription(RequestedDate requestedDate) {
        return $"Occurs every {requestedDate.Period!.Value} days. Schedule will be used on {requestedDate.Date.Date.ToShortDateString()}" +
               $" at {requestedDate.Date.Date.ToShortTimeString()} starting on {requestedDate.StartDate.Date.ToShortDateString()}";
    }
}
