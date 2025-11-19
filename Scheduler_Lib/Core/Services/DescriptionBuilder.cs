using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services.Localization;
using Scheduler_Lib.Resources;
using System.Globalization;
using System.Text;

namespace Scheduler_Lib.Core.Services;

internal class DescriptionBuilder {
    internal static string HandleDescriptionForCalculatedDate(
        SchedulerInput schedulerInput,
        TimeZoneInfo tz,
        DateTimeOffset nextDate) {

        var language = schedulerInput.Language ?? "es_ES";
        var culture = LocalizationService.GetCulture(language);
        var formattedDate = LocalizationService.FormatDate(nextDate, language);

        if (schedulerInput.Periodicity == EnumConfiguration.Once)
            return BuildOnceDescription(schedulerInput, tz, nextDate);

        var description = schedulerInput.Recurrency switch {
            EnumRecurrency.Weekly => BuildWeeklyDescription(schedulerInput, language, culture),
            EnumRecurrency.Daily => BuildDailyDescription(schedulerInput, language, culture),
            EnumRecurrency.Monthly => BuildMonthlyDescription(schedulerInput, language, culture),
            _ => "Unknown recurrence"
        };

        return $"{description}. {LocalizationResources.GetDescription("next.execution", language)}: {formattedDate}";
    }

    private static string BuildOnceDescription(SchedulerInput schedulerInput, TimeZoneInfo tz, DateTimeOffset nextLocal) {
        var language = schedulerInput.Language ?? "es_ES";
        var startDateStr = ConvertStartDateToZone(schedulerInput, tz).ToShortDateString();

        var template = LocalizationResources.GetDescription("occurs.once", language);
        return string.Format(template, LocalizationService.FormatDate(nextLocal, language), LocalizationService.FormatTime(nextLocal.TimeOfDay), startDateStr);
    }

    private static string BuildWeeklyDescription(SchedulerInput schedulerInput, string language, CultureInfo culture) {
        var days = string.Join(", ", schedulerInput.DaysOfWeek!
            .Select(d => LocalizationResources.GetDayName(d, language)));

        var period = schedulerInput.WeeklyPeriod ?? 1;
        var periodText = period == 1
            ? LocalizationResources.GetDescription("weekly.every.week", language)
            : string.Format(LocalizationResources.GetDescription("weekly.every.weeks", language), period);

        var timeInfo = "";
        if (schedulerInput.OccursOnceChk && schedulerInput.OccursOnceAt.HasValue) {
            timeInfo = $" {LocalizationResources.GetDescription("weekly.at", language)} {schedulerInput.OccursOnceAt.Value:hh\\:mm\\:ss}";
        }

        if (schedulerInput.OccursEveryChk && schedulerInput.DailyPeriod.HasValue) {
            var interval = FormatInterval(schedulerInput.DailyPeriod.Value, language);
            var template = LocalizationResources.GetDescription("weekly.occurs.every", language);
            timeInfo = $" {string.Format(template, interval)}";
        }

    var onDays = LocalizationResources.GetDescription("weekly.on.days", language);
    return $"{periodText} {onDays} {days}{timeInfo}";
    }
    private static string BuildDailyDescription(SchedulerInput schedulerInput, string language, CultureInfo culture) {
        if (schedulerInput.OccursOnceChk && schedulerInput.OccursOnceAt.HasValue) {
            var at = LocalizationResources.GetDescription("daily.at", language);
            return $"{LocalizationResources.GetDescription("daily.every.day", language)} {at} {schedulerInput.OccursOnceAt.Value:hh\\:mm\\:ss}";
        }

        if (schedulerInput.OccursEveryChk && schedulerInput.DailyPeriod.HasValue) {
            var interval = FormatInterval(schedulerInput.DailyPeriod.Value, language);
            var template = LocalizationResources.GetDescription("daily.occurs.every", language);
            return string.Format(template, interval);
        }

        return LocalizationResources.GetDescription("daily.every.day", language);
    }

