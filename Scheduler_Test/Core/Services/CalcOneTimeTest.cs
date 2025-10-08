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
    public void Offset_OneTime_OnLimits() {
        var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var requestedDate = new RequestedDate
        {
            Date = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero),
            StartDate = start,
            EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
            Offset = 4,
            Periodicity = EnumPeriodicity.OneTime,
        };

        var preResult = new CalcOneTime();
        var result = preResult.CalcDate(requestedDate);

        var expectedNew = requestedDate.Date.AddDays(requestedDate.Offset.Value);
        Assert.Equal(expectedNew, result.Value.NewDate);
        var expectedDesc =
            $"Occurs Once: Schedule will be used on {expectedNew.Date.ToLongDateString()} starting on {start.Date.ToLongDateString()}";
        Assert.Equal(expectedDesc, result.Value.Description);
    }

    [InlineData(2025, 10, 3)]
    [InlineData(2024, 1, 1)]
    [Theory]
    public void Offset_OneTime_OutLimits(int y, int m, int d) {
        var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var requestedDate = new RequestedDate
        {
            Date = new DateTimeOffset(y, m, d, 0, 0, 0, TimeSpan.Zero),
            StartDate = start,
            EndDate = new DateTimeOffset(2025, 9, 30, 0, 0, 0, TimeSpan.Zero),
            Offset = 4,
            Periodicity = EnumPeriodicity.OneTime,
        };

        var preResult = new CalcOneTime();
        var result = preResult.CalcDate(requestedDate);

        Assert.False(result.IsSuccess);
        Assert.Equal("ERROR: The given date is after the end date.", result.Error);
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
        Assert.Equal("New date time or offset required in Once mode.", result.Message);
    }

    [Fact]
    public void Offset_OneTime_NegativeOffset()
    {
        var requestedDate = new RequestedDate
        {
            Date = new DateTimeOffset(2025, 10, 10, 0, 0, 0, TimeSpan.Zero),
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
            Offset = -5,
            Periodicity = EnumPeriodicity.OneTime
        };

        var preResult = new CalcOneTime();
        var result = preResult.CalcDate(requestedDate);

        var expectedNew = requestedDate.Date.AddDays(-5);
        Assert.Equal(expectedNew, result.Value.NewDate);
    }

    [Fact]
    public void Offset_OneTime_EndDateNull_Throws()
    {
        var requestedDate = new RequestedDate
        {
            Date = new DateTimeOffset(2025, 10, 10, 0, 0, 0, TimeSpan.Zero),
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = null,
            Offset = 1,
            Periodicity = EnumPeriodicity.OneTime
        };

        var preResult = new CalcOneTime();
        var result = preResult.CalcDate(requestedDate);

        var expectedNew = requestedDate.Date.AddDays(requestedDate.Offset.Value);
        Assert.Equal(expectedNew, result.Value.NewDate);
        var expectedDesc =
            $"Occurs Once: Schedule will be used on {expectedNew.Date.ToLongDateString()} starting on {requestedDate.StartDate.Date.ToLongDateString()}";
        Assert.Equal(expectedDesc, result.Value.Description);

    }

}
