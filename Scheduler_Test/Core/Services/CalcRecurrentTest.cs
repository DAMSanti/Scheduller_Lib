using Scheduler_Lib.Core.Model;

namespace Scheduler_Lib.Core.Services;
public class CalcRecurrentTest
{
    [Fact]
    public void OffSet_Recurrent_Valid() {
        var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var requestedDate = new RequestedDate
        {
            Date = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero),
            StartDate = start,
            EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
            Offset = 1,
            Periodicity = EnumPeriodicity.OneTime,
        };

        var preResult = new CalcRecurrent();
        var result = preResult.CalcDate(requestedDate);

        var expectedDate = requestedDate.Date.AddDays(requestedDate.Offset.Value);
        Assert.Equal(expectedDate, result.Value.NewDate);
        var expectedDesc =
            $"Occurs every {requestedDate.Offset.Value} days. Schedule will be used on {requestedDate.Date:dd/MM/yyyy}" +
            $" at {requestedDate.Date:HH:mm} starting on {start:dd/MM/yyyy}";
        Assert.Equal(expectedDesc, result.Value.Description);
    }

    [Theory]
    [InlineData(2024, 12, 31)]
    [InlineData(2026, 1, 1)]
    public void OutsideRange_Recurrent_Invalid(int y, int m, int d) {
        var requestedDate = new RequestedDate
        {
            Date = new DateTimeOffset(y, m, d, 0, 0, 0, TimeSpan.Zero),
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
            Offset = 1,
            Periodicity = EnumPeriodicity.Recurrent
        };

        var preResult = new CalcRecurrent();
        var result = Assert.Throws<Exception>(() => preResult.CalcDate(requestedDate));
        Assert.Equal("The date should be between start and end date.", result.Message);
    }

    [Fact]
    public void CalcDate_FutureDates() {
        var requestedDate = new RequestedDate {
            Date = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 1, 5, 0, 0, 0, TimeSpan.Zero),
            Offset = 1,
            Periodicity = EnumPeriodicity.Recurrent
        };

        var preResult = new CalcRecurrent();
        var result = preResult.CalcDate(requestedDate);

        var expectedDates = new List<DateTimeOffset>
        {
            new DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 1, 3, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 1, 4, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 1, 5, 0, 0, 0, TimeSpan.Zero)
        };

        Assert.Equal(expectedDates, result.Value.FutureDates);
    }

    [Fact]
    public void CalcDate_FutureDates_noOffset() {
        var requestedDate = new RequestedDate
        {
            Date = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = null,
            Offset = 3,
            Periodicity = EnumPeriodicity.Recurrent
        };

        var preResult = new CalcRecurrent();
        var result = preResult.CalcDate(requestedDate);

        var expectedDates = new List<DateTimeOffset>
        {
            new DateTimeOffset(2025, 1, 4, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 1, 7, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 1, 10, 0, 0, 0, TimeSpan.Zero)
        };

        Assert.Equal(expectedDates, result.Value.FutureDates);
    }

    [Fact]
    public void CalcDate_IfCurrentLessThanEndDate()
    {
        var requestedDate = new RequestedDate
        {
            Date = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            StartDate = new DateTimeOffset(2025, 3, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 3, 1, 0, 0, 0, TimeSpan.Zero),
            Offset = 3,
            Periodicity = EnumPeriodicity.Recurrent
        };

        var result = Assert.Throws<Exception>(() => Service.CalcDate(requestedDate));
    }

    [Fact]
    public void CalcDate_IfCurrentGreaterThanEndDate()
    {
        var requestedDate = new RequestedDate
        {
            Date = new DateTimeOffset(2025, 12, 25, 0, 0, 0, TimeSpan.Zero),
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 12, 1, 0, 0, 0, TimeSpan.Zero),
            Offset = 1,
            Periodicity = EnumPeriodicity.Recurrent
        };

        var preResult = new CalcRecurrent();
        var result = Assert.Throws<Exception>(() => preResult.CalcDate(requestedDate));
        Assert.Equal("The date should be between start and end date.", result.Message);
    }



}

