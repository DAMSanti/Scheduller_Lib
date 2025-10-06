using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Interface;
using Scheduler_Lib.Core.Model.Messages;

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
                solucion.Description = Messages.ErrorChangeDateAfterEndDate;
                return solucion;
            }

            solucion.NewDate = newDate;
            solucion.Description =
                $"Occurs Once: Schedule will be used on {newDate:dd/MM/yyyy HH:mm} starting on {requestedDate.StartDate:dd/MM/yyyy HH:mm}";
            return solucion;
        }

        throw new Exception(Messages.ErrorOnceMode);
    }
}
