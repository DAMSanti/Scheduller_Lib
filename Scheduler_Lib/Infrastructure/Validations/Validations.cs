using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Resources;
using System.Text;

namespace Scheduler_Lib.Infrastructure.Validations;
public static class Validations {
    public static ResultPattern<bool> ValidateRecurrent(RequestedDate requestedDate) {
        var errors = new StringBuilder();

        if (requestedDate.Period is not > 0) {
            errors.AppendLine(Messages.ErrorPositiveOffsetRequired);
        }

        if (requestedDate.StartDate > requestedDate.EndDate) {
            errors.AppendLine(Messages.ErrorStartDatePostEndDate);
        }

        if (requestedDate.Date < requestedDate.StartDate || requestedDate.Date > requestedDate.EndDate) {
            errors.AppendLine(Messages.ErrorDateOutOfRange);
        }

        return errors.Length > 0 ? ResultPattern<bool>.Failure(errors.ToString()) : ResultPattern<bool>.Success(true);
    }

    public static ResultPattern<bool> ValidateOnce(RequestedDate requestedDate) {
        var errors = new StringBuilder();

        if (requestedDate.ChangeDate == null) {
            errors.AppendLine(Messages.ErrorOnceMode);
        }
        if (requestedDate.StartDate > requestedDate.EndDate) {
            errors.AppendLine(Messages.ErrorChangeDateAfterEndDate);
        }

        if (requestedDate.ChangeDate == null) {
            errors.AppendLine(Messages.ErrorChangeDateNull);
        }

        if (requestedDate.EndDate == null) {
            errors.AppendLine(Messages.ErrorEndDateNull);
        }

        if (requestedDate.ChangeDate < requestedDate.StartDate || requestedDate.ChangeDate > requestedDate.EndDate) {
            errors.AppendLine(Messages.ErrorChangeDateAfterEndDate);
        }

        return errors.Length > 0 ? ResultPattern<bool>.Failure(errors.ToString()) : ResultPattern<bool>.Success(true);
    }

    public static ResultPattern<bool> ValidateCalc(RequestedDate? requestedDate)
    {
        return requestedDate == null ? ResultPattern<bool>.Failure(Messages.ErrorRequestNull) : ResultPattern<bool>.Success(true);
    }
}