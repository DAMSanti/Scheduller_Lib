using Scheduler_Lib.Core.Model;

namespace Scheduler_Lib.Core.Services;

public class DescriptionBuilder {
    public static string BuildDescriptionForTargetDate(SchedulerInput requestedDate, TimeZoneInfo tz, DateTimeOffset nextLocal) {
        return requestedDate.Recurrency switch {
            EnumRecurrency.Weekly => BuildWeeklyDescription(requestedDate, tz, nextLocal),
            _ => BuildOnceDescription(requestedDate, tz, nextLocal)
        };
    }

    private static string BuildWeeklyDescription(SchedulerInput requestedDate, TimeZoneInfo tz, DateTimeOffset nextLocal)  {
        var daysOfWeek = string.Join(", ", requestedDate.DaysOfWeek!.Select(d => d.ToString()));
        var period = requestedDate.Period.HasValue ? $"{requestedDate.Period.Value.TotalDays} days" : "1 week";
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

    private static DateTime ConvertStartDateToZone(SchedulerInput requestedDate, TimeZoneInfo tz) {
        var startInZone = TimeZoneInfo.ConvertTime(requestedDate.StartDate, tz);
        return startInZone.Date;
    }

    private static string FormatDate(DateTimeOffset dto) => dto.Date.ToShortDateString();

    private static string FormatTime(DateTimeOffset dto) => dto.DateTime.ToShortTimeString();

    private static string TimeSpanToString(TimeSpan timeSpan) => timeSpan.ToString(@"hh\:mm");
}