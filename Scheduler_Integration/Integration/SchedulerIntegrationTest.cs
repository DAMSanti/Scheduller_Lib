using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Resources;
using Xunit;
using Xunit.Abstractions;
// ReSharper disable UseObjectOrCollectionInitializer

namespace Scheduler_IntegrationTests.Integration;

public class CalculateDateIntegrationTests(ITestOutputHelper output) {

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldFail_WhenApplicationIsDisabled() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = false;
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? "NO ERROR" : result.Error);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorApplicationDisabled, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldFail_WhenStartDateIsAfterEndDate() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 20, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 10, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = new TimeSpan(10, 0, 0);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? "NO ERROR" : result.Error);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorStartDatePostEndDate, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldFail_WhenStartDateIsDefault() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = default;
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? "NO ERROR" : result.Error);
        Assert.False(result.IsSuccess);
    }

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldFail_WhenUnsupportedPeriodicity() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = (EnumConfiguration)999;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? "NO ERROR" : result.Error);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorUnsupportedPeriodicity, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldFail_WhenUnsupportedRecurrency() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = (EnumRecurrency)999;

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? "NO ERROR" : result.Error);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorUnsupportedRecurrency, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldSuccess_WhenOnceConfigurationIsValid() {
        var schedulerInput = new SchedulerInput();
        var tz = RecurrenceCalculator.GetTimeZone();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 01, 01)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 01, 01)));
        schedulerInput.TargetDate = new DateTimeOffset(2025, 01, 15, 14, 30, 0, tz.GetUtcOffset(new DateTime(2025, 01, 15, 14, 30, 0)));

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);
        Assert.True(result.IsSuccess);
        Assert.Equal(schedulerInput.TargetDate!.Value.DateTime, result.Value.NextDate.DateTime);
    }

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldFail_WhenOnceConfigurationMissingTargetDate() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = null;

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? "NO ERROR" : result.Error);
        Assert.False(result.IsSuccess);
    }

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldSuccess_WhenDailyRecurrenceIsValid() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 01, 31, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);
        Assert.True(result.IsSuccess);
    }

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldSuccess_WhenDailyWithTimeWindow() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 01, 31, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromHours(2);
        schedulerInput.DailyStartTime = TimeSpan.FromHours(9);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(17);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);
        
        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates.Take(10)) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.True(futureDates!.Count > 0);
    }

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldFail_WhenDailyPeriodIsInvalid() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.Zero;

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? "NO ERROR" : result.Error);
        Assert.False(result.IsSuccess);
    }

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldSuccess_WhenWeeklyRecurrenceIsValid() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 09, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 15, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates.Take(5)) {
                output.WriteLine(dto.ToString());
            }
        }

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.NextDate >= schedulerInput.CurrentDate);
    }

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldSuccess_WhenWeeklyWithMultipleDays() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday];

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);
        Assert.True(result.IsSuccess);
        Assert.Contains("Monday", result.Value.Description);
        Assert.Contains("Wednesday", result.Value.Description);
        Assert.Contains("Friday", result.Value.Description);
    }

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldSuccess_WhenWeeklyWithTimeWindow() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Wednesday];
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromHours(2);
        schedulerInput.DailyStartTime = TimeSpan.FromHours(9);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(17);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates.Take(10)) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.True(futureDates!.Count > 2);
    }

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldSuccess_WhenMonthlyDayIsValid() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 06, 30, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 15;
        schedulerInput.MonthlyDayPeriod = 1;

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);
        Assert.True(result.IsSuccess);
        Assert.Contains("day 15", result.Value.Description);
    }

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldSuccess_WhenMonthlyTheIsValid() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 06, 30, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = false;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.First;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Monday;
        schedulerInput.MonthlyThePeriod = 1;

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);
        Assert.True(result.IsSuccess);
        Assert.Contains("first", result.Value.Description);
        Assert.Contains("Monday", result.Value.Description);
    }

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldSuccess_WhenMonthlyWithTimeWindow() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 03, 31, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 15;
        schedulerInput.MonthlyDayPeriod = 1;
        schedulerInput.DailyStartTime = TimeSpan.FromHours(9);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(17);
        schedulerInput.DailyPeriod = TimeSpan.FromHours(2);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);
        
        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates.Take(10)) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.True(futureDates!.Count > 0);
    }

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldSuccess_WhenCurrentDateIsAfterStartDate() {
        var schedulerInput = new SchedulerInput();
        var tz = RecurrenceCalculator.GetTimeZone();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 01, 01)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 06, 15, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 06, 15)));
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 12, 31)));
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);
        output.WriteLine($"NextDate: {result.Value.NextDate}");
        output.WriteLine($"CurrentDate: {schedulerInput.CurrentDate}");
        output.WriteLine($"NextDate >= CurrentDate: {result.Value.NextDate >= schedulerInput.CurrentDate}");
        
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.NextDate >= schedulerInput.CurrentDate);
    }

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldSuccess_WhenEndDateIsVeryFarInFuture() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2030, 12, 31, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);
        Assert.True(result.IsSuccess);
    }

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldSuccess_WhenComplexScenarioGeneratesCorrectDescription() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 2;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday];
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyStartTime = TimeSpan.FromHours(10);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(18);
        schedulerInput.DailyPeriod = TimeSpan.FromHours(3);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.Description);
        Assert.Contains("every 2 week(s)", result.Value.Description);
        Assert.Contains("Monday", result.Value.Description);
        Assert.Contains("Wednesday", result.Value.Description);
        Assert.Contains("Friday", result.Value.Description);
    }

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldSuccess_WhenLeapYearMonthlyDay29February() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2024, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2024, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2024, 12, 31, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 29;
        schedulerInput.MonthlyDayPeriod = 1;

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);
        
        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:yyyy-MM-dd}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Contains(futureDates!, d => d.Month == 2 && d.Day == 29);
    }

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldSuccess_WhenYearTransitionRecurrenceCrossesYears() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2024, 12, 15, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2024, 12, 15, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 02, 28, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);
        
        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates.Take(10)) {
                output.WriteLine($"{dto:yyyy-MM-dd}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Contains(futureDates!, d => d.Year == 2024);
        Assert.Contains(futureDates!, d => d.Year == 2025);
    }

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldSuccess_WhenMinimumTimeInterval15Minutes() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 01, 02, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyStartTime = TimeSpan.FromHours(10);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(12);
        schedulerInput.DailyPeriod = TimeSpan.FromMinutes(15);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);
        
        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates.Take(20)) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.True(futureDates!.Count >= 9);
    }

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldSuccess_WhenTimeZoneRomanceStandardTime() {
        var schedulerInput = new SchedulerInput();
        var tz = RecurrenceCalculator.GetTimeZone();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 01, 01)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 01, 01)));
        schedulerInput.EndDate = new DateTimeOffset(2025, 01, 05, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 01, 05)));
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);
        output.WriteLine($"TimeZone: {tz.DisplayName}");
        
        Assert.True(result.IsSuccess);
    }
}