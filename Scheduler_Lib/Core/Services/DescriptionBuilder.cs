using System.Globalization;
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;
using System.Text;

namespace Scheduler_Lib.Core.Services;

public class DescriptionBuilder {
    public static string HandleDescriptionForCalculatedDate(SchedulerInput schedulerInput, TimeZoneInfo tz, DateTimeOffset nextLocal) {
        var errors = new StringBuilder();

        if (schedulerInput is { Recurrency: EnumRecurrency.Weekly, Periodicity: EnumConfiguration.Once })
            errors.AppendLine(Messages.ErrorOnceWeekly);

        return schedulerInput.Recurrency switch {
            EnumRecurrency.Weekly => BuildWeeklyDescription(schedulerInput, tz, nextLocal),
            EnumRecurrency.Daily => BuildDailyDescription(schedulerInput, tz, nextLocal),
            EnumRecurrency.Monthly => BuildMonthlyDescription(schedulerInput, tz, nextLocal),
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

        if ((schedulerInput.Periodicity == EnumConfiguration.Recurrent) && (schedulerInput.DailyEndTime.HasValue || schedulerInput.DailyStartTime.HasValue)) {
                return $"Occurs every {weeklyPeriod} week(s) on {daysOfWeek} every {period} between {TimeSpanToString12HourFormat(schedulerInput.DailyStartTime!.Value)} and {TimeSpanToString12HourFormat(schedulerInput.DailyEndTime!.Value)} " +
                       $"starting on {startDateStr}";
        }
        return $"Occurs every {weeklyPeriod} week(s) on {daysOfWeek} every {period} starting on {startDateStr}";
    }

    private static string BuildDailyDescription(SchedulerInput schedulerInput, TimeZoneInfo tz, DateTimeOffset nextLocal) {
        var startDateStr = ConvertStartDateToZone(schedulerInput, tz).ToShortDateString();

        if (schedulerInput.Periodicity != EnumConfiguration.Recurrent)
            return BuildOnceDescription(schedulerInput, tz, nextLocal);
        var periodStr = schedulerInput.DailyPeriod.HasValue ? FormatPeriod(schedulerInput.DailyPeriod.Value) : "1 day";
        if (schedulerInput is { DailyStartTime: not null, DailyEndTime: not null }) {
            return $"Occurs every {periodStr} between {TimeSpanToString12HourFormat(schedulerInput.DailyStartTime!.Value)} and {TimeSpanToString12HourFormat(schedulerInput.DailyEndTime!.Value)} " +
                   $"at starting on {startDateStr}";
        }
        return $"Occurs every {periodStr}. Schedule will be used on {FormatDate(nextLocal)} " +
               $"at {FormatTime(nextLocal)} starting on {startDateStr}";
    }

    private static string BuildMonthlyDescription(SchedulerInput schedulerInput, TimeZoneInfo tz, DateTimeOffset nextLocal) {
        var startDateStr = ConvertStartDateToZone(schedulerInput, tz).ToShortDateString();

        if (schedulerInput.MonthlyTheChk) {
            var frequency = FormatMonthlyFrequency(schedulerInput.MonthlyFrequency!.Value);
            var dateType = FormatMonthlyDateType(schedulerInput.MonthlyDateType!.Value);
            var period = schedulerInput.MonthlyThePeriod ?? 1;

            if (schedulerInput is { DailyStartTime: not null, DailyEndTime: not null, DailyPeriod: not null }) {
                var periodStr = FormatPeriod(schedulerInput.DailyPeriod.Value);
                return $"Occurs the {frequency} {dateType} of every {period} month(s) every {periodStr} between {TimeSpanToString12HourFormat(schedulerInput.DailyStartTime.Value)} and {TimeSpanToString12HourFormat(schedulerInput.DailyEndTime.Value)} starting on {startDateStr}";
            }

            return $"Occurs the {frequency} {dateType} of every {period} month(s) starting on {startDateStr}";
        }

        if (schedulerInput.MonthlyDayChk) {
            var day = schedulerInput.MonthlyDay!.Value;
            var period = schedulerInput.MonthlyDayPeriod ?? 1;

            if (schedulerInput is { DailyStartTime: not null, DailyEndTime: not null, DailyPeriod: not null }) {
                var periodStr = FormatPeriod(schedulerInput.DailyPeriod.Value);
                return $"Occurs day {day} of every {period} month(s) every {periodStr} between {TimeSpanToString12HourFormat(schedulerInput.DailyStartTime.Value)} and {TimeSpanToString12HourFormat(schedulerInput.DailyEndTime.Value)} starting on {startDateStr}";
            }

            return $"Occurs day {day} of every {period} month(s) starting on {startDateStr}";
        }
        return $"Occurs day {startDateStr} of every X month(s) starting on {startDateStr}";
    }

    private static string FormatMonthlyFrequency(EnumMonthlyFrequency frequency)
    {
        return frequency switch
        {
            EnumMonthlyFrequency.First => "first",
            EnumMonthlyFrequency.Second => "second",
            EnumMonthlyFrequency.Third => "third",
            EnumMonthlyFrequency.Fourth => "fourth",
            EnumMonthlyFrequency.Last => "last",
            _ => frequency.ToString().ToLower()
        };
    }

    private static string FormatMonthlyDateType(EnumMonthlyDateType dateType)
    {
        return dateType switch
        {
            EnumMonthlyDateType.Day => "day",
            EnumMonthlyDateType.Weekday => "weekday",
            EnumMonthlyDateType.WeekendDay => "weekend day",
            _ => dateType.ToString()
        };
    }
















    public static DateTime ConvertStartDateToZone(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        var startInZone = TimeZoneInfo.ConvertTime(schedulerInput.StartDate, tz);
        return startInZone.Date;
    }

    public static string TimeSpanToString12HourFormat(TimeSpan timeSpan) {
        var dateTime = DateTime.Today.Add(timeSpan);
        var hour12 = dateTime.Hour % 12;
        if (hour12 == 0) hour12 = 12;

        var period = dateTime.Hour < 12 ? "AM" : "PM";
        return $"{hour12:D2}:{dateTime.Minute:D2} {period}";
    }

    public static string FormatPeriod(TimeSpan period) {
        if (period.TotalDays >= 1)
            return FormatUnit(period.TotalDays, "day", "days");
        if (period.TotalHours >= 1)
            return FormatUnit(period.TotalHours, "hour", "hours");
        return period.TotalMinutes >= 1 ? FormatUnit(period.TotalMinutes, "minute", "minutes") : FormatUnit(period.TotalSeconds, "second", "seconds");
    }
    private static string FormatDate(DateTimeOffset dto) => dto.Date.ToShortDateString();
    private static string FormatTime(DateTimeOffset dto) => dto.DateTime.ToShortTimeString();
    private static string FormatUnit(double value, string singular, string plural) {
        var formatted = value.ToString("0.##", CultureInfo.InvariantCulture);
        return value > 1 ? $"{formatted} {plural}" : $"{formatted} {singular}";
    }
}