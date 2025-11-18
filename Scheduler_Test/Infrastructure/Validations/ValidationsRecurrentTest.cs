using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Resources;
using Xunit.Abstractions;
// ReSharper disable UseObjectOrCollectionInitializer

namespace Scheduler_Lib.Infrastructure.Validations;

public class ValidationsRecurrentTest(ITestOutputHelper output) {
    [Theory]
    [InlineData(null, Messages.ErrorPositiveOffsetRequired)]
    [InlineData(-1.0, Messages.ErrorPositiveOffsetRequired)]
    [InlineData(0.0, Messages.ErrorPositiveOffsetRequired)]
    public void ValidateRecurrent_ShouldFail_WhenPeriodIsInvalid(double? periodDays, string expectedError) {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.OccursOnceChk = false;
        schedulerInput.OccursEveryChk = true;
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.DailyPeriod = periodDays.HasValue ? TimeSpan.FromDays(periodDays.Value) : null;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(expectedError, result.Error);
    }

    [Theory]
    [InlineData("2025-12-31", "2025-01-01", 8, 14, Messages.ErrorStartDatePostEndDate)]
    [InlineData("2025-01-01", "2025-12-31", 14, 8, Messages.ErrorDailyStartAfterEnd)]
    public void ValidateRecurrent_ShouldFail_WhenDateAndTimeRangesAreInvalid(string startDate, string? endDate, int startTime, int endTime, string expectedError) {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.OccursOnceChk = false;
        schedulerInput.OccursEveryChk = true;
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = DateTimeOffset.Parse(startDate);
        schedulerInput.EndDate = DateTimeOffset.Parse(endDate!);
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.DailyStartTime = TimeSpan.FromHours(startTime);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(endTime);

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(expectedError, result.Error);
    }

    [Theory]
    [InlineData(null, Messages.ErrorDaysOfWeekRequired)]
    [InlineData(new DayOfWeek[0], Messages.ErrorDaysOfWeekRequired)]
    [InlineData(new[] { DayOfWeek.Monday , DayOfWeek.Monday}, Messages.ErrorDuplicateDaysOfWeek)]
    public void ValidateRecurrent_ShouldFail_WhenDaysOfWeekAreInvalid(DayOfWeek[]? daysOfWeek, string expectedError) {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.DaysOfWeek = daysOfWeek?.ToList();
        schedulerInput.WeeklyPeriod = 1;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(expectedError, result.Error ?? string.Empty);
    }

    [Theory]
    [InlineData("2025-1-1", "2025-1-1", "2025-12-31")]
    [InlineData("2025-1-1", "2025-1-1", "2025-1-1")]
    [InlineData("2025-1-1", "2025-1-1", null)]
    public void ValidateRecurrent_ShouldSuccess_WhenAllDataIsCorrect(string currentDate, string startDate, string? endDate) {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.CurrentDate = DateTimeOffset.Parse(currentDate);
        schedulerInput.StartDate = DateTimeOffset.Parse(startDate); 
        schedulerInput.EndDate = endDate != null ? DateTimeOffset.Parse(endDate) : null;
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Wednesday];
        schedulerInput.DailyStartTime = TimeSpan.FromHours(8);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(17);
        schedulerInput.DailyPeriod = new TimeSpan(2, 0, 0);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;

