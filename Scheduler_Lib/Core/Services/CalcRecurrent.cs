using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Interface;
using Scheduler_Lib.Infrastructure.Validations;

namespace Scheduler_Lib.Core.Services;
public class CalcRecurrent : ISchedule {
    public ResultPattern<SolvedDate> CalcDate(RequestedDate requestedDate) {
        var validation = Validations.ValidateRecurrent(requestedDate);
        if (!validation.IsSuccess) {
            return ResultPattern<SolvedDate>.Failure(validation.Error!);
        }

        return ResultPattern<SolvedDate>.Success(BuildResultRecurrentDates(requestedDate));
    }

    private static SolvedDate BuildResultRecurrentDates(RequestedDate requestedDate) {
        var futureDates = CalculateFutureDates(requestedDate);

        var solution = new SolvedDate();
        solution.NewDate = requestedDate.Date.AddDays(requestedDate.Period!.Value);
        solution.Description = BuildDescription(requestedDate);
        solution.FutureDates = futureDates;

        return solution;
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
