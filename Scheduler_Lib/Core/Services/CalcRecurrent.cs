using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Interface;
using Scheduler_Lib.Infrastructure.Validations;

namespace Scheduler_Lib.Core.Services;
public class CalcRecurrent : ISchedule {
    private readonly List<DateTimeOffset> _futureDates = [];
    public ResultPattern<SolvedDate> CalcDate(RequestedDate requestedDate) {

        Validations.ValidateRecurrent(requestedDate);

        _futureDates.Clear();

        if (requestedDate.EndDate == null) {
            requestedDate.EndDate = requestedDate.Date.AddDays(requestedDate.Offset.Value * 3);
        }

        var current = requestedDate.Date.AddDays(requestedDate.Offset.Value);
        var nextDate = current;


        while (current <= requestedDate.EndDate) {
            current = current.AddDays(requestedDate.Offset.Value);
            _futureDates.Add(current);
        }

        var solution = new SolvedDate();
        solution.NewDate = nextDate;
        solution.Description =
            $"Occurs every {requestedDate.Offset.Value} days. Schedule will be used on {requestedDate.Date:dd/MM/yyyy}" +
            $" at {requestedDate.Date:HH:mm} starting on {requestedDate.StartDate:dd/MM/yyyy}";
        solution.FutureDates = _futureDates;
        return ResultPattern<SolvedDate>.Success(solution);
    }
}
