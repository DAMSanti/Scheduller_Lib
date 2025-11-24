using Scheduler_Lib.Core.Interfaces;
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Core.Validation.Base;
using Scheduler_Lib.Resources;
using System.Text;

namespace Scheduler_Lib.Core.Validation.Validators;

/// <summary>
/// Validates monthly recurrence specific settings
/// </summary>
public class MonthlyRecurrenceValidator : BaseValidator
{
    protected override ResultPattern<bool> DoValidate(ISchedulerConfiguration configuration)
    {
        if (configuration.Recurrency != EnumRecurrency.Monthly)
            return ResultPattern<bool>.Success(true);

        var errors = new StringBuilder();

        switch (configuration.MonthlyDayChk)
        {
            case true when configuration.MonthlyTheChk:
                errors.AppendLine(Messages.ErrorMonthlyModeConflict);
                break;
            case false when !configuration.MonthlyTheChk:
                errors.AppendLine(Messages.ErrorMonthlyModeRequired);
                break;
        }

        if (configuration.MonthlyDayChk)
        {
            if (configuration.MonthlyDay is null or < 1 or > 31)
                errors.AppendLine(Messages.ErrorMonthlyDayInvalid);

            if (configuration.MonthlyDayPeriod is null or <= 0)
                errors.AppendLine(Messages.ErrorMonthlyDayPeriodRequired);
        }
        else if (configuration.MonthlyTheChk)
        {
            if (configuration.MonthlyFrequency == null)
                errors.AppendLine(Messages.ErrorMonthlyFrequencyRequired);

            if (configuration.MonthlyDateType == null)
                errors.AppendLine(Messages.ErrorMonthlyDateTypeRequired);

            if (configuration.MonthlyThePeriod is null or <= 0)
                errors.AppendLine(Messages.ErrorMonthlyThePeriodRequired);
        }

        if (configuration is { DailyStartTime: not null, DailyEndTime: not null } && 
            configuration.DailyStartTime > configuration.DailyEndTime)
            errors.AppendLine(Messages.ErrorDailyStartAfterEnd);

        if (configuration.DailyPeriod.HasValue && configuration.DailyPeriod <= TimeSpan.Zero)
            errors.AppendLine(Messages.ErrorPositiveOffsetRequired);

        return errors.Length > 0 
            ? ResultPattern<bool>.Failure(errors.ToString()) 
            : ResultPattern<bool>.Success(true);
    }
}
