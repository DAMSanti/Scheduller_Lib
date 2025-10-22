using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;
using Xunit.Abstractions;
// ReSharper disable UseObjectOrCollectionInitializer

namespace Scheduler_Lib.Core.Services;
public class CalculateRecurrentTests(ITestOutputHelper output) {
    [Theory]
    [InlineData( null, "2025-10-03T10:00:00", "2025-10-03T10:00:00", "2025-10-03T10:00:00", null)]
    [InlineData("2025-10-05T08:30:00", "2025-10-03T10:00:00", "2025-10-05T08:30:00", "2025-10-03T10:00:00", null)]
    [InlineData("2025-10-05T08:30:00", "2025-10-03T10:00:00", "2025-10-05T08:30:00", "2025-10-03T10:00:00", "2025-10-10T10:00:00")]
    public void CalculateDailyRecurrent_ShouldSuccess_WhenNextDateCorrect(string? targetDate, string currentDate, string expectedNextDate, string? startDate, string? endDate) {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);
        schedulerInput.StartDate = startDate != null ? new DateTimeOffset(
            DateTime.Parse(startDate),
            tz.GetUtcOffset(DateTime.Parse(startDate))
        ) : default;
        schedulerInput.EndDate = endDate != null ? new DateTimeOffset(
            DateTime.Parse(endDate),
            tz.GetUtcOffset(DateTime.Parse(endDate))
        ) : null;
        schedulerInput.CurrentDate = new DateTimeOffset(
            DateTime.Parse(currentDate), 
            tz.GetUtcOffset(DateTime.Parse(currentDate))
        );

        schedulerInput.TargetDate = targetDate != null ? new DateTimeOffset(
            DateTime.Parse(targetDate),
            tz.GetUtcOffset(DateTime.Parse(targetDate))
        ) : null;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = CalculateRecurrent.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        if (result.Value.FutureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {result.Value.FutureDates.Count}):");
            foreach (var dto in result.Value.FutureDates) {
                output.WriteLine(dto.ToString());
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Equal(DateTimeOffset.Parse(expectedNextDate), result.Value!.NextDate);
    }


    [Theory]
    [InlineData("2025-10-02", "2025-10-01", "2025-11-25", EnumRecurrency.Weekly, "2025-10-06")]
    [InlineData("2025-10-03", "2025-10-01", "2025-11-25", EnumRecurrency.Daily, "2025-10-03")]
    public void CalculateNextDate_ShouldSuccess_WhenExpectedNextDate(string currentDate, string startDate, string endDate, EnumRecurrency recurrency, string expectedNextDate) {
        var schedulerInput = new SchedulerInput();
        schedulerInput.CurrentDate = DateTimeOffset.Parse(currentDate);
        schedulerInput.StartDate = DateTimeOffset.Parse(startDate);
        schedulerInput.EndDate = DateTimeOffset.Parse(endDate);
        schedulerInput.Recurrency = recurrency;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Wednesday];
        schedulerInput.WeeklyPeriod = 2;

