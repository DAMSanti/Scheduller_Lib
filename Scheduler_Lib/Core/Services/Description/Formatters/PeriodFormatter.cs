using System.Globalization;

namespace Scheduler_Lib.Core.Services.Description.Formatters;

public class PeriodFormatter {
    public string Format(TimeSpan period) {
        if (period.TotalDays >= 1)
            return FormatUnit(period.TotalDays, "day", "days");

        if (period.TotalHours >= 1)
            return FormatUnit(period.TotalHours, "hour", "hours");

        return period.TotalMinutes >= 1
            ? FormatUnit(period.TotalMinutes, "minute", "minutes")
            : FormatUnit(period.TotalSeconds, "second", "seconds");
    }

    private string FormatUnit(double value, string singular, string plural) {
        var formatted = value.ToString("0.##", CultureInfo.InvariantCulture);
        return value > 1 ? $"{formatted} {plural}" : $"{formatted} {singular}";
    }
}