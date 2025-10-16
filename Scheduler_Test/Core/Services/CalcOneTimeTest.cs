using Scheduler_Lib.Core.Model;
using Xunit.Abstractions;

namespace Scheduler_Lib.Core.Services;
public class CalcOneTimeTest(ITestOutputHelper output) {

    [Fact]
    public void CalculateOnce_ShouldSuccess_WhenTargetDatePresentOnceDaily() {
        var requestedDate = new SchedulerInput();

        requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Periodicity = EnumConfiguration.Once;
        requestedDate.Recurrency = EnumRecurrency.Daily;

        var result = new CalculateOneTime().CalculateDate(requestedDate);

        output.WriteLine(result.Value.Description);

        var expectedNewDate = requestedDate.TargetDate;
        Assert.Equal(expectedNewDate, result.Value!.NextDate);
        var expectedResult = $"Occurs once: Schedule will be used on {expectedNewDate.Value.LocalDateTime.ToShortDateString()} at {expectedNewDate.Value.LocalDateTime.ToShortTimeString()} starting on {requestedDate.StartDate.Date.ToShortDateString()}";
        Assert.Equal(expectedResult, result.Value.Description);
    }

    [Fact]
    public void CalculateOnce_ShouldSuccess_WhenGeneratesCorrectDescription() {
        var requestedDate = new SchedulerInput();

        requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
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

        var expectedNewDate = new DateTimeOffset(2025, 10, 6, 0, 0, 0, TimeSpan.Zero);
        Assert.Equal(expectedNewDate, result.Value!.NextDate);
        var expectedResult = $"Occurs every Monday, Wednesday: Schedule will be used on 06/10/2025 " +
                             $"at {requestedDate.TargetDate!.Value.UtcDateTime.Date.ToShortTimeString()} starting on " +
                             $"{requestedDate.StartDate.Date.ToShortDateString()}";
        Assert.Equal(expectedResult, result.Value.Description);
    }

    [Fact]
    public void CalculateOnce_ShouldSuccess_WhenGeneratesCorrectFutureDates() {
        var requestedDate = new SchedulerInput();

        requestedDate.StartDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate.EndDate = new DateTimeOffset(2025, 10, 21, 0, 0, 0, TimeSpan.Zero);
        requestedDate.TargetDate = new DateTimeOffset(2025, 10, 7, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Periodicity = EnumConfiguration.Once;
        requestedDate.Recurrency = EnumRecurrency.Weekly;
        requestedDate.WeeklyPeriod = 1;
        requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday };

        var result = new CalculateOneTime().CalculateDate(requestedDate); 

        output.WriteLine(result.Value.NextDate.ToString());

        if (result.Value.FutureDates != null) {
            output.WriteLine("Fechas generadas:");
            foreach (var date in result.Value.FutureDates)
                output.WriteLine(date.ToString());
        }

        var expected = new DateTimeOffset(2025, 10, 8, 0, 0, 0, TimeSpan.Zero);

        var expectedList = new List<DateTimeOffset>();
        expectedList.Add(new DateTimeOffset(2025, 10, 13, 0, 0, 0, TimeSpan.Zero));
        expectedList.Add(new DateTimeOffset(2025, 10, 15, 0, 0, 0, TimeSpan.Zero));
        expectedList.Add(new DateTimeOffset(2025, 10, 20, 0, 0, 0, TimeSpan.Zero));

        Assert.Equal(expected, result.Value.NextDate);
        Assert.Equal(expectedList, result.Value.FutureDates);
    }

    [Fact]
    public void CalculateOnce_ShouldFail_WhenRecurrencyIsNotWeeklyFutureDatesIsNull() {
        var requestedDate = new SchedulerInput();

        requestedDate!.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Recurrency = EnumRecurrency.Daily;
        requestedDate.Periodicity = EnumConfiguration.Once;
        requestedDate.Period = new TimeSpan(2, 0, 0, 0);

        var result = new CalculateOneTime().CalculateDate(requestedDate);

        output.WriteLine(result.Value.Description);

        Assert.Null(result.Value!.FutureDates);
    }


    private static TimeZoneInfo GetTimeZone(SchedulerInput requestedDate) {
        return TimeZoneInfo.FindSystemTimeZoneById(requestedDate.TimeZoneId!);
    }
}