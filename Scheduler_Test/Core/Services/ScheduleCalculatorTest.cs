using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Factory;
using Scheduler_Lib.Resources;
using Xunit.Abstractions;
// ReSharper disable UseObjectOrCollectionInitializer

namespace Scheduler_Lib.Core.Services;

public class ScheduleCalculatorFactoryTest(ITestOutputHelper output) {
   /*
    [Fact]
    public void CreateAndExecute_ShouldSuccess_WhenPeriodicityIsOnce() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput!.Periodicity = EnumConfiguration.Once;
        schedulerInput.TargetDate = DateTimeOffset.Now.AddDays(15);
        schedulerInput.StartDate = DateTimeOffset.Now;
        schedulerInput.EndDate = DateTimeOffset.Now.AddDays(180);
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = ScheduleCalculatorOrchestator.GetPeriodicityType(schedulerInput);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.IsType<SchedulerOutput>(result.Value);
    }

    [Fact]
    public void CreateAndExecute_ShouldSuccess_WhenPeriodicityIsRecurrent() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.OccursOnceChk = false;
        schedulerInput.OccursEveryChk = true;
        schedulerInput!.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.CurrentDate = DateTimeOffset.Now.AddDays(15);
        schedulerInput.StartDate = DateTimeOffset.Now;
        schedulerInput.EndDate = DateTimeOffset.Now.AddDays(180);
        schedulerInput.DailyPeriod = TimeSpan.FromHours(1);
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];

        var result = ScheduleCalculatorOrchestator.GetPeriodicityType(schedulerInput);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.IsType<SchedulerOutput>(result.Value);
    }

    [Fact]
    public void CreateAndExecute_ShouldFail_WhenPeriodicityIsUnsupported() {
        var schedulerInput = new SchedulerInput();

        schedulerInput!.Periodicity = (EnumConfiguration) 5;

        var result = ScheduleCalculatorOrchestator.GetPeriodicityType(schedulerInput);

        Assert.Equal(Messages.ErrorUnsupportedPeriodicity, result.Error);
    }*/
}
