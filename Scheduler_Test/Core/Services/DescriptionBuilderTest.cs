using Scheduler_Lib.Core.Model;
using Xunit.Abstractions;

namespace Scheduler_Lib.Core.Services;

public class DescriptionBuilderTests(ITestOutputHelper output) {

    [Fact]
    public void BuildWeeklyDescription_Recurrent_ShouldReturnExpectedString() {
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");
        var requestedDate = new SchedulerInput {
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1))),
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            WeeklyPeriod = 2,
            DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday },
            Period = TimeSpan.FromDays(7),
            DailyStartTime = new TimeSpan(8, 30, 0),
            DailyEndTime = new TimeSpan(17, 0, 0)
        };

        var nextLocal = new DateTimeOffset(2025, 10, 6, 8, 30, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 6, 8, 30, 0)));

        var periodStr = DescriptionBuilder.FormatPeriod(requestedDate.Period.Value);
        var expected = $"Occurs every {requestedDate.WeeklyPeriod} week(s) on {string.Join(", ", requestedDate.DaysOfWeek!.Select(d => d.ToString()))} every {periodStr} " +
                       $"between {DescriptionBuilder.TimeSpanToString(requestedDate.DailyStartTime!.Value)} and {DescriptionBuilder.TimeSpanToString(requestedDate.DailyEndTime!.Value)} " +
                       $"starting on {DescriptionBuilder.ConvertStartDateToZone(requestedDate, tz).ToShortDateString()}";

        var actual = DescriptionBuilder.BuildDescriptionForTargetDate(requestedDate, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void BuildWeeklyDescription_Once_ShouldReturnExpectedString() {
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");
        var requestedDate = new SchedulerInput {
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1))),
            Periodicity = EnumConfiguration.Once,
            Recurrency = EnumRecurrency.Weekly,
            DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday }
        };

        var nextLocal = new DateTimeOffset(2025, 10, 8, 9, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 8, 9, 0, 0)));

        var startDateStr = DescriptionBuilder.ConvertStartDateToZone(requestedDate, tz).ToShortDateString();
        var expected = $"Occurs every {string.Join(", ", requestedDate.DaysOfWeek!.Select(d => d.ToString()))}: Schedule will be used on {nextLocal.Date.ToShortDateString()} at {nextLocal.DateTime.ToShortTimeString()} starting on {startDateStr}";

        var actual = DescriptionBuilder.BuildDescriptionForTargetDate(requestedDate, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void BuildDailyDescription_Recurrent_ShouldReturnExpectedString() {
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");
        var requestedDate = new SchedulerInput {
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1))),
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Daily,
            Period = TimeSpan.FromDays(2)

        };

        var nextLocal = new DateTimeOffset(2025, 10, 5, 10, 15, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 5, 10, 15, 0)));

        var startDateStr = DescriptionBuilder.ConvertStartDateToZone(requestedDate, tz).ToShortDateString();
        var expected = $"Occurs every {requestedDate.Period!.Value} days. Schedule will be used on {nextLocal.Date.ToShortDateString()} " +
                       $"at {nextLocal.DateTime.ToShortTimeString()} starting on {startDateStr}";

        var actual = DescriptionBuilder.BuildDescriptionForTargetDate(requestedDate, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void BuildOnceDescription_ShouldReturnExpectedString() {
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");
        var requestedDate = new SchedulerInput {
            StartDate = new DateTimeOffset(2025, 3, 2, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 3, 2))),
            Periodicity = EnumConfiguration.Once,
            Recurrency = EnumRecurrency.Daily

        };

        var nextLocal = new DateTimeOffset(2025, 10, 5, 14, 45, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 5, 14, 45, 0)));

        var startDateStr = DescriptionBuilder.ConvertStartDateToZone(requestedDate, tz).ToShortDateString();
        var expected = $"Occurs once: Schedule will be used on {nextLocal.Date.ToShortDateString()} at {nextLocal.DateTime.ToShortTimeString()} starting on {startDateStr}";

        var actual = DescriptionBuilder.BuildDescriptionForTargetDate(requestedDate, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ConvertStartDateToZone_ShouldConvertToGivenTimeZoneDate() {
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");
        var requestedDate = new SchedulerInput {
            StartDate = new DateTimeOffset(2025, 10, 5, 23, 0, 0, TimeSpan.Zero) // UTC 2025-10-05 23:00
        };

        var converted = DescriptionBuilder.ConvertStartDateToZone(requestedDate, tz);
        var expected = TimeZoneInfo.ConvertTime(requestedDate.StartDate, tz).Date;

        Assert.Equal(expected, converted);
    }

    [Fact]
    public void TimeSpanToString_ShouldFormatAsHHmm() {
        var ts = new TimeSpan(5, 30, 0);
        var actual = DescriptionBuilder.TimeSpanToString(ts);
        Assert.Equal("05:30", actual);
    }

    [Theory]
    [InlineData(1, "1 day")]
    [InlineData(3, "3 days")]
    [InlineData(2 , "2 hours", 0, 2, 0, 0)]
    [InlineData(5 , "5 minutes", 0, 0, 5, 0)]
    [InlineData(30 , "30 seconds", 0, 0, 0, 30)]
    public void FormatPeriod_IntegerUnits_ShouldReturnExpected(long value, string expected, int days = 0, int hours = 0, int minutes = 0, int seconds = 0) {
        TimeSpan period;
        if (days > 0) period = TimeSpan.FromDays(days);
        else if (hours > 0) period = TimeSpan.FromHours(hours);
        else if (minutes > 0) period = TimeSpan.FromMinutes(minutes);
        else if (seconds > 0) period = TimeSpan.FromSeconds(seconds);
        else period = TimeSpan.FromDays(value);

        var actual = DescriptionBuilder.FormatPeriod(period);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void FormatPeriod_FractionalHours_ShouldReturnDecimalHoursString()  {
        var period = TimeSpan.FromHours(1.5);
        var actual = DescriptionBuilder.FormatPeriod(period);
        Assert.Equal("1.5 hours", actual);
    }

    [Fact]
    public void FormatPeriod_OneHour_ShouldReturnSingularHour() {
        var period = TimeSpan.FromHours(1);
        var actual = DescriptionBuilder.FormatPeriod(period);
        Assert.Equal("1 hour", actual);
    }

    [Fact]
    public void FormatPeriod_OnePointFiveHours_ShouldReturnFormattedPlural() {
        var period = TimeSpan.FromHours(1.5);
        var actual = DescriptionBuilder.FormatPeriod(period);
        Assert.Equal("1.5 hours", actual);
    }
}

