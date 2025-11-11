using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;
using Xunit.Abstractions;
// ReSharper disable UseObjectOrCollectionInitializer

namespace Scheduler_Lib.Core.Services;

public class CalcDateTest(ITestOutputHelper output) {

    [Fact]
    public void CalculateDate_ShouldSuccess_WhenConfigurationIsOnceDaily() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursEveryChk = false;
        schedulerInput!.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.DailyPeriod = new TimeSpan(1, 0, 0, 0);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void CalculateDate_ShouldSuccess_WhenConfigurationIsRecurrentDaily() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.OccursOnceChk = false;
        schedulerInput.OccursEveryChk = true;
        schedulerInput!.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.DailyPeriod = new TimeSpan(1, 0, 0, 0);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void CalculateDate_ShouldSuccess_WhenConfigurationIsRecurrentWeekly() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput!.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Wednesday];
        schedulerInput.WeeklyPeriod = 1;

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void CalculateDate_ShouldFail_WhenConfigurationIsInvalid() {
        var schedulerInput = new SchedulerInput();

        schedulerInput!.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = default;
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void CalculateDate_ShouldFail_WhenPeriodicityIsUnsupported() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput!.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = (EnumConfiguration)999;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorUnsupportedPeriodicity, result.Error);
    }

    [Fact]
    public void CalculateDate_ShouldFail_WhenEndDateIsBeforeStartDate() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput!.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.DailyPeriod = new TimeSpan(1, 0, 0, 0);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorStartDatePostEndDate, result.Error);
    }
}
