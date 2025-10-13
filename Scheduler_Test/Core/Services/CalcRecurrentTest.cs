using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Services;
public class CalcRecurrentTest {
    [Fact]
    public void OffSet_Recurrent_Valid() {
        var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        RequestedDate requestedDate = new();
        requestedDate.Date = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        requestedDate.StartDate = start;
        requestedDate.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Period = 1;
        requestedDate.Periodicity = EnumPeriodicity.Recurrent;

        var preResult = new CalcRecurrent();
        var result = preResult.CalculateDate(requestedDate);

        var expectedDate = requestedDate.Date.AddDays(requestedDate.Period.Value);
        Assert.Equal(expectedDate, result.Value!.NewDate);
        var expectedDesc =
            $"Occurs every {requestedDate.Period.Value} days. Schedule will be used on {requestedDate.Date.Date.ToShortDateString()}" +
            $" at {requestedDate.Date.Date.ToShortTimeString()} starting on {start.Date.ToShortDateString()}";
        Assert.Equal(expectedDesc, result.Value.Description);
    }

    [Theory]
    [InlineData(2024, 12, 31)]
    [InlineData(2026, 1, 1)]
    public void OutsideRange_Recurrent_Invalid(int y, int m, int d) {
        RequestedDate requestedDate = new();
        requestedDate.Date = new DateTimeOffset(y, m, d, 0, 0, 0, TimeSpan.Zero);
        requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Period = 1;
        requestedDate.Periodicity = EnumPeriodicity.Recurrent;

        var preResult = new CalcRecurrent();
        var result = preResult.CalculateDate(requestedDate);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorDateOutOfRange, result.Error);
    }

    [Fact]
    public void CalcDate_FutureDates() {
        RequestedDate requestedDate = new();
        requestedDate.Date = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.EndDate = new DateTimeOffset(2025, 1, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Period = 1;
        requestedDate.Periodicity = EnumPeriodicity.Recurrent;

        var preResult = new CalcRecurrent();
        var result = preResult.CalculateDate(requestedDate);

        var expectedDates = new List<DateTimeOffset>();
        expectedDates.Add(new DateTimeOffset(2025, 1, 3, 0, 0, 0, TimeSpan.Zero));
        expectedDates.Add(new DateTimeOffset(2025, 1, 4, 0, 0, 0, TimeSpan.Zero));
        expectedDates.Add(new DateTimeOffset(2025, 1, 5, 0, 0, 0, TimeSpan.Zero));

        Assert.Equal(expectedDates, result.Value!.FutureDates);
    }

    [Fact]
    public void CalcDate_FutureDates_noOffset() {
        RequestedDate requestedDate = new();
        requestedDate.Date = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.EndDate = null;
        requestedDate.Period = 3;
        requestedDate.Periodicity = EnumPeriodicity.Recurrent;

        var preResult = new CalcRecurrent();
        var result = preResult.CalculateDate(requestedDate);

        var expectedDates = new List<DateTimeOffset>();
        expectedDates.Add(new DateTimeOffset(2025, 1, 7, 0, 0, 0, TimeSpan.Zero));
        expectedDates.Add(new DateTimeOffset(2025, 1, 10, 0, 0, 0, TimeSpan.Zero));

        Assert.Equal(expectedDates, result.Value!.FutureDates);
    }

    [Fact]
    public void CalcDate_IfCurrentLessThanEndDate() {
        RequestedDate requestedDate = new();
        requestedDate.Date = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.StartDate = new DateTimeOffset(2025, 3, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.EndDate = new DateTimeOffset(2025, 3, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Period = 3;
        requestedDate.Periodicity = EnumPeriodicity.Recurrent;

        var result = Service.CalculateDate(requestedDate);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorDateOutOfRange, result.Error);
    }

    [Fact]
    public void CalcDate_IfCurrentGreaterThanEndDate() {
        RequestedDate requestedDate = new();
        requestedDate.Date = new DateTimeOffset(2025, 12, 25, 0, 0, 0, TimeSpan.Zero);
        requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.EndDate = new DateTimeOffset(2025, 12, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Period = 1;
        requestedDate.Periodicity = EnumPeriodicity.Recurrent;

        var preResult = new CalcRecurrent();
        var result = preResult.CalculateDate(requestedDate);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorDateOutOfRange, result.Error);
    }

    [Fact]
    public void CalcDate_DatesAllEqual_OneFutureDate() {
        var date = new DateTimeOffset(2025, 3, 3, 0, 0, 0, TimeSpan.Zero);
        RequestedDate requestedDate = new();
        requestedDate.Date = date;
        requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.EndDate = date;
        requestedDate.Period = 1;
        requestedDate.Periodicity = EnumPeriodicity.Recurrent;

        var preResult = new CalcRecurrent();
        var result = preResult.CalculateDate(requestedDate);

        var dateTimeOffsets = result.Value!.FutureDates;
        if (dateTimeOffsets != null) Assert.Empty(dateTimeOffsets);
    }

}

