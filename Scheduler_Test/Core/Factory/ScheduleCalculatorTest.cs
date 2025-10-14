using Scheduler_Lib.Core.Model;

namespace Scheduler_Lib.Core.Factory;

public class ScheduleCalculatorTest {
    private readonly RequestedDate? _requestedDate = new();

    [Fact]
    public void GetScheduleCalculator_Once() {
        _requestedDate!.Periodicity = EnumPeriodicity.OneTime;
        _requestedDate.ChangeDate = DateTimeOffset.Now.AddDays(15);
        _requestedDate.StartDate = DateTimeOffset.Now;
        _requestedDate.EndDate = DateTimeOffset.Now.AddDays(180);
        _requestedDate.Ocurrence = EnumOcurrence.None;

        var result = ScheduleCalculator.GetScheduleCalculator(_requestedDate);

        Assert.True(result.IsSuccess);
        Assert.IsType<SolvedDate>(result.Value);
    }

    [Fact]
    public void GetScheduleCalculator_Recurrent() {
        _requestedDate!.Periodicity = EnumPeriodicity.Recurrent;
        _requestedDate.Date = DateTimeOffset.Now.AddDays(15);
        _requestedDate.StartDate = DateTimeOffset.Now;
        _requestedDate.EndDate = DateTimeOffset.Now.AddDays(180);
        _requestedDate.Period = TimeSpan.FromDays(1);
        _requestedDate.Ocurrence = EnumOcurrence.None;

        var result = ScheduleCalculator.GetScheduleCalculator(_requestedDate);
        Assert.True(result.IsSuccess);
        Assert.IsType<SolvedDate>(result.Value);
    }

    [Fact]
    public void GetScheduleCalculator_Unsupported() {
        _requestedDate!.Periodicity = (EnumPeriodicity) 5;
        var result = ScheduleCalculator.GetScheduleCalculator(_requestedDate);
        Assert.Equal("Unsupported periodicity.", result.Error);
    }
}
