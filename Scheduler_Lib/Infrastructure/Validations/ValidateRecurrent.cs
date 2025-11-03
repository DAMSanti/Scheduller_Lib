using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Resources;
using System.Text;

namespace Scheduler_Lib.Infrastructure.Validations;

public class ValidationRecurrent {
    public static ResultPattern<bool> ValidateRecurrent(SchedulerInput schedulerInput) {
        var errors = new StringBuilder();

        if (schedulerInput.EndDate.HasValue && schedulerInput.StartDate > schedulerInput.EndDate)
            errors.AppendLine(Messages.ErrorStartDatePostEndDate);

        if (schedulerInput.CurrentDate < schedulerInput.StartDate || schedulerInput.CurrentDate > schedulerInput.EndDate)
            errors.AppendLine(Messages.ErrorDateOutOfRange);

        var validation = schedulerInput.Recurrency switch {
            EnumRecurrency.Weekly => ValidateWeekly(schedulerInput, errors),
            EnumRecurrency.Daily => ValidateDaily(schedulerInput, errors),
            EnumRecurrency.Monthly => ValidateMonthly(schedulerInput, errors),
            _ => ResultPattern<bool>.Failure(Messages.ErrorUnsupportedRecurrency)
        };

        if (!validation.IsSuccess)
            return ResultPattern<bool>.Failure(validation.Error!);

        return errors.Length > 0
            ? ResultPattern<bool>.Failure(errors.ToString())
            : ResultPattern<bool>.Success(true);
    }

    private static ResultPattern<bool> ValidateWeekly(SchedulerInput schedulerInput, StringBuilder errors) {
        if (schedulerInput.WeeklyPeriod is null or <= 0)
            errors.AppendLine(Messages.ErrorWeeklyPeriodRequired);

        if (schedulerInput.DaysOfWeek == null || schedulerInput.DaysOfWeek.Count == 0)
            errors.AppendLine(Messages.ErrorDaysOfWeekRequired);
        else if (schedulerInput.DaysOfWeek.Distinct().Count() != schedulerInput.DaysOfWeek.Count)
            errors.AppendLine(Messages.ErrorDuplicateDaysOfWeek);

        if (schedulerInput.OccursOnceChk && schedulerInput.OccursEveryChk)
            errors.AppendLine(Messages.ErrorDailyModeConflict);

        if (schedulerInput.OccursOnceChk && !schedulerInput.OccursOnceAt.HasValue)
            errors.AppendLine(Messages.ErrorOccursOnceAtNull);

        if (schedulerInput.OccursEveryChk && (!schedulerInput.DailyPeriod.HasValue || schedulerInput.DailyPeriod <= TimeSpan.Zero))
            errors.AppendLine(Messages.ErrorPositiveOffsetRequired);

        if (schedulerInput is { DailyStartTime: not null, DailyEndTime: not null } && schedulerInput.DailyStartTime > schedulerInput.DailyEndTime)
            errors.AppendLine(Messages.ErrorDailyStartAfterEnd);

        return errors.Length > 0 ? ResultPattern<bool>.Failure(errors.ToString()) : ResultPattern<bool>.Success(true);
    }

    private static ResultPattern<bool> ValidateDaily(SchedulerInput schedulerInput, StringBuilder errors) {
        switch (schedulerInput.OccursOnceChk) {
            case true when schedulerInput.OccursEveryChk:
                errors.AppendLine(Messages.ErrorDailyModeConflict);
                break;
            case false when !schedulerInput.OccursEveryChk:
                errors.AppendLine(Messages.ErrorDailyModeRequired);
                break;
        }

        if (schedulerInput.OccursEveryChk) {
            if (!schedulerInput.DailyPeriod.HasValue || schedulerInput.DailyPeriod <= TimeSpan.Zero)
                errors.AppendLine(Messages.ErrorPositiveOffsetRequired);

            if (schedulerInput is { DailyStartTime: not null, DailyEndTime: not null } && schedulerInput.DailyStartTime > schedulerInput.DailyEndTime)
                errors.AppendLine(Messages.ErrorDailyStartAfterEnd);
        }

        if (!schedulerInput.OccursOnceChk)
            return errors.Length > 0
                ? ResultPattern<bool>.Failure(errors.ToString())
                : ResultPattern<bool>.Success(true);
        if (!schedulerInput.OccursOnceAt.HasValue)
            errors.AppendLine(Messages.ErrorOccursOnceAtNull);
        else {
            if (schedulerInput.OccursOnceAt < schedulerInput.StartDate || (schedulerInput.EndDate.HasValue && schedulerInput.OccursOnceAt > schedulerInput.EndDate))
                errors.AppendLine(Messages.ErrorTargetDateAfterEndDate);
        }

        return errors.Length > 0 ? ResultPattern<bool>.Failure(errors.ToString()) : ResultPattern<bool>.Success(true);
    }
    private static ResultPattern<bool> ValidateMonthly(SchedulerInput schedulerInput, StringBuilder errors) {
        switch (schedulerInput.MonthlyDayChk) {
            case true when schedulerInput.MonthlyTheChk:
                errors.AppendLine(Messages.ErrorMonthlyModeConflict);
                break;
            case false when !schedulerInput.MonthlyTheChk:
                errors.AppendLine(Messages.ErrorMonthlyModeRequired);
                break;
        }

        if (schedulerInput.MonthlyDayChk) {
            if (schedulerInput.MonthlyDay is null or < 1 or > 31)
                errors.AppendLine(Messages.ErrorMonthlyDayInvalid);

            if (schedulerInput.MonthlyDayPeriod is null or <= 0)
                errors.AppendLine(Messages.ErrorMonthlyDayPeriodRequired);
        } else if (schedulerInput.MonthlyTheChk) {
            if (schedulerInput.MonthlyFrequency == null)
                errors.AppendLine(Messages.ErrorMonthlyFrequencyRequired);

            if (schedulerInput.MonthlyDateType == null)
                errors.AppendLine(Messages.ErrorMonthlyDateTypeRequired);

            if (schedulerInput.MonthlyThePeriod is null or <= 0)
                errors.AppendLine(Messages.ErrorMonthlyThePeriodRequired);
        }

        if (schedulerInput is { DailyStartTime: not null, DailyEndTime: not null } && schedulerInput.DailyStartTime > schedulerInput.DailyEndTime)
            errors.AppendLine(Messages.ErrorDailyStartAfterEnd);

        if (schedulerInput.DailyPeriod.HasValue && schedulerInput.DailyPeriod <= TimeSpan.Zero)
            errors.AppendLine(Messages.ErrorPositiveOffsetRequired);

        return errors.Length > 0 ? ResultPattern<bool>.Failure(errors.ToString()) : ResultPattern<bool>.Success(true);
    }
}
