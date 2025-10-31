using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Model.Enum;
using Scheduler_Lib.Core.Services.Calculation.Helpers;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Services.Calculation;

public class MonthlyCalculator {
    private readonly DateTimeHelper _dateTimeHelper;
    private readonly MonthlyDateHelper _monthlyDateHelper;

    public MonthlyCalculator(DateTimeHelper dateTimeHelper, MonthlyDateHelper monthlyDateHelper) {
        _dateTimeHelper = dateTimeHelper;
        _monthlyDateHelper = monthlyDateHelper;
    }

    public List<DateTimeOffset> Calculate(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        var dates = new List<DateTimeOffset>();
        var baseLocal = _dateTimeHelper.GetBaseLocal(schedulerInput);
        var endLocal = schedulerInput.EndDate ?? _dateTimeHelper.GetEffectiveEndDate(schedulerInput);

        var currentMonth = new DateTime(baseLocal.Year, baseLocal.Month, 1);
        var endMonth = new DateTime(endLocal.DateTime.Year, endLocal.DateTime.Month, 1);

        var iteration = 0;
        const int maxIterations = Config.MaxIterations;

        var timeOfDay = schedulerInput.TargetDate?.TimeOfDay ?? schedulerInput.StartDate.TimeOfDay;

        while (currentMonth <= endMonth && iteration < maxIterations) {
            var nextEligible = GetMonthlyEligibleDate(
                currentMonth,
                schedulerInput.MonthlyFrequency!.Value,
                schedulerInput.MonthlyDateType!.Value,
                timeOfDay,
                tz
            );

            if (nextEligible.HasValue) {
                var baseDto = new DateTimeOffset(baseLocal, tz.GetUtcOffset(baseLocal));
                if (nextEligible.Value >= baseDto && nextEligible.Value <= endLocal && !dates.Contains(nextEligible.Value))
                    dates.Add(nextEligible.Value);
            }

            if (dates.Count >= maxIterations)
                break;

            var monthlyPeriod = schedulerInput.MonthlyThePeriod ?? 1;
            currentMonth = currentMonth.AddMonths(monthlyPeriod);
            iteration++;
        }

        dates.Sort();
        return dates;
    }

    private DateTimeOffset? GetMonthlyEligibleDate(DateTime month, EnumMonthlyFrequency frequency, EnumMonthlyDateType dateType, TimeSpan timeOfDay, TimeZoneInfo tz) {
        var eligibleDays = _monthlyDateHelper.GetEligibleDays(month, dateType);

        if (eligibleDays.Count == 0)
            return null;

        var selectedDay = _monthlyDateHelper.SelectDay(eligibleDays, frequency);

        var resultLocal = new DateTime(selectedDay.Year, selectedDay.Month, selectedDay.Day,
            timeOfDay.Hours, timeOfDay.Minutes, timeOfDay.Seconds, DateTimeKind.Unspecified);

        return _dateTimeHelper.CreateDateTimeOffset(resultLocal, tz);
    }
}