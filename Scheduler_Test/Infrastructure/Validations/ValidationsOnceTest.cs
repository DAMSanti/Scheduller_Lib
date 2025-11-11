using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;
using Scheduler_Lib.Core.Services;
using Xunit.Abstractions;
// ReSharper disable UseObjectOrCollectionInitializer

namespace Scheduler_Lib.Infrastructure.Validations;

public class ValidationsOnceTest(ITestOutputHelper output) {
    [Theory]
    [InlineData("2025-10-03", "2025-10-02", "2025-10-01", Messages.ErrorStartDatePostEndDate)]
    [InlineData("2025-10-01", "2025-10-10", "2025-09-30", Messages.ErrorTargetDateAfterEndDate)]
    [InlineData("2025-10-01", "2025-10-10", "2025-10-15", Messages.ErrorTargetDateAfterEndDate)]
    [InlineData("2025-10-01", null, "2025-09-30", Messages.ErrorTargetDateAfterEndDate)]
    [InlineData("2025-10-01", "2025-10-10", null, Messages.ErrorTargetDateNull)]
    [InlineData(null, null, "2025-10-05", Messages.ErrorStartDateMissing)]
    public void ValidateOnce_ShouldFail_WhenDatesAreInvalid(string? startDate, string? endDate, string? targetDate, string expectedError) {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = startDate != null ? DateTimeOffset.Parse(startDate) : default;
        schedulerInput.EndDate = endDate != null ? DateTimeOffset.Parse(endDate) : null;
        schedulerInput.TargetDate = targetDate != null ? DateTimeOffset.Parse(targetDate) : null;
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? "NO ERROR" : result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(expectedError, result.Error);
    }