        var result = CalculateRecurrent.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        if (result.Value.FutureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {result.Value.FutureDates.Count}):");
            foreach (var dto in result.Value.FutureDates) {
                output.WriteLine(dto.ToString());
            }
        }

        Assert.Equal(DateTimeOffset.Parse(expectedNextDate), result.Value.NextDate);
    }

    [Fact]
    public void CalculateDailyRecurrent_ShouldFail_WhenPeriodIsNullOrNonPositive() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = CalculateRecurrent.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorPositiveOffsetRequired, result.Error);
    }


    [Theory]
    [InlineData(new[] { DayOfWeek.Monday }, "2025-10-03T00:00:00", "2025-10-06T00:00:00", "2025-10-01T00:00:00", "2025-12-10T00:00:00", null)]
    [InlineData(new[] { DayOfWeek.Monday }, "2025-10-03T00:00:00", "2025-10-06T00:00:00", "2025-10-01T00:00:00", "2025-12-10T00:00:00", "2025-10-03T00:00:00")]
    public void CalculateWeeklyRecurrent_ShouldSuccess_WhenNextDateCorrect(DayOfWeek[]? daysOfWeek, string currentDate, string expectedNextDate, string? startDate, string? endDate, string? targetDate) {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput();
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = startDate != null ? new DateTimeOffset(
            DateTime.Parse(startDate),
            tz.GetUtcOffset(DateTime.Parse(startDate))
        ) : default;
        schedulerInput.EndDate = endDate != null ? new DateTimeOffset(
            DateTime.Parse(endDate),
            tz.GetUtcOffset(DateTime.Parse(endDate))
        ) : null;
        schedulerInput.CurrentDate = new DateTimeOffset(
            DateTime.Parse(currentDate),
            tz.GetUtcOffset(DateTime.Parse(currentDate))
        );
        schedulerInput.TargetDate = targetDate != null ? new DateTimeOffset(
            DateTime.Parse(targetDate),
            tz.GetUtcOffset(DateTime.Parse(targetDate))
        ) : null;
        schedulerInput.DaysOfWeek = daysOfWeek?.ToList();
        schedulerInput.WeeklyPeriod = 1;

        var result = CalculateRecurrent.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        if (result.Value.FutureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {result.Value.FutureDates.Count}):");
            foreach (var dto in result.Value.FutureDates) {
                output.WriteLine(dto.ToString());
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Equal(DateTimeOffset.Parse(expectedNextDate), result.Value!.NextDate);
    }

    [Fact]
    public void CalculateWeeklyRecurrent_ShouldFail_WhenDaysOfWeekNull() {
        var current = new DateTimeOffset(2025, 10, 3, 7, 0, 0, TimeSpan.Zero);
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.EndDate = current.AddDays(30);
        schedulerInput.CurrentDate = current;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.DaysOfWeek = null;
        schedulerInput.WeeklyPeriod = 2;

        var result = CalculateRecurrent.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorDaysOfWeekRequired, result.Error);
    }

    [Fact]
    public void CalculateWeeklyRecurrent_ShouldFail_WhenWeeklyPeriodNull() {
        var current = new DateTimeOffset(2025, 10, 3, 7, 0, 0, TimeSpan.Zero);
        var schedulerInput = new SchedulerInput();

        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.EndDate = current.AddDays(30);
        schedulerInput.CurrentDate = current;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];
        schedulerInput.WeeklyPeriod = null;

        var result = CalculateRecurrent.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorWeeklyPeriodRequired, result.Error);
    }

    [Fact]
    public void CalculateDailyRecurrent_ShouldSuccess_WhenFutureDates() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);

        var result = CalculateRecurrent.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        if (result.Value.FutureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {result.Value.FutureDates.Count}):");
            foreach (var dto in result.Value.FutureDates) {
                output.WriteLine(dto.ToString());
            }
        }

        Assert.NotNull(result.Value.FutureDates);
        Assert.True(result.Value.FutureDates.Count > 0);
    }

    [Fact]
    public void GenerateDescription_ShouldSuccess_WhenExpectedDescription() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Wednesday];

        var result = CalculateRecurrent.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        if (result.Value.FutureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {result.Value.FutureDates.Count}):");
            foreach (var dto in result.Value.FutureDates) {
                output.WriteLine(dto.ToString());
            }
        }

        Assert.Contains("Occurs every", result.Value.Description);
        Assert.Contains("week(s)", result.Value.Description);
    }

    [Theory]
    [InlineData(new[] { "2025-10-05T08:30:00", "2025-10-06T08:30:00" }, "2025-10-05T08:30:00", 1)]
    [InlineData(new[] { "2025-10-05T08:30:00", "2025-10-05T08:30:00" }, "2025-10-05T08:30:00", 0)]
    [InlineData(new[] { "2025-10-06T08:30:00" }, "2025-10-05T08:30:00", 1)]
    [InlineData(new string[0], "2025-10-05T08:30:00", 0)]
    public void CalculateRecurrent_ShouldSuccess_WhenRemovingNextDateFromFutureDates(string[] futureDatesArr, string nextDateStr, int expectedCount) {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);
        schedulerInput.StartDate =
            new DateTimeOffset(2025, 10, 1, 8, 30, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1, 8, 30, 0)));
        schedulerInput.EndDate =
            new DateTimeOffset(2025, 10, 10, 8, 30, 0, tz.GetUtcOffset(new DateTime(2025, 10, 10, 8, 30, 0)));
        schedulerInput.CurrentDate =
            new DateTimeOffset(2025, 10, 1, 8, 30, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1, 8, 30, 0)));
        schedulerInput.TargetDate = DateTimeOffset.Parse(nextDateStr);

        var result = CalculateRecurrent.CalculateDate(schedulerInput);

        var value = result.Value!;

        //value.FutureDates = futureDatesArr.Select(DateTimeOffset.Parse).ToList();
        value.FutureDates = [.. futureDatesArr.Select(DateTimeOffset.Parse)];

        value.FutureDates.RemoveAll(d => d == value.NextDate);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        if (value.FutureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {value.FutureDates.Count}):");
            foreach (var dto in value.FutureDates) {
                output.WriteLine(dto.ToString());
            }
        }

        Assert.Equal(expectedCount, value.FutureDates.Count);
    }
}