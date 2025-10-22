using Scheduler_Lib.Core.Model;
using Xunit.Abstractions;
// ReSharper disable UseObjectOrCollectionInitializer

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
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Wednesday];

        var result = SchedulerService.CalculateDate(schedulerInput);

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
            StartDate = new DateTimeOffset(2025, 10, 5, 23, 0, 0, TimeSpan.Zero)
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

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenWeeklyRecurrentWithoutTimeWindow() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput {
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1))),
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            WeeklyPeriod = 2,
            DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday },
            DailyPeriod = TimeSpan.FromDays(7)
        };

        var nextLocal = new DateTimeOffset(2025, 10, 6, 8, 30, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 6, 8, 30, 0)));

        var periodStr = DescriptionBuilder.FormatPeriod(schedulerInput.DailyPeriod.Value);
        var expected = $"Occurs every {schedulerInput.WeeklyPeriod} week(s) on {string.Join(", ", schedulerInput.DaysOfWeek!.Select(d => d.ToString()))} every {periodStr} " +
                       $"starting on {DescriptionBuilder.ConvertStartDateToZone(schedulerInput, tz).ToShortDateString()}";

        var actual = DescriptionBuilder.BuildDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenDailyRecurrentWithTimeWindow() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput {
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1)))
            ,
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Daily,
            DailyPeriod = TimeSpan.FromDays(1),
            DailyStartTime = new TimeSpan(8, 0, 0),
            DailyEndTime = new TimeSpan(17, 0, 0)
        };

        var nextLocal = new DateTimeOffset(2025, 10, 5, 9, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 5, 9, 0, 0)));

        var periodStr = DescriptionBuilder.FormatPeriod(schedulerInput.DailyPeriod.Value);
        var startDateStr = DescriptionBuilder.ConvertStartDateToZone(schedulerInput, tz).ToShortDateString();
        var expected = $"Occurs every {periodStr} between 08:00 AM and 17:00 PM " +
                       $"at starting on {startDateStr}";

        var actual = DescriptionBuilder.BuildDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenPmTimeFormat() {
        var ts = new TimeSpan(14, 30, 0);
        var actual = DescriptionBuilder.TimeSpanToString(ts);
        Assert.Equal("14:30 PM", actual);
    }

    [Theory]
    [InlineData(1.25, "1.25 days")]
    [InlineData(0.5, "12 hours")]
    [InlineData(0.75, "18 hours")]
    public void DescriptionBuilder_ShouldSucceed_WhenFractionalDays(double days, string expected) {
        var period = TimeSpan.FromDays(days);
        var actual = DescriptionBuilder.FormatPeriod(period);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenWeeklyWithNoDaysOfWeek() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput {
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1))),
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            WeeklyPeriod = 2,
            DaysOfWeek = null,
            DailyPeriod = TimeSpan.FromDays(7)
        };

        var nextLocal = new DateTimeOffset(2025, 10, 6, 8, 30, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 6, 8, 30, 0)));

        var periodStr = DescriptionBuilder.FormatPeriod(schedulerInput.DailyPeriod.Value);
        var expected = $"Occurs every {schedulerInput.WeeklyPeriod} week(s) on {nextLocal.DayOfWeek} every {periodStr} " +
                       $"starting on {DescriptionBuilder.ConvertStartDateToZone(schedulerInput, tz).ToShortDateString()}";

        var actual = DescriptionBuilder.BuildDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenWeeklyWithEmptyDaysOfWeek() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput {
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1))),
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            WeeklyPeriod = 2,
            DaysOfWeek = new List<DayOfWeek>(),
            DailyPeriod = TimeSpan.FromDays(7)
        };

        var nextLocal = new DateTimeOffset(2025, 10, 6, 8, 30, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 6, 8, 30, 0)));

        var periodStr = DescriptionBuilder.FormatPeriod(schedulerInput.DailyPeriod.Value);
        var expected = $"Occurs every {schedulerInput.WeeklyPeriod} week(s) on {nextLocal.DayOfWeek} every {periodStr} " +
                       $"starting on {DescriptionBuilder.ConvertStartDateToZone(schedulerInput, tz).ToShortDateString()}";

        var actual = DescriptionBuilder.BuildDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenWeeklyWithNoDailyPeriod() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput {
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1))),
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            WeeklyPeriod = 2,
            DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Wednesday],
            DailyPeriod = null
        };

        var nextLocal = new DateTimeOffset(2025, 10, 6, 8, 30, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 6, 8, 30, 0)));

        var expected = $"Occurs every {schedulerInput.WeeklyPeriod} week(s) on {string.Join(", ", schedulerInput.DaysOfWeek!.Select(d => d.ToString()))} every 1 week " +
                       $"starting on {DescriptionBuilder.ConvertStartDateToZone(schedulerInput, tz).ToShortDateString()}";

        var actual = DescriptionBuilder.BuildDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenDailyWithNoDailyPeriod() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput {
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1))),
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Daily,
            DailyPeriod = null
        };

        var nextLocal = new DateTimeOffset(2025, 10, 5, 10, 15, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 5, 10, 15, 0)));

        var startDateStr = DescriptionBuilder.ConvertStartDateToZone(schedulerInput, tz).ToShortDateString();
        var expected = $"Occurs every 1 day. Schedule will be used on {nextLocal.Date.ToShortDateString()} " +
                       $"at {nextLocal.DateTime.ToShortTimeString()} starting on {startDateStr}";

        var actual = DescriptionBuilder.BuildDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenFormatUnitOne() {
        var period = TimeSpan.FromSeconds(1.0);
        var actual = DescriptionBuilder.FormatPeriod(period);
        Assert.Equal("1 second", actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenUnsupportedRecurrency() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput {
            StartDate = new DateTimeOffset(2025, 3, 2, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 3, 2))),
            Periodicity = EnumConfiguration.Once,
            Recurrency = (EnumRecurrency)999
        };

        var nextLocal = new DateTimeOffset(2025, 10, 5, 14, 45, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 5, 14, 45, 0)));

        var actual = DescriptionBuilder.BuildDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);
        
        Assert.Contains("Occurs once:", actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenNoonTime() {
        var ts = new TimeSpan(12, 0, 0);
        var actual = DescriptionBuilder.TimeSpanToString(ts);
        Assert.Equal("12:00 PM", actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenMidnightTime() {
        var ts = new TimeSpan(0, 0, 0);
        var actual = DescriptionBuilder.TimeSpanToString(ts);
        Assert.Equal("00:00 AM", actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenDailyRecurrentWithPartialWindow() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput {
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1))),
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Daily,
            DailyPeriod = TimeSpan.FromDays(1),
            DailyStartTime = new TimeSpan(8, 0, 0),
            DailyEndTime = null
        };

        var nextLocal = new DateTimeOffset(2025, 10, 5, 9, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 5, 9, 0, 0)));

        var periodStr = DescriptionBuilder.FormatPeriod(schedulerInput.DailyPeriod.Value);
        var startDateStr = DescriptionBuilder.ConvertStartDateToZone(schedulerInput, tz).ToShortDateString();
        var expected = $"Occurs every {periodStr}. Schedule will be used on {nextLocal.Date.ToShortDateString()} " +
                       $"at {nextLocal.DateTime.ToShortTimeString()} starting on {startDateStr}";

        var actual = DescriptionBuilder.BuildDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenWeeklyRecurrentWithoutWeeklyPeriod() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput {
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1))),
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            WeeklyPeriod = null,
            DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday },
            DailyPeriod = TimeSpan.FromDays(7)
        };

        var nextLocal = new DateTimeOffset(2025, 10, 6, 8, 30, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 6, 8, 30, 0)));

        var periodStr = DescriptionBuilder.FormatPeriod(schedulerInput.DailyPeriod.Value);
        var expected = $"Occurs every 1 week(s) on {string.Join(", ", schedulerInput.DaysOfWeek!.Select(d => d.ToString()))} every {periodStr} " +
                       $"starting on {DescriptionBuilder.ConvertStartDateToZone(schedulerInput, tz).ToShortDateString()}";

        var actual = DescriptionBuilder.BuildDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenBuildDescriptionDetectsErrors() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput {
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1))),
            Periodicity = EnumConfiguration.Once,
            Recurrency = EnumRecurrency.Weekly
        };

        var nextLocal = new DateTimeOffset(2025, 10, 6, 8, 30, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 6, 8, 30, 0)));

        var actual = DescriptionBuilder.BuildDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);
        
        output.WriteLine(actual);

        Assert.NotNull(actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenFractionalMinutes() {
        var period = TimeSpan.FromMinutes(2.5);
        var actual = DescriptionBuilder.FormatPeriod(period);
        
        output.WriteLine(actual);
        Assert.Equal("2.5 minutes", actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenFractionalSeconds() {
        var period = TimeSpan.FromSeconds(3.75);
        var actual = DescriptionBuilder.FormatPeriod(period);
        
        output.WriteLine(actual);
        Assert.Equal("3.75 seconds", actual);
    }
}

