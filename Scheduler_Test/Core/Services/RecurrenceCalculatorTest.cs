using Scheduler_Lib.Core.Model;
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
        var expectedDate = DateTimeOffset.Parse(expectedDateStr).ToOffset(tz.GetUtcOffset(DateTime.Parse(expectedDateStr)));
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
        schedulerInput.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday };
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
        schedulerInput.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };
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
    public void CalculateWeeklyRecurrence_ShouldSuccess_WhenLongRange() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput();
        schedulerInput.StartDate =
            new DateTimeOffset(2023, 9, 11, 10, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 11)));
        schedulerInput.EndDate = new DateTimeOffset(2024, 9, 30, 10, 0, 0, tz.GetUtcOffset(new DateTime(2024, 9, 30)));
        schedulerInput.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday };
        schedulerInput.WeeklyPeriod = 1;

        var result = RecurrenceCalculator.CalculateWeeklyRecurrence(schedulerInput, tz);

        foreach (var date in result!) {
            output.WriteLine(date.ToString());
            Assert.True(date >= schedulerInput.StartDate);
            Assert.True(date <= schedulerInput.EndDate);
            Assert.Contains(date.DayOfWeek, schedulerInput.DaysOfWeek);
        }

        Assert.NotNull(result);
        Assert.True(result.Count <= Config.MaxIterations);
    }

    [Fact]
    public void NextWeekday_ShouldHandleBoundaryConditions() {
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
    public void TryAddDaysSafely_ShouldReturnFalseForDateTimeMaxValue() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput {
            StartDate = new DateTimeOffset(DateTime.MaxValue.AddDays(-10), TimeSpan.Zero),
            EndDate = new DateTimeOffset(DateTime.MaxValue, TimeSpan.Zero),
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            WeeklyPeriod = 1,
            DaysOfWeek = [DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday]
        };

        var result = RecurrenceCalculator.CalculateWeeklyRecurrence(schedulerInput, tz);

        output.WriteLine($"FutureDates (count = {result}):");
        foreach (var dto in result!) {
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

        Assert.Empty(result);

        schedulerInput.StartDate = new DateTimeOffset(2023, 9, 11, 10, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 11)));
        schedulerInput.EndDate = DateTimeOffset.MaxValue;
        result = RecurrenceCalculator.CalculateFutureDates(schedulerInput, tz);

        Assert.Empty(result);
    }

    [Fact]
    public void CalculateFutureDates_ShouldSuccess_WhenPeriodicityNotRecurrent() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput();
        schedulerInput.StartDate =
                new DateTimeOffset(2023, 9, 11, 10, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 11)));
        schedulerInput.EndDate =
                new DateTimeOffset(2023, 9, 30, 10, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 30)));
        schedulerInput.Periodicity = EnumConfiguration.Once;

        var result = RecurrenceCalculator.CalculateFutureDates(schedulerInput, tz);

        Assert.Empty(result);
    }

    [Fact]
    public void CalculateFutureDates_ShouldSuccess_WhenAddsSimpleDailySlotsInDailyWithoutWindow() {
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
    public void CalculateFutureDates_ShouldSuccess_WhenUsesTargetTimeOfDayInTargetDateProvided() {
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
        schedulerInput.DailyFrequency = TimeSpan.FromHours(2);

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
    public void CalculateFutureDates_ShouldSuccess_WhenWeeklyWithoutDaysOfWeek() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput();


        schedulerInput.StartDate =
            new DateTimeOffset(2023, 9, 11, 10, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 11)));
        schedulerInput.EndDate = new DateTimeOffset(2023, 9, 30, 10, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 30)));
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.WeeklyPeriod = 1;

        var result = RecurrenceCalculator.CalculateFutureDates(schedulerInput, tz);

        foreach (var date in result!) {
            output.WriteLine(date.ToString());
            Assert.True(date >= schedulerInput.StartDate);
            Assert.True(date <= schedulerInput.EndDate);
        }

        Assert.Empty(result);
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
            new DateTimeOffset(2025, 10, 1, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1)));
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 14, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 14)));
        schedulerInput.CurrentDate =
            new DateTimeOffset(2025, 10, 1, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1)));
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Wednesday };
        schedulerInput.DailyStartTime = new TimeSpan(9, 0, 0);
        schedulerInput.DailyEndTime = new TimeSpan(13, 0, 0);
        schedulerInput.DailyFrequency = TimeSpan.FromHours(2);

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
    public void GetBaseLocalTime_ShouldSuccess_WhenTargetDateProvided() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var targetDate = new DateTimeOffset(2023, 9, 11, 10, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 11)));

        var schedulerInput = new SchedulerInput();

        schedulerInput.TargetDate = targetDate;
        schedulerInput.CurrentDate =
                new DateTimeOffset(2023, 9, 1, 8, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 1)));
        schedulerInput.StartDate =
                new DateTimeOffset(2023, 9, 5, 9, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 5)));

        var result = RecurrenceCalculator.GetBaseLocalTime(schedulerInput);

        output.WriteLine(result.ToLongDateString());

        Assert.Equal(targetDate.DateTime, result);
    }

    [Fact]
    public void GetBaseLocalTime_ShouldSuccess_WhenTargetNotProvided() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var currentDate = new DateTimeOffset(2023, 9, 11, 8, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 11)));
        var startDate = new DateTimeOffset(2023, 9, 5, 14, 30, 0, tz.GetUtcOffset(new DateTime(2023, 9, 5)));
        
        var schedulerInput = new SchedulerInput();

        schedulerInput.TargetDate = null;
        schedulerInput.CurrentDate = currentDate;
        schedulerInput.StartDate = startDate;

        var result = RecurrenceCalculator.GetBaseLocalTime(schedulerInput);

        output.WriteLine(result.ToLongDateString());

        Assert.Equal(currentDate.Date, result.Date);
        Assert.Equal(startDate.TimeOfDay, result.TimeOfDay);
    }

    [Fact]
    public void GetTimeZone_ShouldSuccess_WhenCalled() {
        var tz = RecurrenceCalculator.GetTimeZone();

        Assert.Equal(Config.TimeZoneId, tz.Id);
    }

    [Fact]
    public void GetCandidateLocalForWeekAndDay_ShouldHandleOutOfRangeDates() {
        var tz = RecurrenceCalculator.GetTimeZone();
        
        var schedulerInput = new SchedulerInput {
            StartDate = new DateTimeOffset(DateTime.MaxValue.AddDays(-30), TimeSpan.Zero),
            EndDate = new DateTimeOffset(DateTime.MaxValue, TimeSpan.Zero),
            CurrentDate = new DateTimeOffset(DateTime.MaxValue.AddDays(-25), TimeSpan.Zero),
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            WeeklyPeriod = 1,
            DaysOfWeek = [DayOfWeek.Sunday]
        };

        var result = RecurrenceCalculator.CalculateFutureDates(schedulerInput, tz);
 
        Assert.NotNull(result);
    }

    [Fact]
    public void AddSimpleDailySlots_ShouldHandleNullTargetDate() {
        var tz = RecurrenceCalculator.GetTimeZone();
        
        var schedulerInput = new SchedulerInput {
            StartDate = new DateTimeOffset(2023, 9, 11, 10, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 11))),
            EndDate = new DateTimeOffset(2023, 9, 15, 10, 0, 0, tz.GetUtcOffset(new DateTime(2023, 9, 15))),
            CurrentDate = new DateTimeOffset(2023, 9, 11, 14, 30, 0, tz.GetUtcOffset(new DateTime(2023, 9, 11))),
            TargetDate = null,
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Daily,
            DailyPeriod = TimeSpan.FromDays(1)
        };

        var result = RecurrenceCalculator.CalculateFutureDates(schedulerInput, tz);
        
        Assert.NotEmpty(result);
        Assert.Equal(schedulerInput.CurrentDate.TimeOfDay, result[0].TimeOfDay);
    }

    [Fact]
    public void SelectNextEligibleDate_ShouldFilterByLambda_WhenMultipleDaysAvailable() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var targetDate = new DateTimeOffset(2025, 10, 15, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 15)));
        var daysOfWeek = new List<DayOfWeek> { 
            DayOfWeek.Friday,
            DayOfWeek.Monday,
            DayOfWeek.Wednesday
        };

        var result = RecurrenceCalculator.SelectNextEligibleDate(targetDate, daysOfWeek, tz);

        Assert.Equal(DayOfWeek.Wednesday, result.DayOfWeek);
        Assert.Equal(targetDate.Day, result.Day);
    }
}


