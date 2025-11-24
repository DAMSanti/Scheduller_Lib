using Scheduler_Lib.Core.Interfaces;
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services.Calculators.Base;
using Scheduler_Lib.Core.Services.Calculators.Daily;
using Scheduler_Lib.Core.Services.Utilities;
using Scheduler_Lib.Core.Strategies.Base;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Strategies.Recurrence;

/// <summary>
/// Strategy for daily recurrence calculations
/// </summary>
public class DailyRecurrenceStrategy : RecurrenceCalculationStrategyBase
{
    public override bool CanHandle(ISchedulerConfiguration configuration)
    {
        return configuration.Periodicity == EnumConfiguration.Recurrent && 
               configuration.Recurrency == EnumRecurrency.Daily;
    }

    protected override void CalculateDatesCore(
        ISchedulerConfiguration configuration, 
        TimeZoneInfo timeZone, 
        DateTimeOffset baseDto, 
        DateTimeOffset endDate, 
        List<DateTimeOffset> accumulator)
    {
        var slotStep = configuration.DailyPeriod ?? TimeSpan.FromDays(1);

        if (!configuration.DailyStartTime.HasValue || !configuration.DailyEndTime.HasValue)
        {
            AddSimpleDailySlots(baseDto, endDate, slotStep, configuration, accumulator);
            return;
        }

        FillDailyWindowSlots(configuration, timeZone, endDate, slotStep, baseDto, accumulator);
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

    private void FillDailyWindowSlots(
        ISchedulerConfiguration configuration, 
        TimeZoneInfo timeZone, 
        DateTimeOffset endDate, 
        TimeSpan slotStep, 
        DateTimeOffset earliestAllowed, 
        List<DateTimeOffset> accumulator)
    {
        var baseLocal = BaseDateTimeCalculator.GetBaseDateTime(configuration, timeZone);
        var dayCursor = configuration.StartDate.Date > baseLocal.Date 
            ? configuration.StartDate.Date 
            : baseLocal.Date;
        var lastDay = endDate.Date;

        while (dayCursor <= lastDay)
        {
            DailySlotGenerator.GenerateSlotsForDay(
                dayCursor, 
                configuration.DailyStartTime!.Value, 
                configuration.DailyEndTime!.Value, 
                slotStep, 
                timeZone, 
                configuration, 
                endDate, 
                earliestAllowed, 
                accumulator);
            dayCursor = dayCursor.AddDays(1);
        }
    }

    private void AddSimpleDailySlots(
        DateTimeOffset startFrom,
        DateTimeOffset endDate,
        TimeSpan step,
        ISchedulerConfiguration configuration,
        List<DateTimeOffset> accumulator)
    {
        if (configuration.TargetDate == null)
        {
            var startTime = configuration.CurrentDate.TimeOfDay;
            startFrom = new DateTimeOffset(
                startFrom.Year, startFrom.Month, startFrom.Day,
                startTime.Hours, startTime.Minutes, startTime.Seconds,
                startFrom.Offset
            );
        }

        while (startFrom <= endDate)
        {
            accumulator.Add(startFrom);
            startFrom = startFrom.Add(step);
        }
    }
}
