using Scheduler_Lib.Classes;
using Scheduler_Lib.Interface;

namespace Scheduler_Lib.Services;
public class CalcRecurrent : ISchedule {
    private List<DateTimeOffset> _futureDates = new();
    public SolvedDate CalcDate(RequestedDate requestedDate) {

        Validations.Validations.ValidateRecurrent(requestedDate);

        _futureDates.Clear();

        var current = requestedDate.Date.Add(requestedDate.Offset.Value);
        var nextDate = current;

        while (current <= requestedDate.EndDate) {
            _futureDates.Add(current);
            current = current.Add(requestedDate.Offset.Value);
        }

        return new SolvedDate
        {
            NewDate = nextDate,
            Description = $"Occurs every {requestedDate.Offset.Value.Days} days. Schedule will be used on {requestedDate.Date:dd/MM/yyyy}" +
                          $" at {requestedDate.Date:HH:mm} starting on {requestedDate.StartDate:dd/MM/yyyy}",
            FutureDates = _futureDates
        };
    }
}
