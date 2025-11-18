using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services.Calculators.Base;
using Scheduler_Lib.Core.Services.Calculators.Monthly;
using Scheduler_Lib.Core.Services.Utilities;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Services.Calculators.Daily;

internal static class DailyRecurrenceCalculator {
    internal static List<DateTimeOffset> CalculateFutureDates(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        var dates = new List<DateTimeOffset>();

        if (schedulerInput.StartDate == DateTimeOffset.MaxValue || schedulerInput.EndDate == DateTimeOffset.MaxValue)
            return dates;

        var endDate = GetEffectiveEndDate(schedulerInput, tz);
        var slotStep = schedulerInput.DailyPeriod ?? TimeSpan.FromDays(1);

        var baseDateTimeOffset = BaseDateTimeCalculator.GetBaseDateTime(schedulerInput, tz);
        var baseDto = new DateTimeOffset(baseDateTimeOffset, tz.GetUtcOffset(baseDateTimeOffset));

        switch (schedulerInput.Recurrency) {
            case EnumRecurrency.Daily:
                if (!schedulerInput.DailyStartTime.HasValue || !schedulerInput.DailyEndTime.HasValue) {
                    AddSimpleDailySlots(baseDto, endDate, slotStep, schedulerInput, dates);
                    break;
                }
                FillDailyWindowSlots(schedulerInput, tz, endDate, slotStep, baseDto, dates);
                break;

            case EnumRecurrency.Weekly:
                FillWeeklySlots(schedulerInput, tz, endDate, slotStep, baseDto, dates);
                dates.Sort();
                break;

            case EnumRecurrency.Monthly:
                dates = MonthlyRecurrenceCalculator.CalculateFutureDates(schedulerInput, tz);
                break;
        }
        return dates;
    }

    private static void FillDailyWindowSlots(
        SchedulerInput schedulerInput, 
        TimeZoneInfo tz, 
        DateTimeOffset endDate, 
        TimeSpan slotStep, 
        DateTimeOffset earliestAllowed, 
        List<DateTimeOffset> accumulator) {
        
        var baseLocal = BaseDateTimeCalculator.GetBaseDateTime(schedulerInput, tz);
        var dayCursor = schedulerInput.StartDate.Date > baseLocal.Date ? schedulerInput.StartDate.Date : baseLocal.Date;
        var lastDay = endDate.Date;
        
        while (dayCursor <= lastDay) {
            DailySlotGenerator.GenerateSlotsForDay(
                dayCursor, 
                schedulerInput.DailyStartTime!.Value, 
                schedulerInput.DailyEndTime!.Value, 
                slotStep, 
                tz, 
                schedulerInput, 
                endDate, 
                earliestAllowed, 
                accumulator);
            dayCursor = dayCursor.AddDays(1);
        }
    }

    private static void FillWeeklySlots(
        SchedulerInput schedulerInput, 
        TimeZoneInfo tz, 
        DateTimeOffset endDate, 
        TimeSpan slotStep, 
        DateTimeOffset earliestAllowed, 
        List<DateTimeOffset> accumulator) {
        
        var weeklyPeriod = schedulerInput.WeeklyPeriod ?? 1;
        var baseLocal = BaseDateTimeCalculator.GetBaseDateTime(schedulerInput, tz);
        var weekStart = baseLocal.Date;

        var lastWeekDay = endDate.Date;
        const int DaysInWeek = 7;
        
        while (weekStart <= lastWeekDay) {
            foreach (var day in schedulerInput.DaysOfWeek!) {
                var timeOfDay = schedulerInput.TargetDate?.TimeOfDay ?? schedulerInput.StartDate.TimeOfDay;
                var candidateLocal = GetCandidateLocalForWeekAndDay(weekStart, day, timeOfDay);

                if (candidateLocal == null) continue;

                var candidateDayDto = TimeZoneConverter.CreateDateTimeOffset(candidateLocal.Value, tz);
                if (candidateDayDto > endDate) continue;
                if (candidateDayDto < earliestAllowed) continue;

                if (!schedulerInput.DailyStartTime.HasValue || !schedulerInput.DailyEndTime.HasValue) {
                    if (!accumulator.Contains(candidateDayDto)) 
                        accumulator.Add(candidateDayDto);
                    continue;
                }

                DailySlotGenerator.GenerateSlotsForDay(
                    candidateLocal.Value.Date, 
                    schedulerInput.DailyStartTime!.Value, 
                    schedulerInput.DailyEndTime!.Value, 
                    slotStep, 
                    tz, 
                    schedulerInput, 
                    endDate, 
                    earliestAllowed, 
                    accumulator);
            }

            weekStart = weekStart.AddDays(DaysInWeek * weeklyPeriod);
        }
    }

    private static DateTime? GetCandidateLocalForWeekAndDay(DateTime weekStart, DayOfWeek day, TimeSpan timeOfDay) {
        const int DaysInWeek = 7;
        var date = new DateTime(weekStart.Year, weekStart.Month, weekStart.Day, 
            timeOfDay.Hours, timeOfDay.Minutes, timeOfDay.Seconds, DateTimeKind.Unspecified);

        for (var i = 0; i < DaysInWeek; i++) {
            if (date.DayOfWeek == day)
                return date;

            if (!DateSafetyHelper.TryAddDaysSafely(date, 1, out var next))
                return null;

            date = next;
        }

        return null;
    }

    private static DateTimeOffset GetEffectiveEndDate(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        if (schedulerInput.EndDate.HasValue)
            return schedulerInput.EndDate.Value;

        var period = schedulerInput.DailyPeriod ?? TimeSpan.FromDays(3);
        var beginning = BaseDateTimeCalculator.GetBaseDateTime(schedulerInput, tz);

        const int defaultPeriodMultiplier = 1000;
        var endLocal = beginning.Add(period * defaultPeriodMultiplier);
        return new DateTimeOffset(endLocal, tz.GetUtcOffset(endLocal));
    }

    private static void AddSimpleDailySlots(
    DateTimeOffset startFrom,
    DateTimeOffset endDate,
    TimeSpan step,
    SchedulerInput schedulerInput,
    List<DateTimeOffset> accumulator) {

        if (schedulerInput.TargetDate == null) {
            var startTime = schedulerInput.CurrentDate.TimeOfDay;
            startFrom = new DateTimeOffset(
                startFrom.Year, startFrom.Month, startFrom.Day,
                startTime.Hours, startTime.Minutes, startTime.Seconds,
                startFrom.Offset
            );
        }

        while (startFrom <= endDate) {
            accumulator.Add(startFrom);
            startFrom = startFrom.Add(step);
        }
    }
}
