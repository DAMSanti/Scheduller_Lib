using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Core.Services.Utilities;
using Scheduler_Lib.Core.Services.Calculators;
using Scheduler_Lib.Resources;
using System;
using System.Linq;
using Xunit;

namespace Scheduler_IntegrationTests.Integration;

public class RecurrenceCalculator_GetFutureDatesTests {
    [Fact, Trait("Category", "RecurrenceCalculator")]
    public void GetFutureDates_ReturnsEmpty_WhenRecurrencyIsUnsupported() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = (EnumRecurrency)999;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 8, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = schedulerInput.StartDate;

        var result = RecurrenceCalculator.GetFutureDates(schedulerInput);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact, Trait("Category", "RecurrenceCalculator")]
    public void GetFutureDates_Daily_RemovesNext_WhenFutureDatesContainsNextUtcEqual() {
        var tz = TimeZoneConverter.GetTimeZone();
        var schedulerInput = new SchedulerInput();
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 8, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1)));
        schedulerInput.CurrentDate = schedulerInput.StartDate;
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 04, 8, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 4)));
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);

        schedulerInput.TimeZoneId = TimeZoneConverter.GetTimeZoneId(tz);
        var next = RecurrenceCalculator.GetNextExecutionDate(schedulerInput, tz);
        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);

        Assert.DoesNotContain(futureDates, d => d.UtcDateTime == next.UtcDateTime);
    }

    [Fact, Trait("Category", "RecurrenceCalculator")]
    public void GetFutureDates_Weekly_RemovesNext_WhenFutureDatesContainsNext() {
        var tz = TimeZoneConverter.GetTimeZone();
        var schedulerInput = new SchedulerInput();
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 8, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1)));
        schedulerInput.CurrentDate = schedulerInput.StartDate;
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 31, 8, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 31)));
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = new System.Collections.Generic.List<DayOfWeek> { DayOfWeek.Wednesday };

        schedulerInput.TimeZoneId = TimeZoneConverter.GetTimeZoneId(tz);
        var next = RecurrenceCalculator.GetNextExecutionDate(schedulerInput, tz);
        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);

        Assert.DoesNotContain(futureDates, d => d.UtcDateTime == next.UtcDateTime);
    }

    [Fact, Trait("Category", "RecurrenceCalculator")]
    public void GetFutureDates_Monthly_RemovesNext_WhenFutureDatesContainsNext() {
        var tz = TimeZoneConverter.GetTimeZone();
        var schedulerInput = new SchedulerInput();
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 8, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1)));
        schedulerInput.CurrentDate = schedulerInput.StartDate;
        schedulerInput.EndDate = new DateTimeOffset(2026, 1, 01, 8, 0, 0, tz.GetUtcOffset(new DateTime(2026, 1, 1)));
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 5;
        schedulerInput.MonthlyDayPeriod = 1;

        schedulerInput.TimeZoneId = TimeZoneConverter.GetTimeZoneId(tz);
        var next = RecurrenceCalculator.GetNextExecutionDate(schedulerInput, tz);
        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);

        Assert.DoesNotContain(futureDates, d => d.UtcDateTime == next.UtcDateTime);
    }

    [Fact, Trait("Category", "RecurrenceCalculator")]
    public void GetFutureDates_Daily_LeavesFutureDates_WhenNextNotPresent() {
        var tz = TimeZoneConverter.GetTimeZone();
        var schedulerInput = new SchedulerInput();
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 8, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1)));
        schedulerInput.CurrentDate = schedulerInput.StartDate;
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 03, 8, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 3)));
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);
        schedulerInput.TargetDate = new DateTimeOffset(2099, 1, 1, 8, 0, 0, TimeSpan.Zero);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        schedulerInput.TimeZoneId = TimeZoneConverter.GetTimeZoneId(tz);
        var next = RecurrenceCalculator.GetNextExecutionDate(schedulerInput, tz);

        Assert.NotNull(futureDates);
        Assert.All(futureDates, d => Assert.NotEqual(d.UtcDateTime, next.UtcDateTime));
    }
}
