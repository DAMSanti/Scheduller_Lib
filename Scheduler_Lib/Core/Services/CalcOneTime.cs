using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Infrastructure.Validations;

namespace Scheduler_Lib.Core.Services;
public class CalcOneTime {
    public virtual ResultPattern<SolvedDate> CalculateDate(RequestedDate requestedDate) {
        var validation = Validations.ValidateOnce(requestedDate);

        return !validation.IsSuccess ? ResultPattern<SolvedDate>.Failure(validation.Error!) : ResultPattern<SolvedDate>.Success(BuildResultForChangeDate(requestedDate));
    }

    private static SolvedDate BuildResultForChangeDate(RequestedDate requestedDate) {
        var newDateLocal = requestedDate.ChangeDate!.Value.DateTime;
        var newDateConverted = new DateTimeOffset(newDateLocal, requestedDate.TimeZonaId.GetUtcOffset(newDateLocal));
        
        return new SolvedDate {
            NewDate = newDateConverted,
            Description = BuildDescriptionForChangeDate(requestedDate, newDateConverted)
        };
    }

    private static string BuildDescriptionForChangeDate(RequestedDate requestedDate, DateTimeOffset newDateConverted) {
        return $"Occurs once: Schedule will be used on {newDateConverted.Date.ToShortDateString()} at {newDateConverted.Date.ToShortTimeString()} starting on {requestedDate.StartDate.Date.ToShortDateString()}";
    }
}
