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
    public void DescriptionBuilder_ShouldSucceed_WhenWeeklyRecurrentWithTimeWindow() {
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
                       $"between 08:30 AM and 05:00 PM starting on {DescriptionBuilder.ConvertStartDateToZone(schedulerInput, tz).ToShortDateString()}";

        var actual = DescriptionBuilder.HandleDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenDailyRecurrentExpectedString() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1)));
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(2);

        var nextLocal = new DateTimeOffset(2025, 10, 5, 10, 15, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 5, 10, 15, 0)));

        var periodStr = DescriptionBuilder.FormatPeriod(schedulerInput.DailyPeriod.Value);
        var startDateStr = DescriptionBuilder.ConvertStartDateToZone(schedulerInput, tz).ToShortDateString();
        var expected = $"Occurs every {periodStr}. Schedule will be used on {nextLocal.Date.ToShortDateString()} " +
                       $"at {nextLocal.DateTime.ToShortTimeString()} starting on {startDateStr}";

        var actual = DescriptionBuilder.HandleDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenNoDailyPeriodOnce() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 3, 2, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 3, 2)));
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var nextLocal = new DateTimeOffset(2025, 10, 5, 14, 45, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 5, 14, 45, 0)));

        var startDateStr = DescriptionBuilder.ConvertStartDateToZone(schedulerInput, tz).ToShortDateString();
        var expected = $"Occurs once: Schedule will be used on {nextLocal.Date.ToShortDateString()} at {nextLocal.DateTime.ToShortTimeString()} starting on {startDateStr}";

        var actual = DescriptionBuilder.HandleDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenConvertStartDateToZoneAndTimeSpanFormatting() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 5, 23, 0, 0, TimeSpan.Zero);

        var converted = DescriptionBuilder.ConvertStartDateToZone(schedulerInput, tz);
        var expected = TimeZoneInfo.ConvertTime(schedulerInput.StartDate, tz).Date;

        Assert.Equal(expected, converted);

        var ts = new TimeSpan(5, 30, 0);
        Assert.Equal("05:30 AM", DescriptionBuilder.TimeSpanToString12HourFormat(ts));
        Assert.Equal("02:30 PM", DescriptionBuilder.TimeSpanToString12HourFormat(new TimeSpan(14, 30, 0)));
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenFormatPeriodWithFractionalAndUnits() {
        Assert.Equal("1.5 hours", DescriptionBuilder.FormatPeriod(TimeSpan.FromHours(1.5)));
        Assert.Equal("1 hour", DescriptionBuilder.FormatPeriod(TimeSpan.FromHours(1)));
        Assert.Equal("1.25 days", DescriptionBuilder.FormatPeriod(TimeSpan.FromDays(1.25)));
        Assert.Equal("12 hours", DescriptionBuilder.FormatPeriod(TimeSpan.FromDays(0.5)));
        Assert.Equal("18 hours", DescriptionBuilder.FormatPeriod(TimeSpan.FromDays(0.75)));
        Assert.Equal("2.5 minutes", DescriptionBuilder.FormatPeriod(TimeSpan.FromMinutes(2.5)));
        Assert.Equal("3.75 seconds", DescriptionBuilder.FormatPeriod(TimeSpan.FromSeconds(3.75)));
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenDailyRecurrentWithTimeWindow() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1)));
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);
        schedulerInput.DailyStartTime = new TimeSpan(8, 0, 0);
        schedulerInput.DailyEndTime = new TimeSpan(17, 0, 0);

        var nextLocal = new DateTimeOffset(2025, 10, 5, 9, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 5, 9, 0, 0)));

        var periodStr = DescriptionBuilder.FormatPeriod(schedulerInput.DailyPeriod.Value);
        var startDateStr = DescriptionBuilder.ConvertStartDateToZone(schedulerInput, tz).ToShortDateString();
        var expected = $"Occurs every {periodStr} between 08:00 AM and 05:00 PM " +
                       $"at starting on {startDateStr}";

        var actual = DescriptionBuilder.HandleDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenPmTimeFormat() {
        var ts = new TimeSpan(14, 30, 0);
        var actual = DescriptionBuilder.TimeSpanToString12HourFormat(ts);

        output.WriteLine(actual);

        Assert.Equal("02:30 PM", actual);
    }

    [Theory]
    [InlineData(1.25, "1.25 days")]
    [InlineData(0.5, "12 hours")]
    [InlineData(0.75, "18 hours")]
    public void DescriptionBuilder_ShouldSucceed_WhenFractionalDays(double days, string expected) {
        var period = TimeSpan.FromDays(days);
        var actual = DescriptionBuilder.FormatPeriod(period);

        output.WriteLine(actual);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenWeeklyWithNoDailyPeriod() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput();
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1)));
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.WeeklyPeriod = 2;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Wednesday];
        schedulerInput.DailyPeriod = null;

        var nextLocal = new DateTimeOffset(2025, 10, 6, 8, 30, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 6, 8, 30, 0)));

        var expected = $"Occurs every {schedulerInput.WeeklyPeriod} week(s) on {string.Join(", ", schedulerInput.DaysOfWeek!.Select(d => d.ToString()))} every 1 week " +
                       $"starting on {DescriptionBuilder.ConvertStartDateToZone(schedulerInput, tz).ToShortDateString()}";

        var actual = DescriptionBuilder.HandleDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenDailyWithNoDailyPeriod() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1)));
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.DailyPeriod = null;

        var nextLocal = new DateTimeOffset(2025, 10, 5, 10, 15, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 5, 10, 15, 0)));

        var startDateStr = DescriptionBuilder.ConvertStartDateToZone(schedulerInput, tz).ToShortDateString();
        var expected = $"Occurs every 1 day. Schedule will be used on {nextLocal.Date.ToShortDateString()} " +
                       $"at {nextLocal.DateTime.ToShortTimeString()} starting on {startDateStr}";

        var actual = DescriptionBuilder.HandleDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenFormatUnitOne() {
        var period = TimeSpan.FromSeconds(1.0);
        var actual = DescriptionBuilder.FormatPeriod(period);

        output.WriteLine(actual);

        Assert.Equal("1 second", actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenUnsupportedRecurrency() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 3, 2, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 3, 2)));
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = (EnumRecurrency)999;

        var nextLocal = new DateTimeOffset(2025, 10, 5, 14, 45, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 5, 14, 45, 0)));

        var actual = DescriptionBuilder.HandleDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);
        
        Assert.Contains("Occurs once:", actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenNoonTime() {
        var ts = new TimeSpan(12, 0, 0);
        var actual = DescriptionBuilder.TimeSpanToString12HourFormat(ts);

        output.WriteLine(actual);

        Assert.Equal("12:00 PM", actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenMidnightTime() {
        var ts = new TimeSpan(0, 0, 0);
        var actual = DescriptionBuilder.TimeSpanToString12HourFormat(ts);

        output.WriteLine(actual);

        Assert.Equal("12:00 AM", actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenDailyRecurrentWithPartialWindow() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1)));
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);
        schedulerInput.DailyStartTime = new TimeSpan(8, 0, 0);
        schedulerInput.DailyEndTime = null;

        var nextLocal = new DateTimeOffset(2025, 10, 5, 9, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 5, 9, 0, 0)));

        var periodStr = DescriptionBuilder.FormatPeriod(schedulerInput.DailyPeriod.Value);
        var startDateStr = DescriptionBuilder.ConvertStartDateToZone(schedulerInput, tz).ToShortDateString();
        var expected = $"Occurs every {periodStr}. Schedule will be used on {nextLocal.Date.ToShortDateString()} " +
                       $"at {nextLocal.DateTime.ToShortTimeString()} starting on {startDateStr}";

        var actual = DescriptionBuilder.HandleDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenWeeklyRecurrentWithoutWeeklyPeriod() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1)));
            schedulerInput.Periodicity = EnumConfiguration.Recurrent;
            schedulerInput.Recurrency = EnumRecurrency.Weekly;
            schedulerInput.WeeklyPeriod = null;
            schedulerInput.DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Wednesday];
            schedulerInput.DailyPeriod = TimeSpan.FromDays(7);


        var nextLocal = new DateTimeOffset(2025, 10, 6, 8, 30, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 6, 8, 30, 0)));

        var periodStr = DescriptionBuilder.FormatPeriod(schedulerInput.DailyPeriod.Value);
        var expected = $"Occurs every 1 week(s) on {string.Join(", ", schedulerInput.DaysOfWeek!.Select(d => d.ToString()))} every {periodStr} " +
                       $"starting on {DescriptionBuilder.ConvertStartDateToZone(schedulerInput, tz).ToShortDateString()}";

        var actual = DescriptionBuilder.HandleDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenBuildDescriptionDetectsErrors() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput();
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1)));
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;

        var nextLocal = new DateTimeOffset(2025, 10, 6, 8, 30, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 6, 8, 30, 0)));

        var actual = DescriptionBuilder.HandleDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);
        
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
    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenMonthlyTheChkWithFrequencyAndDateType() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1)));
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.First;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Monday;
        schedulerInput.MonthlyThePeriod = 1;

        var nextLocal = new DateTimeOffset(2025, 2, 3, 10, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 2, 3, 10, 0, 0)));

        var startDateStr = DescriptionBuilder.ConvertStartDateToZone(schedulerInput, tz).ToShortDateString();
        var expected = $"Occurs the first Monday of every 1 month(s) starting on {startDateStr}";

        var actual = DescriptionBuilder.HandleDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenMonthlyTheChkWithLastWeekday() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1)));
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.Last;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Weekday;
        schedulerInput.MonthlyThePeriod = 2;

        var nextLocal = new DateTimeOffset(2025, 2, 28, 10, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 2, 28, 10, 0, 0)));

        var startDateStr = DescriptionBuilder.ConvertStartDateToZone(schedulerInput, tz).ToShortDateString();
        var expected = $"Occurs the last weekday of every 2 month(s) starting on {startDateStr}";

        var actual = DescriptionBuilder.HandleDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenMonthlyTheChkWithTimeWindow() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1)));
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.Second;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Friday;
        schedulerInput.MonthlyThePeriod = 1;
        schedulerInput.DailyStartTime = new TimeSpan(9, 0, 0);
        schedulerInput.DailyEndTime = new TimeSpan(17, 0, 0);
        schedulerInput.DailyPeriod = TimeSpan.FromHours(2);

        var nextLocal = new DateTimeOffset(2025, 2, 14, 9, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 2, 14, 9, 0, 0)));

        var startDateStr = DescriptionBuilder.ConvertStartDateToZone(schedulerInput, tz).ToShortDateString();
        var expected = $"Occurs the second Friday of every 1 month(s) every 2 hours between 09:00 AM and 05:00 PM starting on {startDateStr}";

        var actual = DescriptionBuilder.HandleDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenMonthlyDayChk() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1)));
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 15;
        schedulerInput.MonthlyDayPeriod = 1;

        var nextLocal = new DateTimeOffset(2025, 2, 15, 10, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 2, 15, 10, 0, 0)));

        var startDateStr = DescriptionBuilder.ConvertStartDateToZone(schedulerInput, tz).ToShortDateString();
        var expected = $"Occurs day 15 of every 1 month(s) starting on {startDateStr}";

        var actual = DescriptionBuilder.HandleDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenMonthlyDayChkWithTimeWindow() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1)));
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 20;
        schedulerInput.MonthlyDayPeriod = 3;
        schedulerInput.DailyStartTime = new TimeSpan(8, 30, 0);
        schedulerInput.DailyEndTime = new TimeSpan(18, 0, 0);
        schedulerInput.DailyPeriod = TimeSpan.FromHours(3);

        var nextLocal = new DateTimeOffset(2025, 4, 20, 8, 30, 0,
            tz.GetUtcOffset(new DateTime(2025, 4, 20, 8, 30, 0)));

        var startDateStr = DescriptionBuilder.ConvertStartDateToZone(schedulerInput, tz).ToShortDateString();
        var expected = $"Occurs day 20 of every 3 month(s) every 3 hours between 08:30 AM and 06:00 PM starting on {startDateStr}";

        var actual = DescriptionBuilder.HandleDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenMonthlyWithWeekendDay() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1)));
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.Third;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.WeekendDay;
        schedulerInput.MonthlyThePeriod = 1;

        var nextLocal = new DateTimeOffset(2025, 2, 15, 10, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 2, 15, 10, 0, 0)));

        var startDateStr = DescriptionBuilder.ConvertStartDateToZone(schedulerInput, tz).ToShortDateString();
        var expected = $"Occurs the third weekend day of every 1 month(s) starting on {startDateStr}";

        var actual = DescriptionBuilder.HandleDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenMonthlyWithDay() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1)));
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.Fourth;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Day;
        schedulerInput.MonthlyThePeriod = 2;

        var nextLocal = new DateTimeOffset(2025, 2, 4, 10, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 2, 4, 10, 0, 0)));

        var startDateStr = DescriptionBuilder.ConvertStartDateToZone(schedulerInput, tz).ToShortDateString();
        var expected = $"Occurs the fourth day of every 2 month(s) starting on {startDateStr}";

        var actual = DescriptionBuilder.HandleDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenMonthlyWithAllDaysOfWeek() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var daysToTest = new[] {
            (EnumMonthlyDateType.Monday, "Monday"),
            (EnumMonthlyDateType.Tuesday, "Tuesday"),
            (EnumMonthlyDateType.Wednesday, "Wednesday"),
            (EnumMonthlyDateType.Thursday, "Thursday"),
            (EnumMonthlyDateType.Friday, "Friday"),
            (EnumMonthlyDateType.Saturday, "Saturday"),
            (EnumMonthlyDateType.Sunday, "Sunday")
        };

        foreach (var (dateType, expectedDay) in daysToTest) {
            var schedulerInput = new SchedulerInput();
            schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1)));
            schedulerInput.Periodicity = EnumConfiguration.Recurrent;
            schedulerInput.Recurrency = EnumRecurrency.Monthly;
            schedulerInput.MonthlyTheChk = true;
            schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.First;
            schedulerInput.MonthlyDateType = dateType;
            schedulerInput.MonthlyThePeriod = 1;

            var nextLocal = new DateTimeOffset(2025, 2, 1, 10, 0, 0,
                tz.GetUtcOffset(new DateTime(2025, 2, 1, 10, 0, 0)));

            var actual = DescriptionBuilder.HandleDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

            output.WriteLine($"{dateType}: {actual}");
            Assert.Contains(expectedDay, actual);
            Assert.Contains("first", actual);
        }
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenMonthlyWithNullPeriods() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1)));
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.First;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Monday;
        schedulerInput.MonthlyThePeriod = null;

        var nextLocal = new DateTimeOffset(2025, 2, 3, 10, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 2, 3, 10, 0, 0)));

        var actual = DescriptionBuilder.HandleDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Contains("1 month(s)", actual);
    }

    [Fact]
    public void DescriptionBuilder_ShouldSucceed_WhenMonthlyNeitherDayNorTheChk() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1)));
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.MonthlyTheChk = false;
        schedulerInput.MonthlyDayChk = false;

        var nextLocal = new DateTimeOffset(2025, 2, 1, 10, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 2, 1, 10, 0, 0)));

        var startDateStr = DescriptionBuilder.ConvertStartDateToZone(schedulerInput, tz).ToShortDateString();
        var expected = $"Occurs monthly starting on {startDateStr}";

        var actual = DescriptionBuilder.HandleDescriptionForCalculatedDate(schedulerInput, tz, nextLocal);

        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }
}

