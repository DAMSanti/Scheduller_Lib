using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;
using System.Text;

namespace Scheduler_Lib.Core.Services;

public class DescriptionBuilder {
    public static string BuildDescriptionForCalculatedDate(SchedulerInput schedulerInput, TimeZoneInfo tz, DateTimeOffset nextLocal) {
        var errors = new StringBuilder();

        if (schedulerInput is { Recurrency: EnumRecurrency.Weekly, Periodicity: EnumConfiguration.Once })
            errors.AppendLine(Messages.ErrorOnceWeekly);

        return schedulerInput.Recurrency switch {
            EnumRecurrency.Weekly => BuildWeeklyDescription(schedulerInput, tz, nextLocal),
            EnumRecurrency.Daily => BuildDailyDescription(schedulerInput, tz, nextLocal),
            _ => BuildOnceDescription(schedulerInput, tz, nextLocal)
        };
    }

    private static string BuildOnceDescription(SchedulerInput schedulerInput, TimeZoneInfo tz, DateTimeOffset nextLocal) {
        var startDateStr = ConvertStartDateToZone(schedulerInput, tz).ToShortDateString();
        return $"Occurs once: Schedule will be used on {FormatDate(nextLocal)} at {FormatTime(nextLocal)} starting on {startDateStr}";
    }

    private static string BuildWeeklyDescription(SchedulerInput schedulerInput, TimeZoneInfo tz, DateTimeOffset nextLocal)  {
        var daysOfWeek = schedulerInput.DaysOfWeek is { Count: > 0 }
            ? string.Join(", ", schedulerInput.DaysOfWeek.Select(d => d.ToString()))
            : nextLocal.DayOfWeek.ToString(); 
        
        var period = schedulerInput.DailyPeriod.HasValue ? FormatPeriod(schedulerInput.DailyPeriod.Value) : "1 week";
        var startDateStr = ConvertStartDateToZone(schedulerInput, tz).ToShortDateString();
        var weeklyPeriod = schedulerInput.WeeklyPeriod ?? 1;

        if (schedulerInput.Periodicity == EnumConfiguration.Recurrent)
            if (schedulerInput.DailyEndTime.HasValue || schedulerInput.DailyStartTime.HasValue)
                return $"Occurs every {weeklyPeriod} week(s) on {daysOfWeek} every {period} between {TimeSpanToString(schedulerInput.DailyStartTime!.Value)} and {TimeSpanToString(schedulerInput.DailyEndTime!.Value)} " +
                       $"starting on {startDateStr}";

        return $"Occurs every {weeklyPeriod} week(s) on {daysOfWeek} every {period} starting on {startDateStr}";
    }

    private static string BuildDailyDescription(SchedulerInput schedulerInput, TimeZoneInfo tz, DateTimeOffset nextLocal) {
        var startDateStr = ConvertStartDateToZone(schedulerInput, tz).ToShortDateString();

        if (schedulerInput.Periodicity != EnumConfiguration.Recurrent)
            return BuildOnceDescription(schedulerInput, tz, nextLocal);
        var periodStr = schedulerInput.DailyPeriod.HasValue ? FormatPeriod(schedulerInput.DailyPeriod.Value) : "1 day";
        if (schedulerInput is { DailyStartTime: not null, DailyEndTime: not null }) {
            return $"Occurs every {periodStr} between {TimeSpanToString(schedulerInput.DailyStartTime!.Value)} and {TimeSpanToString(schedulerInput.DailyEndTime!.Value)} " +
                   $"at starting on {startDateStr}";
        }
        return $"Occurs every {periodStr}. Schedule will be used on {FormatDate(nextLocal)} " +
               $"at {FormatTime(nextLocal)} starting on {startDateStr}";

    }

    public static DateTime ConvertStartDateToZone(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        var startInZone = TimeZoneInfo.ConvertTime(schedulerInput.StartDate, tz);
        return startInZone.Date;
    }

    public static string TimeSpanToString(TimeSpan timeSpan) {
        var dateTime = DateTime.Today.Add(timeSpan);
        var hour12 = dateTime.Hour % 12;
        if (hour12 == 0) hour12 = 12;

        var period = dateTime.Hour < 12 ? "AM" : "PM";
        return $"{hour12:D2}:{dateTime.Minute:D2} {period}";
    }

    public static string FormatPeriod(TimeSpan period) {
        if (period.TotalDays >= 1) {
            return FormatUnit(period.TotalDays, "day", "days");
        }
        if (period.TotalHours >= 1) {
            return FormatUnit(period.TotalHours, "hour", "hours");
        }
        return period.TotalMinutes >= 1 ? FormatUnit(period.TotalMinutes, "minute", "minutes") : FormatUnit(period.TotalSeconds, "second", "seconds");
    }
    private static string FormatDate(DateTimeOffset dto) => dto.Date.ToShortDateString();
    private static string FormatTime(DateTimeOffset dto) => dto.DateTime.ToShortTimeString();
    private static string FormatUnit(double value, string singular, string plural) {
        var formatted = value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        return Math.Abs(value - 1.0) < 1e-9 ? $"{formatted} {singular}" : $"{formatted} {plural}";
    }
}