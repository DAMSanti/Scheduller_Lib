using Scheduler_Lib.Core.Interfaces;
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services.Calculators.Base;
using Scheduler_Lib.Core.Services.Calculators.Daily;
using Scheduler_Lib.Core.Services.Utilities;
using Scheduler_Lib.Core.Strategies.Base;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Strategies.Recurrence;

/// <summary>
/// Strategy for weekly recurrence calculations
/// </summary>
public class WeeklyRecurrenceStrategy : RecurrenceCalculationStrategyBase
{
    private const int DaysInWeek = 7;

    public override bool CanHandle(ISchedulerConfiguration configuration)
    {
        return configuration.Periodicity == EnumConfiguration.Recurrent && 
               configuration.Recurrency == EnumRecurrency.Weekly;
    }

    protected override void CalculateDatesCore(
        ISchedulerConfiguration configuration, 
        TimeZoneInfo timeZone, 
        DateTimeOffset baseDto, 
        DateTimeOffset endDate, 
        List<DateTimeOffset> accumulator)
    {
        var weeklyPeriod = configuration.WeeklyPeriod ?? 1;
        var baseLocal = BaseDateTimeCalculator.GetBaseDateTime(configuration, timeZone);
        var weekStart = baseLocal.Date;
        var lastWeekDay = endDate.Date;
        var slotStep = configuration.DailyPeriod ?? TimeSpan.FromHours(1);

        while (weekStart <= lastWeekDay)
        {
            foreach (var day in configuration.DaysOfWeek!)
            {
                var timeOfDay = configuration.TargetDate?.TimeOfDay ?? configuration.StartDate.TimeOfDay;
                var candidateLocal = GetCandidateLocalForWeekAndDay(weekStart, day, timeOfDay);

                if (candidateLocal == null) continue;

                var candidateDayDto = TimeZoneConverter.CreateDateTimeOffset(candidateLocal.Value, timeZone);
                if (candidateDayDto > endDate) continue;
                if (candidateDayDto < baseDto) continue;

                if (!configuration.DailyStartTime.HasValue || !configuration.DailyEndTime.HasValue)
                {
                    if (!accumulator.Contains(candidateDayDto))
                        accumulator.Add(candidateDayDto);
                    continue;
                }

                DailySlotGenerator.GenerateSlotsForDay(
                    candidateLocal.Value.Date, 
                    configuration.DailyStartTime!.Value, 
                    configuration.DailyEndTime!.Value, 
                    slotStep, 
                    timeZone, 
                    configuration, 
                    endDate, 
                    baseDto, 
                    accumulator);
            }

            weekStart = weekStart.AddDays(DaysInWeek * weeklyPeriod);
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

    private DateTime? GetCandidateLocalForWeekAndDay(DateTime weekStart, DayOfWeek day, TimeSpan timeOfDay)
    {
        var date = new DateTime(weekStart.Year, weekStart.Month, weekStart.Day,
            timeOfDay.Hours, timeOfDay.Minutes, timeOfDay.Seconds, DateTimeKind.Unspecified);

        for (var i = 0; i < DaysInWeek; i++)
        {
            if (date.DayOfWeek == day)
                return date;

            if (!DateSafetyHelper.TryAddDaysSafely(date, 1, out var next))
                return null;

            date = next;
        }

        return null;
    }
}
