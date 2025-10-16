using Scheduler_Lib.Core.Model;
using Xunit.Abstractions;

namespace Scheduler_Lib.Core.Services;
public class CalcOneTimeTest(ITestOutputHelper output) {

    [Fact]
    public void CalculateOnce_ShouldSuccess_WhenTargetDatePresentOnceDaily() {
        var requestedDate = new SchedulerInput();

        var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");

        requestedDate.StartDate = new DateTimeOffset(
            2025, 1, 1, 0, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)));
        requestedDate.TargetDate = new DateTimeOffset(
            2025, 10, 5, 0, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 5, 0, 0, 0, DateTimeKind.Unspecified)));
        requestedDate.Periodicity = EnumConfiguration.Once;
        requestedDate.Recurrency = EnumRecurrency.Daily;

        var result = new CalculateOneTime().CalculateDate(requestedDate);

        output.WriteLine(result.Value.Description);

        var expectedNewDate = requestedDate.TargetDate;
        Assert.Equal(expectedNewDate, result.Value!.NextDate);
        var expectedResult = $"Occurs once: Schedule will be used on {expectedNewDate.Value.DateTime.ToShortDateString()} at " +
                             $"{expectedNewDate.Value.DateTime.ToShortTimeString()} starting on " +
                             $"{requestedDate.StartDate.Date.ToShortDateString()}";
        Assert.Equal(expectedResult, result.Value.Description);
    }

    [Fact]
    public void CalculateOnce_ShouldSuccess_WhenGeneratesCorrectDescription() {
        var requestedDate = new SchedulerInput();

        var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");

        requestedDate!.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, 
            tz.GetUtcOffset(new DateTime(2025, 10, 3, 0, 0, 0, DateTimeKind.Unspecified)));
        requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, 
            tz.GetUtcOffset(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)));
        requestedDate.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 5, 0, 0, 0, DateTimeKind.Unspecified)));
        requestedDate.Periodicity = EnumConfiguration.Once;
        requestedDate.Recurrency = EnumRecurrency.Weekly;
        requestedDate.WeeklyPeriod = 1;
        requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday };

        var result = new CalculateOneTime().CalculateDate(requestedDate);

        output.WriteLine(result.Value.Description);

        if (result.Value.FutureDates != null) {
            output.WriteLine("Fechas generadas:");
            foreach (var date in result.Value.FutureDates)
                output.WriteLine(date.ToString());
        }

        var expectedNewDate = new DateTimeOffset(2025, 10, 6, 0, 0, 0, 
            tz.GetUtcOffset(new DateTime(2025,10,6,0,0,0,DateTimeKind.Unspecified)));
        Assert.Equal(expectedNewDate, result.Value!.NextDate);
        var expectedResult = $"Occurs every Monday, Wednesday: Schedule will be used on {expectedNewDate.DateTime.ToShortDateString()} " +
                             $"at {expectedNewDate.DateTime.ToShortTimeString()} starting on " +
                             $"{requestedDate.StartDate.Date.ToShortDateString()}";
        Assert.Equal(expectedResult, result.Value.Description);
    }

    [Fact]
    public void CalculateOnce_ShouldSuccess_WhenGeneratesCorrectFutureDates() {
        var requestedDate = new SchedulerInput();

        var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");

        requestedDate.StartDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 5, 0, 0, 0, DateTimeKind.Unspecified)));
        requestedDate.EndDate = new DateTimeOffset(2025, 10, 21, 0, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 21, 0, 0, 0, DateTimeKind.Unspecified)));
        requestedDate.TargetDate = new DateTimeOffset(2025, 10, 7, 0, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 7, 0, 0, 0, DateTimeKind.Unspecified)));
        requestedDate.Periodicity = EnumConfiguration.Once;
        requestedDate.Recurrency = EnumRecurrency.Weekly;
        requestedDate.WeeklyPeriod = 1;
        requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday };

        var result = new CalculateOneTime().CalculateDate(requestedDate);

        output.WriteLine(result.Value.Description);

        if (result.Value.FutureDates != null) {
            output.WriteLine("Fechas generadas:");
            foreach (var date in result.Value.FutureDates)
                output.WriteLine(date.ToString());
        }

        var expected = new DateTimeOffset(2025, 10, 8, 0, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 8, 0, 0, 0, DateTimeKind.Unspecified)));

        var expectedList = new List<DateTimeOffset>();
        expectedList.Add(new DateTimeOffset(2025, 10, 13, 0, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 13, 0, 0, 0, DateTimeKind.Unspecified))));
        expectedList.Add(new DateTimeOffset(2025, 10, 15, 0, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 15, 0, 0, 0, DateTimeKind.Unspecified))));
        expectedList.Add(new DateTimeOffset(2025, 10, 20, 0, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 20, 0, 0, 0, DateTimeKind.Unspecified))));

        var expectedDescription = $"Occurs every Monday, Wednesday: Schedule will be used on {expected.DateTime.ToShortDateString()} " +
                             $"at {expected.DateTime.ToShortTimeString()} starting on " +
                             $"{requestedDate.StartDate.Date.ToShortDateString()}";

        Assert.Equal(expectedDescription, result.Value.Description);
        Assert.Equal(expected, result.Value.NextDate);
        Assert.Equal(expectedList, result.Value.FutureDates);
    }

    [Fact]
    public void CalculateOnce_ShouldFail_WhenRecurrencyIsNotWeeklyFutureDatesIsNull() {
        var requestedDate = new SchedulerInput();

        var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");

        requestedDate!.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 5, 0, 0, 0, DateTimeKind.Unspecified)));
        requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)));
        requestedDate.Recurrency = EnumRecurrency.Daily;
        requestedDate.Periodicity = EnumConfiguration.Once;
        requestedDate.Period = new TimeSpan(2, 0, 0, 0);

        var result = new CalculateOneTime().CalculateDate(requestedDate);

        output.WriteLine(result.Value.Description);

        Assert.Null(result.Value!.FutureDates);
    }
}