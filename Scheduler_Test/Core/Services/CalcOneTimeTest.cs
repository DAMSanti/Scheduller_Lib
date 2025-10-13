using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Services;
public class CalcOneTimeTest
{
    [Fact]
    public void ChangeDate_OneTime() {
        var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var change = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        var timeZone = TimeZoneInfo.Local;

        var requestedDate = new RequestedDate();
        requestedDate.Date = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        requestedDate.StartDate = start;
        requestedDate.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        requestedDate.ChangeDate = change;
        requestedDate.Periodicity = EnumPeriodicity.OneTime;
        requestedDate.TimeZonaId = timeZone;

        var preResult = new CalcOneTime();
        var result = preResult.CalculateDate(requestedDate);

        var expectedNewDate = new DateTimeOffset(change.DateTime, timeZone.GetUtcOffset(change.DateTime));
        Assert.Equal(expectedNewDate, result.Value!.NewDate);
        var expectedResult = $"Occurs once: Schedule will be used on {expectedNewDate.Date.ToShortDateString()} at {expectedNewDate.Date.ToShortTimeString()} starting on {requestedDate.StartDate.Date.ToShortDateString()}";
        Assert.Equal(expectedResult, result.Value.Description);
    }

    [Fact]
    public void NoChange_MissingData() {
        var requestedDate = new RequestedDate();
        requestedDate.Date = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Periodicity = EnumPeriodicity.OneTime;

        var preResult = new CalcOneTime();
        var result = preResult.CalculateDate(requestedDate);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorOnceMode, result.Error);
    }
}