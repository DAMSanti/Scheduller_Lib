using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;
using Xunit.Abstractions;

namespace Scheduler_Lib.Core.Services;

public class CalcOneTimeTest(ITestOutputHelper output) {
    [Fact]
    public void CalculateOnce_ShouldSuccess_WhenTargetDatePresentOnceDaily() {
        var requestedDate = new SchedulerInput();

        var tz = TimeZoneInfo.FindSystemTimeZoneById(Config.TimeZoneId);

        requestedDate.StartDate = new DateTimeOffset(
            2025, 1, 1, 0, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)));
        requestedDate.TargetDate = new DateTimeOffset(
            2025, 10, 5, 0, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 5, 0, 0, 0, DateTimeKind.Unspecified)));
        requestedDate.Periodicity = EnumConfiguration.Once;
        requestedDate.Recurrency = EnumRecurrency.Daily;

        var result = CalculateOneTime.CalculateDate(requestedDate);

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
    public void CalculateOnce_ShouldFail_WhenRecurrencyIsNotWeeklyFutureDatesIsNull() {
        var requestedDate = new SchedulerInput();

        var tz = TimeZoneInfo.FindSystemTimeZoneById(Config.TimeZoneId);

        requestedDate!.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 5, 0, 0, 0, DateTimeKind.Unspecified)));
        requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)));
        requestedDate.Recurrency = EnumRecurrency.Daily;
        requestedDate.Periodicity = EnumConfiguration.Once;
        requestedDate.Period = new TimeSpan(2, 0, 0, 0);

        var result = CalculateOneTime.CalculateDate(requestedDate);

        output.WriteLine(result.Value.Description);

        Assert.Null(result.Value!.FutureDates);
    }

    [Fact]
    public void ValidateOnce_ShouldFail_WhenStartDateAfterEndDate() {
        var requestedDate = new SchedulerInput();

        var tz = TimeZoneInfo.FindSystemTimeZoneById(Config.TimeZoneId);

        requestedDate.StartDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, 
            tz.GetUtcOffset(new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Unspecified)));
        requestedDate.EndDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, 
            tz.GetUtcOffset(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)));
        requestedDate.Periodicity = EnumConfiguration.Once;
        requestedDate.Recurrency = EnumRecurrency.Daily;
        requestedDate.TargetDate = new DateTimeOffset(2025, 5, 1, 0, 0, 0, 
            tz.GetUtcOffset(new DateTime(2025, 5, 1, 0, 0, 0, DateTimeKind.Unspecified)));

        var result = CalculateOneTime.CalculateDate(requestedDate);

        output.WriteLine(result.IsSuccess.ToString());

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains(Messages.ErrorStartDatePostEndDate, result.Error!);
    }
    
    [Fact]
    public void ValidateOnce_ShouldFail_WhenTargetDateNullAndNotWeekly() {
        var requestedDate = new SchedulerInput();

        var tz = TimeZoneInfo.FindSystemTimeZoneById(Config.TimeZoneId);

        requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, 
            tz.GetUtcOffset(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)));
        requestedDate.Periodicity = EnumConfiguration.Once;
        requestedDate.Recurrency = EnumRecurrency.Daily;
        requestedDate.TargetDate = null;

        var result = CalculateOneTime.CalculateDate(requestedDate);

        output.WriteLine(result.IsSuccess.ToString());

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains(Messages.ErrorTargetDateNull, result.Error!);
    }

    [Fact]
    public void ValidateOnce_ShouldFail_WhenTargetDateOutsideRange() {
        var requestedDate = new SchedulerInput();

        var tz = TimeZoneInfo.FindSystemTimeZoneById(Config.TimeZoneId);

        requestedDate.StartDate = new DateTimeOffset(2025, 1, 10, 0, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 1, 10, 0, 0, 0, DateTimeKind.Unspecified)));
        requestedDate.EndDate = new DateTimeOffset(2025, 1, 31, 0, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 1, 31, 0, 0, 0, DateTimeKind.Unspecified)));
        requestedDate.TargetDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)));
        requestedDate.Periodicity = EnumConfiguration.Once;
        requestedDate.Recurrency = EnumRecurrency.Daily;

        var result = CalculateOneTime.CalculateDate(requestedDate);

        output.WriteLine(result.IsSuccess.ToString());

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains(Messages.ErrorTargetDateAfterEndDate, result.Error!);
    }
}