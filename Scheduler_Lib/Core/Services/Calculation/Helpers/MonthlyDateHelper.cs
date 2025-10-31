using Scheduler_Lib.Core.Model.Enum;

namespace Scheduler_Lib.Core.Services.Calculation.Helpers;

public class MonthlyDateHelper {
    public List<DateTime> GetEligibleDays(DateTime month, EnumMonthlyDateType dateType) {
        var firstDay = new DateTime(month.Year, month.Month, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);

        return dateType switch {
            EnumMonthlyDateType.Day => GetAllDaysInMonth(month, lastDay.Day),
            EnumMonthlyDateType.Weekday => GetWeekdaysInMonth(firstDay, lastDay),
            EnumMonthlyDateType.WeekendDay => GetWeekendDaysInMonth(firstDay, lastDay),
            _ => GetSpecificDayOfWeekInMonth(firstDay, lastDay, MapToDayOfWeek(dateType))
        };
    }

    public DateTime SelectDay(List<DateTime> eligibleDays, EnumMonthlyFrequency frequency) {
        if (eligibleDays.Count == 0)
            throw new InvalidOperationException("No eligible days found in the month");

        return frequency switch {
            EnumMonthlyFrequency.First => eligibleDays[0],
            EnumMonthlyFrequency.Second => eligibleDays.Count > 1 ? eligibleDays[1] : eligibleDays.Last(),
            EnumMonthlyFrequency.Third => eligibleDays.Count > 2 ? eligibleDays[2] : eligibleDays.Last(),
            EnumMonthlyFrequency.Fourth => eligibleDays.Count > 3 ? eligibleDays[3] : eligibleDays.Last(),
            EnumMonthlyFrequency.Last => eligibleDays.Last(),
            _ => eligibleDays[0]
        };
    }

    private List<DateTime> GetAllDaysInMonth(DateTime month, int daysInMonth) {
        return Enumerable.Range(1, daysInMonth)
            .Select(d => new DateTime(month.Year, month.Month, d))
            .ToList();
    }

    private List<DateTime> GetWeekdaysInMonth(DateTime firstDay, DateTime lastDay) {
        return Enumerable.Range(0, (lastDay - firstDay).Days + 1)
            .Select(offset => firstDay.AddDays(offset))
            .Where(d => d.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
            .ToList();
    }

    private List<DateTime> GetWeekendDaysInMonth(DateTime firstDay, DateTime lastDay) {
        return Enumerable.Range(0, (lastDay - firstDay).Days + 1)
            .Select(offset => firstDay.AddDays(offset))
            .Where(d => d.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            .ToList();
    }

    private List<DateTime> GetSpecificDayOfWeekInMonth(DateTime firstDay, DateTime lastDay, DayOfWeek targetDay) {
        return Enumerable.Range(0, (lastDay - firstDay).Days + 1)
            .Select(offset => firstDay.AddDays(offset))
            .Where(d => d.DayOfWeek == targetDay)
            .ToList();
    }

    private DayOfWeek MapToDayOfWeek(EnumMonthlyDateType dateType) {
        return (DayOfWeek)(int)dateType;
    }
}