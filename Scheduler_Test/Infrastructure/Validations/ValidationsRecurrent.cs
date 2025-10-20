using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;
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
        schedulerInput.Period = periodDays.HasValue ? TimeSpan.FromDays(periodDays.Value) : null;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];
        schedulerInput.WeeklyPeriod = 1;

        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        output.WriteLine(result.Error ?? "Success");

        Assert.False(result.IsSuccess);
        Assert.Contains(expectedError, result.Error);
    }

    [Theory]
    [InlineData("2025-12-31", "2025-01-01", 14, 8, Messages.ErrorStartDatePostEndDate)]
    [InlineData("2025-01-01", "2025-12-31", 8, 14, Messages.ErrorDailyStartAfterEnd)]
    public void ValidateRecurrent_ShouldFail_WhenInvalidDateAndTimeRanges(string startDate, string? endDate, int startTime, int endTime, string expectedError) {
        var schedulerInput = new SchedulerInput();

        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = DateTimeOffset.Parse(startDate);
        schedulerInput.EndDate = endDate != null ? DateTimeOffset.Parse(endDate) : null;
        schedulerInput.Period = TimeSpan.FromDays(1);
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DailyStartTime = TimeSpan.FromHours(startTime);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(endTime);

        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        output.WriteLine(result.Error ?? "Success");

        Assert.False(result.IsSuccess);
        Assert.Contains(expectedError, result.Error);
    }

    [Theory]
    [InlineData(null, Messages.ErrorDaysOfWeekRequired)]
    [InlineData(new DayOfWeek[0], Messages.ErrorDaysOfWeekRequired)]
    public void ValidateRecurrent_ShouldFail_InvalidDaysOfWeek(DayOfWeek[]? daysOfWeek, string expectedError) {
        var schedulerInput = new SchedulerInput();

        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Period = TimeSpan.FromDays(1);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.DaysOfWeek = daysOfWeek?.ToList();
        schedulerInput.WeeklyPeriod = 1;

        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        output.WriteLine(result.Error ?? "Success");

        Assert.False(result.IsSuccess);
        Assert.Contains(expectedError, result.Error ?? string.Empty);
    }

    [Theory]
    [InlineData("2025-1-1", "2025-1-1", "2025-12-31")]
    [InlineData("2025-1-1", "2025-1-1", "2025-1-1")]
    [InlineData("2025-1-1", "2025-1-1", null)]
    public void ValidateRecurrent_ShouldSuccess_AllDataCorrect(string currentDate, string startDate, string endDate) {
        var schedulerInput = new SchedulerInput();

        schedulerInput.CurrentDate = DateTimeOffset.Parse(currentDate);
        schedulerInput.StartDate = DateTimeOffset.Parse(startDate); 
        schedulerInput.EndDate = endDate != null ? DateTimeOffset.Parse(endDate) : null;
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Wednesday];
        schedulerInput.DailyStartTime = TimeSpan.FromHours(8);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(17);
        schedulerInput.Period = new TimeSpan(2, 0, 0);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;

        var result = ValidationRecurrent.ValidateRecurrent(schedulerInput);

        output.WriteLine(result.IsSuccess.ToString());

        Assert.True(result.IsSuccess);
    }

}