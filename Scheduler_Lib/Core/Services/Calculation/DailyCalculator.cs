using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services.Calculation.Helpers;

namespace Scheduler_Lib.Core.Services.Calculation;

public class DailyCalculator {
    private readonly DateTimeHelper _dateTimeHelper;

    public DailyCalculator(DateTimeHelper dateTimeHelper) {
        _dateTimeHelper = dateTimeHelper;
    }

    public List<DateTimeOffset> Calculate(SchedulerInput schedulerInput, TimeZoneInfo tz, DateTimeOffset endDate, TimeSpan slotStep, DateTimeOffset baseDateTimeOffset) {
        var dates = new List<DateTimeOffset>();

        if (!schedulerInput.DailyStartTime.HasValue || !schedulerInput.DailyEndTime.HasValue) {
            AddSimpleDailySlots(baseDateTimeOffset, endDate, slotStep, schedulerInput, tz, dates);
            return dates;
        }

        FillDailyWindowSlots(schedulerInput, tz, endDate, slotStep, baseDateTimeOffset, dates);
        return dates;
    }

    private void AddSimpleDailySlots(DateTimeOffset startFrom, DateTimeOffset endDate, TimeSpan step, SchedulerInput schedulerInput, TimeZoneInfo tz, List<DateTimeOffset> accumulator) {
        if (schedulerInput.TargetDate == null) {
            var startTime = schedulerInput.CurrentDate.TimeOfDay;
            startFrom = new DateTimeOffset(
                startFrom.Year, startFrom.Month, startFrom.Day,
                startTime.Hours, startTime.Minutes, startTime.Seconds,
                startFrom.Offset
            );
        }

        while (startFrom <= endDate) {
            var adjustedDate = _dateTimeHelper.CreateDateTimeOffset(startFrom.DateTime, tz);
            accumulator.Add(adjustedDate);
            startFrom = startFrom.Add(step);
        }
    }

    private void FillDailyWindowSlots(SchedulerInput schedulerInput, TimeZoneInfo tz, DateTimeOffset endDate, TimeSpan slotStep, DateTimeOffset earliestAllowed, List<DateTimeOffset> accumulator) {
        var baseLocal = _dateTimeHelper.GetBaseLocal(schedulerInput);
        var dayCursor = schedulerInput.StartDate.Date > baseLocal.Date ? schedulerInput.StartDate.Date : baseLocal.Date;
        var lastDay = endDate.Date;

        while (dayCursor <= lastDay) {
            GenerateDailySlotsForDay(dayCursor, schedulerInput.DailyStartTime!.Value, schedulerInput.DailyEndTime!.Value, slotStep, tz, schedulerInput, endDate, earliestAllowed, accumulator);
            dayCursor = dayCursor.AddDays(1);
        }
    }

    private void GenerateDailySlotsForDay(DateTime day, TimeSpan start, TimeSpan end, TimeSpan step, TimeZoneInfo tz, SchedulerInput schedulerInput, DateTimeOffset endDate, DateTimeOffset earliestAllowed, List<DateTimeOffset> accumulator) {
        var startLocal = new DateTime(day.Year, day.Month, day.Day,
            start.Hours, start.Minutes, start.Seconds, DateTimeKind.Unspecified);

        var endLocal = new DateTime(day.Year, day.Month, day.Day,
            end.Hours, end.Minutes, end.Seconds, DateTimeKind.Unspecified);

        var slotLocal = startLocal;
        while (slotLocal <= endLocal) {
            var slotDateTimeOffset = _dateTimeHelper.CreateDateTimeOffset(slotLocal, tz);
            if (slotDateTimeOffset >= schedulerInput.StartDate && slotDateTimeOffset <= endDate && slotDateTimeOffset > earliestAllowed && !accumulator.Contains(slotDateTimeOffset))
                accumulator.Add(slotDateTimeOffset);

            slotLocal = slotLocal.Add(step);
        }
    }
}