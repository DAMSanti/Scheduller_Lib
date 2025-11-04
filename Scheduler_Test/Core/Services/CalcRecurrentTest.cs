using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services.Strategies;
using Scheduler_Lib.Resources;
using Xunit.Abstractions;
// ReSharper disable UseObjectOrCollectionInitializer

namespace Scheduler_Lib.Core.Services;
public class CalculateRecurrentTests(ITestOutputHelper output) {
    [Theory]
    [InlineData( null, "2025-10-03T10:00:00", "2025-10-03T10:00:00", "2025-10-03T10:00:00", null)]
    [InlineData("2025-10-05T08:30:00", "2025-10-03T10:00:00", "2025-10-05T08:30:00", "2025-10-03T10:00:00", null)]
    [InlineData("2025-10-05T08:30:00", "2025-10-03T10:00:00", "2025-10-05T08:30:00", "2025-10-03T10:00:00", "2025-10-10T10:00:00")]
    public void CalculateDailyRecurrent_ShouldSuccess_WhenNextDateIsCorrect(string? targetDate, string currentDate, string expectedNextDate, string? startDate, string? endDate) {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.OccursOnceChk = false;
        schedulerInput.OccursEveryChk = true;
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

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine(dto.ToString());
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Equal(DateTimeOffset.Parse(expectedNextDate), result.Value!.NextDate);
    }


