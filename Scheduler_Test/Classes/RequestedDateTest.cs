using Scheduler_Lib.Enum;

namespace Scheduler_Lib.Classes;

public class RequestedDateTest {
    [Fact]
    public void EndDateDefault() {
        var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var requestedDate = new RequestedDate {
            Date = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero),
            Enabled = true,
            StartDate = start,
            Offset = TimeSpan.FromDays(5),
            Periodicity = Periodicity.OneTime,
        };

        var date = new DateTimeOffset(1, 1, 1, 0, 0, 0, TimeSpan.Zero);

        Assert.Equal(date, requestedDate.EndDate);
    }

    [Fact]
    public void EndDateDefault_NoOffset() {
        var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var requestedDate = new RequestedDate
        {
            Date = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero),
            Enabled = true,
            StartDate = start,
            Periodicity = Periodicity.OneTime,
        };

        var date = new DateTimeOffset(1, 1, 1, 0, 0, 0, TimeSpan.Zero);

        Assert.Equal(date, requestedDate.EndDate);
    }
}