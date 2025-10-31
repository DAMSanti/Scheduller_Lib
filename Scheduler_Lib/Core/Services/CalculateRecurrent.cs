using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Infrastructure.Validations;

namespace Scheduler_Lib.Core.Services;
public class CalculateRecurrent {
    public static ResultPattern<SchedulerOutput> CalculateDate(SchedulerInput schedulerInput) {
        var validation = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        return !validation.IsSuccess ? ResultPattern<SchedulerOutput>.Failure(validation.Error!) :
            ResultPattern<SchedulerOutput>.Success(BuildResultRecurrent(schedulerInput));
    }

    private static SchedulerOutput BuildResultRecurrent(SchedulerInput schedulerInput) {
        var tz = RecurrenceCalculator.GetTimeZone();

        DateTimeOffset next;

        if (schedulerInput.Recurrency == EnumRecurrency.Weekly) {
            var baseLocal = RecurrenceCalculator.GetBaseLocalTime(schedulerInput);
            var baseDtoForNext = new DateTimeOffset(baseLocal, tz.GetUtcOffset(baseLocal));
            next = RecurrenceCalculator.SelectNextEligibleDate(baseDtoForNext, schedulerInput.DaysOfWeek!, tz);
        } else if (schedulerInput.Recurrency == EnumRecurrency.Monthly) {
            //var futureDates = RecurrenceCalculator.CalculateMonthlyRecurrence(schedulerInput, tz);
            next =  new DateTimeOffset(schedulerInput.CurrentDate.DateTime, tz.GetUtcOffset(schedulerInput.CurrentDate.DateTime));
        }else {
            if (schedulerInput.OccursOnceChk) {
                var once = schedulerInput.OccursOnceAt!.Value;
                next = new DateTimeOffset(once.DateTime, tz.GetUtcOffset(once.DateTime));
            } else {
                if (schedulerInput.TargetDate.HasValue) {
                    var td = schedulerInput.TargetDate.Value;
                    next = new DateTimeOffset(td.DateTime, tz.GetUtcOffset(td.DateTime));
                } else {
                    var cur = schedulerInput.CurrentDate;
                    next = new DateTimeOffset(cur.DateTime, tz.GetUtcOffset(cur.DateTime));
                }
            }
        }

        return new SchedulerOutput {
            NextDate = next,
            Description = DescriptionBuilder.HandleDescriptionForCalculatedDate(schedulerInput, tz, next)
        };

    }
}
