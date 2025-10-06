using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Interface;

namespace Scheduler_Lib.Core.Services;
public class CalcOneTime : ISchedule {
    public SolvedDate CalcDate(RequestedDate requestedDate) {
        var solution = new SolvedDate();
        if (requestedDate.ChangeDate != null) {
            solution.NewDate = requestedDate.ChangeDate.Value;
            solution.Description =
                $"Occurs once: Schedule will be used on {requestedDate.ChangeDate.Value:dd/MM/yyyy} at {requestedDate.ChangeDate.Value:HH:mm} starting on {requestedDate.StartDate:dd/MM/yyyy}";
            return solution;
        }

        if (requestedDate.Offset != null) {
            var newDate = requestedDate.Date.AddDays(requestedDate.Offset.Value);
            if (newDate > requestedDate.EndDate || newDate < requestedDate.StartDate) {
                solution.NewDate = requestedDate.Date;
                solution.Description = Messages.ErrorChangeDateAfterEndDate;
                return solution;
            }

            solution.NewDate = newDate;
            solution.Description =
                $"Occurs Once: Schedule will be used on {newDate:dd/MM/yyyy HH:mm} starting on {requestedDate.StartDate:dd/MM/yyyy HH:mm}";
            return solution;
        }

        throw new Exception(Messages.ErrorOnceMode);
    }
}
