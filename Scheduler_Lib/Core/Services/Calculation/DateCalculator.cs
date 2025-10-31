using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Model.Enum;
using Scheduler_Lib.Core.Services.Calculation.Helpers;

namespace Scheduler_Lib.Core.Services.Calculation;

public class DateCalculator {
    private readonly DateTimeHelper _dateTimeHelper;
    private readonly DailyCalculator _dailyCalculator;
    private readonly WeeklyCalculator _weeklyCalculator;
    private readonly MonthlyCalculator _monthlyCalculator;

    public DateCalculator() {
        _dateTimeHelper = new DateTimeHelper();
        var monthlyDateHelper = new MonthlyDateHelper();

        _dailyCalculator = new DailyCalculator(_dateTimeHelper);
        _weeklyCalculator = new WeeklyCalculator(_dateTimeHelper);
        _monthlyCalculator = new MonthlyCalculator(_dateTimeHelper, monthlyDateHelper);
    }

    public List<DateTimeOffset> CalculateFutureDates(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        if (schedulerInput.StartDate == DateTimeOffset.MaxValue || schedulerInput.EndDate == DateTimeOffset.MaxValue)
            return [];

        var endDate = _dateTimeHelper.GetEffectiveEndDate(schedulerInput);
        var slotStep = schedulerInput.DailyPeriod ?? TimeSpan.FromDays(1);
        var baseDateTimeOffset = _dateTimeHelper.GetBaseDateTimeOffset(schedulerInput, tz);

        return schedulerInput.Recurrency switch {
            EnumRecurrency.Daily => _dailyCalculator.Calculate(schedulerInput, tz, endDate, slotStep, baseDateTimeOffset),
            EnumRecurrency.Weekly when schedulerInput.DaysOfWeek is { Count: > 0 } => _weeklyCalculator.Calculate(schedulerInput, tz),
            EnumRecurrency.Monthly when schedulerInput.MonthlyFrequency.HasValue && schedulerInput.MonthlyDateType.HasValue => _monthlyCalculator.Calculate(schedulerInput, tz),
            _ => []
        };
    }

    public DateTimeOffset GetNextExecutionDate(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        if (schedulerInput.Recurrency == EnumRecurrency.Weekly) {
            var baseLocal = _dateTimeHelper.GetBaseLocal(schedulerInput);
            var baseDtoForNext = new DateTimeOffset(baseLocal, tz.GetUtcOffset(baseLocal));
            return _weeklyCalculator.SelectNextEligibleDate(baseDtoForNext, schedulerInput.DaysOfWeek!, tz);
        }

        if (schedulerInput.Recurrency == EnumRecurrency.Monthly) {
            var monthlyDates = _monthlyCalculator.Calculate(schedulerInput, tz);
            if (monthlyDates.Count > 0)
                return monthlyDates[0];
            
            return new DateTimeOffset(schedulerInput.CurrentDate.DateTime, tz.GetUtcOffset(schedulerInput.CurrentDate.DateTime));
        }

        if (schedulerInput.OccursOnceChk) {
            var once = schedulerInput.OccursOnceAt!.Value;
            return new DateTimeOffset(once.DateTime, tz.GetUtcOffset(once.DateTime));
        }

        if (schedulerInput.TargetDate.HasValue) {
            var td = schedulerInput.TargetDate.Value;
            return new DateTimeOffset(td.DateTime, tz.GetUtcOffset(td.DateTime));
        }

        var cur = schedulerInput.CurrentDate;
        return new DateTimeOffset(cur.DateTime, tz.GetUtcOffset(cur.DateTime));
    }
}
