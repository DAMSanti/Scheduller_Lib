using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;
using Xunit.Abstractions;
// ReSharper disable UseObjectOrCollectionInitializer

namespace Scheduler_Lib.Core.Services;

public class CalcDateTest(ITestOutputHelper output) {

    [Fact]
    public void CalculateDate_ShouldSuccess_WhenCorrectConfigurationOnceDaily() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.Enabled = true;
        schedulerInput.OccursOnce = true;
        schedulerInput.OccursEvery = false;
        schedulerInput!.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.DailyPeriod = new TimeSpan(1, 0, 0, 0);

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void CalculateDate_ShouldSuccess_WhenCorrectConfigurationRecurrentDaily() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.Enabled = true;
        schedulerInput.OccursOnce = false;
        schedulerInput.OccursEvery = true;
        schedulerInput!.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.DailyPeriod = new TimeSpan(1, 0, 0, 0);

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void CalculateDate_ShouldSuccess_WhenCorrectConfigurationRecurrentWeekly() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.Enabled = true;
        schedulerInput.OccursOnce = false;
        schedulerInput.OccursEvery = true;
        schedulerInput!.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Wednesday];
        schedulerInput.WeeklyPeriod = 1;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void CalculateDate_ShouldFail_WhenInvalidConfiguration() {
        var schedulerInput = new SchedulerInput();

        schedulerInput!.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = default;
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void CalculateDate_ShouldFail_WhenUnsupportedPeriodicity() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.Enabled = true;
        schedulerInput!.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = (EnumConfiguration)999;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorUnsupportedPeriodicity, result.Error);
    }

    [Fact]
    public void CalculateDate_ShouldFail_WhenEndDateBeforeStartDate() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.Enabled = true;
        schedulerInput!.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.DailyPeriod = new TimeSpan(1, 0, 0, 0);

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorStartDatePostEndDate, result.Error);
    }
}
