using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Infrastructure.Validations;

namespace Scheduler_Lib.Core.Services;
public class CalculateOneTime {
    public static ResultPattern<SchedulerOutput> CalculateDate(SchedulerInput schedulerInput) {
        var validation = ValidationOnce.ValidateOnce(schedulerInput);

        return !validation.IsSuccess
            ? ResultPattern<SchedulerOutput>.Failure(validation.Error!)
            : ResultPattern<SchedulerOutput>.Success(BuildResultOnce(schedulerInput));
    }

    private static SchedulerOutput BuildResultOnce(SchedulerInput schedulerInput) {
        var tz = RecurrenceCalculator.GetTimeZone();
        var targetDate = schedulerInput.TargetDate!.Value;
        var next = new DateTimeOffset(targetDate.DateTime, tz.GetUtcOffset(targetDate.DateTime));

        return new SchedulerOutput {
            NextDate = next,
            Description = DescriptionBuilder.HandleDescriptionForCalculatedDate(schedulerInput, tz, next),
        };
    }
}