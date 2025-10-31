using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Model.Enum;
using Scheduler_Lib.Core.Services.Description.Formatters;

namespace Scheduler_Lib.Core.Services.Description;

public class MonthlyDescriptionBuilder {
    private readonly PeriodFormatter _periodFormatter;
    private readonly TimeFormatter _timeFormatter;

    public MonthlyDescriptionBuilder(PeriodFormatter periodFormatter, TimeFormatter timeFormatter) {
        _periodFormatter = periodFormatter;
        _timeFormatter = timeFormatter;
    }

    public string Build(SchedulerInput input, TimeZoneInfo tz, DateTimeOffset nextLocal) {
        var startDateStr = _timeFormatter.FormatDate(input.StartDate, tz);

        if (input.MonthlyTheChk)
            return BuildTheDescription(input, startDateStr);

        if (input.MonthlyDayChk)
            return BuildDayDescription(input, startDateStr);

        return $"Occurs day {startDateStr} of every X month(s) starting on {startDateStr}";
    }

    private string BuildTheDescription(SchedulerInput input, string startDateStr) {
        var frequency = FormatFrequency(input.MonthlyFrequency!.Value);
        var dateType = FormatDateType(input.MonthlyDateType!.Value);
        var period = input.MonthlyThePeriod ?? 1;

        var baseDescription = $"Occurs the {frequency} {dateType} of every {period} month(s)";

        if (HasTimeWindow(input)) {
            var periodStr = _periodFormatter.Format(input.DailyPeriod!.Value);
            var startTime = _timeFormatter.FormatTime(input.DailyStartTime!.Value);
            var endTime = _timeFormatter.FormatTime(input.DailyEndTime!.Value);
            return $"{baseDescription} every {periodStr} between {startTime} and {endTime} starting on {startDateStr}";
        }

        return $"{baseDescription} starting on {startDateStr}";
    }

    private string BuildDayDescription(SchedulerInput input, string startDateStr) {
        var day = input.MonthlyDay!.Value;
        var period = input.MonthlyDayPeriod ?? 1;

        var baseDescription = $"Occurs day {day} of every {period} month(s)";

        if (HasTimeWindow(input)) {
            var periodStr = _periodFormatter.Format(input.DailyPeriod!.Value);
            var startTime = _timeFormatter.FormatTime(input.DailyStartTime!.Value);
            var endTime = _timeFormatter.FormatTime(input.DailyEndTime!.Value);
            return $"{baseDescription} every {periodStr} between {startTime} and {endTime} starting on {startDateStr}";
        }

        return $"{baseDescription} starting on {startDateStr}";
    }

    private bool HasTimeWindow(SchedulerInput input) {
        return input is { DailyStartTime: not null, DailyEndTime: not null, DailyPeriod: not null };
    }

    private string FormatFrequency(EnumMonthlyFrequency frequency) => frequency switch {
        EnumMonthlyFrequency.First => "first",
        EnumMonthlyFrequency.Second => "second",
        EnumMonthlyFrequency.Third => "third",
        EnumMonthlyFrequency.Fourth => "fourth",
        EnumMonthlyFrequency.Last => "last",
        _ => frequency.ToString().ToLower()
    };

    private string FormatDateType(EnumMonthlyDateType dateType) => dateType switch {
        EnumMonthlyDateType.Day => "day",
        EnumMonthlyDateType.Weekday => "weekday",
        EnumMonthlyDateType.WeekendDay => "weekend day",
        _ => dateType.ToString()
    };
}