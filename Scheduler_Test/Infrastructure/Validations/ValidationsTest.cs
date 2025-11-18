using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Resources;
using Xunit.Abstractions;
// ReSharper disable UseObjectOrCollectionInitializer

namespace Scheduler_Lib.Infrastructure.Validations;

public class ValidationsTest(ITestOutputHelper output) {
    [Fact]
    public void ValidateCalculateDate_ShouldSuccess_WhenWeeklyConfigurationIsValid() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];

        var result = SchedulerService.InitialHandler(schedulerInput);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(data: ["2025-10-03", null, EnumConfiguration.Once, EnumRecurrency.Daily, Messages.ErrorCurrentDateNull])]
    [InlineData(data: [null, "2025-10-03", EnumConfiguration.Once, EnumRecurrency.Daily, Messages.ErrorStartDateMissing])]
    public void ValidateCalculateDate_ShouldFail_WhenInputsAreInvalid(string? startDate, string? currentDate, EnumConfiguration periodicity, EnumRecurrency recurrency, string expectedError) { 
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.CurrentDate = currentDate != null ? DateTimeOffset.Parse(currentDate) : default;
        schedulerInput.StartDate = startDate != null ? DateTimeOffset.Parse(startDate) : default;
        schedulerInput.Periodicity = periodicity;
        schedulerInput.Recurrency = recurrency;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(expectedError, result.Error);
    }

    [Fact]
    public void ValidateCalculateDate_ShouldFail_WhenFieldsAreMissing() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.CurrentDate = DateTimeOffset.Parse("2025-10-03");
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = DateTimeOffset.Parse("2025-12-31");
        schedulerInput.TargetDate = DateTimeOffset.Parse("2025-10-05");
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.DaysOfWeek = null;
        schedulerInput.WeeklyPeriod = null;
        schedulerInput.DailyPeriod = null;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorOnceWeekly, result.Error);
    }

    [Fact]
    public void ValidateCalculateDate_ShouldFail_WhenRequestIsNull() {
        SchedulerInput? schedulerInput = null;

        var result = SchedulerService.InitialHandler(schedulerInput!);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorRequestNull, result.Error);
    }

    
    [Fact]
    public void ValidateCalculateDate_ShouldFail_WhenStartDateIsMissing() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput!.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.DailyPeriod = new TimeSpan(1, 0, 0, 0);

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorStartDateMissing, result.Error);
    }

    [Fact]
    public void ValidateCalculateDate_ShouldFail_WhenPeriodicityIsUnsupported() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = (EnumConfiguration)99;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = Validations.ValidateCalculateDate(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorUnsupportedPeriodicity, result.Error ?? string.Empty);
    }

    [Fact]
    public void ValidateCalculateDate_ShouldFail_WhenRecurrencyIsUnsupported() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = (EnumRecurrency)99;

        var result = Validations.ValidateCalculateDate(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorUnsupportedRecurrency, result.Error ?? string.Empty);
    }

    [Fact]
    public void ValidateCalculateDate_ShouldSuccess_WhenAllInputsAreValid() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = Validations.ValidateCalculateDate(schedulerInput);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateCalculateDate_ShouldSuccess_WhenPeriodicityIsOnceAndRecurrencyIsDaily() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.NotEqual("", result.Value.Description);
    }

    [Fact]
    public void ValidateCalculateDate_ShouldFail_WhenEnabledIsFalse() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var baseLocal = new DateTime(2025, 01, 01);

        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = false;
        schedulerInput.CurrentDate = new DateTimeOffset(baseLocal, tz.GetUtcOffset(baseLocal));
        schedulerInput.StartDate = new DateTimeOffset(baseLocal, tz.GetUtcOffset(baseLocal));
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = Validations.ValidateCalculateDate(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Equal(Messages.ErrorApplicationDisabled, result.Error);
    }

    [Fact]
    public void ValidateRecurrent_ShouldFail_WhenDailyModeHasConflict() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursEveryChk = true;

        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorDailyModeConflict, result.Error ?? string.Empty);
    }

    [Fact]
    public void ValidateRecurrent_ShouldFail_WhenDailyModeIsMissing() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.OccursOnceChk = false;
        schedulerInput.OccursEveryChk = false;

        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorDailyModeRequired, result.Error ?? string.Empty);
    }

    [Fact]
    public void ValidateRecurrent_ShouldFail_WhenPositiveOffsetIsRequired() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.OccursOnceChk = false;
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = null;

        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorPositiveOffsetRequired, result.Error ?? string.Empty);
    }

    [Fact]
    public void ValidateRecurrent_ShouldFail_WhenDailyStartIsAfterEnd() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.OccursOnceChk = false;
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromHours(1);
        schedulerInput.DailyStartTime = new TimeSpan(18, 0, 0);
        schedulerInput.DailyEndTime = new TimeSpan(8, 0, 0);

        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorDailyStartAfterEnd, result.Error ?? string.Empty);
    }

    [Fact]
    public void ValidateRecurrent_ShouldFail_WhenOccursOnceAtIsNull() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursEveryChk = false;
        schedulerInput.OccursOnceAt = null;

        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorOccursOnceAtNull, result.Error ?? string.Empty);
    }

    [Fact]
    public void ValidateRecurrent_ShouldFail_WhenDaysOfWeekHaveDuplicates() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Monday];

        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorDuplicateDaysOfWeek, result.Error ?? string.Empty);
    }

    [Fact]
    public void ValidateRecurrent_ShouldFail_WhenDaysOfWeekAreMissing() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = null;

        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorDaysOfWeekRequired, result.Error ?? string.Empty);
    }

    [Fact]
    public void ValidateRecurrent_ShouldFail_WhenWeeklyPeriodIsRequired() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = null;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];

        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorWeeklyPeriodRequired, result.Error ?? string.Empty);
    }

    [Fact]
    public void ValidateOnce_ShouldFail_WhenTargetDateIsAfterEndDate() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 03, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 05, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2025, 10, 10, 0, 0, 0, TimeSpan.Zero);

        var result = ValidationOnce.ValidateOnce(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorTargetDateAfterEndDate, result.Error ?? string.Empty);
    }

    [Fact]
    public void ValidateOnce_ShouldFail_WhenStartDateIsAfterEndDate() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);

        var result = ValidationOnce.ValidateOnce(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorStartDatePostEndDate, result.Error ?? string.Empty);
    }
}