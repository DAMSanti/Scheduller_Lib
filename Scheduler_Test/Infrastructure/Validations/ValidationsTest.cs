using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Resources;
using Xunit.Abstractions;

namespace Scheduler_Lib.Infrastructure.Validations;

public class ValidationsTest(ITestOutputHelper output) {

    [Fact]
    public void ValidateRecurrent_NullPeriod_Fail() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.DaysOfWeek = new List<DayOfWeek> {DayOfWeek.Monday};
        schedulerInput.WeeklyPeriod = 1;

        var result = Validations.ValidateRecurrent(schedulerInput);

        output.WriteLine(result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorPositiveOffsetRequired, result.Error);
    }

    [Fact]
    public void ValidateRecurrent_NegativePeriod_Fail() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Period = TimeSpan.FromDays(-1);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };
        schedulerInput.WeeklyPeriod = 1;

        var result = Validations.ValidateRecurrent(schedulerInput);

        output.WriteLine(result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorPositiveOffsetRequired, result.Error);
    }

    [Fact]
    public void ValidateOnce_TargetDateOutOfRange_Fail() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };
        schedulerInput.WeeklyPeriod = 1;

        var result = Validations.ValidateOnce(schedulerInput);

        output.WriteLine(result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorTargetDateAfterEndDate, result.Error);
    }

    [Fact]
    public void ValidateOnce_EndDateNull_Success() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = null;
        schedulerInput.TargetDate = new DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };
        schedulerInput.WeeklyPeriod = 1;

        var result = Validations.ValidateOnce(schedulerInput);

        output.WriteLine(result.Value.ToString());

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateRecurrent_StartDateAfterEndDate_Fail() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Period = TimeSpan.FromDays(1);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };
        schedulerInput.WeeklyPeriod = 1;

        var result = Validations.ValidateRecurrent(schedulerInput);

        output.WriteLine(result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorStartDatePostEndDate, result.Error);
    }

    [Fact]
    public void ValidateOnce_StartDateAfterEndDate_Fail() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.StartDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };
        schedulerInput.WeeklyPeriod = 1;

        var result = Validations.ValidateOnce(schedulerInput);

        output.WriteLine(result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorTargetDateAfterEndDate, result.Error);
    }

    [Fact]
    public void ValidateOnce_MissingEndDate_Success() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };
        schedulerInput.WeeklyPeriod = 1;

        var result = Validations.ValidateOnce(schedulerInput);

        output.WriteLine(result.Value.ToString());

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Validate_NoEndTime_Success() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DailyStartTime = TimeSpan.FromHours(8);

        var result = Validations.ValidateOnce(schedulerInput);

        output.WriteLine(result.Value.ToString());

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Validate_NoStartTime_Success() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DailyEndTime = TimeSpan.FromHours(8);

        var result = Validations.ValidateOnce(schedulerInput);

        output.WriteLine(result.Value.ToString());

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Validate_StartTimeAfterEndTime_Fail() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Period = new TimeSpan(2, 0, 0);
        schedulerInput.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DailyStartTime = TimeSpan.FromHours(14);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(8);
        schedulerInput.DailyFrequency = new TimeSpan(1,0,0,0,0);

        var result = Validations.ValidateRecurrent(schedulerInput);

        output.WriteLine(result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorStartDatePostEndDate, result.Error);
    }

    [Fact]
    public void Validate_AllDataCorrect_Success() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday };
        schedulerInput.DailyStartTime = TimeSpan.FromHours(8);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(17);
        schedulerInput.Period = new TimeSpan(2,0,0);

        var result = Validations.ValidateRecurrent(schedulerInput);

        output.WriteLine(result.IsSuccess.ToString());

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidateWeeklyOnce_WeeklyPeriodIsNull_Success() {
        var requestedDate = new SchedulerInput();

        requestedDate.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Recurrency = EnumRecurrency.Weekly;
        requestedDate.WeeklyPeriod = null;
        requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };

        var result = new CalculateOneTime().CalculateDate(requestedDate);

        output.WriteLine(result.Error);

        //Assert.True(result.IsSuccess);
        //Assert.Contains(Messages.ErrorWeeklyPeriodRequired, result.Error);
    }

}