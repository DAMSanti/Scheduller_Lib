using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Services;
public class CalcOneTimeTest {
    private readonly RequestedDate? _requestedDate = new();

    [Fact]
    public void ChangeDate_OneTime() {
        var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var change = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);

        _requestedDate.Date = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.StartDate = start;
        _requestedDate.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.ChangeDate = change;
        _requestedDate.Periodicity = EnumPeriodicity.OneTime;
        _requestedDate.Ocurrence = EnumOcurrence.None;

        var preResult = new CalcOneTime();
        var result = preResult.CalculateDate(_requestedDate);

        var expectedNewDate = new DateTimeOffset(change.DateTime, TimeSpan.Zero);
        Assert.Equal(expectedNewDate, result.Value!.NewDate);
        var expectedResult = $"Occurs once: Schedule will be used on {expectedNewDate.Date.ToShortDateString()} at {expectedNewDate.Date.ToShortTimeString()} starting on {_requestedDate.StartDate.Date.ToShortDateString()}";
        Assert.Equal(expectedResult, result.Value.Description);
    }

    [Fact]
    public void NoChange_MissingData() {
        _requestedDate.Date = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.Periodicity = EnumPeriodicity.OneTime;

        var preResult = new CalcOneTime();
        var result = preResult.CalculateDate(_requestedDate);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorOnceMode, result.Error);
    }

    [Fact]
    public void NextWeekday_ReturnsSameDay_WhenStartIsTargetDay() {
        var start = new DateTime(2025, 10, 5);
        var result = InvokeNextWeekday(start, DayOfWeek.Sunday);
        Assert.Equal(start, result);
    }

    [Fact]
    public void NextWeekday_ReturnsNextCorrectDay() {
        var start = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        DateTimeOffset result = InvokeNextWeekday(start, DayOfWeek.Wednesday);
        Assert.Equal(new DateTimeOffset(2025, 10, 8, 0, 0, 0, TimeSpan.Zero), result);
    }

    [Fact]
    public void CalculateWeeklyRecurrence_GeneratesCorrectDates() {
        _requestedDate.ChangeDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.EndDate = new DateTimeOffset(2025, 10, 21, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.WeeklyPeriod = 1;
        _requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday };
        _requestedDate.StartDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);

        var result = InvokeCalculateWeeklyRecurrence(_requestedDate);

        var expected = new List<DateTimeOffset>();
        expected.Add(new DateTimeOffset(2025, 10, 6, 0, 0, 0, TimeSpan.Zero));
        expected.Add(new DateTimeOffset(2025, 10, 8, 0, 0, 0, TimeSpan.Zero));
        expected.Add(new DateTimeOffset(2025, 10, 13, 0, 0, 0, TimeSpan.Zero));
        expected.Add(new DateTimeOffset(2025, 10, 15, 0, 0, 0, TimeSpan.Zero));
        expected.Add(new DateTimeOffset(2025,10,20,0,0,0,TimeSpan.Zero));

        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateWeeklyRecurrence_RespectsWeeklyPeriod() {
        _requestedDate.ChangeDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.EndDate = new DateTimeOffset(2025, 10, 26, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.WeeklyPeriod = 2;
        _requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };
        _requestedDate.StartDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);

        var result = InvokeCalculateWeeklyRecurrence(_requestedDate);

        var expected = new List<DateTimeOffset>();
        expected.Add(new DateTimeOffset(2025, 10, 6, 0, 0, 0, TimeSpan.Zero));
        expected.Add(new DateTimeOffset(2025, 10, 20, 0, 0, 0, TimeSpan.Zero));

        Assert.Equal(expected, result);
    }

    [Fact]
    public void FutureDates_IsNull_WhenOcurrenceIsNotWeekly() {
        _requestedDate.ChangeDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.Ocurrence = 0;
        _requestedDate.WeeklyPeriod = 1;
        _requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };

        var result = new CalcOneTime().CalculateDate(_requestedDate);
        Assert.Null(result.Value!.FutureDates);
    }

    [Fact]
    public void FutureDates_IsNull_WhenWeeklyPeriodIsNull() {
        _requestedDate.ChangeDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.Ocurrence = EnumOcurrence.Weekly;
        _requestedDate.WeeklyPeriod = null;
        _requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };

        var result = new CalcOneTime().CalculateDate(_requestedDate);
        Assert.Null(result.Value!.FutureDates);
    }

    [Fact]
    public void FutureDates_IsNull_WhenDaysOfWeekIsNullOrEmpty() {
        var requestedDate1 = new RequestedDate();
        requestedDate1.ChangeDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate1.Ocurrence = EnumOcurrence.Weekly;
        requestedDate1.WeeklyPeriod = 1;
        requestedDate1.DaysOfWeek = null;

        var requestedDate2 = new RequestedDate();
        requestedDate2.ChangeDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate2.Ocurrence = EnumOcurrence.Weekly;
        requestedDate2.WeeklyPeriod = 1;
        requestedDate2.DaysOfWeek = new List<DayOfWeek>();

        var result1 = new CalcOneTime().CalculateDate(requestedDate1);
        var result2 = new CalcOneTime().CalculateDate(requestedDate2);

        Assert.Null(result1.Value!.FutureDates);
        Assert.Null(result2.Value!.FutureDates);
    }

    [Fact]
    public void FutureDates_IsNotNull_WhenAllWeeklyConditionsAreMet() {
        _requestedDate.ChangeDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.Ocurrence = EnumOcurrence.Weekly;
        _requestedDate.EndDate = new DateTimeOffset(2025, 10, 25, 0, 0, 0, TimeSpan.Zero);
        _requestedDate.WeeklyPeriod = 1;
        _requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };
        _requestedDate.DailyStartTime = TimeSpan.FromHours(8);
        _requestedDate.DailyEndTime = TimeSpan.FromHours(17);

        var result = new CalcOneTime().CalculateDate(_requestedDate);
        Assert.NotNull(result.Value!.FutureDates);
        Assert.NotEmpty(result.Value!.FutureDates);
    }

    private static DateTimeOffset InvokeNextWeekday(DateTimeOffset start, DayOfWeek day) {
        var method = typeof(CalcOneTime).GetMethod("NextWeekday", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        return (DateTimeOffset)method.Invoke(null, new object[] { start, day });
    }

    private static List<DateTimeOffset> InvokeCalculateWeeklyRecurrence(RequestedDate requestedDate) {
        var method = typeof(CalcOneTime).GetMethod("CalculateWeeklyRecurrence", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        return (List<DateTimeOffset>)method.Invoke(null, new object[] { requestedDate});
    }
}
