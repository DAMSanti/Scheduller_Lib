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
    public void ValidateOnce_ShouldFail_WhenInvalidDates(string? startDate, string? endDate, string? targetDate, string expectedError) {
        var schedulerInput = new SchedulerInput();

        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = startDate != null ? DateTimeOffset.Parse(startDate) : default;
        schedulerInput.EndDate = endDate != null ? DateTimeOffset.Parse(endDate) : null;
        schedulerInput.TargetDate = targetDate != null ? DateTimeOffset.Parse(targetDate) : null;
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "NO ERROR");

        Assert.False(result.IsSuccess);
        Assert.Contains(expectedError, result.Error);
    }

    [Theory]
    [InlineData("2025-10-01", "2025-10-10", "2025-10-05")]
    [InlineData("2025-10-01", "2025-10-10", "2025-10-01")]
    [InlineData("2025-10-01", "2025-10-10", "2025-10-10")]
    [InlineData("2025-10-01", "2025-10-01", "2025-10-01")]
    [InlineData("2025-10-01", null, "2025-10-01")]
    public void ValidateOnce_ShouldSucceed_WhenValidDates(string? startDate, string? endDate, string targetDate) {
        var schedulerInput = new SchedulerInput();

        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = startDate != null ? DateTimeOffset.Parse(startDate) : new DateTimeOffset();
        schedulerInput.EndDate = endDate != null ? DateTimeOffset.Parse(endDate) : null;
        schedulerInput.TargetDate = DateTimeOffset.Parse(targetDate);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "NO ERROR");
        output.WriteLine(result.Value.Description);

        if (result.Value.FutureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {result.Value.FutureDates.Count}):");
            foreach (var dto in result.Value.FutureDates) {
                output.WriteLine(dto.ToString());
            }
        }

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(Messages.ErrorTargetDateAfterEndDate, result.Error ?? string.Empty);
    }

    [Fact]
    public void ValidateOnce_ShouldFail_WhenPeriodicityOnceRecurrencyWeekly() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.TargetDate = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.Periodicity = EnumConfiguration.Once;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorOnceWeekly, result.Error);
    }
}