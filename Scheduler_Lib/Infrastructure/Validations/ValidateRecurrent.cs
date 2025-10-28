using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Resources;
using System.Text;

namespace Scheduler_Lib.Infrastructure.Validations;

public class ValidationRecurrent {
    public static ResultPattern<bool> ValidateRecurrent(SchedulerInput schedulerInput) {
        var errors = new StringBuilder();

        if (schedulerInput.EndDate.HasValue && schedulerInput.StartDate > schedulerInput.EndDate)
            errors.AppendLine(Messages.ErrorStartDatePostEndDate);

        if (schedulerInput.CurrentDate < schedulerInput.StartDate || schedulerInput.CurrentDate > schedulerInput.EndDate)
            errors.AppendLine(Messages.ErrorDateOutOfRange);

        var validation = schedulerInput.Recurrency == EnumRecurrency.Weekly ? ValidateWeekly(schedulerInput, errors) : ValidateDaily(schedulerInput, errors);

        return !validation.IsSuccess ? ResultPattern<bool>.Failure(validation.Error!) : ResultPattern<bool>.Success(true);
    }

    private static ResultPattern<bool> ValidateWeekly(SchedulerInput schedulerInput, StringBuilder errors) {
        if (schedulerInput.WeeklyPeriod is null or < 0)
            errors.AppendLine(Messages.ErrorWeeklyPeriodRequired);

        if (schedulerInput.DaysOfWeek == null || schedulerInput.DaysOfWeek.Count == 0)
            errors.AppendLine(Messages.ErrorDaysOfWeekRequired);
        else if (schedulerInput.DaysOfWeek.Distinct().Count() != schedulerInput.DaysOfWeek.Count)
            errors.AppendLine(Messages.ErrorDuplicateDaysOfWeek);

        return errors.Length > 0 ? ResultPattern<bool>.Failure(errors.ToString()) : ResultPattern<bool>.Success(true);
    }

    private static ResultPattern<bool> ValidateDaily(SchedulerInput schedulerInput, StringBuilder errors) {
        switch (schedulerInput.OccursOnceChk) {
            case true when schedulerInput.OccursEveryChk:
                errors.AppendLine(Messages.ErrorDailyModeConflict);
                break;
            case false when !schedulerInput.OccursEveryChk:
                errors.AppendLine(Messages.ErrorDailyModeRequired);
                break;
        }

        if (schedulerInput.OccursEveryChk) {
            if (!schedulerInput.DailyPeriod.HasValue || schedulerInput.DailyPeriod <= TimeSpan.Zero)
                errors.AppendLine(Messages.ErrorPositiveOffsetRequired);

            if (schedulerInput is { DailyStartTime: not null, DailyEndTime: not null } && schedulerInput.DailyStartTime > schedulerInput.DailyEndTime)
                errors.AppendLine(Messages.ErrorDailyStartAfterEnd);
        }

        if (!schedulerInput.OccursOnceChk)
            return errors.Length > 0
                ? ResultPattern<bool>.Failure(errors.ToString())
                : ResultPattern<bool>.Success(true);
        if (!schedulerInput.OccursOnceAt.HasValue)
            errors.AppendLine(Messages.ErrorOccursOnceAtNull);
        else {
            if (schedulerInput.OccursOnceAt < schedulerInput.StartDate || (schedulerInput.EndDate.HasValue && schedulerInput.OccursOnceAt > schedulerInput.EndDate))
                errors.AppendLine(Messages.ErrorTargetDateAfterEndDate);
        }

        return errors.Length > 0 ? ResultPattern<bool>.Failure(errors.ToString()) : ResultPattern<bool>.Success(true);
    }
}


  
