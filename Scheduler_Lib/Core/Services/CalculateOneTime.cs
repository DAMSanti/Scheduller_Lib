using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Infrastructure.Validations;

namespace Scheduler_Lib.Core.Services;
public class CalculateOneTime {
    public ResultPattern<SchedulerOutput> CalculateDate(SchedulerInput requestedDate) {
        var validation = ValidationOnce.ValidateOnce(requestedDate);

        return !validation.IsSuccess ? ResultPattern<SchedulerOutput>.Failure(validation.Error!) : 
            ResultPattern<SchedulerOutput>.Success(BuildResultForTargetDate(requestedDate));
    }

    private SchedulerOutput BuildResultForTargetDate(SchedulerInput requestedDate) {
        var tz = GetTimeZone(requestedDate);

        List<DateTimeOffset>? futureDates = null;
        if (requestedDate.Recurrency == EnumRecurrency.Weekly) {
            futureDates = RecurrenceCalculator.CalculateWeeklyRecurrence(requestedDate, tz);
        }

        var next = requestedDate.Recurrency == EnumRecurrency.Weekly
            ? RecurrenceCalculator.SelectNextEligibleDate(requestedDate.TargetDate!.Value, requestedDate.DaysOfWeek!, tz)
            : new DateTimeOffset(requestedDate.TargetDate!.Value.DateTime, tz.GetUtcOffset(requestedDate.TargetDate!.Value.DateTime));

        return new SchedulerOutput {
            NextDate = next,
            Description = DescriptionBuilder.BuildDescriptionForTargetDate(requestedDate, tz, next),
            FutureDates = futureDates
        };
    }

    private static TimeZoneInfo GetTimeZone(SchedulerInput requestedDate) {
        return TimeZoneInfo.FindSystemTimeZoneById(requestedDate.TimeZoneId!);
    }
}