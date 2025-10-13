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
        var NewDateLocal = requestedDate.ChangeDate!.Value.DateTime;
        var NewDateConverted = new DateTimeOffset(NewDateLocal, requestedDate.TimeZonaId.GetUtcOffset(NewDateLocal));
        
        var solution = new SolvedDate();
        solution.NewDate = NewDateConverted;
        solution.Description = BuildDescriptionForChangeDate(requestedDate, NewDateConverted);
        
        return solution;
    }

    private static string BuildDescriptionForChangeDate(RequestedDate requestedDate, DateTimeOffset NewDateConverted) {
        return $"Occurs once: Schedule will be used on {NewDateConverted.Date.ToShortDateString()} at {NewDateConverted.Date.ToShortTimeString()} starting on {requestedDate.StartDate.Date.ToShortDateString()}";
    }
}
