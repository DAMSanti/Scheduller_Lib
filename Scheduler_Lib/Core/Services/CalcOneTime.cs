using Scheduler_Lib.Core.Classes;
using Scheduler_Lib.Core.Interface;

namespace Scheduler_Lib.Core.Services;
public class CalcOneTime : ISchedule {
    public SolvedDate CalcDate(RequestedDate requestedDate) {
        var solucion = new SolvedDate();
        if (requestedDate.ChangeDate != null) {
            solucion.NewDate = requestedDate.ChangeDate.Value;
            solucion.Description =
                $"Occurs once: Schedule will be used on {requestedDate.ChangeDate.Value:dd/MM/yyyy} at {requestedDate.ChangeDate.Value:HH:mm} starting on {requestedDate.StartDate:dd/MM/yyyy}";
            return solucion;
        }

        if (requestedDate.Offset != null) {
            var newDate = requestedDate.Date.AddDays(requestedDate.Offset.Value);
            if (newDate > requestedDate.EndDate || newDate < requestedDate.StartDate) {
                solucion.NewDate = requestedDate.Date;
                solucion.Description = "ERROR: The given date is after the end date.";
                return solucion;
            }

            solucion.NewDate = newDate;
            solucion.Description =
                $"Occurs Once: Schedule will be used on {newDate:dd/MM/yyyy HH:mm} starting on {requestedDate.StartDate:dd/MM/yyyy HH:mm}";
            return solucion;
        }

        throw new Exception("New date time or offset required in Once mode.");
    }
}
