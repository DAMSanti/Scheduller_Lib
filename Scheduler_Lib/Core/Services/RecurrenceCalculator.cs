using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Services;

public class RecurrenceCalculator {
    private const int DaysInWeek = 7;
    
    public static DateTimeOffset SelectNextEligibleDate(DateTimeOffset targetDate, List<DayOfWeek> daysOfWeek, TimeZoneInfo tz, EnumMonthlyFrequency? monthlyFrequency = null, EnumMonthlyDateType? monthlyDateType = null, DateTime? currentMonth = null) {
        if (targetDate == DateTimeOffset.MinValue)
            return DateTimeOffset.MinValue;

        var targetLocal = targetDate.DateTime;

        if (monthlyFrequency.HasValue && monthlyDateType.HasValue && currentMonth.HasValue) {
            var eligibleDate = GetMonthlyEligibleDate(currentMonth.Value, monthlyFrequency.Value, monthlyDateType.Value, targetLocal.TimeOfDay, tz);
            return eligibleDate ?? new DateTimeOffset(targetLocal, tz.GetUtcOffset(targetLocal));
        }

        var candidates = daysOfWeek
            .Select(day => NextWeekday(targetLocal, day, tz))
            .OrderBy(dateTimeOffset => dateTimeOffset!.Value.DateTime)
            .Select(dateTimeOffset => dateTimeOffset!.Value)
            .ToList();

        return candidates.Count > 0
            ? candidates.First()
            : new DateTimeOffset(targetLocal, tz.GetUtcOffset(targetLocal));
    }

    public static List<DateTimeOffset>? CalculateWeeklyRecurrence(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        var dates = new List<DateTimeOffset>();
        var baseLocal = GetBaseDateTime(schedulerInput, tz);
        var nextEligible = SelectNextEligibleDate(new DateTimeOffset(baseLocal, tz.GetUtcOffset(baseLocal)),
            schedulerInput.DaysOfWeek!, tz);

        var iteration = 0;
        var weekStart = baseLocal.Date;
        var endLocal = schedulerInput.EndDate;

        const int maxIterations = Config.MaxIterations;

        while (weekStart <= endLocal && iteration < maxIterations) {
            GenerateWeeklySlotsForWeek(weekStart, schedulerInput, tz, nextEligible, dates);

            if (dates.Count >= maxIterations)
                return dates;

            var stepDays = DaysInWeek * schedulerInput.WeeklyPeriod!.Value;
            if (!TryAddDaysSafely(weekStart, stepDays, out var nextWeekStart))
                break;

            weekStart = nextWeekStart;
            iteration++;
        }

        dates.Sort();
        return dates;
    }

