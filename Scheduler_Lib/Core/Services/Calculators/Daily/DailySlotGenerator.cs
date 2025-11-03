using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services.Utilities;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Services.Calculators.Daily;

public static class DailySlotGenerator {
    public static void GenerateSlotsForDay(
        DateTime day, 
        TimeSpan startTime, 
        TimeSpan endTime, 
        TimeSpan step, 
        TimeZoneInfo tz, 
        SchedulerInput schedulerInput, 
        DateTimeOffset endDate, 
        DateTimeOffset earliestAllowed, 
        List<DateTimeOffset> accumulator) {
        
        var startLocal = new DateTime(day.Year, day.Month, day.Day, 
            startTime.Hours, startTime.Minutes, startTime.Seconds, DateTimeKind.Unspecified);
        
        var endLocal = new DateTime(day.Year, day.Month, day.Day, 
            endTime.Hours, endTime.Minutes, endTime.Seconds, DateTimeKind.Unspecified);

        var slotLocal = startLocal;
        while (slotLocal <= endLocal) {
            ProcessSlotTime(slotLocal, tz, schedulerInput, endDate, earliestAllowed, accumulator);
            slotLocal = slotLocal.Add(step);
        }
    }

    private static void ProcessSlotTime(
        DateTime slotLocal, 
        TimeZoneInfo tz, 
        SchedulerInput schedulerInput, 
        DateTimeOffset endDate, 
        DateTimeOffset earliestAllowed, 
        List<DateTimeOffset> accumulator) {

        if (tz.IsAmbiguousTime(slotLocal)) {
            HandleAmbiguousTime(slotLocal, tz, schedulerInput, endDate, earliestAllowed, accumulator);
            return;
        }

        if (tz.IsInvalidTime(slotLocal)) {
            return;
        }

        var slotDateTimeOffset = TimeZoneConverter.CreateDateTimeOffset(slotLocal, tz);
        if (IsSlotValid(slotDateTimeOffset, schedulerInput.StartDate, endDate, earliestAllowed, accumulator)) {
            accumulator.Add(slotDateTimeOffset);
        }
    }

    private static void HandleAmbiguousTime(
        DateTime slotLocal, 
        TimeZoneInfo tz, 
        SchedulerInput schedulerInput, 
        DateTimeOffset endDate, 
        DateTimeOffset earliestAllowed, 
        List<DateTimeOffset> accumulator) {
        
        var offsets = tz.GetAmbiguousTimeOffsets(slotLocal);

        foreach (var offset in offsets.OrderByDescending(o => o)) {
            var slotDateTimeOffset = new DateTimeOffset(slotLocal, offset);
            if (IsSlotValid(slotDateTimeOffset, schedulerInput.StartDate, endDate, earliestAllowed, accumulator)) {
                accumulator.Add(slotDateTimeOffset);
            }
        }
    }

    private static bool IsSlotValid(
        DateTimeOffset slot, 
        DateTimeOffset startDate, 
        DateTimeOffset endDate, 
        DateTimeOffset earliestAllowed, 
        List<DateTimeOffset> accumulator) {
        
        return slot >= startDate 
            && slot <= endDate 
            && slot > earliestAllowed 
            && !accumulator.Contains(slot);
    }

    public static void AddSimpleDailySlots(
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
