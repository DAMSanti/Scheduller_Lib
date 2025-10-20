using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;
using System.Reflection;

namespace Scheduler_Lib.Core.Services;
public class CalculateRecurrentTests
{
    [Fact]
    public void ValidateRecurrent_Fails_When_Period_IsNullOrNonPositive()
    {
        var input = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Recurrent,
            StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
            CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero),
            Recurrency = EnumRecurrency.Daily
        };

        var result = CalculateRecurrent.CalculateDate(input);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Contains(Messages.ErrorPositiveOffsetRequired, result.Error);
    }

    [Fact]
    public void Weekly_Recurrent_Calculates_NextDate_And_FutureDates()
    {
        var tz = RecurrenceCalculator.GetTimeZone();

        var input = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Recurrent,
            Period = TimeSpan.FromHours(2),
            StartDate = new DateTimeOffset(2025, 9, 1, 0, 30, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2026, 12, 31, 0, 0, 0, TimeSpan.Zero),
            CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero),
            Recurrency = EnumRecurrency.Weekly,
            WeeklyPeriod = 1,
            DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday },
            DailyStartTime = new TimeSpan(0, 30, 0),
            DailyEndTime = new TimeSpan(17, 30, 0)
        };

        var result = CalculateRecurrent.CalculateDate(input);

        Assert.True(result.IsSuccess);

        var baseDate = input.TargetDate ?? input.CurrentDate;
        var expectedNext = RecurrenceCalculator.SelectNextEligibleDate(baseDate, input.DaysOfWeek!, tz);

        Assert.Equal(expectedNext, result.Value!.NextDate);

        Assert.NotNull(result.Value.FutureDates);
        Assert.True(result.Value.FutureDates!.Count > 0);
    }

    [Fact]
    public void Daily_Recurrent_WithPeriod_Uses_CurrentDatePlusPeriod_As_Next()
    {
        var tz = RecurrenceCalculator.GetTimeZone();

        var current = new DateTimeOffset(2025, 10, 3, 10, 0, 0, TimeSpan.Zero);
        var period = TimeSpan.FromHours(3);

        var input = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Recurrent,
            Period = period,
            StartDate = current.AddDays(-1),
            EndDate = current.AddDays(10),
            CurrentDate = current,
            Recurrency = EnumRecurrency.Daily
        };

        var result = CalculateRecurrent.CalculateDate(input);

        Assert.True(result.IsSuccess);

        var nextLocal = input.CurrentDate.Add(input.Period!.Value);
        var expected = new DateTimeOffset(nextLocal.DateTime, tz.GetUtcOffset(nextLocal.DateTime));

        Assert.Equal(expected, result.Value!.NextDate);
    }

    [Fact]
    public void Daily_Recurrent_WithTargetDate_Uses_TargetDate_As_Next()
    {
        var tz = RecurrenceCalculator.GetTimeZone();

        var current = new DateTimeOffset(2025, 10, 3, 10, 0, 0, TimeSpan.Zero);
        var target = new DateTimeOffset(2025, 10, 5, 8, 30, 0, TimeSpan.Zero);

        var input = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Recurrent,
            Period = TimeSpan.FromHours(1),
            StartDate = current.AddDays(-1),
            EndDate = current.AddDays(10),
            CurrentDate = current,
            TargetDate = target,
            Recurrency = EnumRecurrency.Daily
        };

        var result = CalculateRecurrent.CalculateDate(input);

        Assert.True(result.IsSuccess);

        var expected = new DateTimeOffset(target.DateTime, tz.GetUtcOffset(target.DateTime));
        Assert.Equal(expected, result.Value!.NextDate);
    }

    [Fact]
    public void Daily_Recurrent_NoPeriodNoTarget_Uses_CurrentDate_As_Next()
    {
        var tz = RecurrenceCalculator.GetTimeZone();

        var current = new DateTimeOffset(2025, 10, 3, 9, 15, 0, TimeSpan.Zero);

        var input = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Recurrent,
            Period = TimeSpan.FromHours(1), 
            StartDate = current.AddDays(-1),
            EndDate = current.AddDays(2),
            CurrentDate = current,
            Recurrency = EnumRecurrency.Daily
        };

        input.TargetDate = null;

        var result = CalculateRecurrent.CalculateDate(input);

        Assert.True(result.IsSuccess);

        var expected = new DateTimeOffset(input.CurrentDate.DateTime, tz.GetUtcOffset(input.CurrentDate.DateTime));
        Assert.Equal(expected, result.Value!.NextDate);
    }

    [Fact]
    public void NonDailyNonWeekly_WithTarget_Uses_TargetDate_As_Next()
    {
        var tz = RecurrenceCalculator.GetTimeZone();

        var current = new DateTimeOffset(2025, 10, 3, 12, 0, 0, TimeSpan.Zero);
        var target = new DateTimeOffset(2025, 11, 1, 18, 0, 0, TimeSpan.Zero);

        var input = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Recurrent,
            Period = TimeSpan.FromHours(1),
            StartDate = current,
            EndDate = current.AddMonths(2),
            CurrentDate = current,
            TargetDate = target,
            Recurrency = EnumRecurrency.None
        };

        var result = CalculateRecurrent.CalculateDate(input);

        Assert.True(result.IsSuccess);

        var expected = new DateTimeOffset(target.DateTime, tz.GetUtcOffset(target.DateTime));
        Assert.Equal(expected, result.Value!.NextDate);
    }
    [Fact]
    public void Weekly_DaysOfWeek_Null_Defaults_To_BaseDay()
    {
        var tz = RecurrenceCalculator.GetTimeZone();

        var current = new DateTimeOffset(2025, 10, 3, 7, 0, 0, TimeSpan.Zero); // Friday
        var input = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Recurrent,
            Period = TimeSpan.FromHours(1),
            StartDate = current.AddDays(-1),
            EndDate = current.AddDays(30),
            CurrentDate = current,
            Recurrency = EnumRecurrency.Weekly,
            DaysOfWeek = null
        };

        var result = CalculateRecurrent.CalculateDate(input);

        Assert.True(result.IsSuccess);

        var baseDate = input.TargetDate ?? input.CurrentDate;
        var expectedNext = RecurrenceCalculator.SelectNextEligibleDate(baseDate, new List<DayOfWeek> { baseDate.DayOfWeek }, tz);

        Assert.Equal(expectedNext, result.Value!.NextDate);
    }

    [Fact]
    public void Weekly_DaysOfWeek_Empty_Returns_BaseDate_When_No_Candidates()
    {
        var tz = RecurrenceCalculator.GetTimeZone();

        var current = new DateTimeOffset(2025, 10, 3, 7, 0, 0, TimeSpan.Zero);
        var input = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Recurrent,
            Period = TimeSpan.FromHours(1),
            StartDate = current,
            EndDate = current.AddDays(10),
            CurrentDate = current,
            Recurrency = EnumRecurrency.Weekly,
            DaysOfWeek = new List<DayOfWeek>()
        };

        var result = CalculateRecurrent.CalculateDate(input);

        Assert.True(result.IsSuccess);

        var baseDate = input.TargetDate ?? input.CurrentDate;
        var expected = new DateTimeOffset(baseDate.DateTime, tz.GetUtcOffset(baseDate.DateTime));

        Assert.Equal(expected, result.Value!.NextDate);
    }

    [Fact]
    public void Weekly_With_TargetDate_Uses_TargetDate_As_Base_For_Next()
    {
        var tz = RecurrenceCalculator.GetTimeZone();

        var current = new DateTimeOffset(2025, 10, 1, 6, 0, 0, TimeSpan.Zero);
        var target = new DateTimeOffset(2025, 10, 6, 6, 0, 0, TimeSpan.Zero); // Monday
        var input = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Recurrent,
            Period = TimeSpan.FromHours(2),
            StartDate = current,
            EndDate = current.AddMonths(3),
            CurrentDate = current,
            TargetDate = target,
            Recurrency = EnumRecurrency.Weekly,
            DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday }
        };

        var result = CalculateRecurrent.CalculateDate(input);

        Assert.True(result.IsSuccess);

        var expectedNext = RecurrenceCalculator.SelectNextEligibleDate(input.TargetDate!.Value, input.DaysOfWeek!, tz);
        Assert.Equal(expectedNext, result.Value!.NextDate);
    }

    [Fact]
    public void Daily_With_DailyStartEnd_Generates_FutureDates_And_Next_Uses_Period()
    {
        var tz = RecurrenceCalculator.GetTimeZone();

        var start = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero);
        var current = new DateTimeOffset(2025, 10, 2, 8, 0, 0, TimeSpan.Zero);
        var input = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Recurrent,
            Period = TimeSpan.FromHours(4),
            StartDate = start,
            EndDate = start.AddDays(2),
            CurrentDate = current,
            Recurrency = EnumRecurrency.Daily,
            DailyStartTime = new TimeSpan(8, 0, 0),
            DailyEndTime = new TimeSpan(12, 0, 0),
            DailyFrequency = new TimeSpan(2, 0, 0)
        };

        var result = CalculateRecurrent.CalculateDate(input);

        Assert.True(result.IsSuccess);

        var nextLocal = input.CurrentDate.Add(input.Period!.Value);
        var expectedNext = new DateTimeOffset(nextLocal.DateTime, tz.GetUtcOffset(nextLocal.DateTime));
        Assert.Equal(expectedNext, result.Value!.NextDate);

        Assert.NotNull(result.Value.FutureDates);
        Assert.True(result.Value.FutureDates!.Count > 0);
    }

    [Fact]
    public void Once_WithTargetDate_Uses_Target_And_No_FutureDates()
    {
        var tz = RecurrenceCalculator.GetTimeZone();

        var current = new DateTimeOffset(2025, 10, 2, 9, 0, 0, TimeSpan.Zero);
        var target = new DateTimeOffset(2025, 10, 5, 14, 30, 0, TimeSpan.Zero);

        var input = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Once,
            StartDate = current,
            EndDate = current.AddMonths(1),
            CurrentDate = current,
            TargetDate = target,
            Recurrency = EnumRecurrency.None
        };

        var result = CalculateRecurrent.CalculateDate(input);

        Assert.True(result.IsSuccess);

        var expected = new DateTimeOffset(target.DateTime, tz.GetUtcOffset(target.DateTime));
        Assert.Equal(expected, result.Value!.NextDate);

        Assert.True(result.Value.FutureDates == null || result.Value.FutureDates.Count == 0);
    }

    [Fact]
    public void Daily_WithNoPeriod_And_TargetDate_Uses_TargetDate_As_Next_By_InvokePrivate()
    {
        var tz = RecurrenceCalculator.GetTimeZone();

        var current = new DateTimeOffset(2025, 10, 3, 10, 0, 0, TimeSpan.Zero);
        var target = new DateTimeOffset(2025, 10, 5, 8, 30, 0, TimeSpan.Zero);

        var input = new SchedulerInput
        {
            StartDate = current.AddDays(-1),
            EndDate = current.AddDays(10),
            CurrentDate = current,
            TargetDate = target,
            Recurrency = EnumRecurrency.Daily,
            Periodicity = EnumConfiguration.Recurrent
        };

        var method = typeof(CalculateRecurrent).GetMethod("BuildResultRecurrentDates", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var resultObj = method.Invoke(null, new object[] { input });
        Assert.NotNull(resultObj);

        var output = Assert.IsType<SchedulerOutput>(resultObj);

        var expected = new DateTimeOffset(target.DateTime, tz.GetUtcOffset(target.DateTime));
        Assert.Equal(expected, output.NextDate);
    }

    [Fact]
    public void Daily_WithNoPeriod_NoTarget_Uses_CurrentDate_As_Next_By_InvokePrivate()
    {
        var tz = RecurrenceCalculator.GetTimeZone();

        var current = new DateTimeOffset(2025, 10, 3, 09, 15, 0, TimeSpan.Zero);

        var input = new SchedulerInput
        {
            StartDate = current.AddDays(-1),
            EndDate = current.AddDays(2),
            CurrentDate = current,
            TargetDate = null,
            Recurrency = EnumRecurrency.Daily,
            Periodicity = EnumConfiguration.Recurrent
        };

        var method = typeof(CalculateRecurrent).GetMethod("BuildResultRecurrentDates", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var resultObj = method.Invoke(null, new object[] { input });
        Assert.NotNull(resultObj);

        var output = Assert.IsType<SchedulerOutput>(resultObj);

        var expected = new DateTimeOffset(current.DateTime, tz.GetUtcOffset(current.DateTime));
        Assert.Equal(expected, output.NextDate);
    }

    [Fact]
    public void NonDailyNonWeekly_NoTarget_Uses_CurrentDate_As_Next_PublicPath()
    {
        var tz = RecurrenceCalculator.GetTimeZone();

        var current = new DateTimeOffset(2025, 10, 3, 12, 0, 0, TimeSpan.Zero);

        var input = new SchedulerInput
        {
            Period = TimeSpan.FromHours(1),
            StartDate = current,
            EndDate = current.AddMonths(2),
            CurrentDate = current,
            TargetDate = null,
            Recurrency = EnumRecurrency.None,
            Periodicity = EnumConfiguration.Recurrent
        };

        var result = CalculateRecurrent.CalculateDate(input);
        Assert.True(result.IsSuccess);

        var expected = new DateTimeOffset(current.DateTime, tz.GetUtcOffset(current.DateTime));
        Assert.Equal(expected, result.Value!.NextDate);
    }
}