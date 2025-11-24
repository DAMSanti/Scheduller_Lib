using Scheduler_Lib.Core.Interfaces;
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Builders;

/// <summary>
/// Builder for creating scheduler descriptions (Builder Pattern)
/// </summary>
public class SchedulerDescriptionBuilder : IDescriptionBuilder
{
    private ISchedulerConfiguration? _configuration;
    private TimeZoneInfo? _timeZone;
    private DateTimeOffset _nextDate;
    private string _language = "en_US";

    public IDescriptionBuilder SetConfiguration(ISchedulerConfiguration configuration)
    {
        _configuration = configuration;
        return this;
    }

    public IDescriptionBuilder SetTimeZone(TimeZoneInfo timeZone)
    {
        _timeZone = timeZone;
        return this;
    }

    public IDescriptionBuilder SetNextDate(DateTimeOffset nextDate)
    {
        _nextDate = nextDate;
        return this;
    }

    public IDescriptionBuilder SetLanguage(string language)
    {
        _language = language ?? "en_US";
        return this;
    }

    public string Build()
    {
        if (_configuration == null)
            throw new InvalidOperationException("Configuration must be set before building description");

        var formattedDate = LocalizationService.FormatDate(_nextDate, _language);

        if (_configuration.Periodicity == EnumConfiguration.Once)
            return BuildOnceDescription();

        var description = _configuration.Recurrency switch
        {
            EnumRecurrency.Weekly => BuildWeeklyDescription(),
            EnumRecurrency.Daily => BuildDailyDescription(),
            EnumRecurrency.Monthly => BuildMonthlyDescription(),
            _ => "Unknown recurrence"
        };

        return $"{description}. {LocalizationResources.GetDescription("next.execution", _language)}: {formattedDate}";
    }

    public void Reset()
    {
        _configuration = null;
        _timeZone = null;
        _nextDate = default;
        _language = "en_US";
    }

    private string BuildOnceDescription()
    {
        if (_timeZone == null || _configuration == null)
            throw new InvalidOperationException("TimeZone and Configuration must be set");

        var startDateStr = ConvertStartDateToZone(_configuration, _timeZone).ToShortDateString();
        var template = LocalizationResources.GetDescription("occurs.once", _language);
        
        return string.Format(template, 
            LocalizationService.FormatDate(_nextDate, _language), 
            LocalizationService.FormatTime(_nextDate.TimeOfDay), 
            startDateStr);
    }

    private string BuildWeeklyDescription()
    {
        if (_configuration == null)
            throw new InvalidOperationException("Configuration must be set");

        var days = string.Join(", ", _configuration.DaysOfWeek!
            .Select(d => LocalizationResources.GetDayName(d, _language)));

        var period = _configuration.WeeklyPeriod ?? 1;
        var periodText = period == 1
            ? LocalizationResources.GetDescription("weekly.every.week", _language)
            : string.Format(LocalizationResources.GetDescription("weekly.every.weeks", _language), period);

        var timeInfo = "";
        if (_configuration.OccursOnceChk && _configuration.OccursOnceAt.HasValue)
        {
            timeInfo = $" {LocalizationResources.GetDescription("weekly.at", _language)} {_configuration.OccursOnceAt.Value:hh\\:mm\\:ss}";
        }

        if (_configuration.OccursEveryChk && _configuration.DailyPeriod.HasValue)
        {
            var interval = FormatInterval(_configuration.DailyPeriod.Value, _language);
            var template = LocalizationResources.GetDescription("weekly.occurs.every", _language);
            timeInfo = $" {string.Format(template, interval)}";
        }

        var onDays = LocalizationResources.GetDescription("weekly.on.days", _language);
        return $"{periodText} {onDays} {days}{timeInfo}";
    }

