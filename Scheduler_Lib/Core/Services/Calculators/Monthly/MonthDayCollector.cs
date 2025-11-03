namespace Scheduler_Lib.Core.Services.Calculators.Monthly;

public static class MonthDayCollector {
    public static List<DateTime> GetDaysInMonth(DateTime firstDay, DateTime lastDay, Func<DateTime, bool> predicate) {
        var days = new List<DateTime>();
        for (var day = firstDay; day <= lastDay; day = day.AddDays(1)) {
            if (predicate(day)) {
                days.Add(day);
            }
        }
        return days;
    }

    public static List<DateTime> GetWeekdaysInMonth(DateTime firstDay, DateTime lastDay) {
        return GetDaysInMonth(firstDay, lastDay, 
            day => day.DayOfWeek != DayOfWeek.Saturday && day.DayOfWeek != DayOfWeek.Sunday);
    }

    public static List<DateTime> GetWeekendDaysInMonth(DateTime firstDay, DateTime lastDay) {
        return GetDaysInMonth(firstDay, lastDay, 
            day => day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday);
    }

    public static List<DateTime> GetSpecificDayOfWeekInMonth(DateTime firstDay, DateTime lastDay, DayOfWeek targetDayOfWeek) {
        return GetDaysInMonth(firstDay, lastDay, 
            day => day.DayOfWeek == targetDayOfWeek);
    }

    public static List<DateTime> GetAllDaysInMonth(DateTime firstDay, DateTime lastDay) {
        return GetDaysInMonth(firstDay, lastDay, _ => true);
    }
}
