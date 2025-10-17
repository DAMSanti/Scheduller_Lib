using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Resources;
using System.Text;

namespace Scheduler_Lib.Infrastructure.Validations;

public class ValidationRecurrent {
    public static ResultPattern<bool> ValidateRecurrent(SchedulerInput requestedDate)
    {
        var errors = new StringBuilder();

        if (requestedDate.Period == null || requestedDate.Period.Value <= TimeSpan.Zero)
        {
            errors.AppendLine(Messages.ErrorPositiveOffsetRequired);
        }

        if (requestedDate.StartDate > requestedDate.EndDate)
        {
            errors.AppendLine(Messages.ErrorStartDatePostEndDate);
        }

        if (requestedDate.CurrentDate < requestedDate.StartDate || requestedDate.CurrentDate > requestedDate.EndDate)
        {
            errors.AppendLine(Messages.ErrorDateOutOfRange);
        }

        if (requestedDate.Recurrency == EnumRecurrency.Weekly)
        {
            /*var weeklyResult = ValidateWeeklyOnce(requestedDate);
            if (!weeklyResult.IsSuccess)
            {
                errors.AppendLine(weeklyResult.Error);
            }*/
        }

        return errors.Length > 0 ? ResultPattern<bool>.Failure(errors.ToString()) : ResultPattern<bool>.Success(true);
    }
}