    [Theory]
    [InlineData("2025-10-01", "2025-10-10", "2025-10-05")]
    [InlineData("2025-10-01", "2025-10-10", "2025-10-01")]
    [InlineData("2025-10-01", "2025-10-10", "2025-10-10")]
    [InlineData("2025-10-01", "2025-10-01", "2025-10-01")]
    [InlineData("2025-10-01", null, "2025-10-01")]
    public void ValidateOnce_ShouldSuccess_WhenDatesAreValid(string? startDate, string? endDate, string targetDate) {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = startDate != null ? DateTimeOffset.Parse(startDate) : new DateTimeOffset();
        schedulerInput.EndDate = endDate != null ? DateTimeOffset.Parse(endDate) : null;
        schedulerInput.TargetDate = DateTimeOffset.Parse(targetDate);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? "NO ERROR" : result.Error);
        output.WriteLine(result.Value.Description);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine(dto.ToString());
            }
        }

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(Messages.ErrorTargetDateAfterEndDate, result.Error ?? string.Empty);
    }

    [Fact]
    public void ValidateOnce_ShouldFail_WhenPeriodicityIsOnceAndRecurrencyIsWeekly() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.TargetDate = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.Periodicity = EnumConfiguration.Once;

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorOnceWeekly, result.Error);
    }

    [Fact]
    public void ValidateOnce_ShouldFail_WhenTargetDateIsBeforeStartDate() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 15, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = ValidationOnce.ValidateOnce(schedulerInput);

        output.WriteLine(result.IsSuccess ? "NO ERROR" : result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorTargetDateAfterEndDate, result.Error ?? string.Empty);
    }

    [Fact]
    public void ValidateOnce_ShouldFail_WhenMultipleConditionsAreInvalid() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 15, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = null;
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = ValidationOnce.ValidateOnce(schedulerInput);

        output.WriteLine(result.IsSuccess ? "NO ERROR" : result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorStartDatePostEndDate, result.Error ?? string.Empty);
        Assert.Contains(Messages.ErrorTargetDateNull, result.Error ?? string.Empty);
    }

    [Theory]
    [InlineData("2025-10-01", null, "2025-10-02")]
    [InlineData("2025-10-01", null, "2025-11-01")]
    [InlineData("2025-01-01", null, "2025-12-31")]
    public void ValidateOnce_ShouldSuccess_WhenEndDateIsNullAndTargetDateIsValid(string startDate, string? endDate, string targetDate) {
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = DateTimeOffset.Parse(startDate);
        schedulerInput.EndDate = endDate != null ? DateTimeOffset.Parse(endDate) : null;
        schedulerInput.TargetDate = DateTimeOffset.Parse(targetDate);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = ValidationOnce.ValidateOnce(schedulerInput);

        output.WriteLine($"StartDate: {startDate}, EndDate: null, TargetDate: {targetDate}");
        output.WriteLine(result.IsSuccess ? "NO ERROR" : result.Error);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateOnce_ShouldSuccess_WhenEndDateIsNotNullAndStartDateIsLessThanEndDate() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2025, 10, 15, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = ValidationOnce.ValidateOnce(schedulerInput);

        output.WriteLine(result.IsSuccess ? "NO ERROR" : result.Error);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public void ValidateOnce_ShouldSuccess_WhenTargetDateIsValidAndEndDateIsNull() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = null;
        schedulerInput.TargetDate = new DateTimeOffset(2025, 10, 15, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = ValidationOnce.ValidateOnce(schedulerInput);

        output.WriteLine(result.IsSuccess ? "NO ERROR" : result.Error);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public void ValidateOnce_ShouldSuccess_WhenEndDateIsNotNullAndStartDateEqualsEndDate() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 15, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 15, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2025, 10, 15, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = ValidationOnce.ValidateOnce(schedulerInput);

        output.WriteLine(result.IsSuccess ? "NO ERROR" : result.Error);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public void ValidateOnce_ShouldSuccess_WhenTargetDateIsNotNullAndIsBetweenStartAndEnd() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2025, 10, 10, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = ValidationOnce.ValidateOnce(schedulerInput);

        output.WriteLine(result.IsSuccess ? "NO ERROR" : result.Error);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public void ValidateOnce_ShouldSuccess_WhenTargetDateIsNotNullAndEqualsStartDateWithEndDatePresent() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 10, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2025, 10, 10, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = ValidationOnce.ValidateOnce(schedulerInput);

        output.WriteLine(result.IsSuccess ? "NO ERROR" : result.Error);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public void ValidateOnce_ShouldSuccess_WhenTargetDateIsNotNullAndEqualsEndDate() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2025, 10, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = ValidationOnce.ValidateOnce(schedulerInput);

        output.WriteLine(result.IsSuccess ? "NO ERROR" : result.Error);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public void ValidateOnce_ShouldSuccess_WhenPeriodicityIsNotOnce() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2025, 10, 15, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;

        var result = ValidationOnce.ValidateOnce(schedulerInput);

        output.WriteLine(result.IsSuccess ? "NO ERROR" : result.Error);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public void ValidateOnce_ShouldSuccess_WhenRecurrencyIsNotWeeklyAndPeriodicityIsOnce() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2025, 10, 15, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = ValidationOnce.ValidateOnce(schedulerInput);

        output.WriteLine(result.IsSuccess ? "NO ERROR" : result.Error);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public void ValidateOnce_ShouldSuccess_WhenPeriodicityIsRecurrentAndRecurrencyIsDaily() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2025, 10, 15, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = ValidationOnce.ValidateOnce(schedulerInput);

        output.WriteLine(result.IsSuccess ? "NO ERROR" : result.Error);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public void ValidateOnce_ShouldSuccess_WhenPeriodicityIsRecurrentAndRecurrencyIsMonthly() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2025, 11, 15, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;

        var result = ValidationOnce.ValidateOnce(schedulerInput);

        output.WriteLine(result.IsSuccess ? "NO ERROR" : result.Error);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Theory]
    [InlineData("2025-10-01", "2025-10-31", "2025-10-01")]
    [InlineData("2025-10-01", "2025-10-31", "2025-10-31")]
    [InlineData("2025-10-01", "2025-10-31", "2025-10-15")]
    public void ValidateOnce_ShouldSuccess_WhenTargetDateIsInValidRange(string startDate, string endDate, string targetDate) {
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = DateTimeOffset.Parse(startDate);
        schedulerInput.EndDate = DateTimeOffset.Parse(endDate);
        schedulerInput.TargetDate = DateTimeOffset.Parse(targetDate);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = ValidationOnce.ValidateOnce(schedulerInput);

        output.WriteLine($"StartDate: {startDate}, EndDate: {endDate}, TargetDate: {targetDate}");
        output.WriteLine(result.IsSuccess ? "NO ERROR" : result.Error);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateOnce_ShouldSuccess_WhenEndDateIsNullAndTargetDateIsGreaterThanStartDate() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = null;
        schedulerInput.TargetDate = new DateTimeOffset(2025, 12, 25, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = ValidationOnce.ValidateOnce(schedulerInput);

        output.WriteLine(result.IsSuccess ? "NO ERROR" : result.Error);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateOnce_ShouldFail_WhenEndDateIsNullAndTargetDateIsLessThanStartDate() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 15, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = null;
        schedulerInput.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = ValidationOnce.ValidateOnce(schedulerInput);

        output.WriteLine(result.IsSuccess ? "NO ERROR" : result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorTargetDateAfterEndDate, result.Error ?? string.Empty);
    }
}