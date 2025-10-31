using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Model.Enum;
using Scheduler_Lib.Core.Services.Calculation;
using Scheduler_Lib.Core.Services.Calculation.Helpers;
using Scheduler_Lib.Core.Services.Description;
using Scheduler_Lib.Infrastructure.Validations;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Services;

public class SchedulerService {
    public static ResultPattern<SchedulerOutput> CalculateDate(SchedulerInput schedulerInput) {
        var validation = Validations.ValidateCalculateDate(schedulerInput);
        if (!validation.IsSuccess)
            return ResultPattern<SchedulerOutput>.Failure(validation.Error!);

        var specificValidation = schedulerInput.Periodicity switch {
            EnumConfiguration.Once => ValidationOnce.ValidateOnce(schedulerInput),
            EnumConfiguration.Recurrent => ValidationRecurrent.ValidateRecurrent(schedulerInput),
            _ => ResultPattern<bool>.Failure(Messages.ErrorUnsupportedPeriodicity)
        };

        if (!specificValidation.IsSuccess)
            return ResultPattern<SchedulerOutput>.Failure(specificValidation.Error!);

        var tz = TimeZoneInfo.FindSystemTimeZoneById(Config.TimeZoneId);

        DateTimeOffset nextDate;
        
        if (schedulerInput.Periodicity == EnumConfiguration.Once) {
            var dateTimeHelper = new DateTimeHelper();
            var targetDate = schedulerInput.TargetDate!.Value;
            nextDate = dateTimeHelper.CreateDateTimeOffset(targetDate.DateTime, tz);
        } else {
            var dateCalculator = new DateCalculator();
            nextDate = dateCalculator.GetNextExecutionDate(schedulerInput, tz);
        }

        var descriptionService = new DescriptionService();
        var description = descriptionService.BuildDescription(schedulerInput, tz, nextDate);

        var output = new SchedulerOutput {
            NextDate = nextDate,
            Description = description
        };

        return ResultPattern<SchedulerOutput>.Success(output);
    }
}