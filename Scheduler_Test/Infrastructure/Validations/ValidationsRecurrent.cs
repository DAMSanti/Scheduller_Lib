using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Resources;
using System;
using Xunit.Abstractions;
// ReSharper disable UseObjectOrCollectionInitializer

namespace Scheduler_Lib.Infrastructure.Validations;

public class ValidationsRecurrent(ITestOutputHelper output) {
    [Theory]
    [InlineData(null, Messages.ErrorPositiveOffsetRequired)]
    [InlineData(-1.0, Messages.ErrorPositiveOffsetRequired)]
    public void ValidateRecurrent_ShouldFail_WhenInvalidPeriod(double? periodDays, string expectedError) {
        var schedulerInput = new SchedulerInput();

        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.DailyPeriod = periodDays.HasValue ? TimeSpan.FromDays(periodDays.Value) : null;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "Success");

        Assert.False(result.IsSuccess);
        Assert.Contains(expectedError, result.Error);
    }

    [Theory]
    [InlineData("2025-12-31", "2025-01-01", 8, 14, Messages.ErrorStartDatePostEndDate)]
    [InlineData("2025-01-01", "2025-12-31", 14, 8, Messages.ErrorDailyStartAfterEnd)]
    public void ValidateRecurrent_ShouldFail_WhenInvalidDateAndTimeRanges(string startDate, string? endDate, int startTime, int endTime, string expectedError) {
        var schedulerInput = new SchedulerInput();

        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = DateTimeOffset.Parse(startDate);
        schedulerInput.EndDate = DateTimeOffset.Parse(endDate!);
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.DailyStartTime = TimeSpan.FromHours(startTime);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(endTime);

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "Success");

        Assert.False(result.IsSuccess);
        Assert.Contains(expectedError, result.Error);
    }

    [Theory]
    [InlineData(null, Messages.ErrorDaysOfWeekRequired)]
    [InlineData(new DayOfWeek[0], Messages.ErrorDaysOfWeekRequired)]
    [InlineData(new[] { DayOfWeek.Monday , DayOfWeek.Monday}, Messages.ErrorDuplicateDaysOfWeek)]
    public void ValidateRecurrent_ShouldFail_WhenInvalidDaysOfWeek(DayOfWeek[]? daysOfWeek, string expectedError) {
        var schedulerInput = new SchedulerInput();

        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.DaysOfWeek = daysOfWeek?.ToList();
        schedulerInput.WeeklyPeriod = 1;

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "Success");

        Assert.False(result.IsSuccess);
        Assert.Contains(expectedError, result.Error ?? string.Empty);
    }

    [Theory]
    [InlineData("2025-1-1", "2025-1-1", "2025-12-31")]
    [InlineData("2025-1-1", "2025-1-1", "2025-1-1")]
    [InlineData("2025-1-1", "2025-1-1", null)]
    public void ValidateRecurrent_ShouldSuccess_WhenAllDataCorrect(string currentDate, string startDate, string? endDate) {
        var schedulerInput = new SchedulerInput();

        schedulerInput.CurrentDate = DateTimeOffset.Parse(currentDate);
        schedulerInput.StartDate = DateTimeOffset.Parse(startDate); 
        schedulerInput.EndDate = endDate != null ? DateTimeOffset.Parse(endDate) : null;
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Wednesday];
        schedulerInput.DailyStartTime = TimeSpan.FromHours(8);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(17);
        schedulerInput.DailyPeriod = new TimeSpan(2, 0, 0);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "NO ERROR");
        output.WriteLine(result.Value.Description);

        if (result.Value.FutureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {result.Value.FutureDates.Count}):");
            foreach (var dto in result.Value.FutureDates) {
                output.WriteLine(dto.ToString());
            }
        }

        Assert.True(result.IsSuccess);
    }
}