    private string BuildDailyDescription()
    {
        if (_configuration == null)
            throw new InvalidOperationException("Configuration must be set");

        if (_configuration.OccursOnceChk && _configuration.OccursOnceAt.HasValue)
        {
            var at = LocalizationResources.GetDescription("daily.at", _language);
            return $"{LocalizationResources.GetDescription("daily.every.day", _language)} {at} {_configuration.OccursOnceAt.Value:hh\\:mm\\:ss}";
        }

        if (_configuration.OccursEveryChk && _configuration.DailyPeriod.HasValue)
        {
            var interval = FormatInterval(_configuration.DailyPeriod.Value, _language);
            var template = LocalizationResources.GetDescription("daily.occurs.every", _language);
            return string.Format(template, interval);
        }

        return LocalizationResources.GetDescription("daily.every.day", _language);
    }

    private string BuildMonthlyDescription()
    {
        if (_configuration == null)
            throw new InvalidOperationException("Configuration must be set");

        if (_configuration.MonthlyDayChk && _configuration.MonthlyDay.HasValue)
        {
            var dayPeriod = _configuration.MonthlyDayPeriod ?? 1;
            var dayPeriodText = dayPeriod == 1
                ? LocalizationResources.GetDescription("monthly.every.month", _language)
                : string.Format(LocalizationResources.GetDescription("monthly.every.months", _language), dayPeriod);

            var template = LocalizationResources.GetDescription("monthly.on.day", _language);
            return $"{dayPeriodText} {string.Format(template, _configuration.MonthlyDay)}";
        }

        if (_configuration.MonthlyTheChk && _configuration.MonthlyFrequency.HasValue && _configuration.MonthlyDateType.HasValue)
        {
            var thePeriod = _configuration.MonthlyThePeriod ?? 1;
            var thePeriodText = thePeriod == 1
                ? LocalizationResources.GetDescription("monthly.every.month", _language)
                : string.Format(LocalizationResources.GetDescription("monthly.every.months", _language), thePeriod);

            var frequency = FormatMonthlyFrequency(_configuration.MonthlyFrequency.Value, _language);
            var dayTypeDescription = FormatMonthlyDateType(_configuration.MonthlyDateType.Value, _language);

            var template = LocalizationResources.GetDescription("monthly.the.day", _language);
            return $"{thePeriodText} {string.Format(template, frequency, dayTypeDescription)}";
        }

        return LocalizationResources.GetDescription("monthly.every.month", _language);
    }

    private static string FormatMonthlyFrequency(EnumMonthlyFrequency frequency, string language)
    {
        return LocalizationResources.GetDescription($"frequency.{frequency.ToString()!.ToLower()}", language);
    }

    private static string FormatMonthlyDateType(EnumMonthlyDateType dateType, string language)
    {
        return dateType switch
        {
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

    private static DateTime ConvertStartDateToZone(ISchedulerConfiguration configuration, TimeZoneInfo timeZone)
    {
        var startInZone = TimeZoneInfo.ConvertTime(configuration.StartDate, timeZone);
        return startInZone.Date;
    }

    private static string FormatInterval(TimeSpan interval, string language)
    {
        if (interval.TotalSeconds < 60 && interval.TotalSeconds >= 1)
        {
            var seconds = (int)interval.TotalSeconds;
            if (seconds > 0)
            {
                var unit = seconds == 1
                    ? LocalizationResources.GetDescription("time.second", language)
                    : LocalizationResources.GetDescription("time.seconds", language);
                return $"{seconds} {unit}";
            }
        }

        if (interval.TotalMinutes >= 1 && interval.TotalMinutes < 60)
        {
            var minutes = (int)interval.TotalMinutes;
            var unit = minutes == 1
                ? LocalizationResources.GetDescription("time.minute", language)
                : LocalizationResources.GetDescription("time.minutes", language);
            return $"{minutes} {unit}";
        }

        if (interval.TotalHours >= 1 && interval.TotalHours < 24)
        {
            var hours = (int)interval.TotalHours;
            var unit = hours == 1
                ? LocalizationResources.GetDescription("time.hour", language)
                : LocalizationResources.GetDescription("time.hours", language);
            return $"{hours} {unit}";
        }

        var days = (int)interval.TotalDays;
        if (days > 0)
        {
            var dayUnit = days == 1
                ? LocalizationResources.GetDescription("time.day", language)
                : LocalizationResources.GetDescription("time.days", language);
            return $"{days} {dayUnit}";
        }

        return $"{interval.TotalSeconds:F0} seconds";
    }
}