    [Theory]
    [InlineData("2025-10-02", "2025-10-01", "2025-11-25", EnumRecurrency.Weekly, "2025-10-06")]
    [InlineData("2025-10-03", "2025-10-01", "2025-11-25", EnumRecurrency.Daily, "2025-10-03")]
    public void CalculateNextDate_ShouldSuccess_WhenDateMatchesExpected(string currentDate, string startDate, string endDate, EnumRecurrency recurrency, string expectedNextDate) {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.OccursOnceChk = false;
        schedulerInput.OccursEveryChk = true;
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

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine(dto.ToString());
            }
        }

        Assert.Equal(DateTimeOffset.Parse(expectedNextDate), result.Value.NextDate);
    }

    [Fact]
    public void CalculateDailyRecurrent_ShouldFail_WhenPeriodIsNullOrNonPositive() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.OccursOnceChk = false;
        schedulerInput.OccursEveryChk = true;
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
    public void CalculateWeeklyRecurrent_ShouldSuccess_WhenNextDateIsCorrect(DayOfWeek[]? daysOfWeek, string currentDate, string expectedNextDate, string? startDate, string? endDate, string? targetDate) {
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

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine(dto.ToString());
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Equal(DateTimeOffset.Parse(expectedNextDate), result.Value!.NextDate);
    }

    [Fact]
    public void CalculateWeeklyRecurrent_ShouldFail_WhenDaysOfWeekIsNull() {
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
    public void CalculateWeeklyRecurrent_ShouldFail_WhenWeeklyPeriodIsNull() {
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
    public void CalculateDailyRecurrent_ShouldSuccess_WhenFutureDatesAreCalculated() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.OccursOnceChk = false;
        schedulerInput.OccursEveryChk = true;
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);

        var result = CalculateRecurrent.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is not { Count: > 0 }) return;
        output.WriteLine($"FutureDates (count = {futureDates.Count}):");
        foreach (var dto in futureDates) {
            output.WriteLine(dto.ToString());
        }

        Assert.NotNull(futureDates);
        Assert.True(futureDates.Count > 0);
    }

    [Fact]
    public void GenerateDescription_ShouldSuccess_WhenDescriptionMatchesExpected() {
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

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine(dto.ToString());
            }
        }

        Assert.Contains("Occurs every", result.Value.Description);
        Assert.Contains("week(s)", result.Value.Description);
    }

    [Theory]
    [InlineData(new[] { "2025-10-05T08:30:00", "2025-10-06T08:30:00" }, "2025-10-05T08:30:00")]
    [InlineData(new[] { "2025-10-05T08:30:00", "2025-10-05T08:30:00" }, "2025-10-05T08:30:00")]
    [InlineData(new[] { "2025-10-06T08:30:00" }, "2025-10-05T08:30:00")]
    [InlineData(new string[0], "2025-10-05T08:30:00")]
    public void CalculateRecurrent_ShouldSuccess_WhenNextDateIsRemovedFromFutureDates(string[] futureDatesArr, string nextDateStr) {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.OccursOnceChk = false;
        schedulerInput.OccursEveryChk = true;
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

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);
        Assert.True(result.IsSuccess);

        var value = result.Value!;

        var testFutureDates = futureDatesArr.Select(DateTimeOffset.Parse).ToList();
        testFutureDates.RemoveAll(d => d == value.NextDate);

        output.WriteLine($"NextDate: {value.NextDate}");
        output.WriteLine($"Test FutureDates after removal (count = {testFutureDates.Count}):");
        foreach (var dto in testFutureDates) {
            output.WriteLine(dto.ToString());
        }

        var actualFutureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        output.WriteLine($"Actual FutureDates from RecurrenceCalculator (count = {actualFutureDates.Count}):");
        foreach (var dto in actualFutureDates) {
            output.WriteLine(dto.ToString());
        }

        Assert.DoesNotContain(value.NextDate, actualFutureDates);
    }

    [Fact]
    public void CalculateRecurrent_ShouldSuccess_WhenOccursOnceAtIsUsedForNextDate() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 03, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 03)));
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursEveryChk = false;
        schedulerInput.OccursOnceAt = new TimeSpan(8, 30, 0);
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);

        var result = CalculateRecurrent.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        Assert.True(result.IsSuccess);
        Assert.Equal(8, result.Value!.NextDate.Hour);
        Assert.Equal(30, result.Value!.NextDate.Minute);
    }

    [Fact]
    public void CalculateRecurrent_ShouldSuccess_WhenTargetDateIsPresent() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var targetLocal = new DateTime(2025, 10, 05, 9, 15, 0);

        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 03, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 03)));
        schedulerInput.OccursOnceChk = false;
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);
        schedulerInput.TargetDate = new DateTimeOffset(targetLocal, tz.GetUtcOffset(targetLocal));

        var result = CalculateRecurrent.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        Assert.True(result.IsSuccess);
        var expected = new DateTimeOffset(targetLocal, tz.GetUtcOffset(targetLocal));
        Assert.Equal(expected, result.Value!.NextDate);
    }

    [Fact]
    public void CalculateRecurrent_ShouldApplyOccursOnceAt_WhenWeeklyRecurrencyWithOccursOnce() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 03, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 03)));
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursEveryChk = false;
        schedulerInput.OccursOnceAt = new TimeSpan(14, 30, 0);
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Friday];
        schedulerInput.WeeklyPeriod = 1;

        var result = CalculateRecurrent.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        Assert.True(result.IsSuccess);
        Assert.Equal(14, result.Value!.NextDate.Hour);
        Assert.Equal(30, result.Value!.NextDate.Minute);
        Assert.Equal(0, result.Value!.NextDate.Second);
    }

    [Fact]
    public void CalculateRecurrent_ShouldApplyOccursOnceAt_WhenMonthlyRecurrencyWithOccursOnce() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 03, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 03)));
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursEveryChk = false;
        schedulerInput.OccursOnceAt = new TimeSpan(16, 45, 30);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 15;
        schedulerInput.MonthlyDayPeriod = 1;

        var result = CalculateRecurrent.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        Assert.True(result.IsSuccess);
        Assert.Equal(16, result.Value!.NextDate.Hour);
        Assert.Equal(45, result.Value!.NextDate.Minute);
        Assert.Equal(30, result.Value!.NextDate.Second);
    }

    [Fact]
    public void CalculateRecurrent_ShouldUseGetNextExecutionDate_WhenRecurrencyIsNotDailyWeeklyOrMonthly() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = (EnumRecurrency)999; // Valor no esperado para cubrir el else
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 03, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 03, 10, 0, 0)));
        schedulerInput.OccursOnceChk = false;
        schedulerInput.OccursEveryChk = true;

        var result = CalculateRecurrent.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        // El test debe fallar porque la recurrency no es soportada
        Assert.False(result.IsSuccess);
        Assert.Contains("Unsupported recurrency", result.Error);
    }
}