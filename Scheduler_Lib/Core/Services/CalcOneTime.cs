using Scheduler_Lib.Core.Interface;
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Infrastructure.Validations;

namespace Scheduler_Lib.Core.Services;
public class CalcOneTime : ISchedule {
    public ResultPattern<SolvedDate> CalcDate(RequestedDate requestedDate) {
        var validation = Validations.ValidateOnce(requestedDate);
        if (!validation.IsSuccess) {
            return ResultPattern<SolvedDate>.Failure(validation.Error!);
        }

        return BuildResultForChangeDate(requestedDate);
    }

    private ResultPattern<SolvedDate> BuildResultForChangeDate(RequestedDate requestedDate) {
        var solution = new SolvedDate();
        solution.NewDate = requestedDate.ChangeDate!.Value;
        solution.Description = BuildDescriptionForChangeDate(requestedDate);
        
        return ResultPattern<SolvedDate>.Success(solution);
    }

    private string BuildDescriptionForChangeDate(RequestedDate requestedDate) {
        return $"Occurs once: Schedule will be used on {requestedDate.ChangeDate!.Value.Date.ToShortDateString()} at {requestedDate.ChangeDate.Value.Date.ToShortTimeString()} starting on {requestedDate.StartDate.Date.ToShortDateString()}";
    }
}
