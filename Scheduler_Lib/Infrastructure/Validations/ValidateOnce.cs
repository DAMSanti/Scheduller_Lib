using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Resources;
using System.Text;

namespace Scheduler_Lib.Infrastructure.Validations;

internal class ValidationOnce {
    internal static ResultPattern<bool> ValidateOnce(SchedulerInput schedulerInput) {
        var errors = new StringBuilder();

        if (schedulerInput.Periodicity == EnumConfiguration.Once) {
            if (schedulerInput.Recurrency == EnumRecurrency.Weekly)
                errors.AppendLine(Messages.ErrorOnceWeekly);
        }

        if (schedulerInput.TargetDate != null) {
            var target = schedulerInput.TargetDate.Value;
            if (target < schedulerInput.StartDate)
                errors.AppendLine(Messages.ErrorTargetDateAfterEndDate);
            else if (schedulerInput.EndDate != null && target > schedulerInput.EndDate)
                errors.AppendLine(Messages.ErrorTargetDateAfterEndDate);
        }

        if (schedulerInput.EndDate != null) {
            if (schedulerInput.StartDate > schedulerInput.EndDate)
                errors.AppendLine(Messages.ErrorStartDatePostEndDate);
        }

        if (schedulerInput.TargetDate == null && schedulerInput.Recurrency != EnumRecurrency.Weekly)
            errors.AppendLine(Messages.ErrorTargetDateNull);

        return errors.Length > 0 ? ResultPattern<bool>.Failure(errors.ToString()) : ResultPattern<bool>.Success(true);
    }
}