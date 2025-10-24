using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Infrastructure.Validations;
public static class Validations {
    public static ResultPattern<bool> ValidateCalculateDate(SchedulerInput? schedulerInput) {
        if (schedulerInput == null)
            return ResultPattern<bool>.Failure(Messages.ErrorRequestNull);

        if (!schedulerInput.Enabled)
            return ResultPattern<bool>.Failure(Messages.ErrorApplicationDisabled);

        if (schedulerInput.Periodicity != EnumConfiguration.Once && schedulerInput.Periodicity != EnumConfiguration.Recurrent)
            return ResultPattern<bool>.Failure(Messages.ErrorUnsupportedPeriodicity);

        if (schedulerInput.Recurrency != EnumRecurrency.Weekly && schedulerInput.Recurrency != EnumRecurrency.Daily)
            return ResultPattern<bool>.Failure(Messages.ErrorUnsupportedRecurrency);

        if (schedulerInput.CurrentDate == default)
            return ResultPattern<bool>.Failure(Messages.ErrorCurrentDateNull);

        return schedulerInput.StartDate == default ? ResultPattern<bool>.Failure(Messages.ErrorStartDateMissing) : ResultPattern<bool>.Success(true);
    }
}