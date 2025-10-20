using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Resources;
using System.Text;

namespace Scheduler_Lib.Infrastructure.Validations;

public class ValidationRecurrent {
    public static ResultPattern<bool> ValidateRecurrent(SchedulerInput schedulerInput) {
        var errors = new StringBuilder();

        if (schedulerInput.Period == null || schedulerInput.Period.Value <= TimeSpan.Zero)
            errors.AppendLine(Messages.ErrorPositiveOffsetRequired);

        if (schedulerInput.StartDate > schedulerInput.EndDate)
            errors.AppendLine(Messages.ErrorStartDatePostEndDate);

        if (schedulerInput.CurrentDate < schedulerInput.StartDate || schedulerInput.CurrentDate > schedulerInput.EndDate)
            errors.AppendLine(Messages.ErrorDateOutOfRange);

        if (schedulerInput.Recurrency == EnumRecurrency.Daily &&
            (schedulerInput.DailyStartTime == null || schedulerInput.DailyEndTime == null))
            errors.AppendLine(Messages.ErrorDailyTimeWindowIncomplete);

        if (schedulerInput.Recurrency == EnumRecurrency.Daily && 
            (schedulerInput.DailyStartTime < schedulerInput.DailyEndTime))
            errors.AppendLine(Messages.ErrorDailyStartAfterEnd);

        if (!schedulerInput.WeeklyPeriod.HasValue)
            errors.AppendLine(Messages.ErrorWeeklyPeriodRequired);

        if (schedulerInput.DaysOfWeek == null || schedulerInput.DaysOfWeek.Count == 0)
            errors.AppendLine(Messages.ErrorDaysOfWeekRequired);

        return errors.Length > 0 ? ResultPattern<bool>.Failure(errors.ToString()) : ResultPattern<bool>.Success(true);
    }
}