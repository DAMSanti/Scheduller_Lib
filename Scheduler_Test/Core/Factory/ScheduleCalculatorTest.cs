using Scheduler_Lib.Core.Interface;
using Scheduler_Lib.Core.Model;

namespace Scheduler_Lib.Core.Factory;

public class ScheduleCalculatorTest {
    [Fact]
    public void GetScheduleCalculator_Once() {
        var calc = ScheduleCalculator.GetScheduleCalculator(EnumPeriodicity.OneTime);
        Assert.IsAssignableFrom<ISchedule>(calc);
        Assert.Equal("Scheduler_Lib.Core.Services.CalcOneTime", calc.GetType().FullName);
    }

    [Fact]
    public void GetScheduleCalculator_Recurrent() {
        var calc = ScheduleCalculator.GetScheduleCalculator(EnumPeriodicity.Recurrent);
        Assert.IsAssignableFrom<ISchedule>(calc);
        Assert.Equal("Scheduler_Lib.Core.Services.CalcRecurrent", calc.GetType().FullName);
    }

    [Fact]
    public void GetScheduleCalculator_Unsupported() {
        var calc = (EnumPeriodicity)5;
        var result = Assert.Throws<UnsupportedPeriodicityException>(() => ScheduleCalculator.GetScheduleCalculator(calc));
        Assert.Equal("Unsupported periodicity.", result.Message);
    }

    [Fact]
    public void GetScheduleCalculator_NullPeriodicity_ThrowsException() {
        EnumPeriodicity? calc = null;
        var result = Assert.Throws<UnsupportedPeriodicityException>(() => ScheduleCalculator.GetScheduleCalculator(calc));
        Assert.Equal("Unsupported periodicity.", result.Message);
    }

}
