using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services.Calculators.Monthly;
using Scheduler_Lib.Core.Services.Utilities;

namespace Scheduler_Lib.Core.Services.Helpers;

/// <summary>
/// Helper for selecting eligible dates in monthly recurrence with "The" mode
/// (e.g., "the first Monday", "the last Friday")
/// </summary>
internal static class MonthlyEligibleDateHelper
{
    /// <summary>
    /// Gets the eligible date for a month based on frequency and date type
    /// </summary>
    internal static DateTimeOffset? GetEligibleDate(
        DateTime month,
        EnumMonthlyFrequency frequency,
        EnumMonthlyDateType dateType,
        TimeSpan timeOfDay,
        TimeZoneInfo timeZone)
    {
        var firstDayOfMonth = new DateTime(month.Year, month.Month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        List<DateTime> eligibleDays = dateType switch
        {
            EnumMonthlyDateType.Day => MonthDayCollector.GetAllDaysInMonth(firstDayOfMonth, lastDayOfMonth),
            EnumMonthlyDateType.Weekday => MonthDayCollector.GetWeekdaysInMonth(firstDayOfMonth, lastDayOfMonth),
            EnumMonthlyDateType.WeekendDay => MonthDayCollector.GetWeekendDaysInMonth(firstDayOfMonth, lastDayOfMonth),
            EnumMonthlyDateType.Monday => MonthDayCollector.GetSpecificDayOfWeekInMonth(firstDayOfMonth, lastDayOfMonth, DayOfWeek.Monday),
            EnumMonthlyDateType.Tuesday => MonthDayCollector.GetSpecificDayOfWeekInMonth(firstDayOfMonth, lastDayOfMonth, DayOfWeek.Tuesday),
            EnumMonthlyDateType.Wednesday => MonthDayCollector.GetSpecificDayOfWeekInMonth(firstDayOfMonth, lastDayOfMonth, DayOfWeek.Wednesday),
            EnumMonthlyDateType.Thursday => MonthDayCollector.GetSpecificDayOfWeekInMonth(firstDayOfMonth, lastDayOfMonth, DayOfWeek.Thursday),
            EnumMonthlyDateType.Friday => MonthDayCollector.GetSpecificDayOfWeekInMonth(firstDayOfMonth, lastDayOfMonth, DayOfWeek.Friday),
            EnumMonthlyDateType.Saturday => MonthDayCollector.GetSpecificDayOfWeekInMonth(firstDayOfMonth, lastDayOfMonth, DayOfWeek.Saturday),
            EnumMonthlyDateType.Sunday => MonthDayCollector.GetSpecificDayOfWeekInMonth(firstDayOfMonth, lastDayOfMonth, DayOfWeek.Sunday),
            _ => new List<DateTime>()
        };

        if (eligibleDays.Count == 0)
            return null;

        var selectedDay = frequency switch
        {
            EnumMonthlyFrequency.First => eligibleDays.First(),
            EnumMonthlyFrequency.Second => eligibleDays.Count > 1 ? eligibleDays[1] : eligibleDays.Last(),
            EnumMonthlyFrequency.Third => eligibleDays.Count > 2 ? eligibleDays[2] : eligibleDays.Last(),
            EnumMonthlyFrequency.Fourth => eligibleDays.Count > 3 ? eligibleDays[3] : eligibleDays.Last(),
            EnumMonthlyFrequency.Last => eligibleDays.Last(),
            _ => eligibleDays.First()
        };

        var resultLocal = new DateTime(selectedDay.Year, selectedDay.Month, selectedDay.Day,
            timeOfDay.Hours, timeOfDay.Minutes, timeOfDay.Seconds, DateTimeKind.Unspecified);

        return TimeZoneConverter.CreateDateTimeOffset(resultLocal, timeZone);
    }
}
