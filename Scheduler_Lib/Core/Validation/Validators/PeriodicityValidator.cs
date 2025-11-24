using Scheduler_Lib.Core.Interfaces;
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Core.Validation.Base;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Validation.Validators;

/// <summary>
/// Validates periodicity settings
/// </summary>
public class PeriodicityValidator : BaseValidator
{
    protected override ResultPattern<bool> DoValidate(ISchedulerConfiguration configuration)
    {
        if (configuration.Periodicity != EnumConfiguration.Once && 
            configuration.Periodicity != EnumConfiguration.Recurrent)
            return ResultPattern<bool>.Failure(Messages.ErrorUnsupportedPeriodicity);

        if (configuration.Recurrency != EnumRecurrency.Weekly && 
            configuration.Recurrency != EnumRecurrency.Daily && 
            configuration.Recurrency != EnumRecurrency.Monthly)
            return ResultPattern<bool>.Failure(Messages.ErrorUnsupportedRecurrency);

        return ResultPattern<bool>.Success(true);
    }
}