    private static string BuildMonthlyDescription(SchedulerInput schedulerInput, string language, CultureInfo culture) {
        if (schedulerInput.MonthlyDayChk && schedulerInput.MonthlyDay.HasValue) {
            var dayPeriod = schedulerInput.MonthlyDayPeriod ?? 1;
            var dayPeriodText = dayPeriod == 1
                ? LocalizationResources.GetDescription("monthly.every.month", language)
                : string.Format(LocalizationResources.GetDescription("monthly.every.months", language), dayPeriod);

            var template = LocalizationResources.GetDescription("monthly.on.day", language);
            return $"{dayPeriodText} {string.Format(template, schedulerInput.MonthlyDay)}";
        }

        if (schedulerInput.MonthlyTheChk && schedulerInput.MonthlyFrequency.HasValue && schedulerInput.MonthlyDateType.HasValue) {
            var thePeriod = schedulerInput.MonthlyThePeriod ?? 1;
            var thePeriodText = thePeriod == 1
                ? LocalizationResources.GetDescription("monthly.every.month", language)
                : string.Format(LocalizationResources.GetDescription("monthly.every.months", language), thePeriod);

            var frequency = FormatMonthlyFrequency(schedulerInput.MonthlyFrequency.Value, language);

            var dayTypeDescription = FormatMonthlyDateType(schedulerInput.MonthlyDateType.Value, language);

            var template = LocalizationResources.GetDescription("monthly.the.day", language);

            return $"{thePeriodText} {string.Format(template, frequency, dayTypeDescription)}";
        }

        return LocalizationResources.GetDescription("monthly.every.month", language);
    }

    private static string GetMonthlyDateTypeDescription(EnumMonthlyDateType dateType, string language) {
        return dateType switch {
            EnumMonthlyDateType.Day => LocalizationResources.GetDescription("monthlytype.day", language),
            EnumMonthlyDateType.Weekday => LocalizationResources.GetDescription("monthlytype.weekday", language),
            EnumMonthlyDateType.WeekendDay => LocalizationResources.GetDescription("monthlytype.weekendday", language),

            EnumMonthlyDateType.Monday => LocalizationResources.GetDayName(DayOfWeek.Monday, language),
            EnumMonthlyDateType.Tuesday => LocalizationResources.GetDayName(DayOfWeek.Tuesday, language),
            EnumMonthlyDateType.Wednesday => LocalizationResources.GetDayName(DayOfWeek.Wednesday, language),
            EnumMonthlyDateType.Thursday => LocalizationResources.GetDayName(DayOfWeek.Thursday, language),
            EnumMonthlyDateType.Friday => LocalizationResources.GetDayName(DayOfWeek.Friday, language),
            EnumMonthlyDateType.Saturday => LocalizationResources.GetDayName(DayOfWeek.Saturday, language),
            EnumMonthlyDateType.Sunday => LocalizationResources.GetDayName(DayOfWeek.Sunday, language),

            _ => dateType.ToString()
        };
    }
    private static string FormatMonthlyFrequency(EnumMonthlyFrequency frequency, string language) {
        return LocalizationResources.GetDescription($"frequency.{frequency.ToString()!.ToLower()}", language);
    }

    private static string FormatMonthlyDateType(EnumMonthlyDateType dateType, string language) {
        // Reuse localization: use monthlytype.* keys for generic types and day names for specific weekdays
        return dateType switch {
            EnumMonthlyDateType.Day => LocalizationResources.GetDescription("monthlytype.day", language),
            EnumMonthlyDateType.Weekday => LocalizationResources.GetDescription("monthlytype.weekday", language),
            EnumMonthlyDateType.WeekendDay => LocalizationResources.GetDescription("monthlytype.weekendday", language),
            EnumMonthlyDateType.Monday => LocalizationResources.GetDayName(DayOfWeek.Monday, language),
            EnumMonthlyDateType.Tuesday => LocalizationResources.GetDayName(DayOfWeek.Tuesday, language),
            EnumMonthlyDateType.Wednesday => LocalizationResources.GetDayName(DayOfWeek.Wednesday, language),
            EnumMonthlyDateType.Thursday => LocalizationResources.GetDayName(DayOfWeek.Thursday, language),
            EnumMonthlyDateType.Friday => LocalizationResources.GetDayName(DayOfWeek.Friday, language),
            EnumMonthlyDateType.Saturday => LocalizationResources.GetDayName(DayOfWeek.Saturday, language),
            EnumMonthlyDateType.Sunday => LocalizationResources.GetDayName(DayOfWeek.Sunday, language),
            _ => dateType.ToString()
        };
    }

