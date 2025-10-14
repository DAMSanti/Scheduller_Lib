using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;
using Xunit.Abstractions;

namespace Scheduler_Lib.Core.Services;
public class CalcOneTimeTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public CalcOneTimeTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void ChangeDate_OneTime() {
        var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var change = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        var timeZone = TimeZoneInfo.Local;

        RequestedDate requestedDate = new();
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
        RequestedDate requestedDate = new();
        requestedDate.Date = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Periodicity = EnumPeriodicity.OneTime;

        var preResult = new CalcOneTime();
        var result = preResult.CalculateDate(requestedDate);

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
        var start = new DateTime(2025, 10, 5);
        var result = InvokeNextWeekday(start, DayOfWeek.Wednesday);
        Assert.Equal(new DateTime(2025, 10, 8), result);
    }

    [Fact]
    public void CalculateWeeklyRecurrence_GeneratesCorrectDates() {
        var requestedDate = new RequestedDate();
        requestedDate.ChangeDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate.EndDate = new DateTimeOffset(2025, 10, 19, 0, 0, 0, TimeSpan.Zero);
        requestedDate.WeeklyPeriod = 1;
        requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday };
        requestedDate.TimeZonaId = TimeZoneInfo.Utc;

        var startDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);

        var result = InvokeCalculateWeeklyRecurrence(requestedDate, startDate);

        var expected = new List<DateTimeOffset>();
        
        expected.Add(new DateTimeOffset(2025, 10, 6, 0, 0, 0, TimeSpan.Zero));
        expected.Add(new DateTimeOffset(2025, 10, 8, 0, 0, 0, TimeSpan.Zero));
        expected.Add(new DateTimeOffset(2025, 10, 13, 0, 0, 0, TimeSpan.Zero));
        expected.Add(new DateTimeOffset(2025, 10, 15, 0, 0, 0, TimeSpan.Zero));

        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateWeeklyRecurrence_RespectsWeeklyPeriod() {
        var requestedDate = new RequestedDate();
        requestedDate.ChangeDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate.EndDate = new DateTimeOffset(2025, 10, 26, 0, 0, 0, TimeSpan.Zero);
        requestedDate.WeeklyPeriod = 2;
        requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };
        requestedDate.TimeZonaId = TimeZoneInfo.Utc;
        
        var startDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);

        var result = InvokeCalculateWeeklyRecurrence(requestedDate, startDate);

        var expected = new List<DateTimeOffset>();
        expected.Add(new DateTimeOffset(2025, 10, 6, 0, 0, 0, TimeSpan.Zero));
        expected.Add(new DateTimeOffset(2025, 10, 20, 0, 0, 0, TimeSpan.Zero));

        Assert.Equal(expected, result);
    }

    [Fact]
    public void FutureDates_IsNull_WhenOcurrenceIsNotWeekly() {
        var requestedDate = new RequestedDate();
        requestedDate.ChangeDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Ocurrence = 0;
        requestedDate.WeeklyPeriod = 1;
        requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };
        requestedDate.TimeZonaId = TimeZoneInfo.Utc;

        var result = new CalcOneTime().CalculateDate(requestedDate);
        Assert.Null(result.Value!.FutureDates);
    }

    [Fact]
    public void FutureDates_IsNull_WhenWeeklyPeriodIsNull() {
        var requestedDate = new RequestedDate();
        requestedDate.ChangeDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
            requestedDate.Ocurrence = EnumOcurrence.Weekly;
            requestedDate.WeeklyPeriod = null;
            requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };
            requestedDate.TimeZonaId = TimeZoneInfo.Utc;

        var result = new CalcOneTime().CalculateDate(requestedDate);
        Assert.Null(result.Value!.FutureDates);
    }

    [Fact]
    public void FutureDates_IsNull_WhenDaysOfWeekIsNullOrEmpty() {
        var requestedDate1 = new RequestedDate();
        requestedDate1.ChangeDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate1.Ocurrence = EnumOcurrence.Weekly;
        requestedDate1.WeeklyPeriod = 1;
        requestedDate1.DaysOfWeek = null;
        requestedDate1.TimeZonaId = TimeZoneInfo.Utc;

        var requestedDate2 = new RequestedDate();
        requestedDate2.ChangeDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate2.Ocurrence = EnumOcurrence.Weekly;
        requestedDate2.WeeklyPeriod = 1;
        requestedDate2.DaysOfWeek = new List<DayOfWeek>();
        requestedDate2.TimeZonaId = TimeZoneInfo.Utc;

        var result1 = new CalcOneTime().CalculateDate(requestedDate1);
        var result2 = new CalcOneTime().CalculateDate(requestedDate2);

        Assert.Null(result1.Value!.FutureDates);
        Assert.Null(result2.Value!.FutureDates);
    }

    [Fact]
    public void FutureDates_IsNotNull_WhenAllWeeklyConditionsAreMet() {
        var requestedDate = new RequestedDate();
        requestedDate.ChangeDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Ocurrence = EnumOcurrence.Weekly;
        requestedDate.EndDate = new DateTimeOffset(2025, 10, 25, 0, 0, 0, TimeSpan.Zero);
        requestedDate.WeeklyPeriod = 1;
        requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };
        requestedDate.TimeZonaId = TimeZoneInfo.Utc;

        var result = new CalcOneTime().CalculateDate(requestedDate);
        Assert.NotNull(result.Value!.FutureDates);
        Assert.NotEmpty(result.Value!.FutureDates);
    }

    private static DateTime InvokeNextWeekday(DateTime start, DayOfWeek day) {
        var method = typeof(CalcOneTime).GetMethod("NextWeekday", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        return (DateTime)method.Invoke(null, new object[] { start, day });
    }

    private static List<DateTimeOffset> InvokeCalculateWeeklyRecurrence(RequestedDate requestedDate, DateTimeOffset startDate) {
        var method = typeof(CalcOneTime).GetMethod("CalculateWeeklyRecurrence", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        return (List<DateTimeOffset>)method.Invoke(null, new object[] { requestedDate, startDate });
    }
    /*
    [Fact]
    public void ChangeDate_OneTimeAA() {
        var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var change = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        var timeZone = TimeZoneInfo.Local;

        RequestedDate requestedDate = new();
        requestedDate.Date = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        requestedDate.StartDate = start;
        requestedDate.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        requestedDate.ChangeDate = change;
        requestedDate.Periodicity = EnumPeriodicity.OneTime;
        requestedDate.TimeZonaId = timeZone;

        requestedDate.Ocurrence = EnumOcurrence.Weekly;
        requestedDate.WeeklyPeriod = 1;
        requestedDate.DaysOfWeek = new List<DayOfWeek>();
        requestedDate.DaysOfWeek.Add(DayOfWeek.Monday);
        requestedDate.DaysOfWeek.Add(DayOfWeek.Wednesday);
        requestedDate.DaysOfWeek.Add(DayOfWeek.Friday);
        requestedDate.TimeZonaId = TimeZoneInfo.Utc;
        requestedDate.DailyStartTime = new TimeSpan(4,0,0);
        requestedDate.DailyEndTime = new TimeSpan(22,0,0);

        var preResult = new CalcOneTime();
        var result = preResult.CalculateDate(requestedDate);

        _testOutputHelper.WriteLine("Descripción generada: " + result.Value!.Description);

        var offset = timeZone.GetUtcOffset(result.Value!.NewDate.DateTime);
        _testOutputHelper.WriteLine("UTC Offset aplicado: " + offset);

        var newDateWithOffset = new DateTimeOffset(result.Value!.NewDate.DateTime, offset);
        _testOutputHelper.WriteLine("NewDate con offset aplicado: " + newDateWithOffset);

        var expectedNewDate = new DateTimeOffset(change.DateTime, timeZone.GetUtcOffset(change.DateTime));
        Assert.Equal(expectedNewDate, result.Value!.NewDate);
        var expectedResult = $"Occurs once: Schedule will be used on {expectedNewDate.Date.ToShortDateString()} at {expectedNewDate.Date.ToShortTimeString()} starting on {requestedDate.StartDate.Date.ToShortDateString()}";
        Assert.Equal(expectedResult, result.Value.Description);
    }
    */
}
