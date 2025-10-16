using System.Runtime.InteropServices.JavaScript;
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Resources;
using System.Text;

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

        if (requestedDate.StartDate == default)
            return ResultPattern<bool>.Failure(Messages.ErrorStartDateMissing);

        var validation = requestedDate.Recurrency == EnumRecurrency.Weekly ? ValidateWeekly(requestedDate) : ValidateDaily(requestedDate);

        if (!validation.IsSuccess) return ResultPattern<bool>.Failure(validation.Error!);

        return ResultPattern<bool>.Success(true);
    }
    
    private static ResultPattern<bool> ValidateWeekly(SchedulerInput requestedDate) {
        var errors = new StringBuilder();

        if (!requestedDate.WeeklyPeriod.HasValue)
            errors.AppendLine(Messages.ErrorWeeklyPeriodRequired);

        if (requestedDate.DaysOfWeek == null || requestedDate.DaysOfWeek.Count == 0)
            errors.AppendLine(Messages.ErrorDaysOfWeekRequired);

        return errors.Length > 0 ? ResultPattern<bool>.Failure(errors.ToString()) : ResultPattern<bool>.Success(true);
    }

    private static ResultPattern<bool> ValidateDaily(SchedulerInput requestedDate) {
        var errors = new StringBuilder();

        if (!requestedDate.Period.HasValue)
            errors.AppendLine(Messages.ErrorPositiveOffsetRequired);

        return errors.Length > 0 ? ResultPattern<bool>.Failure(errors.ToString()) : ResultPattern<bool>.Success(true);
    }

}