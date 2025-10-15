using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Resources;
using System.Text;

namespace Scheduler_Lib.Infrastructure.Validations;
public static class Validations {
    public static ResultPattern<bool> ValidateCalculateDate(SchedulerInput? requestedDate) {
        return requestedDate == null ? ResultPattern<bool>.Failure(Messages.ErrorRequestNull) : ResultPattern<bool>.Success(true);
    }

    public static ResultPattern<bool> ValidateOnce(SchedulerInput requestedDate) {
        var errors = new StringBuilder();

        if (requestedDate.TargetDate == null) {
            errors.AppendLine(Messages.ErrorOnceMode);
        }
        if (requestedDate.StartDate > requestedDate.EndDate) {
            errors.AppendLine(Messages.ErrorChangeDateAfterEndDate);
        }
        if (requestedDate.TargetDate == null) {
            errors.AppendLine(Messages.ErrorChangeDateNull);
        }
        if (requestedDate.EndDate == null) {
            errors.AppendLine(Messages.ErrorEndDateNull);
        }
        if (requestedDate.TargetDate < requestedDate.StartDate || requestedDate.TargetDate > requestedDate.EndDate) {
            errors.AppendLine(Messages.ErrorChangeDateAfterEndDate);
        }
        if (requestedDate.Recurrency == EnumRecurrency.Weekly) {
            var weeklyResult = ValidateWeekly(requestedDate);
            if (!weeklyResult.IsSuccess) errors.AppendLine(weeklyResult.Error);
        }

        return errors.Length > 0 ? ResultPattern<bool>.Failure(errors.ToString()) : ResultPattern<bool>.Success(true);
    }

    public static ResultPattern<bool> ValidateRecurrent(SchedulerInput requestedDate) {
        var errors = new StringBuilder();

        if (requestedDate.Period == null || requestedDate.Period.Value <= TimeSpan.Zero) {
            errors.AppendLine(Messages.ErrorPositiveOffsetRequired);
        }

        if (requestedDate.StartDate > requestedDate.EndDate) {
            errors.AppendLine(Messages.ErrorStartDatePostEndDate);
        }

        if (requestedDate.CurrentDate < requestedDate.StartDate || requestedDate.CurrentDate > requestedDate.EndDate) {
            errors.AppendLine(Messages.ErrorDateOutOfRange);
        }

        if (requestedDate.Recurrency == EnumRecurrency.Weekly) {
            var weeklyResult = ValidateWeekly(requestedDate);
            if (!weeklyResult.IsSuccess) {
                errors.AppendLine(weeklyResult.Error);
            }
        }

        return errors.Length > 0 ? ResultPattern<bool>.Failure(errors.ToString()) : ResultPattern<bool>.Success(true);
    }

    private static ResultPattern<bool> ValidateWeekly(SchedulerInput requestedDate) {
        var errors = new StringBuilder();

        if (!requestedDate.WeeklyPeriod.HasValue || requestedDate.WeeklyPeriod.Value <= 0)
            errors.AppendLine(Messages.ErrorWeeklyPeriodRequired);

        if (requestedDate.DaysOfWeek == null || requestedDate.DaysOfWeek.Count == 0)
            errors.AppendLine(Messages.ErrorDaysOfWeekRequired);

        bool hasStart = requestedDate.DailyStartTime.HasValue;
        bool hasEnd = requestedDate.DailyEndTime.HasValue;

        if (hasStart ^ hasEnd)
            errors.AppendLine(Messages.ErrorDailyTimeWindowIncomplete);

        if (hasStart && hasEnd && requestedDate.DailyStartTime.Value >= requestedDate.DailyEndTime.Value)
            errors.AppendLine(Messages.ErrorDailyStartAfterEnd);

        return errors.Length > 0 ? ResultPattern<bool>.Failure(errors.ToString()) : ResultPattern<bool>.Success(true);
    }


}