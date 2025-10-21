using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Infrastructure.Validations;

namespace Scheduler_Lib.Core.Services;
public class CalculateRecurrent {
    public static ResultPattern<SchedulerOutput> CalculateDate(SchedulerInput requestedDate) {
        var validation = ValidationRecurrent.ValidateRecurrent(requestedDate);

        return !validation.IsSuccess ? ResultPattern<SchedulerOutput>.Failure(validation.Error!) :
            ResultPattern<SchedulerOutput>.Success(BuildResult(requestedDate));
    }

    private static SchedulerOutput BuildResult(SchedulerInput requestedDate) {
        var tz = RecurrenceCalculator.GetTimeZone();

        List<DateTimeOffset>? futureDates = null;
        if (requestedDate.Periodicity == EnumConfiguration.Recurrent)
            futureDates = RecurrenceCalculator.CalculateFutureDates(requestedDate, tz);

        DateTimeOffset next;

        if (requestedDate.Recurrency == EnumRecurrency.Weekly) {

            DateTime baseLocal;
            if (requestedDate.TargetDate.HasValue) {
                baseLocal = requestedDate.TargetDate.Value.DateTime;
            } else {
                var cur = requestedDate.CurrentDate.DateTime;
                var startTime = requestedDate.StartDate.TimeOfDay;
                baseLocal = new DateTime(cur.Year, cur.Month, cur.Day,
                    startTime.Hours, startTime.Minutes, startTime.Seconds, DateTimeKind.Unspecified);
            }

            var baseDtoForNext = new DateTimeOffset(baseLocal, tz.GetUtcOffset(baseLocal));

            next = RecurrenceCalculator.SelectNextEligibleDate(baseDtoForNext, requestedDate.DaysOfWeek!, tz);

            if (futureDates is { Count: > 0 }) {
                futureDates.RemoveAll(d => d == next);
            }
        } else {
            if (requestedDate.TargetDate.HasValue) {
                var td = requestedDate.TargetDate.Value;
                next = new DateTimeOffset(td.DateTime, tz.GetUtcOffset(td.DateTime));
            } else {
                var cur = requestedDate.CurrentDate;
                next = new DateTimeOffset(cur.DateTime, tz.GetUtcOffset(cur.DateTime));
            }

            if (futureDates is { Count: > 0 }) {
                futureDates.RemoveAll(d => d == next);
            }
        }

        return new SchedulerOutput {
            NextDate = next,
            Description = DescriptionBuilder.BuildDescriptionForCalculatedDate(requestedDate, tz, next),
            FutureDates = futureDates
        };

    }
}
