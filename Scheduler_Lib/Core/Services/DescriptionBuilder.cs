using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;
using System.Text;

namespace Scheduler_Lib.Core.Services;

public class DescriptionBuilder {
    public static string BuildDescriptionForTargetDate(SchedulerInput requestedDate, TimeZoneInfo tz, DateTimeOffset nextLocal) {
        var errors = new StringBuilder();

        if (requestedDate is { Recurrency: EnumRecurrency.Weekly, Periodicity: EnumConfiguration.Once })
            errors.AppendLine(Messages.ErrorOnceWeekly);

        return requestedDate.Recurrency switch {
            EnumRecurrency.Weekly => BuildWeeklyDescription(requestedDate, tz, nextLocal),
            EnumRecurrency.Daily => BuildDailyDescription(requestedDate, tz, nextLocal),
            _ => BuildOnceDescription(requestedDate, tz, nextLocal)
        };
    }
    private static string BuildOnceDescription(SchedulerInput requestedDate, TimeZoneInfo tz, DateTimeOffset nextLocal) {
        var startDateStr = ConvertStartDateToZone(requestedDate, tz).ToShortDateString();
        return $"Occurs once: Schedule will be used on {FormatDate(nextLocal)} at {FormatTime(nextLocal)} starting on {startDateStr}";
    }

    private static string BuildWeeklyDescription(SchedulerInput requestedDate, TimeZoneInfo tz, DateTimeOffset nextLocal)  {
        var daysOfWeek = requestedDate.DaysOfWeek is { Count: > 0 }
            ? string.Join(", ", requestedDate.DaysOfWeek.Select(d => d.ToString()))
            : nextLocal.DayOfWeek.ToString(); 
        
        var period = requestedDate.DailyPeriod.HasValue ? FormatPeriod(requestedDate.DailyPeriod.Value) : "1 week";
        var startDateStr = ConvertStartDateToZone(requestedDate, tz).ToShortDateString();
        var weeklyPeriod = requestedDate.WeeklyPeriod ?? 1;

        if (requestedDate.Periodicity == EnumConfiguration.Recurrent)
                return $"Occurs every {weeklyPeriod} week(s) on {daysOfWeek} every {period} starting on {startDateStr}";
        
        return $"Occurs every {daysOfWeek}: Schedule will be used on {FormatDate(nextLocal)} at {FormatTime(nextLocal)} starting on {startDateStr}";
    }

    private static string BuildDailyDescription(SchedulerInput requestedDate, TimeZoneInfo tz, DateTimeOffset nextLocal) {
        var startDateStr = ConvertStartDateToZone(requestedDate, tz).ToShortDateString();

        if (requestedDate.Periodicity == EnumConfiguration.Recurrent) {
            var periodStr = requestedDate.DailyPeriod.HasValue ? FormatPeriod(requestedDate.DailyPeriod.Value) : "1 day";
            if (requestedDate is { DailyStartTime: not null, DailyEndTime: not null }) {
                return $"Occurs every {periodStr} between {TimeSpanToString(requestedDate.DailyStartTime!.Value)} and {TimeSpanToString(requestedDate.DailyEndTime!.Value)} " +
                       $"at starting on {startDateStr}";
            }
            return $"Occurs every {periodStr}. Schedule will be used on {FormatDate(nextLocal)} " +
                   $"at {FormatTime(nextLocal)} starting on {startDateStr}";
        }


        return BuildOnceDescription(requestedDate, tz, nextLocal);
    }

    public static DateTime ConvertStartDateToZone(SchedulerInput requestedDate, TimeZoneInfo tz) {
        var startInZone = TimeZoneInfo.ConvertTime(requestedDate.StartDate, tz);
        return startInZone.Date;
    }
    public static string TimeSpanToString(TimeSpan timeSpan) => timeSpan.ToString(@"hh\:mm");
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
}