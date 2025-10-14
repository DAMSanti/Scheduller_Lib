using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Services;
public class CalcRecurrentTest {
    private readonly RequestedDate? _requestedDate = new();

    [Fact]
    public void OffSet_Recurrent_Valid() {
        var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.Date = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.StartDate = start;
        _requestedDate.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.Period = TimeSpan.FromDays(1);
        _requestedDate.Periodicity = EnumPeriodicity.Recurrent;

        var preResult = new CalcRecurrent();
        var result = preResult.CalculateDate(_requestedDate);

        var expectedDate = _requestedDate.Date.Add(_requestedDate.Period.Value);
        Assert.Equal(expectedDate, result.Value!.NewDate);
        var expectedDesc =
            $"Occurs every {_requestedDate.Period.Value} days. Schedule will be used on {_requestedDate.Date.Date.ToShortDateString()}" +
            $" at {_requestedDate.Date.Date.ToShortTimeString()} starting on {start.Date.ToShortDateString()}";
        Assert.Equal(expectedDesc, result.Value.Description);
    }

    [Theory]
    [InlineData(2024, 12, 31)]
    [InlineData(2026, 1, 1)]
    public void OutsideRange_Recurrent_Invalid(int y, int m, int d) {
        _requestedDate.Date = new DateTimeOffset(y, m, d, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.Period = TimeSpan.FromDays(1);
        _requestedDate.Periodicity = EnumPeriodicity.Recurrent;

        var preResult = new CalcRecurrent();
        var result = preResult.CalculateDate(_requestedDate);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorDateOutOfRange, result.Error);
    }

    [Fact]
    public void CalcDate_FutureDates() {
        _requestedDate.Date = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.EndDate = new DateTimeOffset(2025, 1, 5, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.Period = TimeSpan.FromDays(1);
        _requestedDate.Periodicity = EnumPeriodicity.Recurrent;

        var preResult = new CalcRecurrent();
        var result = preResult.CalculateDate(_requestedDate);

        var expectedDates = new List<DateTimeOffset>();
        expectedDates.Add(new DateTimeOffset(2025, 1, 3, 0, 0, 0, TimeSpan.Zero));
        expectedDates.Add(new DateTimeOffset(2025, 1, 4, 0, 0, 0, TimeSpan.Zero));
        expectedDates.Add(new DateTimeOffset(2025, 1, 5, 0, 0, 0, TimeSpan.Zero));

        Assert.Equal(expectedDates, result.Value!.FutureDates);
    }

    [Fact]
    public void CalcDate_FutureDates_noOffset() {
        _requestedDate.Date = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.EndDate = null;
        _requestedDate.Period = TimeSpan.FromDays(3);
        _requestedDate.Periodicity = EnumPeriodicity.Recurrent;

        var preResult = new CalcRecurrent();
        var result = preResult.CalculateDate(_requestedDate);

        var expectedDates = new List<DateTimeOffset>();
        expectedDates.Add(new DateTimeOffset(2025, 1, 7, 0, 0, 0, TimeSpan.Zero));
        expectedDates.Add(new DateTimeOffset(2025, 1, 10, 0, 0, 0, TimeSpan.Zero));

        Assert.Equal(expectedDates, result.Value!.FutureDates);
    }

    [Fact]
    public void CalcDate_IfCurrentLessThanEndDate() {
        _requestedDate.Date = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.StartDate = new DateTimeOffset(2025, 3, 1, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.EndDate = new DateTimeOffset(2025, 3, 1, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.Period = TimeSpan.FromDays(3);
        _requestedDate.Periodicity = EnumPeriodicity.Recurrent;

        var result = Service.CalculateDate(_requestedDate);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorDateOutOfRange, result.Error);
    }

    [Fact]
    public void CalcDate_IfCurrentGreaterThanEndDate() {
        _requestedDate.Date = new DateTimeOffset(2025, 12, 25, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.EndDate = new DateTimeOffset(2025, 12, 1, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.Period = TimeSpan.FromDays(1);
        _requestedDate.Periodicity = EnumPeriodicity.Recurrent;

        var preResult = new CalcRecurrent();
        var result = preResult.CalculateDate(_requestedDate);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorDateOutOfRange, result.Error);
    }

    [Fact]
    public void CalcDate_DatesAllEqual_OneFutureDate() {
        var date = new DateTimeOffset(2025, 3, 3, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.Date = date;
        _requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.EndDate = date;
        _requestedDate.Period = TimeSpan.FromDays(1);
        _requestedDate.Periodicity = EnumPeriodicity.Recurrent;

        var preResult = new CalcRecurrent();
        var result = preResult.CalculateDate(_requestedDate);

        var dateTimeOffsets = result.Value!.FutureDates;
        if (dateTimeOffsets != null) Assert.Empty(dateTimeOffsets);
    }


    [Fact]
    public void CalcDate_FutureDates_DLS() {
        _requestedDate.Date = new DateTimeOffset(2025, 3, 23, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.StartDate = new DateTimeOffset(2025, 3, 20, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.EndDate = new DateTimeOffset(2025, 4, 5, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.Period = TimeSpan.FromDays(2);
        _requestedDate.Periodicity = EnumPeriodicity.Recurrent;

        var preResult = new CalcRecurrent();
        var result = preResult.CalculateDate(_requestedDate);

        var expectedDates = new List<DateTimeOffset>();
        expectedDates.Add(new DateTimeOffset(2025, 3, 27, 1, 0, 0, TimeSpan.FromHours(1)));
        expectedDates.Add(new DateTimeOffset(2025, 3, 29, 1, 0, 0, TimeSpan.FromHours(1)));
        expectedDates.Add(new DateTimeOffset(2025, 3, 31, 2, 0, 0, TimeSpan.FromHours(2)));
        expectedDates.Add(new DateTimeOffset(2025, 4, 2, 2, 0, 0, TimeSpan.FromHours(2)));
        expectedDates.Add(new DateTimeOffset(2025, 4, 4, 2, 0, 0, TimeSpan.FromHours(2)));

        Assert.Equal(expectedDates, result.Value!.FutureDates);
    }


}

