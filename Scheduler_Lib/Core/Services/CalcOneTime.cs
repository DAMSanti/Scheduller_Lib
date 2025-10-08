using Scheduler_Lib.Core.Interface;
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Infrastructure.Validations;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Services;
public class CalcOneTime : ISchedule {
    public ResultPattern<SolvedDate> CalcDate(RequestedDate requestedDate) {
        Validations.ValidateOnce(requestedDate);

        var solution = new SolvedDate();     

        return BuildResultForChangeDate(requestedDate);
    }

    private ResultPattern<SolvedDate> BuildResultForChangeDate(RequestedDate requestedDate) {
        var solution = new SolvedDate();
        solution.NewDate = requestedDate.ChangeDate.Value;
        solution.Description = BuildDescriptionForChangeDate(requestedDate);
        
        return ResultPattern<SolvedDate>.Success(solution);
    }

    private string BuildDescriptionForChangeDate(RequestedDate requestedDate) {
        return $"Occurs once: Schedule will be used on {requestedDate.ChangeDate.Value.Date.ToShortDateString()} at {requestedDate.ChangeDate.Value.Date.ToShortTimeString()} starting on {requestedDate.StartDate.Date.ToShortDateString()}";
    }
}
