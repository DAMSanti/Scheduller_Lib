using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;
using Xunit.Abstractions;

namespace Scheduler_Lib.Core.Services;

public class CalcDateTest(ITestOutputHelper output)
{

    [Fact]
    public void CalculateDate_ShouldSuccess_WhenCorrectConfigurationOnceDaily()
    {
        var requestedDate = new SchedulerInput();

        requestedDate!.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        requestedDate.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Periodicity = EnumConfiguration.Once;
        requestedDate.Recurrency = EnumRecurrency.Daily;
        requestedDate.Period = new TimeSpan(1, 0, 0, 0);

        var result = Service.CalculateDate(requestedDate);

        output.WriteLine(result.IsSuccess.ToString());

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void CalculateDate_ShouldSuccess_WhenCorrectConfigurationOnceWeekly()
    {
        var requestedDate = new SchedulerInput();

        requestedDate!.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        requestedDate.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Periodicity = EnumConfiguration.Once;
        requestedDate.Recurrency = EnumRecurrency.Weekly;
        requestedDate.WeeklyPeriod = 2;
        requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };

        var result = Service.CalculateDate(requestedDate);

        output.WriteLine(result.IsSuccess.ToString());

        Assert.True(result.IsSuccess);
    }
}