    public static List<DateTimeOffset> CalculateMonthlyRecurrence(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        var dates = new List<DateTimeOffset>();
        var baseLocal = GetBaseDateTime(schedulerInput, tz);
        var endLocal = schedulerInput.EndDate ?? GetEffectiveEndDate(schedulerInput);

        var currentMonth = new DateTime(baseLocal.Year, baseLocal.Month, 1);
        var endMonth = new DateTime(endLocal.DateTime.Year, endLocal.DateTime.Month, 1);

        var iteration = 0;
        const int maxIterations = Config.MaxIterations;

        var timeOfDay = schedulerInput.TargetDate?.TimeOfDay ?? schedulerInput.StartDate.TimeOfDay;

        while (currentMonth <= endMonth && iteration < maxIterations) {
            DateTimeOffset? nextEligible = null;

            if (schedulerInput.MonthlyDayChk && schedulerInput.MonthlyDay.HasValue) {
                nextEligible = GetMonthlyEligibleDateByDay(currentMonth, schedulerInput.MonthlyDay.Value, timeOfDay, tz);
            }

            else if (schedulerInput.MonthlyTheChk && schedulerInput.MonthlyFrequency.HasValue && schedulerInput.MonthlyDateType.HasValue) {
                var targetDate = new DateTimeOffset(
                    new DateTime(currentMonth.Year, currentMonth.Month, 1, timeOfDay.Hours, timeOfDay.Minutes, timeOfDay.Seconds),
                    tz.GetUtcOffset(currentMonth));

                nextEligible = SelectNextEligibleDate(targetDate, null!, tz, schedulerInput.MonthlyFrequency, schedulerInput.MonthlyDateType, currentMonth);
            }

            if (nextEligible.HasValue) {
                var baseOffset = new DateTimeOffset(baseLocal, tz.GetUtcOffset(baseLocal));
                

                if (schedulerInput.DailyStartTime.HasValue && schedulerInput.DailyEndTime.HasValue) {
                    var slotStep = schedulerInput.DailyPeriod ?? TimeSpan.FromMinutes(30);
                    GenerateDailySlotsForDay(
                        nextEligible.Value.DateTime.Date,
                        schedulerInput.DailyStartTime.Value,
                        schedulerInput.DailyEndTime.Value,
                        slotStep,
                        tz,
                        schedulerInput,
                        endLocal,
                        baseOffset,
                        dates);
                } else
                    if (nextEligible.Value >= baseOffset && nextEligible.Value <= endLocal && !dates.Contains(nextEligible.Value))
                        dates.Add(nextEligible.Value);
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

    private static DateTimeOffset? GetMonthlyEligibleDate(DateTime month, EnumMonthlyFrequency frequency, EnumMonthlyDateType dateType, TimeSpan timeOfDay, TimeZoneInfo tz) {
        var firstDayOfMonth = new DateTime(month.Year, month.Month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        List<DateTime> eligibleDays = dateType switch {
            EnumMonthlyDateType.Day => Enumerable.Range(1, lastDayOfMonth.Day)
                .Select(d => new DateTime(month.Year, month.Month, d))
                .ToList(),

            EnumMonthlyDateType.Weekday => GetWeekdaysInMonth(firstDayOfMonth, lastDayOfMonth),

            EnumMonthlyDateType.WeekendDay => GetWeekendDaysInMonth(firstDayOfMonth, lastDayOfMonth),

            EnumMonthlyDateType.Monday => GetSpecificDayOfWeekInMonth(firstDayOfMonth, lastDayOfMonth, DayOfWeek.Monday),
            EnumMonthlyDateType.Tuesday => GetSpecificDayOfWeekInMonth(firstDayOfMonth, lastDayOfMonth, DayOfWeek.Tuesday),
            EnumMonthlyDateType.Wednesday => GetSpecificDayOfWeekInMonth(firstDayOfMonth, lastDayOfMonth, DayOfWeek.Wednesday),
            EnumMonthlyDateType.Thursday => GetSpecificDayOfWeekInMonth(firstDayOfMonth, lastDayOfMonth, DayOfWeek.Thursday),
            EnumMonthlyDateType.Friday => GetSpecificDayOfWeekInMonth(firstDayOfMonth, lastDayOfMonth, DayOfWeek.Friday),
            EnumMonthlyDateType.Saturday => GetSpecificDayOfWeekInMonth(firstDayOfMonth, lastDayOfMonth, DayOfWeek.Saturday),
            EnumMonthlyDateType.Sunday => GetSpecificDayOfWeekInMonth(firstDayOfMonth, lastDayOfMonth, DayOfWeek.Sunday),

            _ => []
        };

        if (eligibleDays.Count == 0)
            return null;

        var selectedDay = frequency switch  {
            EnumMonthlyFrequency.First => eligibleDays.First(),
            EnumMonthlyFrequency.Second => eligibleDays.Count > 1 ? eligibleDays[1] : eligibleDays.Last(),
            EnumMonthlyFrequency.Third => eligibleDays.Count > 2 ? eligibleDays[2] : eligibleDays.Last(),
            EnumMonthlyFrequency.Fourth => eligibleDays.Count > 3 ? eligibleDays[3] : eligibleDays.Last(),
            EnumMonthlyFrequency.Last => eligibleDays.Last(),
            _ => eligibleDays.First()
        };

        var resultLocal = new DateTime(selectedDay.Year, selectedDay.Month, selectedDay.Day,
            timeOfDay.Hours, timeOfDay.Minutes, timeOfDay.Seconds, DateTimeKind.Unspecified);

        return CreateDateTimeOffset(resultLocal, tz);
    }

    private static DateTimeOffset? GetMonthlyEligibleDateByDay(DateTime month, int dayOfMonth, TimeSpan timeOfDay, TimeZoneInfo tz) {
        var lastDayOfMonth = DateTime.DaysInMonth(month.Year, month.Month);

        if (dayOfMonth > lastDayOfMonth)
            return null;
        
        var resultLocal = new DateTime(month.Year, month.Month, dayOfMonth,
            timeOfDay.Hours, timeOfDay.Minutes, timeOfDay.Seconds, DateTimeKind.Unspecified);

        return CreateDateTimeOffset(resultLocal, tz);
    }

    private static List<DateTime> GetWeekdaysInMonth(DateTime firstDay, DateTime lastDay) {
        var days = new List<DateTime>();
        for (var day = firstDay; day <= lastDay; day = day.AddDays(1)) {
            if (day.DayOfWeek != DayOfWeek.Saturday && day.DayOfWeek != DayOfWeek.Sunday)
                days.Add(day);
        }
        return days;
    }
    private static List<DateTime> GetWeekendDaysInMonth(DateTime firstDay, DateTime lastDay) {
        var days = new List<DateTime>();
        for (var day = firstDay; day <= lastDay; day = day.AddDays(1)) {
            if (day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                days.Add(day);
        }
        return days;
    }
    private static List<DateTime> GetSpecificDayOfWeekInMonth(DateTime firstDay, DateTime lastDay, DayOfWeek targetDayOfWeek) {
        var days = new List<DateTime>();
        for (var day = firstDay; day <= lastDay; day = day.AddDays(1)) {
            if (day.DayOfWeek == targetDayOfWeek)
                days.Add(day);
        }
        return days;
    }
    public static List<DateTimeOffset> CalculateFutureDates(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        var dates = new List<DateTimeOffset>();

        if (schedulerInput.StartDate == DateTimeOffset.MaxValue || schedulerInput.EndDate == DateTimeOffset.MaxValue)
            return dates;

        var endDate = GetEffectiveEndDate(schedulerInput);
        var slotStep = schedulerInput.DailyPeriod ?? TimeSpan.FromDays(1);

        var baseDateTimeOffset = GetBaseDateTime(schedulerInput, tz);
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
                if (schedulerInput.DaysOfWeek == null || schedulerInput.DaysOfWeek.Count == 0)
                    return dates;

                FillWeeklySlots(schedulerInput, tz, endDate, slotStep, baseDto, dates);
                dates.Sort();
                break;
            case EnumRecurrency.Monthly:

                if (schedulerInput.MonthlyDayChk && schedulerInput.MonthlyDay.HasValue && schedulerInput.MonthlyDayPeriod.HasValue) {
                    dates = CalculateMonthlyRecurrence(schedulerInput, tz);
                    break;
                }
                
                if (schedulerInput.MonthlyTheChk && schedulerInput.MonthlyFrequency.HasValue && schedulerInput.MonthlyDateType.HasValue) {
                    dates = CalculateMonthlyRecurrence(schedulerInput, tz);
                    break;
                }
                
                return dates;
            default:
                break;
        }
        return dates;
    }


    private static void AddSimpleDailySlots(DateTimeOffset startFrom, DateTimeOffset endDate, TimeSpan step, SchedulerInput schedulerInput, List<DateTimeOffset> accumulator) {
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

    private static void FillDailyWindowSlots(SchedulerInput schedulerInput, TimeZoneInfo tz, DateTimeOffset endDate, TimeSpan slotStep, DateTimeOffset earliestAllowed, List<DateTimeOffset> accumulator) {
        var baseLocal = GetBaseDateTime(schedulerInput, tz);
        var dayCursor = schedulerInput.StartDate.Date > baseLocal.Date ? schedulerInput.StartDate.Date : baseLocal.Date;
        var lastDay = endDate.Date;
        while (dayCursor <= lastDay) {
            GenerateDailySlotsForDay(dayCursor, schedulerInput.DailyStartTime!.Value, schedulerInput.DailyEndTime!.Value, slotStep, tz, schedulerInput, endDate, earliestAllowed, accumulator);
            dayCursor = dayCursor.AddDays(1);
        }
    }

    private static void FillWeeklySlots(SchedulerInput schedulerInput, TimeZoneInfo tz, DateTimeOffset endDate, TimeSpan slotStep, DateTimeOffset earliestAllowed, List<DateTimeOffset> accumulator) {
        var weeklyPeriod = schedulerInput.WeeklyPeriod ?? 1;
        var baseLocal = GetBaseDateTime(schedulerInput, tz);
        var weekStart = baseLocal.Date;

        var lastWeekDay = endDate.Date;
        while (weekStart <= lastWeekDay) {
            foreach (var day in schedulerInput.DaysOfWeek!) {
                var timeOfDay = schedulerInput.TargetDate?.TimeOfDay ?? schedulerInput.StartDate.TimeOfDay;
                var candidateLocal = GetCandidateLocalForWeekAndDay(weekStart, day, timeOfDay);

                var candidateDayDto = CreateDateTimeOffset(candidateLocal!.Value, tz);
                if (candidateDayDto > endDate) continue;

                if (candidateDayDto < earliestAllowed) continue;

                if (!schedulerInput.DailyStartTime.HasValue || !schedulerInput.DailyEndTime.HasValue) {
                    if (!accumulator.Contains(candidateDayDto)) accumulator.Add(candidateDayDto);
                    continue;
                }

                GenerateDailySlotsForDay(candidateLocal.Value.Date, schedulerInput.DailyStartTime!.Value, schedulerInput.DailyEndTime!.Value, slotStep, tz, schedulerInput, endDate, earliestAllowed, accumulator);
            }

            weekStart = weekStart.AddDays(DaysInWeek * weeklyPeriod);
        }
    }

    private static DateTimeOffset GetEffectiveEndDate(SchedulerInput schedulerInput) {
        if (schedulerInput.EndDate.HasValue)
            return schedulerInput.EndDate.Value;

        var period = schedulerInput.DailyPeriod ?? TimeSpan.FromDays(3);
        var beginning = GetBaseDateTime(schedulerInput, GetTimeZone());

        const int defaultPeriodMultiplier = 1000;
        var tz = GetTimeZone();
        var endLocal = beginning.Add(period * defaultPeriodMultiplier);
        return new DateTimeOffset(endLocal, tz.GetUtcOffset(endLocal));
    }

    /// <summary>
    /// Unified method to get the base DateTime for all calculations.
    /// This ensures consistency across all methods.
    /// Priority: TargetDate -> CurrentDate (with StartDate time) -> StartDate
    /// </summary>
    private static DateTime GetBaseDateTime(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        // If TargetDate is set, use it directly
        if (schedulerInput.TargetDate.HasValue)
            return schedulerInput.TargetDate.Value.DateTime;

        // If CurrentDate is set (and not default), use CurrentDate with StartDate's time
        // Convert CurrentDate's UTC time to the target timezone first
        if (schedulerInput.CurrentDate != default) {
            var utcTime = schedulerInput.CurrentDate.UtcDateTime;
            var startTime = schedulerInput.StartDate.TimeOfDay;
            
            // Get the local date in the target timezone at the same UTC moment
            var localInTz = TimeZoneInfo.ConvertTimeFromUtc(utcTime, tz);
            return new DateTime(localInTz.Year, localInTz.Month, localInTz.Day,
                startTime.Hours, startTime.Minutes, startTime.Seconds, DateTimeKind.Unspecified);
        }

        // Fall back to StartDate
        return schedulerInput.StartDate.DateTime;
    }

    private static DateTimeOffset CreateDateTimeOffset(DateTime localWallClock, TimeZoneInfo tz) =>
        new(localWallClock, tz.GetUtcOffset(localWallClock));

    private static DateTime? GetCandidateLocalForWeekAndDay(DateTime weekStart, DayOfWeek day, TimeSpan timeOfDay) {
        var date = new DateTime(weekStart.Year, weekStart.Month, weekStart.Day, timeOfDay.Hours, timeOfDay.Minutes, timeOfDay.Seconds, DateTimeKind.Unspecified);

        for (var i = 0; i < DaysInWeek; i++) {
            if (date.DayOfWeek == day)
                return date;

            if (!TryAddDaysSafely(date, 1, out var next))
                return null;

            date = next;
        }

        return null;
    }

    private static bool TryAddDaysSafely(DateTime dt, int days, out DateTime result) {
        result = dt;
        if (days == 0) return true;
        if (dt > DateTime.MaxValue.AddDays(-days)) return false;
        result = dt.AddDays(days);
        return true;
    }

    private static void GenerateDailySlotsForDay(DateTime day, TimeSpan start, TimeSpan end, TimeSpan step, TimeZoneInfo tz, SchedulerInput schedulerInput, DateTimeOffset endDate, DateTimeOffset earliestAllowed, List<DateTimeOffset> accumulator) {
        var startLocal = new DateTime(day.Year, day.Month, day.Day,
            start.Hours, start.Minutes, start.Seconds, DateTimeKind.Unspecified);

        var endLocal = new DateTime(day.Year, day.Month, day.Day,
            end.Hours, end.Minutes, end.Seconds, DateTimeKind.Unspecified);

        var slotLocal = startLocal;
        while (slotLocal <= endLocal) {
            if (tz.IsAmbiguousTime(slotLocal)) {
                var offsets = tz.GetAmbiguousTimeOffsets(slotLocal);

                foreach (var offset in offsets.OrderByDescending(o => o)) {
                    var slotDateTimeOffset = new DateTimeOffset(slotLocal, offset);
                    if (slotDateTimeOffset >= schedulerInput.StartDate && 
                        slotDateTimeOffset <= endDate && 
                        slotDateTimeOffset > earliestAllowed && 
                        !accumulator.Contains(slotDateTimeOffset))
                        accumulator.Add(slotDateTimeOffset);
                }
            }

            else if (tz.IsInvalidTime(slotLocal)) {

            }
            else {
                var slotDateTimeOffset = CreateDateTimeOffset(slotLocal, tz);
                if (slotDateTimeOffset >= schedulerInput.StartDate && 
                    slotDateTimeOffset <= endDate && 
                    slotDateTimeOffset > earliestAllowed && 
                    !accumulator.Contains(slotDateTimeOffset))
                    accumulator.Add(slotDateTimeOffset);
            }

            slotLocal = slotLocal.Add(step);
        }
    }

    private static void GenerateWeeklySlotsForWeek(DateTime weekStart, SchedulerInput schedulerInput, TimeZoneInfo tz, DateTimeOffset nextEligible, List<DateTimeOffset> accumulator) {
        for (var index = 0; index < schedulerInput.DaysOfWeek!.Count; index++) {
            var day = schedulerInput.DaysOfWeek![index];
            var timeOfDay = schedulerInput.TargetDate?.TimeOfDay ?? schedulerInput.StartDate.TimeOfDay;
            var candidateLocal = GetCandidateLocalForWeekAndDay(weekStart, day, timeOfDay);
            if (candidateLocal == null) continue;

            var candidate = CreateDateTimeOffset(candidateLocal.Value, tz);

            if (candidate <= nextEligible) continue;
            if (!accumulator.Contains(candidate)) accumulator.Add(candidate);
        }
    }

    private static DateTimeOffset? NextWeekday(DateTimeOffset startLocal, DayOfWeek day, TimeZoneInfo tz) {
        var date = startLocal.Date;
        while (date.DayOfWeek != day)
            date = date.AddDays(1);

        var localWallClock = new DateTime(date.Year, date.Month, date.Day, startLocal.Hour, startLocal.Minute,
            startLocal.Second, DateTimeKind.Unspecified);
        return CreateDateTimeOffset(localWallClock, tz);
    }

    public static TimeZoneInfo GetTimeZone()  {
        return TimeZoneInfo.FindSystemTimeZoneById(Config.TimeZoneId);
    }

    public static DateTimeOffset GetNextExecutionDate(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        // For Weekly recurrency, calculate next eligible date based on days of week
        if (schedulerInput.Recurrency == EnumRecurrency.Weekly) {
            var baseLocal = GetBaseDateTime(schedulerInput, tz);
            var baseDtoForNext = new DateTimeOffset(baseLocal, tz.GetUtcOffset(baseLocal));
            return SelectNextEligibleDate(baseDtoForNext, schedulerInput.DaysOfWeek!, tz);
        }
        
        // For OccursOnce mode, use the specified time
        if (schedulerInput.OccursOnceChk && schedulerInput.OccursOnceAt.HasValue) {
            return schedulerInput.OccursOnceAt.Value;
        }
        
        // For TargetDate mode, use it directly
        if (schedulerInput.TargetDate.HasValue) {
            return schedulerInput.TargetDate.Value;
        }
        
        // For Daily and other recurrences, convert CurrentDate to the target timezone first
        // If we have CurrentDate, we need to respect its UTC time but express it in the target timezone
        if (schedulerInput.CurrentDate != default) {
            var utcTime = schedulerInput.CurrentDate.UtcDateTime;
            var startTime = schedulerInput.StartDate.TimeOfDay;
            
            // Get the local date in the target timezone at the same UTC moment
            var localInTz = TimeZoneInfo.ConvertTimeFromUtc(utcTime, tz);
            var baseLocal = new DateTime(localInTz.Year, localInTz.Month, localInTz.Day,
                startTime.Hours, startTime.Minutes, startTime.Seconds, DateTimeKind.Unspecified);
            
            return new DateTimeOffset(baseLocal, tz.GetUtcOffset(baseLocal));
        }
        
        // Fall back to StartDate
        var baseDateTime = GetBaseDateTime(schedulerInput, tz);
        return new DateTimeOffset(baseDateTime, tz.GetUtcOffset(baseDateTime));
    }
    
    public static List<DateTimeOffset> GetFutureDates(SchedulerInput schedulerInput) {
        if (schedulerInput.Periodicity != EnumConfiguration.Recurrent)
            return [];

        var tz = GetTimeZone();
        var futureDates = CalculateFutureDates(schedulerInput, tz);
        var next = GetNextExecutionDate(schedulerInput, tz);

        futureDates.RemoveAll(d => d.UtcDateTime == next.UtcDateTime || 
                                   (d.DateTime == next.DateTime && d.Offset == next.Offset));
        return futureDates;
    }
}