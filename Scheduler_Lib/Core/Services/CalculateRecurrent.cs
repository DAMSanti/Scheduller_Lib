using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Infrastructure.Validations;

namespace Scheduler_Lib.Core.Services;
public class CalculateRecurrent {
    public static ResultPattern<SchedulerOutput> CalculateDate(SchedulerInput schedulerInput) {
        var validation = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        return !validation.IsSuccess ? ResultPattern<SchedulerOutput>.Failure(validation.Error!) :
            ResultPattern<SchedulerOutput>.Success(BuildResult(schedulerInput));
    }

    private static SchedulerOutput BuildResult(SchedulerInput schedulerInput) {
        var tz = RecurrenceCalculator.GetTimeZone();

        List<DateTimeOffset>? futureDates = null;
        if (schedulerInput.Periodicity == EnumConfiguration.Recurrent)
            futureDates = RecurrenceCalculator.CalculateFutureDates(schedulerInput, tz);

        DateTimeOffset next;

        if (schedulerInput.Recurrency == EnumRecurrency.Weekly) {

            DateTime baseLocal = RecurrenceCalculator.GetBaseLocalTime(schedulerInput);

            var baseDtoForNext = new DateTimeOffset(baseLocal, tz.GetUtcOffset(baseLocal));

            next = RecurrenceCalculator.SelectNextEligibleDate(baseDtoForNext, schedulerInput.DaysOfWeek!, tz);

            if (futureDates is { Count: > 0 }) {
                futureDates.RemoveAll(d => d == next);
            }
        } else {
            if (schedulerInput.TargetDate.HasValue) {
                var td = schedulerInput.TargetDate.Value;
                next = new DateTimeOffset(td.DateTime, tz.GetUtcOffset(td.DateTime));
            } else {
                var cur = schedulerInput.CurrentDate;
                next = new DateTimeOffset(cur.DateTime, tz.GetUtcOffset(cur.DateTime));
            }

            if (futureDates is { Count: > 0 }) {
                futureDates.RemoveAll(d => d == next);
            }
        }

        return new SchedulerOutput {
            NextDate = next,
            Description = DescriptionBuilder.BuildDescriptionForCalculatedDate(schedulerInput, tz, next),
            FutureDates = futureDates
        };

    }
}
