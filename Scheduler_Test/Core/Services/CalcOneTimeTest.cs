using Scheduler_Lib.Core.Model;

namespace Scheduler_Lib.Core.Services;
public class CalcOneTimeTest
{
    [Fact]
    public void ChangeDate_OneTime() {
        var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var change = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        var requestedDate = new RequestedDate
        {
            Date = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero),
            StartDate = start,
            EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
            ChangeDate = change,
            Periodicity = EnumPeriodicity.OneTime,
        };

        var preResult = new CalcOneTime();
        var result = preResult.CalcDate(requestedDate);

        Assert.Equal(change, result.Value.NewDate);
        var expectedResult = $"Occurs once: Schedule will be used on {change.Date.ToShortDateString()} at {change.Date.ToShortTimeString()} starting on {requestedDate.StartDate.Date.ToShortDateString()}";
        Assert.Equal(expectedResult, result.Value.Description);
    }

    [Fact]
    public void NoChange_MissingData() {
        var requestedDate = new RequestedDate
        {
            Date = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero),
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
            Periodicity = EnumPeriodicity.OneTime
        };

        var preResult = new CalcOneTime();
        var result = Assert.Throws<OnceModeException>(() => preResult.CalcDate(requestedDate));
        Assert.Equal("New date time required in Once mode.", result.Message);
    }
}