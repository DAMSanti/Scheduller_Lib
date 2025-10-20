using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Infrastructure.Validations;

namespace Scheduler_Lib.Core.Services;
public class CalculateOneTime {
    public static ResultPattern<SchedulerOutput> CalculateDate(SchedulerInput requestedDate) {
        var validation = ValidationOnce.ValidateOnce(requestedDate);

        return !validation.IsSuccess ? ResultPattern<SchedulerOutput>.Failure(validation.Error!) : 
            ResultPattern<SchedulerOutput>.Success(BuildResultForTargetDate(requestedDate));
    }

    private static SchedulerOutput BuildResultForTargetDate(SchedulerInput requestedDate) {
        var tz = RecurrenceCalculator.GetTimeZone();

        var next = new DateTimeOffset(requestedDate.TargetDate!.Value.DateTime, tz.GetUtcOffset(requestedDate.TargetDate!.Value.DateTime));

        return new SchedulerOutput {
            NextDate = next,
            Description = DescriptionBuilder.BuildDescriptionForTargetDate(requestedDate, tz, next),
            FutureDates = null
        };
    }
}