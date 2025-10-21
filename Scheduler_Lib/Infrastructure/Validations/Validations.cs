using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Infrastructure.Validations;
public static class Validations {
    public static ResultPattern<bool> ValidateCalculateDate(SchedulerInput? requestedDate) {
        if (requestedDate == null)
            return ResultPattern<bool>.Failure(Messages.ErrorRequestNull);

        if (requestedDate.Periodicity != EnumConfiguration.Once && requestedDate.Periodicity != EnumConfiguration.Recurrent)
            return ResultPattern<bool>.Failure(Messages.ErrorUnsupportedPeriodicity);

        if (requestedDate.Recurrency != EnumRecurrency.Weekly && requestedDate.Recurrency != EnumRecurrency.Daily)
            return ResultPattern<bool>.Failure(Messages.ErrorUnsupportedRecurrency);

        if (requestedDate.CurrentDate == default)
            return ResultPattern<bool>.Failure(Messages.ErrorCurrentDateNull);

        return requestedDate.StartDate == default ? ResultPattern<bool>.Failure(Messages.ErrorStartDateMissing) : ResultPattern<bool>.Success(true);
    }
}