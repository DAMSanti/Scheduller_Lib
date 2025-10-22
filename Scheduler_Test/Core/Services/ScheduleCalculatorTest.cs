using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;
using Xunit.Abstractions;
// ReSharper disable UseObjectOrCollectionInitializer

namespace Scheduler_Lib.Core.Services;

public class ScheduleCalculatorTest(ITestOutputHelper output) {
    [Fact]
    public void GetScheduleCalculator_ShouldSucceed_WhenPeriodicityOnce() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput!.Periodicity = EnumConfiguration.Once;
        schedulerInput.TargetDate = DateTimeOffset.Now.AddDays(15);
        schedulerInput.StartDate = DateTimeOffset.Now;
        schedulerInput.EndDate = DateTimeOffset.Now.AddDays(180);
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "NO ERROR");
        output.WriteLine(result.Value.Description);

        if (result.Value.FutureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {result.Value.FutureDates.Count}):");
            foreach (var dto in result.Value.FutureDates) {
                output.WriteLine(dto.ToString());
            }
        }

        Assert.True(result.IsSuccess);
        Assert.IsType<SchedulerOutput>(result.Value);
    }

    [Fact]
    public void GetScheduleCalculator_ShouldSucceed_WhenPeriodicityRecurrent() {
        var schedulerInput = new SchedulerInput();

        schedulerInput!.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.CurrentDate = DateTimeOffset.Now.AddDays(15);
        schedulerInput.StartDate = DateTimeOffset.Now;
        schedulerInput.EndDate = DateTimeOffset.Now.AddDays(180);
        schedulerInput.DailyPeriod = TimeSpan.FromHours(1);
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday]; ;

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "NO ERROR");
        output.WriteLine(result.Value.Description);

        if (result.Value.FutureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {result.Value.FutureDates.Count}):");
            foreach (var dto in result.Value.FutureDates) {
                output.WriteLine(dto.ToString());
            }
        }

        Assert.True(result.IsSuccess);
        Assert.IsType<SchedulerOutput>(result.Value);
    }

    [Fact]
    public void GetScheduleCalculator_ShouldFail_WhenConfigUnsupported() {
        var schedulerInput = new SchedulerInput();

        schedulerInput!.Periodicity = (EnumConfiguration) 5;

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "NO ERROR");

        Assert.Equal(Messages.ErrorUnsupportedPeriodicity, result.Error);
    }
}
