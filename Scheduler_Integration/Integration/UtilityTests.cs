using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Core.Services.Utilities;
using System.Reflection;
using Scheduler_Lib.Resources;
using Xunit;
using Xunit.Abstractions;
#pragma warning disable IDE0017

namespace Scheduler_IntegrationTests.Integration;

public class UtilitiesTests() {

    private static SchedulerInput CreateBaseValidRecurrentInput() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 15, 12, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 15, 12, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 1, 20, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);
        return schedulerInput;
    }

    [Fact, Trait("Category", "BaseDateTimeCalculator_Integration")]
    public void BaseDateTimeCalculator_ShouldReturnStartDate_WhenNoTargetAndCurrentIsDefault() {
        var tz = TimeZoneConverter.GetTimeZone();
        var schedulerInput = new SchedulerInput();
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 10, 8, 30, 0, tz.GetUtcOffset(new DateTime(2025, 10, 10)));

        var assembly = typeof(Scheduler_Lib.Core.Services.RecurrenceCalculator).Assembly;
        var type = assembly.GetType("Scheduler_Lib.Core.Services.Calculators.Base.BaseDateTimeCalculator");
        Assert.NotNull(type);

        var method = type!.GetMethod("GetBaseDateTime", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.NotNull(method);

        var result = (DateTime)method!.Invoke(null, [schedulerInput, tz])!;

        Assert.Equal(schedulerInput.StartDate.DateTime, result);
    }

    [Fact, Trait("Category", "DateSafetyHelper_Integration")]
    public void InitialHandler_ShouldCoverDateSafetyHelper_WhenDaysIsZero() {
        var schedulerInput = CreateBaseValidRecurrentInput();

        var result = SchedulerService.InitialOrchestator(schedulerInput);

        Assert.True(result.IsSuccess);
    }

    [Fact, Trait("Category", "DateSafetyHelper_Integration")]
    public void InitialHandler_ShouldCoverDateSafetyHelper_WhenPositiveDaysWouldOverflow() {
        var schedulerInput = CreateBaseValidRecurrentInput();

        var threshold = DateTime.MaxValue.AddDays(-5);
        var maxDate = new DateTimeOffset(threshold, TimeSpan.Zero);

        schedulerInput.StartDate = maxDate;
        schedulerInput.CurrentDate = maxDate;
        schedulerInput.EndDate = new DateTimeOffset(DateTime.MaxValue, TimeSpan.Zero);

        var result = SchedulerService.InitialOrchestator(schedulerInput);
        Assert.True(result.IsSuccess);
    }

    [Fact, Trait("Category", "DateSafetyHelper_Integration")]
    public void InitialHandler_ShouldCoverDateSafetyHelper_WhenPositiveDaysDoNotOverflow() {
        var schedulerInput = CreateBaseValidRecurrentInput();

        var startDate = new DateTime(2025, 1, 1, 10, 0, 0);
        var tz = TimeZoneConverter.GetTimeZone();

        schedulerInput.StartDate = new DateTimeOffset(startDate, tz.GetUtcOffset(startDate));
        schedulerInput.CurrentDate = new DateTimeOffset(startDate, tz.GetUtcOffset(startDate));
        schedulerInput.EndDate = new DateTimeOffset(startDate.AddDays(365), tz.GetUtcOffset(startDate.AddDays(365)));
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);

        var result = SchedulerService.InitialOrchestator(schedulerInput);

        Assert.True(result.IsSuccess);
    }
    
    [Fact, Trait("Category", "DateSafetyHelper_Integration")]
    public void InitialHandler_ShouldCoverDateSafetyHelper_WhenNegativeDaysWouldUnderflow() {
        var schedulerInput = CreateBaseValidRecurrentInput();

        var threshold = DateTime.MinValue.AddDays(5);
        var minDate = new DateTimeOffset(threshold, TimeSpan.Zero);

        schedulerInput.StartDate = minDate;
        schedulerInput.CurrentDate = minDate;
        schedulerInput.EndDate = new DateTimeOffset(threshold.AddDays(10), TimeSpan.Zero);

        var result = SchedulerService.InitialOrchestator(schedulerInput);
        Assert.True(result.IsSuccess);
    }

    [Fact, Trait("Category", "DateSafetyHelper_Integration")]
    public void InitialHandler_ShouldCoverDateSafetyHelper_WhenNegativeDaysDoNotUnderflow() {
        var schedulerInput = CreateBaseValidRecurrentInput();

        var tz = TimeZoneConverter.GetTimeZone();
        var startDate = new DateTime(2025, 6, 15, 10, 0, 0);

        schedulerInput.StartDate = new DateTimeOffset(startDate, tz.GetUtcOffset(startDate));
        schedulerInput.CurrentDate = new DateTimeOffset(startDate, tz.GetUtcOffset(startDate));
        schedulerInput.EndDate = new DateTimeOffset(startDate.AddMonths(6), tz.GetUtcOffset(startDate.AddMonths(6)));

        var result = SchedulerService.InitialOrchestator(schedulerInput);

        Assert.True(result.IsSuccess);
    }

    [Fact, Trait("Category", "DateSafetyHelper_Integration")]
    public void InitialHandler_ShouldCoverDateSafetyHelper_SuccessfulAddDaysPath() {
        var schedulerInput = CreateBaseValidRecurrentInput();
        var tz = TimeZoneConverter.GetTimeZone();

        var startDate = new DateTime(2025, 3, 15, 14, 45, 30);
        schedulerInput.StartDate = new DateTimeOffset(startDate, tz.GetUtcOffset(startDate));
        schedulerInput.CurrentDate = new DateTimeOffset(startDate, tz.GetUtcOffset(startDate));
        schedulerInput.EndDate = new DateTimeOffset(startDate.AddDays(90), tz.GetUtcOffset(startDate.AddDays(90)));
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(5);

        var result = SchedulerService.InitialOrchestator(schedulerInput);

        Assert.True(result.IsSuccess);
    }

    [Fact, Trait("Category", "DateSafetyHelper_Integration")]
    public void InitialHandler_ShouldCoverDateSafetyHelper_WithLeapYearBoundary() {
        var schedulerInput = CreateBaseValidRecurrentInput();
        var tz = TimeZoneConverter.GetTimeZone();

        var leapYearDate = new DateTime(2024, 2, 28, 10, 0, 0);
        schedulerInput.StartDate = new DateTimeOffset(leapYearDate, tz.GetUtcOffset(leapYearDate));
        schedulerInput.CurrentDate = new DateTimeOffset(leapYearDate, tz.GetUtcOffset(leapYearDate));
        schedulerInput.EndDate = new DateTimeOffset(leapYearDate.AddDays(5), tz.GetUtcOffset(leapYearDate.AddDays(5)));

        var result = SchedulerService.InitialOrchestator(schedulerInput);

        Assert.True(result.IsSuccess);
    }

    [Fact, Trait("Category", "DateSafetyHelper_Integration")]
    public void InitialHandler_ShouldCoverDateSafetyHelper_WithMonthBoundary() {
        var schedulerInput = CreateBaseValidRecurrentInput();
        var tz = TimeZoneConverter.GetTimeZone();

        var monthEndDate = new DateTime(2025, 1, 31, 10, 0, 0);
        schedulerInput.StartDate = new DateTimeOffset(monthEndDate, tz.GetUtcOffset(monthEndDate));
        schedulerInput.CurrentDate = new DateTimeOffset(monthEndDate, tz.GetUtcOffset(monthEndDate));
        schedulerInput.EndDate = new DateTimeOffset(monthEndDate.AddDays(10), tz.GetUtcOffset(monthEndDate.AddDays(10)));

        var result = SchedulerService.InitialOrchestator(schedulerInput);

        Assert.True(result.IsSuccess);
    }

    [Fact, Trait("Category", "DateSafetyHelper_Integration")]
    public void InitialHandler_ShouldCoverDateSafetyHelper_WithYearBoundary() {
        var schedulerInput = CreateBaseValidRecurrentInput();
        var tz = TimeZoneConverter.GetTimeZone();

        var yearEndDate = new DateTime(2025, 12, 31, 10, 0, 0);
        schedulerInput.StartDate = new DateTimeOffset(yearEndDate, tz.GetUtcOffset(yearEndDate));
        schedulerInput.CurrentDate = new DateTimeOffset(yearEndDate, tz.GetUtcOffset(yearEndDate));
        schedulerInput.EndDate = new DateTimeOffset(yearEndDate.AddDays(5), tz.GetUtcOffset(yearEndDate.AddDays(5)));

        var result = SchedulerService.InitialOrchestator(schedulerInput);

        Assert.True(result.IsSuccess);
    }

    [Fact, Trait("Category", "DateSafetyHelper_Integration")]
    public void DateSafetyHelper_ShouldReturnTrue_WhenAddingToMaxValueAtThreshold() {
        var date = DateTime.MaxValue.AddDays(-1);
        var result = DateSafetyHelper.TryAddDaysSafely(date, 1, out var resultDate);

        Assert.True(result);
        Assert.Equal(DateTime.MaxValue, resultDate);
    }

    [Fact, Trait("Category", "DateSafetyHelper_Integration")]
    public void DateSafetyHelper_ShouldReturnTrue_WhenSubtractingToMinValueAtThreshold() {
        var date = DateTime.MinValue.AddDays(1);
        var result = DateSafetyHelper.TryAddDaysSafely(date, -1, out var resultDate);

        Assert.True(result);
        Assert.Equal(DateTime.MinValue, resultDate);
    }

    [Fact, Trait("Category", "DateSafetyHelper_Integration")]
    public void DateSafetyHelper_ShouldReturnTrue_WhenDaysIsZero_DirectCall() {
        var dt = new DateTime(2025, 10, 10, 10, 0, 0);
        var success = DateSafetyHelper.TryAddDaysSafely(dt, 0, out var result);
        Assert.True(success);
        Assert.Equal(dt, result);
    }

    [Fact, Trait("Category", "DateSafetyHelper_Integration")]
    public void DateSafetyHelper_ShouldReturnFalse_WhenNegativeDaysUnderflow_DirectCall() {
        var dt = DateTime.MinValue.AddDays(1);
        var success = DateSafetyHelper.TryAddDaysSafely(dt, -5, out _);
        Assert.False(success);
    }

    [Fact, Trait("Category", "TimeZoneConverter_Integration")]
    public void InitialHandler_ShouldCoverTimeZoneConverter_ConvertToTimeZone() {
        var schedulerInput = CreateBaseValidRecurrentInput();

        var result = SchedulerService.InitialOrchestator(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.NextDate.Offset.TotalHours >= -14 &&
                   result.Value.NextDate.Offset.TotalHours <= 14);
    }

    [Fact, Trait("Category", "TimeZoneConverter_Integration")]
    public void InitialHandler_ShouldPreserveUTCTime_InTimeZoneConversion() {
        var schedulerInput = CreateBaseValidRecurrentInput();
        var tz = TimeZoneConverter.GetTimeZone();

        var result = SchedulerService.InitialOrchestator(schedulerInput);

        Assert.True(result.IsSuccess);

        var nextLocal = result.Value.NextDate.DateTime;
        var expectedOffset = tz.GetUtcOffset(nextLocal);

        Assert.Equal(expectedOffset, result.Value.NextDate.Offset);
    }

    [Fact, Trait("Category", "TimeZoneConverter_Integration")]
    public void InitialHandler_ShouldCalculateCorrectLocalTime_FromUTC() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 15, 12, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 15, 12, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 1, 20, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);

        var result = SchedulerService.InitialOrchestator(schedulerInput);
        Assert.True(result.IsSuccess);
    }

    [Fact, Trait("Category", "TimeZoneConverter_Integration")]
    public void InitialHandler_ShouldHandleDaylightSavingTime_InConversion() {
        var schedulerInput = CreateBaseValidRecurrentInput();

        
        var tz = TimeZoneConverter.GetTimeZone();

        
        var winterDate = new DateTime(2025, 1, 15, 12, 0, 0);
        schedulerInput.StartDate = new DateTimeOffset(winterDate, tz.GetUtcOffset(winterDate));
        schedulerInput.CurrentDate = new DateTimeOffset(winterDate, tz.GetUtcOffset(winterDate));
        schedulerInput.EndDate = new DateTimeOffset(winterDate.AddDays(7), tz.GetUtcOffset(winterDate.AddDays(7)));

        var winterResult = SchedulerService.InitialOrchestator(schedulerInput);
        Assert.True(winterResult.IsSuccess);

        
        var summerDate = new DateTime(2025, 7, 15, 12, 0, 0);
        schedulerInput.StartDate = new DateTimeOffset(summerDate, tz.GetUtcOffset(summerDate));
        schedulerInput.CurrentDate = new DateTimeOffset(summerDate, tz.GetUtcOffset(summerDate));
        schedulerInput.EndDate = new DateTimeOffset(summerDate.AddDays(7), tz.GetUtcOffset(summerDate.AddDays(7)));

        var summerResult = SchedulerService.InitialOrchestator(schedulerInput);
        Assert.True(summerResult.IsSuccess);
    }

    [Fact, Trait("Category", "TimeZoneConverter_Integration")]
    public void InitialHandler_ShouldHandleMaxDateTime_InTimeZoneConversion() {
        var schedulerInput = CreateBaseValidRecurrentInput();

        var maxDate = DateTime.MaxValue.AddDays(-10);
        schedulerInput.StartDate = new DateTimeOffset(maxDate, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(maxDate, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(DateTime.MaxValue, TimeSpan.Zero);

        var result = SchedulerService.InitialOrchestator(schedulerInput);
        Assert.True(result.IsSuccess);
    }

    [Fact, Trait("Category", "TimeZoneConverter_Integration")]
    public void InitialHandler_ShouldHandleMinDateTime_InTimeZoneConversion() {
        var schedulerInput = CreateBaseValidRecurrentInput();

        var minDate = DateTime.MinValue.AddDays(10);
        schedulerInput.StartDate = new DateTimeOffset(minDate, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(minDate, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(minDate.AddDays(20), TimeSpan.Zero);

        var result = SchedulerService.InitialOrchestator(schedulerInput);
        Assert.True(result.IsSuccess);
    }
    
    [Fact, Trait("Category", "TimeZoneConverter_Integration")]
    public void InitialHandler_ShouldCoverTimeZoneConverter_GetUtcOffset() {
        var schedulerInput = CreateBaseValidRecurrentInput();

        var result = SchedulerService.InitialOrchestator(schedulerInput);

        Assert.True(result.IsSuccess);

        var tz = TimeZoneConverter.GetTimeZone();
        var offset = TimeZoneConverter.GetUtcOffset(result.Value.NextDate.DateTime, tz);

        Assert.Equal(offset, result.Value.NextDate.Offset);
    }

    [Fact, Trait("Category", "TimeZoneConverter_Integration")]
    public void InitialHandler_ShouldUseCorrectOffset_ForLocalDateTime() {
        var schedulerInput = CreateBaseValidRecurrentInput();
        var tz = TimeZoneConverter.GetTimeZone();

        var summerDate = new DateTime(2025, 7, 15, 10, 0, 0);
        schedulerInput.StartDate = new DateTimeOffset(summerDate, tz.GetUtcOffset(summerDate));
        schedulerInput.CurrentDate = new DateTimeOffset(summerDate, tz.GetUtcOffset(summerDate));
        schedulerInput.EndDate = new DateTimeOffset(summerDate.AddDays(7), tz.GetUtcOffset(summerDate.AddDays(7)));

        var result = SchedulerService.InitialOrchestator(schedulerInput);

        Assert.True(result.IsSuccess);

        var expectedOffset = tz.GetUtcOffset(result.Value.NextDate.DateTime);
        Assert.Equal(expectedOffset, result.Value.NextDate.Offset);
    }
}