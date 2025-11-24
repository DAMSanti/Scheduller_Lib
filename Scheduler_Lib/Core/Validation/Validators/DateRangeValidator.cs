using Scheduler_Lib.Core.Interfaces;
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Core.Validation.Base;
using Scheduler_Lib.Resources;
using System.Text;

namespace Scheduler_Lib.Core.Validation.Validators;

/// <summary>
/// Validates date range settings
/// </summary>
public class DateRangeValidator : BaseValidator
{
    protected override ResultPattern<bool> DoValidate(ISchedulerConfiguration configuration)
    {
        var errors = new StringBuilder();

        if (configuration.EndDate.HasValue && configuration.StartDate > configuration.EndDate)
            errors.AppendLine(Messages.ErrorStartDatePostEndDate);

        if (configuration.Periodicity == EnumConfiguration.Recurrent)
        {
            if (configuration.CurrentDate < configuration.StartDate || 
                configuration.CurrentDate > configuration.EndDate)
                errors.AppendLine(Messages.ErrorDateOutOfRange);
        }

        if (configuration.Periodicity == EnumConfiguration.Once && configuration.TargetDate != null)
        {
            var target = configuration.TargetDate.Value;
            if (target < configuration.StartDate)
                errors.AppendLine(Messages.ErrorTargetDateAfterEndDate);
            else if (configuration.EndDate != null && target > configuration.EndDate)
                errors.AppendLine(Messages.ErrorTargetDateAfterEndDate);
        }

        return errors.Length > 0 
            ? ResultPattern<bool>.Failure(errors.ToString()) 
            : ResultPattern<bool>.Success(true);
    }
}
