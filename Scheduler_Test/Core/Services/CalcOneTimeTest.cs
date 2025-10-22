using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;
using Xunit.Abstractions;
// ReSharper disable UseObjectOrCollectionInitializer

namespace Scheduler_Lib.Core.Services;

public class CalcOneTimeTest(ITestOutputHelper output) {
    [Theory]
    [InlineData("2025-12-31", "2025-01-01", "2025-01-01", Messages.ErrorTargetDateAfterEndDate)]
    [InlineData("2025-01-01", "2025-12-30", "2024-12-31", Messages.ErrorTargetDateAfterEndDate)]
    [InlineData("2025-01-01", "2025-12-31", null, Messages.ErrorTargetDateNull)]
    public void ValidateOnce_ShouldFail_WhenInvalidDates(string startDate, string endDate, string? targetDate, string expectedError) {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput();

        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(
            DateTime.Parse(startDate),
            tz.GetUtcOffset(DateTime.Parse(startDate))
        );
        schedulerInput.EndDate = new DateTimeOffset(
            DateTime.Parse(startDate),
            tz.GetUtcOffset(DateTime.Parse(startDate))
        );
        schedulerInput.TargetDate = targetDate != null ? DateTimeOffset.Parse(targetDate) : null;
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = CalculateOneTime.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "Success");

        Assert.False(result.IsSuccess);
        Assert.Contains(expectedError, result.Error);
    }

    [Theory]
    [InlineData("2025-01-01", "2025-12-31", "2025-01-01")]
    [InlineData("2025-01-01", "2025-12-31", "2025-12-31")]
    public void ValidateOnce_ShouldSuccess_WhenValidTargetDate(string startDate, string endDate, string targetDate) {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput();

        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(
            DateTime.Parse(startDate),
            tz.GetUtcOffset(DateTime.Parse(startDate))
        );
        schedulerInput.EndDate = new DateTimeOffset(
            DateTime.Parse(endDate),
            tz.GetUtcOffset(DateTime.Parse(endDate))
        );
        schedulerInput.TargetDate = new DateTimeOffset(
            DateTime.Parse(targetDate),
            tz.GetUtcOffset(DateTime.Parse(targetDate))
        );
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = CalculateOneTime.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "NO ERROR");
        output.WriteLine(result.Value.Description);

        if (result.Value.FutureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {result.Value.FutureDates.Count}):");
            foreach (var dto in result.Value.FutureDates) {
                output.WriteLine(dto.ToString());
            }
        }

        var expectedNewDate = schedulerInput.TargetDate;
        Assert.Equal(expectedNewDate, result.Value!.NextDate);
        var expectedResult =
            $"Occurs once: Schedule will be used on {expectedNewDate.Value.DateTime.ToShortDateString()} at " +
            $"{expectedNewDate.Value.DateTime.ToShortTimeString()} starting on " +
            $"{schedulerInput.StartDate.Date.ToShortDateString()}";
        Assert.Equal(expectedResult, result.Value.Description);
    }

    [Fact]
    public void CalculateOnce_ShouldFail_WhenRecurrencyIsNotWeeklyFutureDatesIsNull() {
        var schedulerInput = new SchedulerInput();

        var tz = RecurrenceCalculator.GetTimeZone();

        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput!.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 5, 0, 0, 0, DateTimeKind.Unspecified)));
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0,
            tz.GetUtcOffset(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)));
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.DailyPeriod = new TimeSpan(2, 0, 0, 0);

        var result = CalculateOneTime.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "NO ERROR");

        Assert.Null(result.Value!.FutureDates);
    }

    [Fact]
    public void ValidateOnce_ShouldFail_WhenTargetDateNullAndNotWeekly() {
        var schedulerInput = new SchedulerInput();

        var tz = RecurrenceCalculator.GetTimeZone();

        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, 
            tz.GetUtcOffset(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)));
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.TargetDate = null;

        var result = CalculateOneTime.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "NO ERROR");

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains(Messages.ErrorTargetDateNull, result.Error!);
    }
}