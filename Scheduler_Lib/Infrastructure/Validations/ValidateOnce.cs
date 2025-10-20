using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Resources;
using System.Text;

namespace Scheduler_Lib.Infrastructure.Validations;

public class ValidationOnce {
    public static ResultPattern<bool> ValidateOnce(SchedulerInput schedulerInput) {
        var errors = new StringBuilder();

        if (schedulerInput.Periodicity == EnumConfiguration.Recurrent)
            errors.AppendLine(Messages.ErrorUnsupportedPeriodicity);

        if (schedulerInput.EndDate != null && schedulerInput.StartDate > schedulerInput.EndDate)
            errors.AppendLine(Messages.ErrorStartDatePostEndDate);

        if (schedulerInput.TargetDate == null && schedulerInput.Recurrency != EnumRecurrency.Weekly)
            errors.AppendLine(Messages.ErrorTargetDateNull);

        if (schedulerInput.TargetDate != null && ((schedulerInput.TargetDate < schedulerInput.StartDate || schedulerInput.TargetDate > schedulerInput.EndDate)))
            errors.AppendLine(Messages.ErrorTargetDateAfterEndDate);

        if (schedulerInput is { Periodicity: EnumConfiguration.Once, Recurrency: EnumRecurrency.Weekly })
            errors.AppendLine(Messages.ErrorOnceWeekly);

        return errors.Length > 0 ? ResultPattern<bool>.Failure(errors.ToString()) : ResultPattern<bool>.Success(true);
    }
}