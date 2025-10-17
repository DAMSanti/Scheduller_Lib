using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Resources;
using System.Text;

namespace Scheduler_Lib.Infrastructure.Validations;

public class ValidationOnce {
    public static ResultPattern<bool> ValidateOnce(SchedulerInput requestedDate) {
        var errors = new StringBuilder();

        if (requestedDate.EndDate != null && requestedDate.StartDate > requestedDate.EndDate)
            errors.AppendLine(Messages.ErrorStartDatePostEndDate);

        if (requestedDate.TargetDate == null && requestedDate.Recurrency != EnumRecurrency.Weekly)
            errors.AppendLine(Messages.ErrorTargetDateNull);

        if (requestedDate.TargetDate != null && ((requestedDate.TargetDate < requestedDate.StartDate || requestedDate.TargetDate > requestedDate.EndDate)))
            errors.AppendLine(Messages.ErrorTargetDateAfterEndDate);

        return errors.Length > 0 ? ResultPattern<bool>.Failure(errors.ToString()) : ResultPattern<bool>.Success(true);
    }
}