    private static DateTime ConvertStartDateToZone(SchedulerInput schedulerInput, TimeZoneInfo timeZone) {
        var startInZone = TimeZoneInfo.ConvertTime(schedulerInput.StartDate, timeZone);
        return startInZone.Date;
    }

    private static string TimeSpanToString12HourFormat(TimeSpan timeSpan) {
        var dateTime = DateTime.Today.Add(timeSpan);
        var hour12 = dateTime.Hour % 12;
        if (hour12 == 0) hour12 = 12;

        var period = dateTime.Hour < 12 ? "AM" : "PM";
        return $"{hour12:D2}:{dateTime.Minute:D2} {period}";
    }

    private static string FormatPeriod(TimeSpan period) {
        if (period.TotalDays >= 1)
            return FormatUnit(period.TotalDays, "day", "days");
        if (period.TotalHours >= 1)
            return FormatUnit(period.TotalHours, "hour", "hours");
        return period.TotalMinutes >= 1 ? FormatUnit(period.TotalMinutes, "minute", "minutes") : FormatUnit(period.TotalSeconds, "second", "seconds");
    }
    
    private static string FormatDate(DateTimeOffset dateTimeOffset) => dateTimeOffset.Date.ToShortDateString();
    
    private static string FormatTime(DateTimeOffset dateTimeOffset) => dateTimeOffset.DateTime.ToShortTimeString();
    
    private static string FormatUnit(double value, string singular, string plural) {
        var formatted = value.ToString("0.##", CultureInfo.InvariantCulture);
        return value > 1 ? $"{formatted} {plural}" : $"{formatted} {singular}";
    }

    private static string FormatInterval(TimeSpan interval, string language) {
        if (interval.TotalSeconds < 60 && interval.TotalSeconds >= 1) {
            var seconds = (int)interval.TotalSeconds;
            if (seconds > 0) {
                var unit = seconds == 1
                    ? LocalizationResources.GetDescription("time.second", language)
                    : LocalizationResources.GetDescription("time.seconds", language);
                return $"{seconds} {unit}";
            }
        }

        if (interval.TotalMinutes >= 1 && interval.TotalMinutes < 60) {
            var minutes = (int)interval.TotalMinutes;
            var unit = minutes == 1
                ? LocalizationResources.GetDescription("time.minute", language)
                : LocalizationResources.GetDescription("time.minutes", language);
            return $"{minutes} {unit}";
        }

        if (interval.TotalHours >= 1 && interval.TotalHours < 24) {
            var hours = (int)interval.TotalHours;
            var unit = hours == 1
                ? LocalizationResources.GetDescription("time.hour", language)
                : LocalizationResources.GetDescription("time.hours", language);
            return $"{hours} {unit}";
        }

        var days = (int)interval.TotalDays;
        if (days > 0) {
            var dayUnit = days == 1
                ? LocalizationResources.GetDescription("time.day", language)
                : LocalizationResources.GetDescription("time.days", language);
            return $"{days} {dayUnit}";
        }

        return $"{interval.TotalSeconds:F0} seconds";
    }

    private static DayOfWeek GetDayOfWeekFromType(EnumMonthlyDateType dateType) => dateType switch {
        EnumMonthlyDateType.Monday => DayOfWeek.Monday,
        EnumMonthlyDateType.Tuesday => DayOfWeek.Tuesday,
        EnumMonthlyDateType.Wednesday => DayOfWeek.Wednesday,
        EnumMonthlyDateType.Thursday => DayOfWeek.Thursday,
        EnumMonthlyDateType.Friday => DayOfWeek.Friday,
        EnumMonthlyDateType.Saturday => DayOfWeek.Saturday,
        EnumMonthlyDateType.Sunday => DayOfWeek.Sunday,
        _ => DayOfWeek.Monday
    };
}