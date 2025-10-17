using Scheduler_Lib.Core.Model;

namespace Scheduler_Lib.Core.Services;

public class DescriptionBuilder {
    public static string BuildDescriptionForTargetDate(SchedulerInput requestedDate, TimeZoneInfo tz, DateTimeOffset nextLocal) {
        return requestedDate.Recurrency switch {
            EnumRecurrency.Weekly => BuildWeeklyDescription(requestedDate, tz, nextLocal),
            EnumRecurrency.Daily => BuildDailyDescription(requestedDate, tz, nextLocal),
            _ => BuildOnceDescription(requestedDate, tz, nextLocal)
        };
    }

    private static string BuildWeeklyDescription(SchedulerInput requestedDate, TimeZoneInfo tz, DateTimeOffset nextLocal)  {
        var daysOfWeek = string.Join(", ", requestedDate.DaysOfWeek!.Select(d => d.ToString()));
        var period = requestedDate.Period.HasValue ? FormatPeriod(requestedDate.Period.Value) : "1 week";
        var startDateStr = ConvertStartDateToZone(requestedDate, tz).ToShortDateString();

        if (requestedDate.Periodicity == EnumConfiguration.Recurrent)  {
            return $"Occurs every {requestedDate.WeeklyPeriod} week(s) on {daysOfWeek} every {period} " +
                   $"between {TimeSpanToString(requestedDate.DailyStartTime!.Value)} and {TimeSpanToString(requestedDate.DailyEndTime!.Value)} " +
                   $"starting on {startDateStr}";
        }

        return $"Occurs every {daysOfWeek}: Schedule will be used on {FormatDate(nextLocal)} at {FormatTime(nextLocal)} starting on {startDateStr}";
    }

    private static string BuildOnceDescription(SchedulerInput requestedDate, TimeZoneInfo tz, DateTimeOffset nextLocal) {
        var startDateStr = ConvertStartDateToZone(requestedDate, tz).ToShortDateString();
        return $"Occurs once: Schedule will be used on {FormatDate(nextLocal)} at {FormatTime(nextLocal)} starting on {startDateStr}";
    }

    private static string BuildDailyDescription(SchedulerInput requestedDate, TimeZoneInfo tz, DateTimeOffset nextLocal) {
        var startDateStr = ConvertStartDateToZone(requestedDate, tz).ToShortDateString();

        if (requestedDate.Periodicity == EnumConfiguration.Recurrent) {
            return $"Occurs every {requestedDate.Period!.Value} days. Schedule will be used on {FormatDate(nextLocal)} " +
                   $"at {FormatTime(nextLocal)} starting on {startDateStr}";
        }

        return BuildOnceDescription(requestedDate, tz, nextLocal);
    }

    public static DateTime ConvertStartDateToZone(SchedulerInput requestedDate, TimeZoneInfo tz) {
        var startInZone = TimeZoneInfo.ConvertTime(requestedDate.StartDate, tz);
        return startInZone.Date;
    }

    public static string FormatPeriod(TimeSpan period) {
        if (period.TotalDays >= 1) {
            return FormatUnit(period.TotalDays, "day", "days");
        }
        if (period.TotalHours >= 1) {
            return FormatUnit(period.TotalHours, "hour", "hours");
        }
        if (period.TotalMinutes >= 1) {
            return FormatUnit(period.TotalMinutes, "minute", "minutes");
        }

        return FormatUnit(period.TotalSeconds, "second", "seconds");
    }

    private static string FormatDate(DateTimeOffset dto) => dto.Date.ToShortDateString();

    private static string FormatTime(DateTimeOffset dto) => dto.DateTime.ToShortTimeString();

    private static string FormatUnit(double value, string singular, string plural) {
        var rounded = Math.Round(value);
        if (Math.Abs(value - rounded) < 1e-9) {
            var vInt = (long)rounded;
            return vInt == 1 ? $"1 {singular}" : $"{vInt} {plural}";
        }

        var formatted = value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        return Math.Abs(value - 1.0) < 1e-9 ? $"{formatted} {singular}" : $"{formatted} {plural}";
    }

    public static string TimeSpanToString(TimeSpan timeSpan) => timeSpan.ToString(@"hh\:mm");

}