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

        return BuildResultRecurrentDates(requestedDate);
    }

    private ResultPattern<SolvedDate> BuildResultRecurrentDates(RequestedDate requestedDate) {
        var futureDates = CalculateFutureDates(requestedDate);

        var solution = new SolvedDate();
        solution.NewDate = requestedDate.Date.AddDays(requestedDate.Offset!.Value);
        solution.Description = BuildDescription(requestedDate);
        solution.FutureDates = futureDates;

        return ResultPattern<SolvedDate>.Success(solution);
    }

    private List<DateTimeOffset> CalculateFutureDates(RequestedDate requestedDate) {
        var dates = new List<DateTimeOffset>();
        var endDate = requestedDate.EndDate ?? requestedDate.Date.AddDays(requestedDate.Offset!.Value * 3);
        var current = requestedDate.Date.AddDays(requestedDate.Offset!.Value*2);

        while (current <= endDate) {
            dates.Add(current);
            current = current.AddDays(requestedDate.Offset.Value);
        }

        return dates;
    }

    private string BuildDescription(RequestedDate requestedDate) {
        return $"Occurs every {requestedDate.Offset!.Value} days. Schedule will be used on {requestedDate.Date.Date.ToShortDateString()}" +
               $" at {requestedDate.Date.Date.ToShortTimeString()} starting on {requestedDate.StartDate.Date.ToShortDateString()}";
    }
}
