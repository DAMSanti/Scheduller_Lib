using Scheduler_Lib.Core.Interfaces;
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services.Calculators.Base;
using Scheduler_Lib.Core.Services.Calculators.Daily;
using Scheduler_Lib.Core.Services.Helpers;
using Scheduler_Lib.Core.Services.Utilities;
using Scheduler_Lib.Core.Strategies.Base;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Strategies.Recurrence;

/// <summary>
/// Strategy for monthly recurrence calculations
/// </summary>
public class MonthlyRecurrenceStrategy : RecurrenceCalculationStrategyBase
{
    public override bool CanHandle(ISchedulerConfiguration configuration)
    {
        return configuration.Periodicity == EnumConfiguration.Recurrent && 
               configuration.Recurrency == EnumRecurrency.Monthly;
    }

    protected override void CalculateDatesCore(
        ISchedulerConfiguration configuration, 
        TimeZoneInfo timeZone, 
        DateTimeOffset baseDto, 
        DateTimeOffset endDate, 
        List<DateTimeOffset> accumulator)
    {
        var baseLocal = BaseDateTimeCalculator.GetBaseDateTime(configuration, timeZone);
        var currentMonth = new DateTime(baseLocal.Year, baseLocal.Month, 1);
        var endMonth = new DateTime(endDate.DateTime.Year, endDate.DateTime.Month, 1);

        var iteration = 0;
        var timeOfDay = configuration.TargetDate?.TimeOfDay ?? configuration.StartDate.TimeOfDay;
        var slotStep = configuration.DailyPeriod ?? TimeSpan.FromHours(1);

        while (currentMonth <= endMonth && iteration < Config.MaxIterations)
        {
            DateTimeOffset? nextEligible = null;

            if (configuration.MonthlyDayChk && configuration.MonthlyDay.HasValue)
            {
                nextEligible = GetEligibleDateByDay(currentMonth, configuration.MonthlyDay.Value, timeOfDay, timeZone);
            }
            else if (configuration.MonthlyTheChk && configuration.MonthlyFrequency.HasValue && configuration.MonthlyDateType.HasValue)
            {
                nextEligible = MonthlyEligibleDateHelper.GetEligibleDate(
                    currentMonth,
                    configuration.MonthlyFrequency.Value,
                    configuration.MonthlyDateType.Value,
                    timeOfDay,
                    timeZone);
            }

            if (nextEligible.HasValue && nextEligible.Value <= endDate)
            {
                if (configuration.DailyStartTime.HasValue && configuration.DailyEndTime.HasValue)
                {
                    DailySlotGenerator.GenerateSlotsForDay(
                        nextEligible.Value.DateTime.Date,
                        configuration.DailyStartTime.Value,
                        configuration.DailyEndTime.Value,
                        slotStep,
                        timeZone,
                        configuration,
                        endDate,
                        baseDto,
                        accumulator);
                }
                else
                {
                    if (!accumulator.Contains(nextEligible.Value))
                        accumulator.Add(nextEligible.Value);
                }
            }

            if (accumulator.Count >= Config.MaxIterations)
                break;

            var monthlyPeriod = (configuration.MonthlyDayChk ? configuration.MonthlyDayPeriod : configuration.MonthlyThePeriod) ?? 1;
            currentMonth = currentMonth.AddMonths(monthlyPeriod);
            iteration++;
        }
    }

    protected override DateTimeOffset GetEffectiveEndDate(ISchedulerConfiguration configuration, TimeZoneInfo timeZone)
    {
        if (configuration.EndDate.HasValue)
            return configuration.EndDate.Value;

        var period = configuration.DailyPeriod ?? TimeSpan.FromDays(3);
        var beginning = BaseDateTimeCalculator.GetBaseDateTime(configuration, timeZone);

        var endLocal = beginning.Add(period * Config.EffectiveEndDateMultiplier);
        return new DateTimeOffset(endLocal, timeZone.GetUtcOffset(endLocal));
    }

    private DateTimeOffset? GetEligibleDateByDay(DateTime month, int dayOfMonth, TimeSpan timeOfDay, TimeZoneInfo timeZone)
    {
        var lastDayOfMonth = DateTime.DaysInMonth(month.Year, month.Month);

        if (dayOfMonth > lastDayOfMonth)
            return null;

        var resultLocal = new DateTime(month.Year, month.Month, dayOfMonth,
            timeOfDay.Hours, timeOfDay.Minutes, timeOfDay.Seconds, DateTimeKind.Unspecified);

        return TimeZoneConverter.CreateDateTimeOffset(resultLocal, timeZone);
    }
}
