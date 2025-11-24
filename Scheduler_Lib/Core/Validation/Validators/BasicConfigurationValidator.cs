using Scheduler_Lib.Core.Interfaces;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Core.Validation.Base;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Validation.Validators;

/// <summary>
/// Validates basic configuration settings
/// </summary>
public class BasicConfigurationValidator : BaseValidator
{
    protected override ResultPattern<bool> DoValidate(ISchedulerConfiguration configuration)
    {
        if (configuration == null)
            return ResultPattern<bool>.Failure(Messages.ErrorRequestNull);

        if (string.IsNullOrWhiteSpace(configuration.Language))
            return ResultPattern<bool>.Failure(Messages.ErrorLanguageRequired);

        if (!LocalizationService.IsSupportedLanguage(configuration.Language))
        {
            var supportedLanguages = string.Join(", ", LocalizationService.GetSupportedLanguages());
            return ResultPattern<bool>.Failure($"{Messages.ErrorLanguageNotSupported} {supportedLanguages}");
        }

        if (!configuration.EnabledChk)
            return ResultPattern<bool>.Failure(Messages.ErrorApplicationDisabled);

        if (configuration.CurrentDate == default)
            return ResultPattern<bool>.Failure(Messages.ErrorCurrentDateNull);

        if (configuration.StartDate == default)
            return ResultPattern<bool>.Failure(Messages.ErrorStartDateMissing);

        return ResultPattern<bool>.Success(true);
    }
}
