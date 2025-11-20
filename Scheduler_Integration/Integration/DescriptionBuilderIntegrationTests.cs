using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Core.Services.Localization;
using Scheduler_Lib.Core.Services.Utilities;
using Scheduler_Lib.Resources;
using Xunit;
using Xunit.Abstractions;
#pragma warning disable IDE0017

namespace Scheduler_IntegrationTests.Integration;

public class DescriptionBuilderIntegrationTests() {
    [Fact]
    public void DescriptionBuilder_Once_ReturnsOccursOnceDescription() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.Language = "en_US";
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2025, 10, 05, 09, 15, 0, TimeSpan.Zero);

        var result = SchedulerService.InitialOrchestator(schedulerInput);
        Assert.True(result.IsSuccess);
        Assert.Contains("Occurs once", result.Value.Description);
        Assert.Contains("starting on", result.Value.Description);
    }

    [Fact]
    public void DescriptionBuilder_Daily_OccursEvery_PeriodDisplayed() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.Language = "en_US";
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.DailyPeriod = TimeSpan.FromDays(2);
        schedulerInput.OccursEveryChk = true;

        var result = SchedulerService.InitialOrchestator(schedulerInput);
        Assert.True(result.IsSuccess);
        Assert.Contains("every", result.Value.Description);
        Assert.Contains("day", result.Value.Description);
    }

    [Fact]
    public void DescriptionBuilder_Daily_OccursOnceAt_ShowsTime() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.Language = "en_US";
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = new TimeSpan(8, 30, 0);

        var result = SchedulerService.InitialOrchestator(schedulerInput);
        Assert.True(result.IsSuccess);
        Assert.Contains("at", result.Value.Description);
        Assert.Contains("08:30:00", result.Value.Description);
    }

    [Fact]
    public void DescriptionBuilder_Weekly_OccursEvery_IncludesDays() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.Language = "en_US";
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Wednesday];
        schedulerInput.DailyPeriod = TimeSpan.FromDays(7);

        var result = SchedulerService.InitialOrchestator(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.Contains("every", result.Value.Description);
        Assert.Contains("on", result.Value.Description);
        Assert.Contains("Monday", result.Value.Description);
    }

    [Fact]
    public void DescriptionBuilder_Weekly_OccursOnceAt_ShowsTime() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.Language = "en_US";
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Tuesday];
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = new TimeSpan(14, 0, 0);

        var result = SchedulerService.InitialOrchestator(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.Contains("at", result.Value.Description);
        Assert.Contains("14:00:00", result.Value.Description);
    }

    [Fact]
    public void DescriptionBuilder_Monthly_DayChk_IncludesDay() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.Language = "en_US";
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 15;
        schedulerInput.MonthlyDayPeriod = 1;

        var result = SchedulerService.InitialOrchestator(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.Contains("day 15", result.Value.Description);
    }

    [Fact]
    public void DescriptionBuilder_Monthly_TheChk_FirstMonday() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.Language = "en_US";
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.First;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Monday;
        schedulerInput.MonthlyThePeriod = 1;

        var result = SchedulerService.InitialOrchestator(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.Contains("first", result.Value.Description);
        Assert.Contains("Monday", result.Value.Description);
    }

    [Fact]
    public void DescriptionBuilder_FormatInterval_SecondsAndMinutes() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.Language = "en_US";
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

        schedulerInput.DailyPeriod = TimeSpan.FromSeconds(30);
        schedulerInput.OccursEveryChk = true;
        var r1 = SchedulerService.InitialOrchestator(schedulerInput);
        Assert.True(r1.IsSuccess);
        Assert.Contains("seconds", r1.Value.Description);

        schedulerInput.DailyPeriod = TimeSpan.FromMinutes(30);
        schedulerInput.OccursEveryChk = true;
        var r2 = SchedulerService.InitialOrchestator(schedulerInput);
        Assert.True(r2.IsSuccess);
        Assert.Contains("minutes", r2.Value.Description);
    }
}
