using Scheduler_Lib.Core.Model;
using Xunit.Abstractions;
// ReSharper disable UseObjectOrCollectionInitializer

namespace Scheduler_Lib.Core.Services;

public class CalcDateTest(ITestOutputHelper output) {

    [Fact]
    public void CalculateDate_ShouldSuccess_WhenCorrectConfigurationOnceDaily() {
        var schedulerInput = new SchedulerInput();

        schedulerInput!.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.DailyPeriod = new TimeSpan(1, 0, 0, 0);

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess.ToString());

        Assert.True(result.IsSuccess);
    }
}
