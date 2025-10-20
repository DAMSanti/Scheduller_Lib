using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;
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
    public void ValidateOnce_ShouldFail_WithInvalidDates(string startDate, string? endDate, string? targetDate, string expectedError) {
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = DateTimeOffset.Parse(startDate);
        schedulerInput.EndDate = endDate != null ? DateTimeOffset.Parse(endDate) : null;
        schedulerInput.TargetDate = targetDate != null ? DateTimeOffset.Parse(targetDate) : null;
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = ValidationOnce.ValidateOnce(schedulerInput);

        output.WriteLine(result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(expectedError, result.Error);
    }

    [Theory]
    [InlineData("2025-10-01", "2025-10-10", "2025-10-05")]
    [InlineData("2025-10-01", "2025-10-10", "2025-10-01")]
    [InlineData("2025-10-01", "2025-10-10", "2025-10-10")]
    [InlineData("2025-10-01", "2025-10-01", "2025-10-01")]
    [InlineData(null, null, "2025-10-05")]
    [InlineData("2025-10-01", null, "2025-10-01")]
    public void ValidateOnce_ShouldSucceed_WithValidDates(string? startDate, string? endDate, string targetDate) {
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = startDate != null ? DateTimeOffset.Parse(startDate) : new DateTimeOffset();
        schedulerInput.EndDate = endDate != null ? DateTimeOffset.Parse(endDate) : null;
        schedulerInput.TargetDate = DateTimeOffset.Parse(targetDate);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = ValidationOnce.ValidateOnce(schedulerInput);

        output.WriteLine(result.Value.ToString());

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(Messages.ErrorTargetDateAfterEndDate, result.Error ?? string.Empty);
    }

    [Fact]
    public void ValidateOnce_ShouldFail_WhenPeriodicityOnceRecurrencyWeekly() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.TargetDate = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.Periodicity = EnumConfiguration.Once;

        var result = ValidationOnce.ValidateOnce(schedulerInput);

        output.WriteLine(result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorOnceWeekly, result.Error);
    }

    [Fact]
    public void ValidateOnce_ShouldFail_WhenPeriodicityRecurrentRecurrencyWeekly() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.TargetDate = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;

        var result = ValidationOnce.ValidateOnce(schedulerInput);

        output.WriteLine(result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorUnsupportedPeriodicity, result.Error);
    }
}