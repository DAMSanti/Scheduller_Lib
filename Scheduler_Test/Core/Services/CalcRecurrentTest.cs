using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Services;
public class CalcRecurrentTest {
    private readonly RequestedDate? _requestedDate = new();

    [Fact]
    public void OffSet_Recurrent_Valid() {
        var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        _requestedDate!.Date = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.StartDate = start;
        _requestedDate.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.Period = TimeSpan.FromHours(1);
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
        _requestedDate!.Date = new DateTimeOffset(y, m, d, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.Period = TimeSpan.FromHours(1);
        _requestedDate.Periodicity = EnumPeriodicity.Recurrent;

        var preResult = new CalcRecurrent();
        var result = preResult.CalculateDate(_requestedDate);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorDateOutOfRange, result.Error);
    }

    [Fact]
    public void CalcDate_FutureDates() {
        _requestedDate!.Date = new DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.EndDate = new DateTimeOffset(2025, 1, 15, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.Period = TimeSpan.FromHours(1);
        _requestedDate.Periodicity = EnumPeriodicity.Recurrent;
        _requestedDate.WeeklyPeriod = 1;
        _requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };
        _requestedDate.DailyStartTime = TimeSpan.FromHours(8);
        _requestedDate.DailyEndTime = TimeSpan.FromHours(11);

        var preResult = new CalcRecurrent();
        var result = preResult.CalculateDate(_requestedDate);

        var expectedDates = new List<DateTimeOffset>();
        expectedDates.Add(new DateTimeOffset(2025, 1, 6, 8, 0, 0, TimeSpan.Zero));
        expectedDates.Add(new DateTimeOffset(2025, 1, 6, 9, 0, 0, TimeSpan.Zero));
        expectedDates.Add(new DateTimeOffset(2025, 1, 6, 10, 0, 0, TimeSpan.Zero));
        expectedDates.Add(new DateTimeOffset(2025, 1, 6, 11, 0, 0, TimeSpan.Zero));
        expectedDates.Add(new DateTimeOffset(2025, 1, 13, 8, 0, 0, TimeSpan.Zero));
        expectedDates.Add(new DateTimeOffset(2025, 1, 13, 9, 0, 0, TimeSpan.Zero));
        expectedDates.Add(new DateTimeOffset(2025, 1, 13, 10, 0, 0, TimeSpan.Zero));
        expectedDates.Add(new DateTimeOffset(2025, 1, 13, 11, 0, 0, TimeSpan.Zero));

        Assert.Equal(expectedDates, result.Value!.FutureDates);
    }

    [Fact]
    public void CalcDate_FutureDates_noOffset() {
        _requestedDate.Date = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.EndDate = null;
        _requestedDate.Period = TimeSpan.FromHours(3);
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
        _requestedDate.Period = TimeSpan.FromHours(3);
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
        _requestedDate.Period = TimeSpan.FromHours(1);
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
        _requestedDate.Period = TimeSpan.FromHours(1);
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
        _requestedDate.EndDate = new DateTimeOffset(2025, 4, 30, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.Period = TimeSpan.FromHours(2);
        _requestedDate.Periodicity = EnumPeriodicity.Recurrent;
        _requestedDate.WeeklyPeriod = 1;
        _requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };
        _requestedDate.DailyStartTime = TimeSpan.FromHours(8);
        _requestedDate.DailyEndTime = TimeSpan.FromHours(11);

        var preResult = new CalcRecurrent();
        var result = preResult.CalculateDate(_requestedDate);

        var expectedDates = new List<DateTimeOffset>();
        expectedDates.Add(new DateTimeOffset(2025, 3, 23, 8, 0, 0, TimeSpan.Zero));
        expectedDates.Add(new DateTimeOffset(2025, 3, 23, 10, 0, 0, TimeSpan.Zero));

        expectedDates.Add(new DateTimeOffset(2025, 3, 30, 8, 0, 0, TimeSpan.Zero));
        expectedDates.Add(new DateTimeOffset(2025, 3, 23, 14, 0, 0, TimeSpan.Zero));
        expectedDates.Add(new DateTimeOffset(2025, 3, 23, 16, 0, 0, TimeSpan.Zero));

        Assert.Equal(expectedDates, result.Value!.FutureDates);
    }

    [Fact]
    public void CalculateFutureDates_UsesDefaultEndDate_WhenEndDateIsNull() {
        var requestedDate = new RequestedDate
        {
            Date = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero),
            StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = null,
            WeeklyPeriod = 1,
            DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday },
            Period = TimeSpan.FromHours(24),
            DailyStartTime = TimeSpan.FromHours(8),
            DailyEndTime = TimeSpan.FromHours(10)
        };

        var method = typeof(Scheduler_Lib.Core.Services.CalcRecurrent)
            .GetMethod("CalculateFutureDates", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var result = (List<DateTimeOffset>)method!.Invoke(null, new object[] { requestedDate })!;

        Assert.NotEmpty(result);
        Assert.All(result, d => Assert.True(d <= requestedDate.Date.Add(requestedDate.Period.Value * 3)));
    }

    [Fact]
    public void CalculateFutureDates_DayDateOutOfRange_IsSkipped()
    {
        var requestedDate = new RequestedDate
        {
            Date = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero),
            StartDate = new DateTimeOffset(2025, 10, 10, 0, 0, 0, TimeSpan.Zero), // después de Date
            EndDate = new DateTimeOffset(2025, 10, 12, 0, 0, 0, TimeSpan.Zero),
            WeeklyPeriod = 1,
            DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday },
            Period = TimeSpan.FromHours(24),
            DailyStartTime = TimeSpan.FromHours(8),
            DailyEndTime = TimeSpan.FromHours(10)
        };

        var method = typeof(Scheduler_Lib.Core.Services.CalcRecurrent)
            .GetMethod("CalculateFutureDates", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var result = (List<DateTimeOffset>)method!.Invoke(null, new object[] { requestedDate })!;

        Assert.Empty(result);
    }
    [Fact]
    public void CalculateFutureDates_SlotInRange_IsAdded()
    {
        var requestedDate = new RequestedDate
        {
            Date = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero),
            StartDate = new DateTimeOffset(2025, 10, 6, 8, 0, 0, TimeSpan.Zero), // lunes 8:00
            EndDate = new DateTimeOffset(2025, 10, 6, 10, 0, 0, TimeSpan.Zero), // lunes 10:00
            WeeklyPeriod = 1,
            DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday },
            Period = TimeSpan.FromHours(1),
            DailyStartTime = TimeSpan.FromHours(8),
            DailyEndTime = TimeSpan.FromHours(10)
        };

        var method = typeof(Scheduler_Lib.Core.Services.CalcRecurrent)
            .GetMethod("CalculateFutureDates", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var result = (List<DateTimeOffset>)method!.Invoke(null, new object[] { requestedDate })!;

        var expected = new List<DateTimeOffset>
        {
            new DateTimeOffset(2025, 10, 6, 8, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 10, 6, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 10, 6, 10, 0, 0, TimeSpan.Zero)
        };
        Assert.Equal(expected, result);
    }
}

