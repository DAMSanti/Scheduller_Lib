using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Resources;
using Xunit;
using Xunit.Abstractions;
// ReSharper disable UseObjectOrCollectionInitializer

namespace Scheduler_IntegrationTests.Integration;

public class MonthlyRecurrenceIntegrationTests(ITestOutputHelper output) {
    [Fact, Trait("Category", "Integration")]
    public void MonthlyRecurrence_ShouldSuccess_WhenMonthlyDayModeIsValid() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 06, 30, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyTheChk = false;
        schedulerInput.MonthlyDay = 15;
        schedulerInput.MonthlyDayPeriod = 1;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Contains("day 15", result.Value.Description);
        Assert.True(futureDates!.All(d => d.Day == 15));
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyRecurrence_ShouldSuccess_WhenMonthlyDaySkipsInvalidMonths() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyTheChk = false;
        schedulerInput.MonthlyDay = 31;
        schedulerInput.MonthlyDayPeriod = 1;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.True(futureDates!.All(d => d.Day == 31));
        Assert.DoesNotContain(futureDates, d => d.Month == 2);
        Assert.DoesNotContain(futureDates, d => d.Month == 4);
        Assert.DoesNotContain(futureDates, d => d.Month == 6);
        Assert.DoesNotContain(futureDates, d => d.Month == 9);
        Assert.DoesNotContain(futureDates, d => d.Month == 11);
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyRecurrence_ShouldSuccess_WhenMonthlyDayWithMultipleMonthsPeriod() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyTheChk = false;
        schedulerInput.MonthlyDay = 10;
        schedulerInput.MonthlyDayPeriod = 3;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.True(futureDates!.All(d => d.Day == 10));
        Assert.Contains("every 3 month(s)", result.Value.Description);
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyRecurrence_ShouldSuccess_WhenMonthlyTheModeFirstMonday() {
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

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Contains("first", result.Value.Description);
        Assert.Contains("Monday", result.Value.Description);
        Assert.True(futureDates!.All(d => d.DayOfWeek == DayOfWeek.Monday));
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyRecurrence_ShouldSuccess_WhenMonthlyTheModeLastFriday() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 06, 30, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = false;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.Last;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Friday;
        schedulerInput.MonthlyThePeriod = 1;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Contains("last", result.Value.Description);
        Assert.Contains("Friday", result.Value.Description);
        Assert.True(futureDates!.All(d => d.DayOfWeek == DayOfWeek.Friday));
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyRecurrence_ShouldSuccess_WhenMonthlyTheModeSecondWeekday() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 06, 30, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = false;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.Second;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Weekday;
        schedulerInput.MonthlyThePeriod = 1;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Contains("second", result.Value.Description);
        Assert.Contains("weekday", result.Value.Description);
        Assert.True(futureDates!.All(d => d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday));
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyRecurrence_ShouldSuccess_WhenMonthlyTheModeThirdWeekendDay() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 06, 30, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = false;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.Third;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.WeekendDay;
        schedulerInput.MonthlyThePeriod = 1;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Contains("third", result.Value.Description);
        Assert.Contains("weekend day", result.Value.Description);
        Assert.True(futureDates!.All(d => d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday));
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyRecurrence_ShouldSuccess_WhenMonthlyTheModeFirstDay() {
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
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Day;
        schedulerInput.MonthlyThePeriod = 1;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Contains("first", result.Value.Description);
        Assert.Contains("day", result.Value.Description);
        Assert.True(futureDates!.All(d => d.Day == 1));
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyRecurrence_ShouldSuccess_WhenMonthlyTheModeFourthTuesday() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 06, 30, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = false;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.Fourth;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Tuesday;
        schedulerInput.MonthlyThePeriod = 1;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Contains("fourth", result.Value.Description);
        Assert.Contains("Tuesday", result.Value.Description);
        Assert.True(futureDates!.All(d => d.DayOfWeek == DayOfWeek.Tuesday));
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyDay_ShouldSuccess_WhenDailyTimeWindowGeneratesMultipleSlotsPerDay() {
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

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Equal(15, futureDates!.Count);
        Assert.True(futureDates.All(d => d.Day == 15));

        var expectedHours = new[] { 9, 11, 13, 15, 17 };
        Assert.All(futureDates, slot => Assert.Contains(slot.Hour, expectedHours));
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyDay_ShouldSuccess_WhenShortDailyIntervalsGenerateManySlots() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 02, 28, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 10;
        schedulerInput.MonthlyDayPeriod = 1;
        schedulerInput.DailyStartTime = TimeSpan.FromHours(10);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(12);
        schedulerInput.DailyPeriod = TimeSpan.FromMinutes(30);

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Equal(10, futureDates!.Count);
        Assert.True(futureDates.All(d => d.Day == 10));
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyDay_ShouldSuccess_WhenDailyTimeWindowWithBiMonthlyPeriod() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 1;
        schedulerInput.MonthlyDayPeriod = 2;
        schedulerInput.DailyStartTime = TimeSpan.FromHours(8);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(16);
        schedulerInput.DailyPeriod = TimeSpan.FromHours(4);

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Equal(18, futureDates!.Count);
        
        var months = futureDates.Select(d => d.Month).Distinct().OrderBy(m => m).ToList();
        Assert.Equal(new[] { 1, 3, 5, 7, 9, 11 }, months);
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyDay_ShouldSuccess_WhenAllDayTimeWindowGeneratesManySlots() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 01, 31, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 15;
        schedulerInput.MonthlyDayPeriod = 1;
        schedulerInput.DailyStartTime = TimeSpan.FromHours(0);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(23);
        schedulerInput.DailyPeriod = TimeSpan.FromHours(3);

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Equal(8, futureDates!.Count);
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyThe_ShouldSuccess_WhenDailyTimeWindowWithFirstMondayMultipleSlots() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 03, 31, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = false;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.First;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Monday;
        schedulerInput.MonthlyThePeriod = 1;
        schedulerInput.DailyStartTime = TimeSpan.FromHours(9);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(17);
        schedulerInput.DailyPeriod = TimeSpan.FromHours(2);

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Equal(15, futureDates!.Count);
        Assert.True(futureDates.All(d => d.DayOfWeek == DayOfWeek.Monday));
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyThe_ShouldSuccess_WhenDailyTimeWindowWithLastWeekday() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 06, 30, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = false;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.Last;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Weekday;
        schedulerInput.MonthlyThePeriod = 1;
        schedulerInput.DailyStartTime = TimeSpan.FromHours(10);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(14);
        schedulerInput.DailyPeriod = TimeSpan.FromHours(1);

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Equal(30, futureDates!.Count);
        Assert.True(futureDates.All(d => d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday));
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyThe_ShouldSuccess_WhenDailyTimeWindowWithSecondWeekendDay() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 04, 30, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = false;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.Third;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.WeekendDay;
        schedulerInput.MonthlyThePeriod = 1;
        schedulerInput.DailyStartTime = TimeSpan.FromHours(8);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(20);
        schedulerInput.DailyPeriod = TimeSpan.FromHours(4);

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Equal(16, futureDates!.Count);
        Assert.True(futureDates.All(d => d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday));
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyRecurrence_ShouldFail_WhenBothMonthlyModesAreEnabled() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyDay = 15;
        schedulerInput.MonthlyDayPeriod = 1;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.First;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Monday;
        schedulerInput.MonthlyThePeriod = 1;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? "SUCCESS" : result.Error);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorMonthlyModeConflict, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyRecurrence_ShouldFail_WhenNoMonthlyModeIsSpecified() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = false;
        schedulerInput.MonthlyTheChk = false;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? "SUCCESS" : result.Error);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorMonthlyModeRequired, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyRecurrence_ShouldFail_WhenMonthlyDayIsInvalid() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyTheChk = false;
        schedulerInput.MonthlyDay = 35;
        schedulerInput.MonthlyDayPeriod = 1;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? "SUCCESS" : result.Error);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorMonthlyDayInvalid, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyRecurrence_ShouldFail_WhenMonthlyDayPeriodIsInvalid() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyTheChk = false;
        schedulerInput.MonthlyDay = 15;
        schedulerInput.MonthlyDayPeriod = 0;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? "SUCCESS" : result.Error);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorMonthlyDayPeriodRequired, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyRecurrence_ShouldFail_WhenMonthlyFrequencyIsMissing() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = false;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = null;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Monday;
        schedulerInput.MonthlyThePeriod = 1;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? "SUCCESS" : result.Error);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorMonthlyFrequencyRequired, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyRecurrence_ShouldFail_WhenMonthlyDateTypeIsMissing() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = false;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.First;
        schedulerInput.MonthlyDateType = null;
        schedulerInput.MonthlyThePeriod = 1;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? "SUCCESS" : result.Error);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorMonthlyDateTypeRequired, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyRecurrence_ShouldFail_WhenMonthlyThePeriodIsInvalid() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = false;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.First;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Monday;
        schedulerInput.MonthlyThePeriod = -1;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? "SUCCESS" : result.Error);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorMonthlyThePeriodRequired, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyDay_ShouldSuccess_WhenFallBackDSTTransitionGeneratesExpectedSlots() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var startDate = new DateTimeOffset(2025, 10, 25, 0, 0, 0, TimeSpan.Zero);
        var endDate = new DateTimeOffset(2025, 10, 27, 23, 59, 59, TimeSpan.Zero);

        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = startDate;
        schedulerInput.CurrentDate = startDate;
        schedulerInput.EndDate = endDate;
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 26;
        schedulerInput.MonthlyDayPeriod = 1;
        schedulerInput.DailyStartTime = new TimeSpan(1, 0, 0);
        schedulerInput.DailyEndTime = new TimeSpan(4, 0, 0);
        schedulerInput.DailyPeriod = TimeSpan.FromMinutes(30);

        var result = RecurrenceCalculator.GetFutureDates(schedulerInput);

        output.WriteLine($"Result slots count: {result.Count}");
        output.WriteLine($"\nTimeZone: {tz.DisplayName}");
        output.WriteLine($"Date range: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
        
        output.WriteLine("\n=== All Slots (incluyendo per�odo de cambio de hora) ===");
        foreach (var slot in result) {
            var localTime = TimeZoneInfo.ConvertTime(slot, tz);
            var utcTime = slot.ToUniversalTime();
            output.WriteLine($"  Local: {slot:yyyy-MM-dd HH:mm:ss} | UTC: {utcTime:yyyy-MM-dd HH:mm:ss} | IsDST: {tz.IsDaylightSavingTime(slot.DateTime)}");
        }
        
        output.WriteLine("\n=== Slots por d�a ===");
        var groupedByDay = result.GroupBy(d => d.Day);
        foreach (var group in groupedByDay) {
            output.WriteLine($"\nD�a {group.Key}: {group.Count()} slots");
            output.WriteLine($"  Primera franja: {group.First():yyyy-MM-dd HH:mm:ss}");
            output.WriteLine($"  �ltima franja:  {group.Last():yyyy-MM-dd HH:mm:ss}");
        }
        
        Assert.True(result.Count > 0, "Deber�a haber al menos un slot generado");

        Assert.All(result, slot => Assert.Equal(26, slot.Day));

        Assert.All(result, slot => Assert.Equal(10, slot.Month));

        Assert.All(result, slot => {
            var hour = slot.Hour;
            Assert.True(hour >= 1 && hour <= 4, 
                $"La hora {hour} deber�a estar entre 1:00 y 4:00 AM");
        });

        output.WriteLine($"\n*** N�mero total de slots generados: {result.Count} ***");
        output.WriteLine("Nota: Durante el cambio de hora (26 oct 3:00->2:00), algunas franjas pueden duplicarse");
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyDay_ShouldSuccess_WhenSpringForwardDSTTransitionGeneratesExpectedSlots() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var startDate = new DateTimeOffset(2025, 3, 29, 0, 0, 0, TimeSpan.Zero);
        var endDate = new DateTimeOffset(2025, 3, 31, 23, 59, 59, TimeSpan.Zero);

        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = startDate;
        schedulerInput.CurrentDate = startDate;
        schedulerInput.EndDate = endDate;
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 30;
        schedulerInput.MonthlyDayPeriod = 1;
        schedulerInput.DailyStartTime = new TimeSpan(1, 0, 0);
        schedulerInput.DailyEndTime = new TimeSpan(4, 0, 0);
        schedulerInput.DailyPeriod = TimeSpan.FromMinutes(30);

        var result = RecurrenceCalculator.GetFutureDates(schedulerInput);

        output.WriteLine($"Result slots count: {result.Count}");
        output.WriteLine($"\nTimeZone: {tz.DisplayName}");
        output.WriteLine($"Date range: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
        output.WriteLine("\n*** CAMBIO DE HORA: ADELANTO (Spring Forward) ***");
        output.WriteLine("*** El 30 de marzo de 2025 a las 2:00 AM -> 3:00 AM ***");
        output.WriteLine("*** La hora 2:00-3:00 NO EXISTE (se salta) ***\n");
        
        output.WriteLine("\n=== All Slots (incluyendo per�odo de cambio de hora) ===");
        foreach (var slot in result) {
            var localTime = TimeZoneInfo.ConvertTime(slot, tz);
            var utcTime = slot.ToUniversalTime();
            output.WriteLine($"  Local: {slot:yyyy-MM-dd HH:mm:ss} | UTC: {utcTime:yyyy-MM-dd HH:mm:ss} | IsDST: {tz.IsDaylightSavingTime(slot.DateTime)}");
        }
        
        output.WriteLine("\n=== Slots por d�a ===");
        var groupedByDay = result.GroupBy(d => d.Day);
        foreach (var group in groupedByDay) {
            output.WriteLine($"\nD�a {group.Key}: {group.Count()} slots");
            output.WriteLine($"  Primera franja: {group.First():yyyy-MM-dd HH:mm:ss}");
            output.WriteLine($"  �ltima franja:  {group.Last():yyyy-MM-dd HH:mm:ss}");
        }

        Assert.True(result.Count > 0, "Deber�a haber al menos un slot generado");

        Assert.All(result, slot => Assert.Equal(30, slot.Day));

        Assert.All(result, slot => Assert.Equal(3, slot.Month));

        var slotsInInvalidHour = result.Where(slot => {
            var hour = slot.Hour;
            var minute = slot.Minute;
            return hour == 2;
        }).ToList();
        
        output.WriteLine($"\n*** Slots en la hora 'inexistente' (2:00-2:59): {slotsInInvalidHour.Count} ***");
        foreach (var invalidSlot in slotsInInvalidHour) {
            output.WriteLine($"    {invalidSlot:yyyy-MM-dd HH:mm:ss}");
        }

        output.WriteLine($"\n*** N�mero total de slots generados: {result.Count} ***");
        output.WriteLine("Nota: Durante el cambio de hora (30 marzo 2:00->3:00), la franja 2:00-3:00 se salta");
        output.WriteLine("Esperamos aproximadamente 5 slots en lugar de 7");
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyRecurrence_ShouldSuccess_WhenNoValidDatesInRange() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 02, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 02, 15, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 02, 20, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 31;
        schedulerInput.MonthlyDayPeriod = 1;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);
        output.WriteLine($"NextDate: {result.Value.NextDate:yyyy-MM-dd HH:mm:ss}");

        Assert.True(result.IsSuccess);
        Assert.Equal(schedulerInput.CurrentDate.DateTime, result.Value.NextDate.DateTime);
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyRecurrence_ShouldSuccess_WhenCurrentDateAfterAllValidDates() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 02, 28, 23, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 02, 28, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 15;
        schedulerInput.MonthlyDayPeriod = 1;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);
        output.WriteLine($"NextDate: {result.Value.NextDate:yyyy-MM-dd HH:mm:ss}");

        Assert.True(result.IsSuccess);
        Assert.Equal(schedulerInput.CurrentDate.DateTime, result.Value.NextDate.DateTime);
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyRecurrence_ShouldSuccess_WhenAllDatesFilteredOut() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 02, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 02, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 02, 05, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 30;
        schedulerInput.MonthlyDayPeriod = 1;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);
        output.WriteLine($"NextDate: {result.Value.NextDate:yyyy-MM-dd HH:mm:ss}");

        Assert.True(result.IsSuccess);
        Assert.Equal(schedulerInput.CurrentDate.DateTime, result.Value.NextDate.DateTime);
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyRecurrence_ShouldSuccess_WhenMonthlyTheSecondThursday() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 06, 30, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = false;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.Second;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Thursday;
        schedulerInput.MonthlyThePeriod = 1;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss} - {dto.DayOfWeek}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Contains("second", result.Value.Description);
        Assert.Contains("Thursday", result.Value.Description);
        Assert.True(futureDates!.All(d => d.DayOfWeek == DayOfWeek.Thursday));
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyRecurrence_ShouldSuccess_WhenMonthlyTheLastDay() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = false;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.Last;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Day;
        schedulerInput.MonthlyThePeriod = 1;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss} - Day: {dto.Day}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Contains("last", result.Value.Description);
        Assert.Contains("day", result.Value.Description);
        
        Assert.All(futureDates!, d => {
            var lastDayOfMonth = DateTime.DaysInMonth(d.Year, d.Month);
            Assert.Equal(lastDayOfMonth, d.Day);
        });
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyRecurrence_ShouldSuccess_WhenMonthlyTheWithQuarterlyPeriod() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = false;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.First;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Wednesday;
        schedulerInput.MonthlyThePeriod = 3;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Contains("every 3 month(s)", result.Value.Description);
        Assert.True(futureDates!.All(d => d.DayOfWeek == DayOfWeek.Wednesday));

        var months = futureDates.Select(d => d.Month).Distinct().OrderBy(m => m).ToList();
        Assert.Equal(new[] { 4, 7, 10 }, months);
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyRecurrence_ShouldSuccess_WhenMonthlyDayFirstOfMonth() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 06, 30, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 1;
        schedulerInput.MonthlyDayPeriod = 1;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Contains("day 1", result.Value.Description);
        Assert.True(futureDates!.All(d => d.Day == 1));
        Assert.Equal(5, futureDates.Count);
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyRecurrence_ShouldSuccess_WhenTargetDateIsProvided() {
        var tz = RecurrenceCalculator.GetTimeZone();
        var localDate = new DateTime(2025, 3, 15, 16, 45, 0);
        var targetDate = new DateTimeOffset(localDate, tz.GetUtcOffset(localDate));

        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = targetDate;
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 15;
        schedulerInput.MonthlyDayPeriod = 1;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);
        output.WriteLine($"NextDate: {result.Value.NextDate:yyyy-MM-dd HH:mm:ss}");

        Assert.True(result.IsSuccess);
        Assert.Equal(targetDate, result.Value.NextDate);
        Assert.Equal(16, result.Value.NextDate.Hour);
        Assert.Equal(45, result.Value.NextDate.Minute);
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyRecurrence_ShouldSuccess_WhenLeapYearFebruary29() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2024, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2024, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2024, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 29;
        schedulerInput.MonthlyDayPeriod = 1;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Contains("day 29", result.Value.Description);

        Assert.Contains(futureDates!, d => d.Month == 2 && d.Day == 29);

        var monthsWithDay29 = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        Assert.All(futureDates!, d => Assert.Contains(d.Month, monthsWithDay29));
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyRecurrence_ShouldSuccess_WhenNotLeapYearFebruary29() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 29;
        schedulerInput.MonthlyDayPeriod = 1;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);

        var monthsWithDay29 = new[] { 1, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        Assert.All(futureDates!, d => Assert.Contains(d.Month, monthsWithDay29));
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyRecurrence_ShouldSuccess_WhenMonthlyTheSaturday() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 06, 30, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = false;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.Third;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Saturday;
        schedulerInput.MonthlyThePeriod = 1;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss} - {dto.DayOfWeek}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Contains("third", result.Value.Description);
        Assert.Contains("Saturday", result.Value.Description);
        Assert.True(futureDates!.All(d => d.DayOfWeek == DayOfWeek.Saturday));
    }

    [Fact, Trait("Category", "Integration")]
    public void MonthlyRecurrence_ShouldSuccess_WhenMonthlyTheSunday() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 06, 30, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.MonthlyDayChk = false;
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.Fourth;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Sunday;
        schedulerInput.MonthlyThePeriod = 1;

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss} - {dto.DayOfWeek}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Contains("fourth", result.Value.Description);
        Assert.Contains("Sunday", result.Value.Description);
        Assert.True(futureDates!.All(d => d.DayOfWeek == DayOfWeek.Sunday));
    }
}

