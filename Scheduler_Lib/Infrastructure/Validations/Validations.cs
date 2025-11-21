using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Resources;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Scheduler_Lib.Infrastructure.Validations;
internal static class Validations {
    internal static ResultPattern<bool> ValidateCalculateDate(SchedulerInput? schedulerInput) {
        if (schedulerInput == null)
            return ResultPattern<bool>.Failure(Messages.ErrorRequestNull);

        if (string.IsNullOrWhiteSpace(schedulerInput.Language)) {
            return ResultPattern<bool>.Failure(Messages.ErrorLanguageRequired);
        } else if (!LocalizationService.IsSupportedLanguage(schedulerInput.Language)) {
            var supportedLanguages = string.Join(", ", LocalizationService.GetSupportedLanguages());
            return ResultPattern<bool>.Failure($"{Messages.ErrorLanguageNotSupported} {supportedLanguages}");
        }

        if (!schedulerInput.EnabledChk)
            return ResultPattern<bool>.Failure(Messages.ErrorApplicationDisabled);

        if (schedulerInput.Periodicity != EnumConfiguration.Once && schedulerInput.Periodicity != EnumConfiguration.Recurrent)
            return ResultPattern<bool>.Failure(Messages.ErrorUnsupportedPeriodicity);

        if (schedulerInput.Recurrency != EnumRecurrency.Weekly && schedulerInput.Recurrency != EnumRecurrency.Daily && schedulerInput.Recurrency != EnumRecurrency.Monthly)
            return ResultPattern<bool>.Failure(Messages.ErrorUnsupportedRecurrency);

        if (schedulerInput.CurrentDate == default)
            return ResultPattern<bool>.Failure(Messages.ErrorCurrentDateNull);

        return schedulerInput.StartDate == default ? ResultPattern<bool>.Failure(Messages.ErrorStartDateMissing) : ResultPattern<bool>.Success(true);
    }
}