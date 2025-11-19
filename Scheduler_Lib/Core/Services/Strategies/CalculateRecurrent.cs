using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services.Calculators.Daily;
using Scheduler_Lib.Core.Services.Calculators.Monthly;
using Scheduler_Lib.Core.Services.Utilities;
using Scheduler_Lib.Infrastructure.Validations;

namespace Scheduler_Lib.Core.Services.Strategies;

internal class CalculateRecurrent {
    internal static ResultPattern<SchedulerOutput> CalculateRecurrentScheduler(SchedulerInput schedulerInput) {
        return ValidateAndCalculateSchedule(schedulerInput);
    }

    private static ResultPattern<SchedulerOutput> ValidateAndCalculateSchedule(SchedulerInput schedulerInput) {
        var validation = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        return !validation.IsSuccess ? ResultPattern<SchedulerOutput>.Failure(validation.Error!) :
            ResultPattern<SchedulerOutput>.Success(BuildResultRecurrent(schedulerInput));
    }

    private static SchedulerOutput BuildResultRecurrent(SchedulerInput schedulerInput) {
        var tz = TimeZoneConverter.GetTimeZone();

        DateTimeOffset next;

        if (schedulerInput.Recurrency == EnumRecurrency.Weekly) {
            next = RecurrenceCalculator.GetNextExecutionDate(schedulerInput, tz);
            
            if (schedulerInput.OccursOnceChk && schedulerInput.OccursOnceAt.HasValue) {
                next = OccursOnceHelper.ApplyOccursOnceAt(next, schedulerInput.OccursOnceAt, tz);
            }
        } else if (schedulerInput.Recurrency == EnumRecurrency.Monthly) {
            var futureDates = MonthlyRecurrenceCalculator.CalculateFutureDates(schedulerInput, tz);
            if (futureDates.Count > 0) {
                next = futureDates.First();
                
                if (schedulerInput.OccursOnceChk && schedulerInput.OccursOnceAt.HasValue) {
                    next = OccursOnceHelper.ApplyOccursOnceAt(next, schedulerInput.OccursOnceAt, tz);
                }
            } else {
                next = schedulerInput.CurrentDate;
            }
        } else if (schedulerInput.Recurrency == EnumRecurrency.Daily) {
            var futureDates = DailyRecurrenceCalculator.CalculateFutureDates(schedulerInput, tz);
            if (futureDates.Count > 0) {
                next = futureDates.First();
                
                if (schedulerInput.OccursOnceChk && schedulerInput.OccursOnceAt.HasValue) {
                    next = OccursOnceHelper.ApplyOccursOnceAt(next, schedulerInput.OccursOnceAt, tz);
                }
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
