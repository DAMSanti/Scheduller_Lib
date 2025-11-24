using Scheduler_Lib.Core.Interfaces;
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Core.Validation.Base;
using Scheduler_Lib.Resources;
using System.Text;

namespace Scheduler_Lib.Core.Validation.Validators;

/// <summary>
/// Validates daily recurrence specific settings
/// </summary>
public class DailyRecurrenceValidator : BaseValidator
{
    protected override ResultPattern<bool> DoValidate(ISchedulerConfiguration configuration)
    {
        if (configuration.Recurrency != EnumRecurrency.Daily)
            return ResultPattern<bool>.Success(true);

        var errors = new StringBuilder();

        switch (configuration.OccursOnceChk)
        {
            case true when configuration.OccursEveryChk:
                errors.AppendLine(Messages.ErrorDailyModeConflict);
                break;
            case false when !configuration.OccursEveryChk:
                errors.AppendLine(Messages.ErrorDailyModeRequired);
                break;
        }

        if (configuration.OccursEveryChk)
        {
            if (!configuration.DailyPeriod.HasValue || configuration.DailyPeriod <= TimeSpan.Zero)
                errors.AppendLine(Messages.ErrorPositiveOffsetRequired);

            if (configuration is { DailyStartTime: not null, DailyEndTime: not null } && 
                configuration.DailyStartTime > configuration.DailyEndTime)
                errors.AppendLine(Messages.ErrorDailyStartAfterEnd);
        }

        if (configuration.OccursOnceChk && !configuration.OccursOnceAt.HasValue)
            errors.AppendLine(Messages.ErrorOccursOnceAtNull);

        return errors.Length > 0 
            ? ResultPattern<bool>.Failure(errors.ToString()) 
            : ResultPattern<bool>.Success(true);
    }
}
