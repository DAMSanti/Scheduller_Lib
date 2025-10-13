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

        return ResultPattern<SolvedDate>.Success(BuildResultForChangeDate(requestedDate));
    }

    private static SolvedDate BuildResultForChangeDate(RequestedDate requestedDate) {
        var newDateLocal = requestedDate.ChangeDate!.Value.DateTime;
        var newDateConverted = new DateTimeOffset(newDateLocal, requestedDate.TimeZonaId.GetUtcOffset(newDateLocal));
        
        var solution = new SolvedDate();
        solution.NewDate = newDateConverted;
        solution.Description = BuildDescriptionForChangeDate(requestedDate, newDateConverted);
        
        return solution;
    }

    private static string BuildDescriptionForChangeDate(RequestedDate requestedDate, DateTimeOffset newDateConverted) {
        return $"Occurs once: Schedule will be used on {newDateConverted.Date.ToShortDateString()} at {newDateConverted.Date.ToShortTimeString()} starting on {requestedDate.StartDate.Date.ToShortDateString()}";
    }
}
