using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Infrastructure.Validations;

namespace Scheduler_Lib.Core.Services;
public class CalculateOneTime {
    public static ResultPattern<SchedulerOutput> CalculateDate(SchedulerInput schedulerInput) {
        var validation = ValidationOnce.ValidateOnce(schedulerInput);

        return !validation.IsSuccess
            ? ResultPattern<SchedulerOutput>.Failure(validation.Error ?? "Unknown validation error")
            : ResultPattern<SchedulerOutput>.Success(BuildResult(schedulerInput));
    }

    private static SchedulerOutput BuildResult(SchedulerInput schedulerInput) {
        var tz = RecurrenceCalculator.GetTimeZone();
        var targetDate = schedulerInput.TargetDate!.Value;
        var next = new DateTimeOffset(targetDate.DateTime, tz.GetUtcOffset(targetDate.DateTime));

        return new SchedulerOutput {
            NextDate = next,
            Description = DescriptionBuilder.BuildDescriptionForCalculatedDate(schedulerInput, tz, next),
            FutureDates = null
        };
    }
}