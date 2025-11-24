using Scheduler_Lib.Core.Interfaces;
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Core.Validation.Base;
using Scheduler_Lib.Resources;
using System.Text;

namespace Scheduler_Lib.Core.Validation.Validators;

/// <summary>
/// Validates one-time execution specific settings
/// </summary>
public class OneTimeExecutionValidator : BaseValidator
{
    protected override ResultPattern<bool> DoValidate(ISchedulerConfiguration configuration)
    {
        if (configuration.Periodicity != EnumConfiguration.Once)
            return ResultPattern<bool>.Success(true);

        var errors = new StringBuilder();

        if (configuration.Recurrency == EnumRecurrency.Weekly)
            errors.AppendLine(Messages.ErrorOnceWeekly);

        if (configuration.TargetDate == null && configuration.Recurrency != EnumRecurrency.Weekly)
            errors.AppendLine(Messages.ErrorTargetDateNull);

        return errors.Length > 0 
            ? ResultPattern<bool>.Failure(errors.ToString()) 
            : ResultPattern<bool>.Success(true);
    }
}
