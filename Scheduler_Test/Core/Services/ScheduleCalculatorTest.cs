using Scheduler_Lib.Core.Model;

namespace Scheduler_Lib.Core.Services;

public class ScheduleCalculatorTest {
    [Fact]
    public void GetScheduleCalculator_Once() {
        var requestedDate = new SchedulerInput();

        requestedDate!.Periodicity = EnumConfiguration.Once;
        requestedDate.TargetDate = DateTimeOffset.Now.AddDays(15);
        requestedDate.StartDate = DateTimeOffset.Now;
        requestedDate.EndDate = DateTimeOffset.Now.AddDays(180);
        requestedDate.Recurrency = EnumRecurrency.Daily;

        var result = ScheduleCalculator.GetScheduleCalculator(requestedDate);

        Assert.True(result.IsSuccess);
        Assert.IsType<SchedulerOutput>(result.Value);
    }

    [Fact]
    public void GetScheduleCalculator_Recurrent() {
        var requestedDate = new SchedulerInput();

        requestedDate!.Periodicity = EnumConfiguration.Recurrent;
        requestedDate.CurrentDate = DateTimeOffset.Now.AddDays(15);
        requestedDate.StartDate = DateTimeOffset.Now;
        requestedDate.EndDate = DateTimeOffset.Now.AddDays(180);
        requestedDate.Period = TimeSpan.FromHours(1);
        requestedDate.Recurrency = EnumRecurrency.Daily;
        requestedDate.WeeklyPeriod = 1;
        requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday }; ;


        var result = ScheduleCalculator.GetScheduleCalculator(requestedDate);
        Assert.True(result.IsSuccess);
        Assert.IsType<SchedulerOutput>(result.Value);
    }

    [Fact]
    public void GetScheduleCalculator_Unsupported() {
        var requestedDate = new SchedulerInput();

        requestedDate!.Periodicity = (EnumConfiguration) 5;
        var result = ScheduleCalculator.GetScheduleCalculator(requestedDate);
        Assert.Equal("ERROR: Unsupported periodicity.", result.Error);
    }
}
