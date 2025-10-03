using Scheduler_Lib.Classes;
using Scheduler_Lib.Enum;

namespace Scheduler_Lib.Services;
public class CalcOneTimeTest
{
    [Fact]
    public void ChangeDate_OneTime() {
        var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var change = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        var requestedDate = new RequestedDate
        {
            Date = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero),
            Enabled = true,
            StartDate = start,
            EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
            ChangeDate = change,
            Periodicity = Periodicity.OneTime,
        };

        var preResult = new CalcOneTime();
        var result = preResult.CalcDate(requestedDate);

        Assert.Equal(change, result.NewDate);
        var expectedResult = $"Occurs once: Schedule will be used on {change:dd/MM/yyyy} at {change:HH:mm} starting on {requestedDate.StartDate:dd/MM/yyyy}";
        Assert.Equal(expectedResult, result.Description);
    }

    [Fact]
    public void Offset_OneTime_OnLimits() {
        var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var requestedDate = new RequestedDate
        {
            Date = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero),
            Enabled = true,
            StartDate = start,
            EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
            Offset = TimeSpan.FromDays(4),
            Periodicity = Periodicity.OneTime,
        };

        var preResult = new CalcOneTime();
        var result = preResult.CalcDate(requestedDate);

        var expectedNew = requestedDate.Date.Add(requestedDate.Offset.Value);
        Assert.Equal(expectedNew, result.NewDate);
        var expectedDesc =
            $"Occurs Once: Schedule will be used on {expectedNew:dd/MM/yyyy HH:mm} starting on {start:dd/MM/yyyy HH:mm}";
        Assert.Equal(expectedDesc, result.Description);
    }

    [Fact]
    public void Offset_OneTime_OutLimits() {
        var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var requestedDate = new RequestedDate
        {
            Date = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero),
            Enabled = true,
            StartDate = start,
            EndDate = new DateTimeOffset(2025, 9, 30, 0, 0, 0, TimeSpan.Zero),
            Offset = TimeSpan.FromDays(4),
            Periodicity = Periodicity.OneTime,
        };

        var preResult = new CalcOneTime();
        var result = preResult.CalcDate(requestedDate);

        Assert.Equal(requestedDate.Date, result.NewDate);
        Assert.Equal("ERROR: The given date is after the end date.", result.Description);
    }

    [Fact]
    public void NoChange_MissingData() {
        var requestedDate = new RequestedDate
        {
            Date = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero),
            Enabled = true,
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
            Periodicity = Periodicity.OneTime
        };

        var preResult = new CalcOneTime();
        var result = Assert.Throws<Exception>(() => preResult.CalcDate(requestedDate));
        Assert.Equal("New date time or offset required in Once mode.", result.Message);
    }
}