        var result = SchedulerService.InitialHandler(schedulerInput);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateRecurrent_ShouldFail_WhenCurrentDateIsOutOfRange() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.CurrentDate = new DateTimeOffset(2024, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorDateOutOfRange, result.Error ?? string.Empty);
    }
    
    [Fact]
    public void ValidateRecurrent_ShouldFail_WhenWeeklyPeriodIsNegative() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];
        schedulerInput.WeeklyPeriod = -1;

        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorWeeklyPeriodRequired, result.Error ?? string.Empty);
    }

    [Fact]
    public void ValidateRecurrent_ShouldFail_WhenMultipleConditionsAreInvalid() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.OccursOnceChk = false;
        schedulerInput.OccursEveryChk = true;
        schedulerInput.CurrentDate = new DateTimeOffset(2024, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.DailyPeriod = TimeSpan.FromDays(-1);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorStartDatePostEndDate, result.Error ?? string.Empty);
        Assert.Contains(Messages.ErrorDateOutOfRange, result.Error ?? string.Empty);
        Assert.Contains(Messages.ErrorPositiveOffsetRequired, result.Error ?? string.Empty);
    }

    [Fact]
    public void ValidateRecurrent_ShouldSuccess_WhenDailyRecurrentIsConfigured() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.OccursOnceChk = false;
        schedulerInput.OccursEveryChk = true;
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = SchedulerService.InitialHandler(schedulerInput);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.NotEqual("", result.Value.Description);
    }

    [Fact]
    public void ValidateMonthly_ShouldFail_WhenBothMonthlyDayAndMonthlyTheAreTrue() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyTheChk = true;

        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorMonthlyModeConflict, result.Error ?? string.Empty);
    }

    [Fact]
    public void ValidateMonthly_ShouldFail_WhenNeitherMonthlyDayNorMonthlyTheAreSelected() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = false;
        schedulerInput.MonthlyTheChk = false;

        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorMonthlyModeRequired, result.Error ?? string.Empty);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(32)]
    [InlineData(100)]
    public void ValidateMonthly_ShouldFail_WhenMonthlyDayIsInvalid(int? monthlyDay) {
        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyTheChk = false;
        schedulerInput.MonthlyDay = monthlyDay;
        schedulerInput.MonthlyDayPeriod = 1;

        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorMonthlyDayInvalid, result.Error ?? string.Empty);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    public void ValidateMonthly_ShouldFail_WhenMonthlyDayPeriodIsInvalid(int? period) {
        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyTheChk = false;
        schedulerInput.MonthlyDay = 15;
        schedulerInput.MonthlyDayPeriod = period;

        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorMonthlyDayPeriodRequired, result.Error ?? string.Empty);
    }

    [Fact]
    public void ValidateMonthly_ShouldFail_WhenMonthlyFrequencyIsMissing() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = false;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = null;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Monday;
        schedulerInput.MonthlyThePeriod = 1;

        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorMonthlyFrequencyRequired, result.Error ?? string.Empty);
    }

    [Fact]
    public void ValidateMonthly_ShouldFail_WhenMonthlyDateTypeIsMissing() {
        var schedulerInput = new SchedulerInput();
        
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = false;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.First;
        schedulerInput.MonthlyDateType = null;
        schedulerInput.MonthlyThePeriod = 1;

        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorMonthlyDateTypeRequired, result.Error ?? string.Empty);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    public void ValidateMonthly_ShouldFail_WhenMonthlyThePeriodIsInvalid(int? period) {
        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = false;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.First;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Monday;
        schedulerInput.MonthlyThePeriod = period;

        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorMonthlyThePeriodRequired, result.Error ?? string.Empty);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(15)]
    [InlineData(31)]
    public void ValidateMonthly_ShouldSuccess_WhenMonthlyDayModeIsValid(int monthlyDay) {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyTheChk = false;
        schedulerInput.MonthlyDay = monthlyDay;
        schedulerInput.MonthlyDayPeriod = 1;
        
        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(EnumMonthlyFrequency.First, EnumMonthlyDateType.Monday)]
    [InlineData(EnumMonthlyFrequency.Second, EnumMonthlyDateType.Day)]
    [InlineData(EnumMonthlyFrequency.Third, EnumMonthlyDateType.Weekday)]
    [InlineData(EnumMonthlyFrequency.Fourth, EnumMonthlyDateType.WeekendDay)]
    [InlineData(EnumMonthlyFrequency.Last, EnumMonthlyDateType.Sunday)]
    public void ValidateMonthly_ShouldSuccess_WhenMonthlyTheModeIsValid(EnumMonthlyFrequency frequency, EnumMonthlyDateType dateType) {
        var schedulerInput = new SchedulerInput();
        
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = false;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = frequency;
        schedulerInput.MonthlyDateType = dateType;
        schedulerInput.MonthlyThePeriod = 1;
        
        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateMonthly_ShouldFail_WhenMultipleErrorsInMonthlyDayMode() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyTheChk = false;
        schedulerInput.MonthlyDay = 50;
        schedulerInput.MonthlyDayPeriod = -1;

        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorMonthlyDayInvalid, result.Error ?? string.Empty);
        Assert.Contains(Messages.ErrorMonthlyDayPeriodRequired, result.Error ?? string.Empty);
    }

    [Fact]
    public void ValidateMonthly_ShouldFail_WhenMultipleErrorsInMonthlyTheMode() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = false;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = null;
        schedulerInput.MonthlyDateType = null;
        schedulerInput.MonthlyThePeriod = 0;

        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorMonthlyFrequencyRequired, result.Error ?? string.Empty);
        Assert.Contains(Messages.ErrorMonthlyDateTypeRequired, result.Error ?? string.Empty);
        Assert.Contains(Messages.ErrorMonthlyThePeriodRequired, result.Error ?? string.Empty);
    }

    [Fact]
    public void ValidateMonthly_ShouldSuccess_WhenMonthlyDayChkCalculatesDates() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 6, 30, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyTheChk = false;
        schedulerInput.MonthlyDay = 15;
        schedulerInput.MonthlyDayPeriod = 1;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.NotEqual("", result.Value.Description);
    }

    [Fact]
    public void ValidateMonthly_ShouldSuccess_WhenMonthlyDayChkHandlesFebruaryEdgeCase() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyTheChk = false;
        schedulerInput.MonthlyDay = 31;
        schedulerInput.MonthlyDayPeriod = 1;

        var result = SchedulerService.InitialHandler(schedulerInput);

        if (result.IsSuccess) {
            var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
            if (futureDates is { Count: > 0 }) {                
                var februaryDate = futureDates.FirstOrDefault(d => d.Month == 2);
                Assert.Equal(default(DateTimeOffset), februaryDate);

                var april = futureDates.FirstOrDefault(d => d.Month == 4);
                var june = futureDates.FirstOrDefault(d => d.Month == 6);
                var september = futureDates.FirstOrDefault(d => d.Month == 9);
                var november = futureDates.FirstOrDefault(d => d.Month == 11);
                Assert.Equal(default(DateTimeOffset), april);
                Assert.Equal(default(DateTimeOffset), june);
                Assert.Equal(default(DateTimeOffset), september);
                Assert.Equal(default(DateTimeOffset), november);

                Assert.True(futureDates.All(d => d.Day == 31));

                Assert.True(futureDates.Count >= 6);

                var monthsWith31Days = new[] { 1, 3, 5, 7, 8, 10, 12 };
                foreach (var date in futureDates) {
                    Assert.Contains(date.Month, monthsWith31Days);
                }
            }
        }

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateMonthly_ShouldSucceed_WhenMonthlyDayChkWithMultipleMonthsPeriod() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyTheChk = false;
        schedulerInput.MonthlyDay = 10;
        schedulerInput.MonthlyDayPeriod = 3;

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? "SUCCESS" : result.Error ?? "NO ERROR");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateRecurrent_ShouldFail_WhenRecurrencyIsUnsupported()    {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;

        schedulerInput.Recurrency = (EnumRecurrency)999;

        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorUnsupportedRecurrency, result.Error ?? string.Empty);
    }
    [Fact]
    public void MonthlyRecurrence_ShouldSuccess_WhenFirstWednesdayIsConfigured() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 3, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = false;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.First;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Wednesday;
        schedulerInput.MonthlyThePeriod = 1;

        var result = SchedulerService.InitialHandler(schedulerInput);
        Assert.True(result.IsSuccess);
        Assert.Contains("Wednesday", result.Value.Description);
    }

    [Fact]
    public void MonthlyRecurrence_ShouldSuccess_WhenSecondThursdayIsConfigured() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 3, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = false;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.Second;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Thursday;
        schedulerInput.MonthlyThePeriod = 1;

        var result = SchedulerService.InitialHandler(schedulerInput);
        Assert.True(result.IsSuccess);
        Assert.Contains("Thursday", result.Value.Description);
    }

    [Fact]
    public void MonthlyRecurrence_ShouldSuccess_WhenThirdSaturdayIsConfigured() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 3, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = false;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.Third;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Saturday;
        schedulerInput.MonthlyThePeriod = 1;

        var result = SchedulerService.InitialHandler(schedulerInput);
        Assert.True(result.IsSuccess);
        Assert.Contains("Saturday", result.Value.Description);
    }

    [Fact]
    public void MonthlyRecurrence_ShouldSuccess_WhenFourthSundayIsConfigured() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 3, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = false;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.Fourth;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Sunday;
        schedulerInput.MonthlyThePeriod = 1;

        var result = SchedulerService.InitialHandler(schedulerInput);
        Assert.True(result.IsSuccess);
        Assert.Contains("Sunday", result.Value.Description);
    }

    [Fact]
    public void MonthlyRecurrence_ShouldSuccess_WhenMonthlyDateTypeIsInvalid() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 3, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = false;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.First;
        schedulerInput.MonthlyDateType = (EnumMonthlyDateType)999;
        schedulerInput.MonthlyThePeriod = 1;

        var result = SchedulerService.InitialHandler(schedulerInput);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void MonthlyRecurrence_ShouldSuccess_WhenMonthlyFrequencyIsInvalid() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 3, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = false;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = (EnumMonthlyFrequency)999;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Monday;
        schedulerInput.MonthlyThePeriod = 1;

        var result = SchedulerService.InitialHandler(schedulerInput);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateMonthly_ShouldFail_WhenDailyStartTimeIsAfterEndTime() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyTheChk = false;
        schedulerInput.MonthlyDay = 10;
        schedulerInput.MonthlyDayPeriod = 1;
        schedulerInput.DailyStartTime = TimeSpan.FromHours(18);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(8);

        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorDailyStartAfterEnd, result.Error ?? string.Empty);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ValidateMonthly_ShouldFail_WhenDailyPeriodIsZeroOrNegative(int hours) {
        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyTheChk = false;
        schedulerInput.MonthlyDay = 10;
        schedulerInput.MonthlyDayPeriod = 1;
        schedulerInput.DailyPeriod = TimeSpan.FromHours(hours);

        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorPositiveOffsetRequired, result.Error ?? string.Empty);
    }

    [Fact]
    public void ValidateRecurrent_ShouldFail_WhenStartDateIsAfterEndDate_OnlyErrorStartDatePostEndDate() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);

        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorStartDatePostEndDate, result.Error ?? string.Empty);
    }

    [Fact]
    public void ValidateRecurrent_ShouldFail_WhenOnlyGeneralErrorsExistAndValidationIsSuccess() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2024, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);

        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorDateOutOfRange, result.Error ?? string.Empty);
    }
}