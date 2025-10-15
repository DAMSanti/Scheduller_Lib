using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Infrastructure.Validations;

public class ValidationsTest {
    [Fact]
    public void NullOffset_Recurrent_Invalid() {
        var requestedDate = new SchedulerInput {
            CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero),
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
            Period = null,
            Periodicity = EnumConfiguration.Recurrent
        };

        var result = Validations.ValidateRecurrent(requestedDate);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorPositiveOffsetRequired, result.Error);
    }

    [Fact]
    public void NegativeOffset_Recurrent_Invalid() {
        var requestedDate = new SchedulerInput {
            CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero),
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
            Period = TimeSpan.FromDays(-1),
            Periodicity = EnumConfiguration.Recurrent
        };

        var result = Validations.ValidateRecurrent(requestedDate);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorPositiveOffsetRequired, result.Error);
    }

    [Fact]
    public void Recurrent_NoOffset() {
        var requestedDate = new SchedulerInput {
            CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero),
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
            Period = null,
            Periodicity = EnumConfiguration.Recurrent
        };

        var result = Validations.ValidateRecurrent(requestedDate);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorPositiveOffsetRequired, result.Error);
    }

    [Fact]
    public void Once_ChangeDate_Null() {
        var requestedDate = new SchedulerInput {
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
            TargetDate = null,
            Periodicity = EnumConfiguration.OneTime
        };

        var result = Validations.ValidateOnce(requestedDate);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorOnceMode, result.Error);
        Assert.Contains(Messages.ErrorChangeDateNull, result.Error);
    }

    [Fact]
    public void Once_ChangeDate_OutOfRange() {
        var requestedDate = new SchedulerInput {
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
            TargetDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Periodicity = EnumConfiguration.OneTime
        };

        var result = Validations.ValidateOnce(requestedDate);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorChangeDateAfterEndDate, result.Error);
    }

    [Fact]
    public void Once_EndDate_Null() {
        var requestedDate = new SchedulerInput {
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = null,
            TargetDate = new DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero),
            Periodicity = EnumConfiguration.OneTime
        };

        var result = Validations.ValidateOnce(requestedDate);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorEndDateNull, result.Error);
    }

    [Fact]
    public void Recurrent_StartDateAfterEndDate() {
        var requestedDate = new SchedulerInput {
            CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero),
            StartDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Period = TimeSpan.FromDays(1),
            Periodicity = EnumConfiguration.Recurrent
        };

        var result = Validations.ValidateRecurrent(requestedDate);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorStartDatePostEndDate, result.Error);
    }
    [Fact]
    public void Once_StartDateAfterEndDate_ReturnsError() {
        var requestedDate = new SchedulerInput {
            StartDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            TargetDate = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero),
            Periodicity = EnumConfiguration.OneTime
        };

        var result = Validations.ValidateOnce(requestedDate);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorChangeDateAfterEndDate, result.Error);
    }

    [Fact]
    public void WeeklyPeriod_NullOrZeroOrNegative_ShouldFail() {
        var req1 = new SchedulerInput { WeeklyPeriod = null, DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday } };
        var req2 = new SchedulerInput { WeeklyPeriod = 0, DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday } };
        var req3 = new SchedulerInput { WeeklyPeriod = -1, DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday } };

        Assert.Contains(Messages.ErrorWeeklyPeriodRequired, ValidationsTest_InvokeWeekly(req1));
        Assert.Contains(Messages.ErrorWeeklyPeriodRequired, ValidationsTest_InvokeWeekly(req2));
        Assert.Contains(Messages.ErrorWeeklyPeriodRequired, ValidationsTest_InvokeWeekly(req3));
    }

    [Fact]
    public void DaysOfWeek_NullOrEmpty_ShouldFail() {
        var req1 = new SchedulerInput { WeeklyPeriod = 1, DaysOfWeek = null };
        var req2 = new SchedulerInput { WeeklyPeriod = 1, DaysOfWeek = new List<DayOfWeek>() };

        Assert.Contains(Messages.ErrorDaysOfWeekRequired, ValidationsTest_InvokeWeekly(req1));
        Assert.Contains(Messages.ErrorDaysOfWeekRequired, ValidationsTest_InvokeWeekly(req2));
    }

    [Fact]
    public void OnlyStartTimeOrEndTime_ShouldFail() {
        var req1 = new SchedulerInput { WeeklyPeriod = 1, DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday }, DailyStartTime = TimeSpan.FromHours(8), DailyEndTime = null };
        var req2 = new SchedulerInput { WeeklyPeriod = 1, DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday }, DailyStartTime = null, DailyEndTime = TimeSpan.FromHours(17) };

        Assert.Contains(Messages.ErrorDailyTimeWindowIncomplete, ValidationsTest_InvokeWeekly(req1));
        Assert.Contains(Messages.ErrorDailyTimeWindowIncomplete, ValidationsTest_InvokeWeekly(req2));
    }

    [Fact]
    public void StartTimeGreaterOrEqualToEndTime_ShouldFail() {
        var req1 = new SchedulerInput
        {
            WeeklyPeriod = 1,
            DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday },
            DailyStartTime = TimeSpan.FromHours(18),
            DailyEndTime = TimeSpan.FromHours(8)
        };
        var req2 = new SchedulerInput
        {
            WeeklyPeriod = 1,
            DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday },
            DailyStartTime = TimeSpan.FromHours(8),
            DailyEndTime = TimeSpan.FromHours(8)
        };

        Assert.Contains(Messages.ErrorDailyStartAfterEnd, ValidationsTest_InvokeWeekly(req1));
        Assert.Contains(Messages.ErrorDailyStartAfterEnd, ValidationsTest_InvokeWeekly(req2));
    }

    [Fact]
    public void AllValid_ShouldPass() {
        var req = new SchedulerInput
        {
            WeeklyPeriod = 1,
            DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday },
            DailyStartTime = TimeSpan.FromHours(8),
            DailyEndTime = TimeSpan.FromHours(17)
        };

        var result = ValidationsTest_InvokeWeeklyResult(req);
        Assert.True(result.IsSuccess);
    }

    private static string ValidationsTest_InvokeWeekly(SchedulerInput requestedDate) {
        var method = typeof(Validations).GetMethod("ValidateWeekly", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var result = (ResultPattern<bool>)method.Invoke(null, new object[] { requestedDate });
        return result.Error ?? "";
    }

    private static ResultPattern<bool> ValidationsTest_InvokeWeeklyResult(SchedulerInput requestedDate) {
        var method = typeof(Validations).GetMethod("ValidateWeekly", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        return (ResultPattern<bool>)method.Invoke(null, new object[] { requestedDate });
    }
}