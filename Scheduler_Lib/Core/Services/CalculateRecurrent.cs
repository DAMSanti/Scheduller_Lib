using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Infrastructure.Validations;

namespace Scheduler_Lib.Core.Services;
public class CalculateRecurrent {
    public static ResultPattern<SchedulerOutput> CalculateDate(SchedulerInput requestedDate) {
        if (requestedDate.Periodicity == EnumConfiguration.Once) {
            var validationOnce = ValidationOnce.ValidateOnce(requestedDate);
            if (!validationOnce.IsSuccess)
                return ResultPattern<SchedulerOutput>.Failure(validationOnce.Error!);
            return CalculateOneTime.CalculateDate(requestedDate);
        }

        var validation = ValidationRecurrent.ValidateRecurrent(requestedDate);
        return !validation.IsSuccess ? ResultPattern<SchedulerOutput>.Failure(validation.Error!) :
            ResultPattern<SchedulerOutput>.Success(BuildResultRecurrentDates(requestedDate));
    }

    private static SchedulerOutput BuildResultRecurrentDates(SchedulerInput requestedDate) {
        var tz = RecurrenceCalculator.GetTimeZone();

        List<DateTimeOffset>? futureDates = null;
        if (requestedDate.Periodicity == EnumConfiguration.Recurrent)
            futureDates = RecurrenceCalculator.CalculateFutureDates(requestedDate, tz);

        DateTimeOffset next;

        if (requestedDate.Recurrency == EnumRecurrency.Weekly) {
            var baseDate = requestedDate.TargetDate ?? requestedDate.CurrentDate;
            var days = requestedDate.DaysOfWeek ?? [baseDate.DayOfWeek];

            next = RecurrenceCalculator.SelectNextEligibleDate(baseDate, days, tz);
        } else {
            if (requestedDate.TargetDate.HasValue) {
                var td = requestedDate.TargetDate.Value;
                next = new DateTimeOffset(td.DateTime, tz.GetUtcOffset(td.DateTime));
            } else {
                var cur = requestedDate.CurrentDate;
                next = new DateTimeOffset(cur.DateTime, tz.GetUtcOffset(cur.DateTime));
            }
        }

        return new SchedulerOutput {
            NextDate = next,
            Description = DescriptionBuilder.BuildDescriptionForTargetDate(requestedDate, tz, next),
            FutureDates = futureDates
        };

    }
}
