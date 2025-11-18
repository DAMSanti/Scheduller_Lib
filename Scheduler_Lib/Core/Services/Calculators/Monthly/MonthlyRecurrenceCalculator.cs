using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services.Calculators.Base;
using Scheduler_Lib.Core.Services.Calculators.Weekly;
using Scheduler_Lib.Core.Services.Calculators.Daily;
using Scheduler_Lib.Core.Services.Utilities;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Services.Calculators.Monthly;

internal static class MonthlyRecurrenceCalculator {
    internal static List<DateTimeOffset> CalculateFutureDates(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        var dates = new List<DateTimeOffset>();
        var baseLocal = BaseDateTimeCalculator.GetBaseDateTime(schedulerInput, tz);
        var baseDto = new DateTimeOffset(baseLocal, tz.GetUtcOffset(baseLocal));
        var endLocal = schedulerInput.EndDate ?? GetEffectiveEndDate(schedulerInput, tz);

        var currentMonth = new DateTime(baseLocal.Year, baseLocal.Month, 1);
        var endMonth = new DateTime(endLocal.DateTime.Year, endLocal.DateTime.Month, 1);

        var iteration = 0;
        const int maxIterations = Config.MaxIterations;

        var timeOfDay = schedulerInput.TargetDate?.TimeOfDay ?? schedulerInput.StartDate.TimeOfDay;
        var slotStep = schedulerInput.DailyPeriod ?? TimeSpan.FromHours(1);

        while (currentMonth <= endMonth && iteration < maxIterations) {
            DateTimeOffset? nextEligible = null;

            if (schedulerInput.MonthlyDayChk && schedulerInput.MonthlyDay.HasValue) {
                nextEligible = GetEligibleDateByDay(currentMonth, schedulerInput.MonthlyDay.Value, timeOfDay, tz);
            } else if (schedulerInput.MonthlyTheChk && schedulerInput.MonthlyFrequency.HasValue && schedulerInput.MonthlyDateType.HasValue) {
                var targetDate = new DateTimeOffset(
                    new DateTime(currentMonth.Year, currentMonth.Month, 1, timeOfDay.Hours, timeOfDay.Minutes, timeOfDay.Seconds),
                    tz.GetUtcOffset(currentMonth));

                nextEligible = WeeklyRecurrenceCalculator.SelectNextEligibleDate(
                    targetDate, null!, tz,
                    schedulerInput.MonthlyFrequency,
                    schedulerInput.MonthlyDateType,
                    currentMonth);
            }

            if (nextEligible.HasValue && nextEligible.Value <= endLocal) {
                if (schedulerInput.DailyStartTime.HasValue && schedulerInput.DailyEndTime.HasValue) {
                    DailySlotGenerator.GenerateSlotsForDay(
                        nextEligible.Value.DateTime.Date,
                        schedulerInput.DailyStartTime.Value,
                        schedulerInput.DailyEndTime.Value,
                        slotStep,
                        tz,
                        schedulerInput,
                        endLocal,
                        baseDto,
                        dates);
                } else {
                    if (!dates.Contains(nextEligible.Value))
                        dates.Add(nextEligible.Value);
                }
            }

            if (dates.Count >= maxIterations)
                break;

            var monthlyPeriod = (schedulerInput.MonthlyDayChk ? schedulerInput.MonthlyDayPeriod : schedulerInput.MonthlyThePeriod) ?? 1;
            currentMonth = currentMonth.AddMonths(monthlyPeriod);
            iteration++;
        }

        dates.Sort();
        return dates;
    }

    private static DateTimeOffset? GetEligibleDateByDay(DateTime month, int dayOfMonth, TimeSpan timeOfDay, TimeZoneInfo tz) {
        var lastDayOfMonth = DateTime.DaysInMonth(month.Year, month.Month);

        if (dayOfMonth > lastDayOfMonth)
            return null;
        
        var resultLocal = new DateTime(month.Year, month.Month, dayOfMonth,
            timeOfDay.Hours, timeOfDay.Minutes, timeOfDay.Seconds, DateTimeKind.Unspecified);

        return TimeZoneConverter.CreateDateTimeOffset(resultLocal, tz);
    }

    private static DateTimeOffset GetEffectiveEndDate(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        var period = schedulerInput.DailyPeriod ?? TimeSpan.FromDays(3);
        var beginning = BaseDateTimeCalculator.GetBaseDateTime(schedulerInput, tz);

        const int defaultPeriodMultiplier = 1000;
        var endLocal = beginning.Add(period * defaultPeriodMultiplier);
        return new DateTimeOffset(endLocal, tz.GetUtcOffset(endLocal));
    }
}
