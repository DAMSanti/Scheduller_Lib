using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services.Utilities;
using Scheduler_Lib.Resources;
using Xunit.Abstractions;
// ReSharper disable UseObjectOrCollectionInitializer

namespace Scheduler_Lib.Core.Services;

public class RecurrenceCalculatorTests(ITestOutputHelper output) {

    [Fact]
    public void SelectNextEligibleDate_ShouldSuccess_WhenTargetDateIsMinValue() {
        var targetDate = DateTimeOffset.MinValue;
        var daysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };
        var tz = RecurrenceCalculator.GetTimeZone();

        var result = RecurrenceCalculator.SelectNextEligibleDate(targetDate, daysOfWeek, tz);

        output.WriteLine(result.DateTime.ToLongDateString());

        Assert.Equal(DateTimeOffset.MinValue, result);
    }

    [Theory]
    [InlineData("2025-10-11", DayOfWeek.Monday, "2025-10-13")]
    [InlineData("2025-10-11", DayOfWeek.Tuesday, "2025-10-14")]
    [InlineData("2025-10-11", DayOfWeek.Sunday, "2025-10-12")]
    public void SelectNextEligibleDate_ShouldSuccess_WhenSingleDayOfWeek(string targetDateStr, DayOfWeek dayOfWeek, string expectedDateStr) {
        var tz = RecurrenceCalculator.GetTimeZone();
        var targetDate = DateTimeOffset.Parse(targetDateStr).ToOffset(tz.GetUtcOffset(DateTime.Parse(targetDateStr)));
        var expectedDate = DateTimeOffset.Parse(expectedDateStr)
            .ToOffset(tz.GetUtcOffset(DateTime.Parse(expectedDateStr)));
        var daysOfWeek = new List<DayOfWeek> { dayOfWeek };

        var result = RecurrenceCalculator.SelectNextEligibleDate(targetDate, daysOfWeek, tz);

        output.WriteLine(result.DateTime.ToLongDateString());

        Assert.Equal(expectedDate.Date, result.Date);
        Assert.Equal(targetDate.TimeOfDay, result.TimeOfDay);
    }

    [Fact]
    public void SelectNextEligibleDate_ShouldSuccess_WhenMultipleDaysOfWeek() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var targetDate = new DateTimeOffset(2023, 9, 11, 10, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 11)));
        var daysOfWeek = new List<DayOfWeek> { DayOfWeek.Wednesday, DayOfWeek.Friday };
        var expected = new DateTimeOffset(2023, 9, 13, 10, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 13)));

        var result = RecurrenceCalculator.SelectNextEligibleDate(targetDate, daysOfWeek, tz);

        output.WriteLine(result.DateTime.ToLongDateString());

        Assert.Equal(expected.Date, result.Date);
        Assert.Equal(targetDate.TimeOfDay, result.TimeOfDay);
    }

    [Fact]
    public void SelectNextEligibleDate_ShouldSuccess_WhenNoCandidatesFound() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var targetDate = new DateTimeOffset(2023, 9, 11, 10, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 11)));
        var daysOfWeek = new List<DayOfWeek>();

        var result = RecurrenceCalculator.SelectNextEligibleDate(targetDate, daysOfWeek, tz);

        output.WriteLine(result.DateTime.ToLongDateString());

        Assert.Equal(targetDate, result);
    }

    [Fact]
    public void CalculateWeeklyRecurrence_ShouldSuccess_WhenValidInput() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput();
        schedulerInput.StartDate =
            new DateTimeOffset(2023, 9, 11, 10, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 11)));
        schedulerInput.EndDate =
            new DateTimeOffset(2023, 9, 30, 10, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 30)));
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Wednesday];
        schedulerInput.WeeklyPeriod = 1;

        var result = RecurrenceCalculator.CalculateWeeklyRecurrence(schedulerInput, tz);

        foreach (var date in result!) {
            output.WriteLine(date.ToString());
            Assert.True(date >= schedulerInput.StartDate);
            Assert.True(date <= schedulerInput.EndDate);
        }

        Assert.NotNull(result);
        Assert.True(result.Count > 0);
        foreach (var date in result) {
            output.WriteLine(date.ToString());
            Assert.True(date >= schedulerInput.StartDate);
            Assert.True(date <= schedulerInput.EndDate);
            Assert.Contains(date.DayOfWeek, schedulerInput.DaysOfWeek);
        }
    }

    [Fact]
    public void CalculateWeeklyRecurrence_ShouldSuccess_WhenWeeklyPeriodSet() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput();
        schedulerInput.StartDate =
            new DateTimeOffset(2023, 9, 11, 10, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 11)));
        schedulerInput.EndDate =
            new DateTimeOffset(2023, 10, 30, 10, 0, 0, tz.GetUtcOffset(new DateTime(2023, 10, 30)));
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];
        schedulerInput.WeeklyPeriod = 2;

        var result = RecurrenceCalculator.CalculateWeeklyRecurrence(schedulerInput, tz);

        foreach (var date in result!) {
            output.WriteLine(date.ToString());
            Assert.True(date >= schedulerInput.StartDate);
            Assert.True(date <= schedulerInput.EndDate);
            Assert.Contains(date.DayOfWeek, schedulerInput.DaysOfWeek);
        }

        Assert.NotNull(result);
        Assert.True(result.Count > 1);

        for (var i = 1; i < result.Count; i++) {
            var daysDifference = (result[i].Date - result[i - 1].Date).TotalDays;
            Assert.Equal(14, daysDifference);
        }
    }

    [Fact]
    public void SelectNextEligibleDate_ShouldSuccess_WhenHandlingBoundaryConditions() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var nearMaxDate = DateTime.MaxValue.AddDays(-7);
        var targetDate = new DateTimeOffset(nearMaxDate, tz.GetUtcOffset(nearMaxDate));

        var dayToFind = nearMaxDate.DayOfWeek;
        var daysOfWeek = new List<DayOfWeek> { dayToFind };

        var result = RecurrenceCalculator.SelectNextEligibleDate(targetDate, daysOfWeek, tz);

        output.WriteLine($"Target: {targetDate}, Result: {result}");

        Assert.Equal(dayToFind, result.DayOfWeek);
    }

    [Fact]
    public void CalculateWeeklyRecurrence_ShouldSuccess_WhenDateTimeMaxValue() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(DateTime.MaxValue.AddDays(-10), TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(DateTime.MaxValue, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday];

        var result = RecurrenceCalculator.CalculateWeeklyRecurrence(schedulerInput, tz);

        output.WriteLine($"FutureDates (count = {result?.Count ?? 0}):");
        foreach (var dto in result ?? []) {
            output.WriteLine(dto.ToString());
        }

        Assert.NotNull(result);
    }

    [Fact]
    public void CalculateFutureDates_ShouldSuccess_WhenStartOrEndDateIsMaxValue() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = DateTimeOffset.MaxValue;
        schedulerInput.EndDate = new DateTimeOffset(2023, 9, 30, 10, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 30)));
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;

        var result = RecurrenceCalculator.CalculateFutureDates(schedulerInput, tz);

        output.WriteLine($"FutureDates (count = {result?.Count ?? 0}):");
        foreach (var dto in result ?? []) {
            output.WriteLine(dto.ToString());
        }

        Assert.Empty(result!);

        schedulerInput.StartDate =
            new DateTimeOffset(2023, 9, 11, 10, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 11)));
        schedulerInput.EndDate = DateTimeOffset.MaxValue;
        result = RecurrenceCalculator.CalculateFutureDates(schedulerInput, tz);

        Assert.Empty(result);
    }

    [Fact]
    public void CalculateFutureDates_ShouldSuccess_WhenAddingSimpleDailySlotsWithoutWindow() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate =
            new DateTimeOffset(2023, 9, 11, 10, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 11)));
        schedulerInput.EndDate =
            new DateTimeOffset(2023, 9, 15, 10, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 15)));
        schedulerInput.CurrentDate =
            new DateTimeOffset(2023, 9, 11, 10, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 11)));
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);

        var result = RecurrenceCalculator.CalculateFutureDates(schedulerInput, tz);

        foreach (var date in result!) {
            output.WriteLine(date.ToString());
            Assert.True(date >= schedulerInput.StartDate);
            Assert.True(date <= schedulerInput.EndDate);
        }

        Assert.Equal(5, result.Count);
        for (var i = 0; i < result.Count; i++) {
            Assert.Equal(schedulerInput.StartDate.AddDays(i).Date, result[i].Date);
            Assert.Equal(schedulerInput.StartDate.TimeOfDay, result[i].TimeOfDay);
        }
    }

    [Fact]
    public void CalculateFutureDates_ShouldSuccess_WhenUsingTargetTimeOfDay() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate =
            new DateTimeOffset(2023, 9, 11, 10, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 11)));
        schedulerInput.EndDate =
            new DateTimeOffset(2023, 9, 15, 10, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 15)));
        schedulerInput.CurrentDate =
            new DateTimeOffset(2023, 9, 11, 10, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 11)));
        schedulerInput.TargetDate =
            new DateTimeOffset(2023, 9, 11, 14, 30, 0, tz.GetUtcOffset(new DateTime(2023, 9, 11)));
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);

        var result = RecurrenceCalculator.CalculateFutureDates(schedulerInput, tz);

        foreach (var date in result!) {
            output.WriteLine(date.ToString());
            Assert.True(date >= schedulerInput.StartDate);
            Assert.True(date <= schedulerInput.EndDate);
        }

        foreach (var date in result) {
            Assert.Equal(new TimeSpan(14, 30, 0), date.TimeOfDay);
        }
    }

    [Fact]
    public void CalculateFutureDates_ShouldSuccess_WhenDailyWindowSpecified() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate =
            new DateTimeOffset(2023, 9, 11, 10, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 11)));
        schedulerInput.EndDate =
            new DateTimeOffset(2023, 9, 12, 10, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 12)));
        schedulerInput.CurrentDate =
            new DateTimeOffset(2023, 9, 11, 9, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 11)));
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);
        schedulerInput.DailyStartTime = new TimeSpan(9, 0, 0);
        schedulerInput.DailyEndTime = new TimeSpan(17, 0, 0);
        schedulerInput.DailyPeriod = TimeSpan.FromHours(2);

        var result = RecurrenceCalculator.CalculateFutureDates(schedulerInput, tz);

        foreach (var date in result!) {
            output.WriteLine(date.ToString());
            Assert.True(date >= schedulerInput.StartDate);
            Assert.True(date <= schedulerInput.EndDate);
        }

        Assert.Equal(5, result.Count);

        Assert.Equal(new TimeSpan(11, 0, 0), result[0].TimeOfDay);
        Assert.Equal(new TimeSpan(13, 0, 0), result[1].TimeOfDay);
        Assert.Equal(new TimeSpan(15, 0, 0), result[2].TimeOfDay);
        Assert.Equal(new TimeSpan(17, 0, 0), result[3].TimeOfDay);
        Assert.Equal(new TimeSpan(9, 0, 0), result[4].TimeOfDay);
    }

    [Fact]
    public void CalculateFutureDates_ShouldSuccess_WhenDaysOfWeekProvided() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate =
            new DateTimeOffset(2025, 10, 11, 10, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 11)));
        schedulerInput.EndDate =
            new DateTimeOffset(2025, 10, 26, 11, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 24)));
        schedulerInput.CurrentDate =
            new DateTimeOffset(2025, 10, 11, 10, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 11)));
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday];

        var result = RecurrenceCalculator.CalculateFutureDates(schedulerInput, tz);

        foreach (var date in result!) {
            output.WriteLine(date.ToString());
            Assert.True(date >= schedulerInput.StartDate);
            Assert.True(date <= schedulerInput.EndDate);
        }

        Assert.Equal(6, result.Count);

        Assert.Equal(DayOfWeek.Monday, result[0].DayOfWeek);
        Assert.Equal(DayOfWeek.Wednesday, result[1].DayOfWeek);
        Assert.Equal(DayOfWeek.Friday, result[2].DayOfWeek);

        Assert.Equal(DayOfWeek.Monday, result[3].DayOfWeek);
        Assert.Equal(DayOfWeek.Wednesday, result[4].DayOfWeek);
        Assert.Equal(DayOfWeek.Friday, result[5].DayOfWeek);
    }

    [Fact]
    public void CalculateFutureDates_ShouldSuccess_WhenWeeklyWithDailyWindow() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate =
            new DateTimeOffset(2025, 10, 2, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1)));
        schedulerInput.EndDate =
            new DateTimeOffset(2025, 10, 14, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 14)));
        schedulerInput.CurrentDate =
            new DateTimeOffset(2025, 10, 1, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1)));
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Wednesday];
        schedulerInput.DailyStartTime = new TimeSpan(9, 0, 0);
        schedulerInput.DailyEndTime = new TimeSpan(13, 0, 0);
        schedulerInput.DailyPeriod = TimeSpan.FromHours(2);

        var result = RecurrenceCalculator.CalculateFutureDates(schedulerInput, tz);

        foreach (var date in result!) {
            output.WriteLine(date.ToString());
            Assert.True(date >= schedulerInput.StartDate);
            Assert.True(date <= schedulerInput.EndDate);
        }

        Assert.Equal(3, result.Count);

        foreach (var date in result) {
            Assert.Equal(DayOfWeek.Wednesday, date.DayOfWeek);
        }

        Assert.Equal(new TimeSpan(9, 0, 0), result[0].TimeOfDay);
        Assert.Equal(new TimeSpan(11, 0, 0), result[1].TimeOfDay);
        Assert.Equal(new TimeSpan(13, 0, 0), result[2].TimeOfDay);
    }

    [Fact]
    public void GetNextExecutionDate_ShouldSuccess_WhenTargetDateProvided() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var targetDate = new DateTimeOffset(2023, 9, 11, 10, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 11)));

        var schedulerInput = new SchedulerInput();

        schedulerInput.TargetDate = targetDate;
        schedulerInput.CurrentDate =
            new DateTimeOffset(2023, 9, 1, 8, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 1)));
        schedulerInput.StartDate =
            new DateTimeOffset(2023, 9, 5, 9, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 5)));
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = RecurrenceCalculator.GetNextExecutionDate(schedulerInput, tz);

        output.WriteLine(result.DateTime.ToLongDateString());

        Assert.Equal(targetDate.DateTime, result.DateTime);
    }

    [Fact]
    public void GetNextExecutionDate_ShouldSuccess_WhenTargetNotProvided() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var currentDate = new DateTimeOffset(2023, 9, 11, 8, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 11)));
        var startDate = new DateTimeOffset(2023, 9, 5, 14, 30, 0, tz.GetUtcOffset(new DateTime(2023, 9, 5)));

        var schedulerInput = new SchedulerInput();

        schedulerInput.TargetDate = null;
        schedulerInput.CurrentDate = currentDate;
        schedulerInput.StartDate = startDate;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = RecurrenceCalculator.GetNextExecutionDate(schedulerInput, tz);

        output.WriteLine(result.DateTime.ToLongDateString());

        Assert.Equal(currentDate.Date, result.Date);
        Assert.Equal(startDate.TimeOfDay, result.TimeOfDay);
    }

    [Fact]
    public void GetTimeZone_ShouldSuccess_WhenCalled() {
        var tz = RecurrenceCalculator.GetTimeZone();

        Assert.Equal(Config.TimeZoneId, tz.Id);
    }

    [Fact]
    public void CalculateFutureDates_ShouldFail_WhenDaysOfWeekIsEmpty() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 11, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 11)));
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 26, 11, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 26)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 11, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 11)));
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [];

        var result = RecurrenceCalculator.CalculateFutureDates(schedulerInput, tz);

        Assert.Empty(result);
    }

    [Fact]
    public void TryAddDaysSafely_ShouldSuccess_WhenDaysIsZero() {
        var date = new DateTime(2025, 10, 15, 10, 30, 0);

        var result = DateSafetyHelper.TryAddDaysSafely(date, 0, out var resultDate);

        output.WriteLine($"Original: {date}, Days: 0, Result: {resultDate}, Success: {result}");

        Assert.True(result);
        Assert.Equal(date, resultDate);
    }

    [Fact]
    public void TryAddDaysSafely_ShouldSuccess_WhenAddingPositiveDays() {
        var date = new DateTime(2025, 10, 15, 10, 30, 0);
        var daysToAdd = 5;

        var result = DateSafetyHelper.TryAddDaysSafely(date, daysToAdd, out var resultDate);

        output.WriteLine($"Original: {date}, Days: {daysToAdd}, Result: {resultDate}, Success: {result}");

        Assert.True(result);
        Assert.Equal(new DateTime(2025, 10, 20, 10, 30, 0), resultDate);
    }

    [Fact]
    public void TryAddDaysSafely_ShouldFail_WhenOverflowingMaxValue() {
        var date = DateTime.MaxValue.AddDays(-5);
        var daysToAdd = 10;

        var result = DateSafetyHelper.TryAddDaysSafely(date, daysToAdd, out var resultDate);

        output.WriteLine($"Original: {date}, Days: {daysToAdd}, Result: {resultDate}, Success: {result}");

        Assert.False(result);
        Assert.Equal(date, resultDate);
    }

    [Fact]
    public void TryAddDaysSafely_ShouldSuccess_WhenAddingNegativeDays() {
        var date = new DateTime(2025, 10, 15, 10, 30, 0);
        var daysToAdd = -5;

        var result = DateSafetyHelper.TryAddDaysSafely(date, daysToAdd, out var resultDate);

        output.WriteLine($"Original: {date}, Days: {daysToAdd}, Result: {resultDate}, Success: {result}");

        Assert.True(result);
        Assert.Equal(new DateTime(2025, 10, 10, 10, 30, 0), resultDate);
    }

    [Fact]
    public void TryAddDaysSafely_ShouldFail_WhenOverflowingMinValue() {
        var date = DateTime.MinValue.AddDays(5);
        var daysToAdd = -10;

        var result = DateSafetyHelper.TryAddDaysSafely(date, daysToAdd, out var resultDate);

        output.WriteLine($"Original: {date}, Days: {daysToAdd}, Result: {resultDate}, Success: {result}");

        Assert.False(result);
        Assert.Equal(date, resultDate);
    }

    [Fact]
    public void TryAddDaysSafely_ShouldSuccess_WhenDateIsMaxValueAndDaysIsZero() {
        var date = DateTime.MaxValue;
        var daysToAdd = 0;

        var result = DateSafetyHelper.TryAddDaysSafely(date, daysToAdd, out var resultDate);

        output.WriteLine($"Original: {date}, Days: {daysToAdd}, Result: {resultDate}, Success: {result}");

        Assert.True(result);
        Assert.Equal(date, resultDate);
    }

    [Fact]
    public void GetUtcOffset_ShouldSuccess_WhenValidDateTime() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var dateTime = new DateTime(2025, 1, 15, 10, 30, 0);

        var offset = TimeZoneConverter.GetUtcOffset(dateTime, tz);

        output.WriteLine($"DateTime: {dateTime}, TimeZone: {tz.Id}, Offset: {offset}");
        Assert.Equal(tz.GetUtcOffset(dateTime), offset);
    }

    [Fact]
    public void GetUtcOffset_ShouldSuccess_WhenDSTChanges() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var winterDate = new DateTime(2025, 1, 15, 10, 30, 0);
        var summerDate = new DateTime(2025, 7, 15, 10, 30, 0);

        var winterOffset = TimeZoneConverter.GetUtcOffset(winterDate, tz);
        var summerOffset = TimeZoneConverter.GetUtcOffset(summerDate, tz);

        output.WriteLine($"Winter Offset: {winterOffset}, Summer Offset: {summerOffset}");

        if (tz.SupportsDaylightSavingTime) {
            Assert.NotEqual(winterOffset, summerOffset);
        }
    }

    [Fact]
    public void ConvertToTimeZone_ShouldSuccess_WhenUtcDateProvided() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var utcDate = new DateTimeOffset(2025, 10, 15, 10, 30, 0, TimeSpan.Zero);

        var result = TimeZoneConverter.ConvertToTimeZone(utcDate, tz);

        output.WriteLine($"UTC: {utcDate}, Converted: {result}, TimeZone: {tz.Id}");

        if (tz.Id != "UTC") {
            Assert.NotEqual(utcDate.Offset, result.Offset);
        }

        var expectedOffset = tz.GetUtcOffset(result.DateTime);
        Assert.Equal(expectedOffset, result.Offset);
    }

    [Fact]
    public void ConvertToTimeZone_ShouldSuccess_WhenPreservingUtcTime() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var utcDate = new DateTimeOffset(2025, 10, 15, 10, 30, 0, TimeSpan.Zero);

        var result = TimeZoneConverter.ConvertToTimeZone(utcDate, tz);

        output.WriteLine($"Original UTC: {utcDate.UtcDateTime}, Result UTC: {result.UtcDateTime}");

        Assert.Equal(utcDate.UtcDateTime, result.UtcDateTime);
    }

    [Fact]
    public void CalculateMonthlyRecurrence_ShouldUseEffectiveEndDate_WhenEndDateIsNull() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput();
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.EndDate = null;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 15;
        schedulerInput.MonthlyDayPeriod = 1;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(2);

        var result = RecurrenceCalculator.CalculateMonthlyRecurrence(schedulerInput, tz);

        output.WriteLine($"Monthly Recurrence dates (count = {result?.Count ?? 0}):");
        foreach (var date in result ?? []) {
            output.WriteLine(date.ToString());
        }

        Assert.NotNull(result);
        Assert.True(result.Count > 0);
  
        foreach (var date in result) {
            Assert.Equal(15, date.Day);
        }
    }

    [Fact]
    public void CalculateMonthlyRecurrence_ShouldUseCustomPeriod_WhenDailyPeriodIsSet() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput();
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.EndDate = null;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 1;
        schedulerInput.MonthlyDayPeriod = 1;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(5);

        var result = RecurrenceCalculator.CalculateMonthlyRecurrence(schedulerInput, tz);

        output.WriteLine($"Monthly Recurrence with custom period (count = {result?.Count ?? 0}):");
        foreach (var date in result ?? []) {
            output.WriteLine(date.ToString());
        }

        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }

    [Fact]
    public void CalculateMonthlyRecurrence_ShouldUseDefaultPeriod_WhenDailyPeriodIsNull() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput();
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.EndDate = null;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 10;
        schedulerInput.MonthlyDayPeriod = 1;
        schedulerInput.DailyPeriod = null;

        var result = RecurrenceCalculator.CalculateMonthlyRecurrence(schedulerInput, tz);

        output.WriteLine($"Monthly Recurrence with default period (count = {result?.Count ?? 0}):");
        foreach (var date in result ?? []) {
            output.WriteLine(date.ToString());
        }

        Assert.NotNull(result);
        Assert.True(result.Count > 0);
        foreach (var date in result) {
            Assert.Equal(10, date.Day);
        }
    }

    [Fact]
    public void CalculateMonthlyRecurrence_ShouldCalculateCorrectly_WhenUsingMonthlyTheChk() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput();
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 14, 30, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 14, 30, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.EndDate = null;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.First;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Monday;
        schedulerInput.MonthlyThePeriod = 1;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);

        var result = RecurrenceCalculator.CalculateMonthlyRecurrence(schedulerInput, tz);

        output.WriteLine($"Monthly Recurrence using MonthlyTheChk (count = {result?.Count ?? 0}):");
        foreach (var date in result ?? []) {
            output.WriteLine($"{date} - {date.DayOfWeek}");
        }

        Assert.NotNull(result);
        Assert.True(result.Count > 0);

        foreach (var date in result) {
            Assert.Equal(DayOfWeek.Monday, date.DayOfWeek);
        }
    }
}

