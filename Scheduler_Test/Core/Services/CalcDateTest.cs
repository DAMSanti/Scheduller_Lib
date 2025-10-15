using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Services;
public class CalcDateTest {
    private readonly SchedulerInput? _requestedDate = new();

    [Fact]
    public void CalcDate_Valid() {
        var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var change = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.StartDate = start;
        _requestedDate.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.TargetDate = change;
        _requestedDate.Periodicity = EnumConfiguration.OneTime;
        _requestedDate.Recurrency = EnumRecurrency.Daily;

        var result = Service.CalculateDate(_requestedDate);

        var expectedResult = $"Occurs once: Schedule will be used on {change.Date.ToShortDateString()} at {change.Date.ToShortTimeString()} starting on {_requestedDate.StartDate.Date.ToShortDateString()}";
        Assert.Equal(expectedResult, result.Value!.Description);
    }

    [Fact]
    public void NullRequest() {
        SchedulerInput? requestedDate = null;
        var result = Service.CalculateDate(requestedDate!);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorRequestNull, result.Error);
    }
}
