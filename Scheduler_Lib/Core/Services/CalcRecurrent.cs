using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Interface;
using Scheduler_Lib.Infrastructure.Validations;

namespace Scheduler_Lib.Core.Services;
public class CalcRecurrent : ISchedule {
    public ResultPattern<SolvedDate> CalcDate(RequestedDate requestedDate) {
        Validations.ValidateRecurrent(requestedDate);

        var futureDates = CalculateFutureDates(requestedDate);

        var solution = new SolvedDate();
        solution.NewDate = requestedDate.Date.AddDays(requestedDate.Offset.Value);
        solution.Description =
            $"Occurs every {requestedDate.Offset.Value} days. Schedule will be used on {requestedDate.Date:dd/MM/yyyy}" +
            $" at {requestedDate.Date:HH:mm} starting on {requestedDate.StartDate:dd/MM/yyyy}";
        solution.FutureDates = futureDates;
        return ResultPattern<SolvedDate>.Success(solution);
    }

    public List<DateTimeOffset> CalculateFutureDates(RequestedDate requestedDate)
    {
        var dates = new List<DateTimeOffset>();
        dates.Clear();
        var endDate = requestedDate.EndDate ?? requestedDate.Date.AddDays(requestedDate.Offset.Value * 3);
        var current = requestedDate.Date.AddDays(requestedDate.Offset.Value*2);

        while (current <= endDate) {
            dates.Add(current);
            current = current.AddDays(requestedDate.Offset.Value);
        }

        return dates;
    }
}
