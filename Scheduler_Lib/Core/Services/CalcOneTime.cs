using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Interface;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Services;
public class CalcOneTime : ISchedule {
    public ResultPattern<SolvedDate> CalcDate(RequestedDate requestedDate) {
        var solution = new SolvedDate();
        if (requestedDate.ChangeDate != null) {
            return BuildResultForChangeDate(requestedDate);
        }

        if (requestedDate.Offset != null) {
            return BuildResultForOffset(requestedDate);
        }

        throw new OnceModeException(Messages.ErrorOnceMode);
    }

    private ResultPattern<SolvedDate> BuildResultForChangeDate(RequestedDate requestedDate) {
        var solution = new SolvedDate();
        solution.NewDate = requestedDate.ChangeDate.Value;
        solution.Description = BuildDescriptionForChangeDate(requestedDate);
        
        return ResultPattern<SolvedDate>.Success(solution);
    }

    private ResultPattern<SolvedDate> BuildResultForOffset(RequestedDate requestedDate) {
        var newDate = requestedDate.Date.AddDays(requestedDate.Offset.Value);
        if (newDate > requestedDate.EndDate || newDate < requestedDate.StartDate)
            return ResultPattern<SolvedDate>.Failure(Messages.ErrorChangeDateAfterEndDate);

        var solution = new SolvedDate();
        solution.NewDate = newDate;
        solution.Description = BuildDescriptionForOffset(newDate, requestedDate);

        return ResultPattern<SolvedDate>.Success(solution);
    }

    private string BuildDescriptionForChangeDate(RequestedDate requestedDate) =>
        $"Occurs once: Schedule will be used on {requestedDate.ChangeDate.Value:dd/MM/yyyy} at {requestedDate.ChangeDate.Value:HH:mm} starting on {requestedDate.StartDate:dd/MM/yyyy}";

    private string BuildDescriptionForOffset(DateTimeOffset newDate, RequestedDate requestedDate) =>
        $"Occurs Once: Schedule will be used on {newDate:dd/MM/yyyy HH:mm} starting on {requestedDate.StartDate:dd/MM/yyyy HH:mm}";
}
