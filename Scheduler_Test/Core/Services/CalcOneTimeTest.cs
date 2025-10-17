using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;
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
        var expectedResult =
            $"Occurs once: Schedule will be used on {expectedNewDate.Value.DateTime.ToShortDateString()} at " +
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
            tz.GetUtcOffset(new DateTime(2025, 10, 6, 0, 0, 0, DateTimeKind.Unspecified)));
        Assert.Equal(expectedNewDate, result.Value!.NextDate);
        var expectedResult =
            $"Occurs every Monday, Wednesday: Schedule will be used on {expectedNewDate.DateTime.ToShortDateString()} " +
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

        var expectedDescription =
            $"Occurs every Monday, Wednesday: Schedule will be used on {expected.DateTime.ToShortDateString()} " +
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

    [Fact]
    public void CalculateOnceWeekly_ShouldSuccess_WhenTargetDatePresentShouldReturnNextCalculatedAndFutureDatesNotNull() {
        var requestedDate = new SchedulerInput();

        var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");

        requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.TargetDate = new DateTimeOffset(2025, 10, 4, 0, 0, 0, TimeSpan.Zero);
        requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };
        requestedDate.Periodicity = EnumConfiguration.Once;
        requestedDate.Recurrency = EnumRecurrency.Weekly;
        requestedDate.EndDate = requestedDate.TargetDate.Value.AddMonths(2);
        requestedDate.WeeklyPeriod = 1;

        var result = new CalculateOneTime().CalculateDate(requestedDate);

        output.WriteLine(result.Value.Description);

        if (result.Value.FutureDates != null) {
            output.WriteLine("Fechas generadas:");
            foreach (var date in result.Value.FutureDates)
                output.WriteLine(date.ToString());
        }

        Assert.True(result.IsSuccess);

        var expectedNext = new DateTimeOffset(2025, 10, 6, 0, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 6, 0, 0, 0, DateTimeKind.Unspecified)));
        Assert.Equal(expectedNext, result.Value!.NextDate);

        Assert.NotNull(result.Value.FutureDates);
        Assert.True(result.Value.FutureDates!.Count > 0);

        var expectedDesc =
            $"Occurs every {string.Join(", ", requestedDate.DaysOfWeek!.Select(d => d.ToString()))}: Schedule will be used on " +
            $"{expectedNext.Date.ToShortDateString()} at {expectedNext.DateTime.ToShortTimeString()} starting on {requestedDate.StartDate.Date.ToShortDateString()}";
        Assert.Equal(expectedDesc, result.Value.Description);
    }

    [Fact]
    public void ValidateOnce_ShouldFail_WhenStartDateAfterEndDate() {
        var requestedDate = new SchedulerInput();

        var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");

        requestedDate.StartDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        requestedDate.EndDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Periodicity = EnumConfiguration.Once;
        requestedDate.Recurrency = EnumRecurrency.Daily;
        requestedDate.TargetDate = new DateTimeOffset(2025, 5, 1, 0, 0, 0, TimeSpan.Zero);

        var result = new CalculateOneTime().CalculateDate(requestedDate);

        output.WriteLine(result.IsSuccess.ToString());

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains(Messages.ErrorStartDatePostEndDate, result.Error!);
    }
    
    [Fact]
    public void ValidateOnce_ShouldFail_WhenTargetDateNullAndNotWeekly() {
        var requestedDate = new SchedulerInput();

        requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Periodicity = EnumConfiguration.Once;
        requestedDate.Recurrency = EnumRecurrency.Daily; // not weekly
        requestedDate.TargetDate = null;

        var result = new CalculateOneTime().CalculateDate(requestedDate);

        output.WriteLine(result.IsSuccess.ToString());

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains(Messages.ErrorTargetDateNull, result.Error!);
    }

    [Fact]
    public void ValidateOnce_ShouldFail_WhenTargetDateOutsideRange() {
        var requestedDate = new SchedulerInput();

        var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");

        requestedDate.StartDate = new DateTimeOffset(2025, 1, 10, 0, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 1, 10, 0, 0, 0, DateTimeKind.Unspecified)));
        requestedDate.EndDate = new DateTimeOffset(2025, 1, 31, 0, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 1, 31, 0, 0, 0, DateTimeKind.Unspecified)));
        requestedDate.TargetDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)));
        requestedDate.Periodicity = EnumConfiguration.Once;
        requestedDate.Recurrency = EnumRecurrency.Daily;

        var result = new CalculateOneTime().CalculateDate(requestedDate);

        output.WriteLine(result.IsSuccess.ToString());

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains(Messages.ErrorTargetDateAfterEndDate, result.Error!);
    }
}