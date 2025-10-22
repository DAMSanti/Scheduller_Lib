using Scheduler_Lib.Core.Model;
using Xunit.Abstractions;

namespace Scheduler_Lib.Core.Services;

public class DescriptionBuilderTests(ITestOutputHelper output) {
    [Theory]
    [InlineData(1, "1 day")]
    [InlineData(3, "3 days")]
    [InlineData(2, "2 hours", 0, 2, 0, 0)]
    [InlineData(5, "5 minutes", 0, 0, 5, 0)]
    [InlineData(30, "30 seconds", 0, 0, 0, 30)]
    public void DescriptionBuilder_ShouldSucceed_WhenIntegerUnits(long value, string expected, int days = 0, int hours = 0, int minutes = 0, int seconds = 0) {
        TimeSpan period;
        if (days > 0) period = TimeSpan.FromDays(days);
        else if (hours > 0) period = TimeSpan.FromHours(hours);
        else if (minutes > 0) period = TimeSpan.FromMinutes(minutes);
        else if (seconds > 0) period = TimeSpan.FromSeconds(seconds);
        else period = TimeSpan.FromDays(value);

        var actual = DescriptionBuilder.FormatPeriod(period);

        output.WriteLine(actual);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenWeeklyRecurrent() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1)));
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.WeeklyPeriod = 2;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Wednesday];
        schedulerInput.DailyPeriod = TimeSpan.FromDays(7);
        schedulerInput.DailyStartTime = new TimeSpan(8, 30, 0);
        schedulerInput.DailyEndTime = new TimeSpan(17, 0, 0);


        var nextLocal = new DateTimeOffset(2025, 10, 6, 8, 30, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 6, 8, 30, 0)));

        var periodStr = DescriptionBuilder.FormatPeriod(schedulerInput.DailyPeriod.Value);
        var expected = $"Occurs every {schedulerInput.WeeklyPeriod} week(s) on {string.Join(", ", schedulerInput.DaysOfWeek!.Select(d => d.ToString()))} every {periodStr} " +
                       $"between 08:30 AM and 17:00 PM starting on {DescriptionBuilder.ConvertStartDateToZone(schedulerInput, tz).ToShortDateString()}";

        var actual = DescriptionBuilder.BuildDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenWeeklyOnce() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput();
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 2, 2, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday };

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "No error");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenExpectedString() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput {
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1))),
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Daily,
            DailyPeriod = TimeSpan.FromDays(2)

        };

        var nextLocal = new DateTimeOffset(2025, 10, 5, 10, 15, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 5, 10, 15, 0)));

        var periodStr = DescriptionBuilder.FormatPeriod(schedulerInput.DailyPeriod.Value);
        var startDateStr = DescriptionBuilder.ConvertStartDateToZone(schedulerInput, tz).ToShortDateString();
        var expected = $"Occurs every {periodStr}. Schedule will be used on {nextLocal.Date.ToShortDateString()} " +
                       $"at {nextLocal.DateTime.ToShortTimeString()} starting on {startDateStr}";

        var actual = DescriptionBuilder.BuildDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenNoDailyPeriod() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput {
            StartDate = new DateTimeOffset(2025, 3, 2, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 3, 2))),
            Periodicity = EnumConfiguration.Once,
            Recurrency = EnumRecurrency.Daily

        };

        var nextLocal = new DateTimeOffset(2025, 10, 5, 14, 45, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 5, 14, 45, 0)));

        var startDateStr = DescriptionBuilder.ConvertStartDateToZone(schedulerInput, tz).ToShortDateString();
        var expected = $"Occurs once: Schedule will be used on {nextLocal.Date.ToShortDateString()} at {nextLocal.DateTime.ToShortTimeString()} starting on {startDateStr}";

        var actual = DescriptionBuilder.BuildDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenConvertToGivenTimeZoneDate() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput {
            StartDate = new DateTimeOffset(2025, 10, 5, 23, 0, 0, TimeSpan.Zero) // UTC 2025-10-05 23:00
        };

        var converted = DescriptionBuilder.ConvertStartDateToZone(schedulerInput, tz);
        var expected = TimeZoneInfo.ConvertTime(schedulerInput.StartDate, tz).Date;

        Assert.Equal(expected, converted);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenFormatAsHHmm() {
        var ts = new TimeSpan(5, 30, 0);
        var actual = DescriptionBuilder.TimeSpanToString(ts);
        Assert.Equal("05:30 AM", actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenFractionalHours()  {
        var period = TimeSpan.FromHours(1.5);
        var actual = DescriptionBuilder.FormatPeriod(period);
        Assert.Equal("1.5 hours", actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenOneHour() {
        var period = TimeSpan.FromHours(1);
        var actual = DescriptionBuilder.FormatPeriod(period);
        Assert.Equal("1 hour", actual);
    }
}

