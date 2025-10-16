using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;
using Xunit.Abstractions;

namespace Scheduler_Lib.Infrastructure.Validations;

public class ValidationsOnceTest(ITestOutputHelper output) {
    [Fact]
    public void ValidateOnce_ShouldSuccess_WhenEndDateNull() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = null;
        schedulerInput.TargetDate = new DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };
        schedulerInput.WeeklyPeriod = 1;

        var result = ValidationOnce.ValidateOnce(schedulerInput);

        output.WriteLine(result.Value.ToString());

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateOnce_ShouldFail_WhenTargetDateOutOfRange() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };
        schedulerInput.WeeklyPeriod = 1;

        var result = ValidationOnce.ValidateOnce(schedulerInput);

        output.WriteLine(result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorTargetDateAfterEndDate, result.Error);
    }

    [Fact]
    public void ValidateOnce_ShouldFail_WhenStartDateAfterEndDate() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.StartDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };
        schedulerInput.WeeklyPeriod = 1;

        var result = ValidationOnce.ValidateOnce(schedulerInput);

        output.WriteLine(result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorTargetDateAfterEndDate, result.Error);
    }

    [Fact]
    public void ValidateOnce_ShouldSuccess_WhenMissingEndDate() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };
        schedulerInput.WeeklyPeriod = 1;

        var result = ValidationOnce.ValidateOnce(schedulerInput);

        output.WriteLine(result.Value.ToString());

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateOnce_ShouldFail_WhenDailyAndTargetNull() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.Periodicity = EnumConfiguration.Once;

        var result = ValidationOnce.ValidateOnce(schedulerInput);

        output.WriteLine(result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorTargetDateNull, result.Error);
    }

    [Fact]
    public void Validate_ShouldSuccess_MinimumConfigurationDaily() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.TargetDate = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.Periodicity = EnumConfiguration.Once;

        var result = ValidationOnce.ValidateOnce(schedulerInput);

        output.WriteLine(result.Value.ToString());

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Validate_ShouldSuccess_MinimumConfigurationWeekly() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.TargetDate = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.Periodicity = EnumConfiguration.Once;

        var result = ValidationOnce.ValidateOnce(schedulerInput);

        output.WriteLine(result.Value.ToString());

        Assert.True(result.IsSuccess);
    }
}