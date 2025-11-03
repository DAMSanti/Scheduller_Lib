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
            next = RecurrenceCalculator.GetNextExecutionDate(schedulerInput, tz);
            
            if (schedulerInput.OccursOnceChk && schedulerInput.OccursOnceAt.HasValue) {
                var occursTime = schedulerInput.OccursOnceAt.Value.TimeOfDay;
                var nextDate = next.DateTime.Date;
                var nextWithTime = new DateTime(nextDate.Year, nextDate.Month, nextDate.Day,
                    occursTime.Hours, occursTime.Minutes, occursTime.Seconds, DateTimeKind.Unspecified);
                next = new DateTimeOffset(nextWithTime, tz.GetUtcOffset(nextWithTime));
            }
        } else if (schedulerInput.Recurrency == EnumRecurrency.Monthly) {
            var futureDates = RecurrenceCalculator.CalculateMonthlyRecurrence(schedulerInput, tz);
            if (futureDates.Count > 0) {
                next = futureDates.First();
            } else {
                next = schedulerInput.CurrentDate;
            }
        } else {
            next = RecurrenceCalculator.GetNextExecutionDate(schedulerInput, tz);
        }

        return new SchedulerOutput {
            NextDate = next,
            Description = DescriptionBuilder.HandleDescriptionForCalculatedDate(schedulerInput, tz, next)
        };

    }
}
