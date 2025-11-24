using Scheduler_Lib.Core.Interfaces;
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Core.Validation.Base;
using Scheduler_Lib.Resources;
using System.Text;

namespace Scheduler_Lib.Core.Validation.Validators;

/// <summary>
/// Validates weekly recurrence specific settings
/// </summary>
public class WeeklyRecurrenceValidator : BaseValidator
{
    protected override ResultPattern<bool> DoValidate(ISchedulerConfiguration configuration)
    {
        if (configuration.Recurrency != EnumRecurrency.Weekly)
            return ResultPattern<bool>.Success(true);

        var errors = new StringBuilder();

        if (configuration.WeeklyPeriod is null or <= 0)
            errors.AppendLine(Messages.ErrorWeeklyPeriodRequired);

        if (configuration.DaysOfWeek == null || configuration.DaysOfWeek.Count == 0)
            errors.AppendLine(Messages.ErrorDaysOfWeekRequired);
        else if (configuration.DaysOfWeek.Distinct().Count() != configuration.DaysOfWeek.Count)
            errors.AppendLine(Messages.ErrorDuplicateDaysOfWeek);

        if (configuration.OccursOnceChk && configuration.OccursEveryChk)
            errors.AppendLine(Messages.ErrorDailyModeConflict);

        if (configuration.OccursOnceChk && !configuration.OccursOnceAt.HasValue)
            errors.AppendLine(Messages.ErrorOccursOnceAtNull);

        if (configuration.OccursEveryChk && 
            (!configuration.DailyPeriod.HasValue || configuration.DailyPeriod <= TimeSpan.Zero))
            errors.AppendLine(Messages.ErrorPositiveOffsetRequired);

        if (configuration is { DailyStartTime: not null, DailyEndTime: not null } && 
            configuration.DailyStartTime > configuration.DailyEndTime)
            errors.AppendLine(Messages.ErrorDailyStartAfterEnd);

        return errors.Length > 0 
            ? ResultPattern<bool>.Failure(errors.ToString()) 
            : ResultPattern<bool>.Success(true);
    }
}
