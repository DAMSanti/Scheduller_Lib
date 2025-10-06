using Scheduler_Lib.Classes;
using Scheduler_Lib.Interface;

namespace Scheduler_Lib.Services;
public class CalcRecurrent : ISchedule {
    private List<DateTimeOffset> _futureDates = new();
    public SolvedDate CalcDate(RequestedDate requestedDate) {

        Validations.Validations.ValidateRecurrent(requestedDate);

        _futureDates.Clear();

        if (requestedDate.EndDate == null) {
            requestedDate.EndDate = requestedDate.Date.AddDays(requestedDate.Offset.Value * 3);
        }

        var current = requestedDate.Date.AddDays(requestedDate.Offset.Value);
        var nextDate = current;


        while (current <= requestedDate.EndDate) {
            _futureDates.Add(current);
            current = current.AddDays(requestedDate.Offset.Value);
        }

        var solucion = new SolvedDate();
        solucion.NewDate = nextDate;
        solucion.Description =
            $"Occurs every {requestedDate.Offset.Value} days. Schedule will be used on {requestedDate.Date:dd/MM/yyyy}" +
            $" at {requestedDate.Date:HH:mm} starting on {requestedDate.StartDate:dd/MM/yyyy}";
        solucion.FutureDates = _futureDates;
        return solucion;
    }
}
