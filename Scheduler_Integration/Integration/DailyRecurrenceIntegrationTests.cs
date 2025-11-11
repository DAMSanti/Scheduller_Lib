using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Resources;
using Xunit;
using Xunit.Abstractions;
// ReSharper disable UseObjectOrCollectionInitializer

namespace Scheduler_IntegrationTests.Integration;

public class DailyRecurrenceIntegrationTests(ITestOutputHelper output) {
    [Fact, Trait("Category", "Integration")]
    public void DailyRecurrence_ShouldSuccess_WhenOccursOnceIsConfigured() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = new TimeSpan(14, 30, 0);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine(dto.ToString());
            }
        }

        Assert.True(result.IsSuccess);
    }

    [Fact, Trait("Category", "Integration")]
    public void DailyRecurrence_ShouldSuccess_WhenOccursOnceWithEndDate() {
        var schedulerInput = new SchedulerInput();
        var tz = RecurrenceCalculator.GetTimeZone();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 31, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 10, 31, 23, 59, 59)));
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = new TimeSpan(14, 30, 0);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.NextDate <= schedulerInput.EndDate);
        Assert.Equal(14, result.Value.NextDate.Hour);
        Assert.Equal(30, result.Value.NextDate.Minute);
    }

    [Fact, Trait("Category", "Integration")]
    public void DailyRecurrence_ShouldSuccess_WhenOccursOnceWithFutureTargetDate() {
        var schedulerInput = new SchedulerInput();
        var tz = RecurrenceCalculator.GetTimeZone();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.TargetDate = new DateTimeOffset(2025, 10, 20, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 20, 10, 0, 0)));
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = new TimeSpan(15, 30, 0);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        Assert.True(result.IsSuccess);
        Assert.Equal(15, result.Value.NextDate.Hour);
        Assert.Equal(30, result.Value.NextDate.Minute);
    }

    [Fact, Trait("Category", "Integration")]
    public void DailyRecurrence_ShouldSuccess_WhenCurrentDateAfterStartDate() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 09, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 15, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = new TimeSpan(10, 0, 0);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.NextDate >= schedulerInput.CurrentDate);
    }

    [Fact, Trait("Category", "Integration")]
    public void DailyRecurrence_ShouldSuccess_WhenOccursEveryWithTimeRangeIsConfigured() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromHours(2);
        schedulerInput.DailyStartTime = TimeSpan.FromHours(8);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(16);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine(dto.ToString());
            }
        }

        Assert.True(result.IsSuccess);
    }

    [Fact, Trait("Category", "Integration")]
    public void DailyRecurrence_ShouldSuccess_WhenOccursEveryWithHourlyPeriod() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 03, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromHours(4);
        schedulerInput.DailyStartTime = TimeSpan.FromHours(0);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(23);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.True(futureDates!.Count >= 6);
    }

    [Fact, Trait("Category", "Integration")]
    public void DailyRecurrence_ShouldSuccess_WhenOccursEveryWithMultipleDaysPeriod() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 15, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(3);

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
        Assert.True(futureDates!.Count >= 4); 
        Assert.Contains("3 days", result.Value.Description);
    }

    [Fact, Trait("Category", "Integration")]
    public void DailyRecurrence_ShouldSuccess_WhenOccursEveryWithWeeklyPeriod() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(7);

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
        Assert.True(futureDates!.Count >= 4);
        Assert.Contains("7 days", result.Value.Description);
    }

    [Fact, Trait("Category", "Integration")]
    public void DailyRecurrence_ShouldSuccess_WhenEndDateIsSpecified() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 05, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = new TimeSpan(10, 0, 0);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine(dto.ToString());
            }
        }

        Assert.True(result.IsSuccess);
        Assert.All(futureDates!, date => Assert.True(date <= schedulerInput.EndDate));
    }

    [Fact, Trait("Category", "Integration")]
    public void DailyRecurrence_ShouldSuccess_WhenShortIntervalGeneratesMultipleOccurrences() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromMinutes(30);
        schedulerInput.DailyStartTime = TimeSpan.FromHours(8);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(12);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine(dto.ToString());
            }
        }

        Assert.True(result.IsSuccess);
        Assert.True(futureDates!.Count > 5);
    }

    [Fact, Trait("Category", "Integration")]
    public void DailyRecurrence_ShouldSuccess_WhenOccursEveryWith15MinuteIntervals() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 01, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromMinutes(15);
        schedulerInput.DailyStartTime = TimeSpan.FromHours(9);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(10);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Equal(5, futureDates!.Count);
        Assert.Contains("15 minutes", result.Value.Description);
    }

    [Fact, Trait("Category", "Integration")]
    public void DailyRecurrence_ShouldSuccess_WhenOccursEveryWithoutTimeWindow() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 10, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine(dto.ToString());
            }
        }

        Assert.True(result.IsSuccess);
        Assert.True(futureDates!.Count >= 9);
    }

    [Fact, Trait("Category", "Integration")]
    public void DailyRecurrence_ShouldSuccess_WhenOccursEveryAcrossMultipleDaysWithEndDate() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 05, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromHours(6);
        schedulerInput.DailyStartTime = TimeSpan.FromHours(0);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(23);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.All(futureDates!, date => Assert.True(date <= schedulerInput.EndDate));
        Assert.True(futureDates!.Count >= 16);
    }

    [Fact, Trait("Category", "Integration")]
    public void DailyRecurrence_ShouldSuccess_WhenOccursEveryWithTargetDateAndTimeWindow() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2025, 10, 15, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 20, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromHours(3);
        schedulerInput.DailyStartTime = TimeSpan.FromHours(9);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(18);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Contains(futureDates!, d => d.Date == schedulerInput.TargetDate!.Value.Date);
    }

    [Fact, Trait("Category", "Integration")]
    public void DailyRecurrence_ShouldSuccess_WhenOccursEveryWithTargetDateWithoutTimeWindow() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2025, 10, 10, 14, 30, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 20, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(2);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:yyyy-MM-dd HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Equal(schedulerInput.TargetDate!.Value.DateTime, result.Value.NextDate.DateTime);
    }

    [Fact, Trait("Category", "Integration")]
    public void DailyRecurrence_ShouldSuccess_WhenVeryShortTimeWindow() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 01, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromSeconds(30);
        schedulerInput.DailyStartTime = TimeSpan.FromHours(10);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(10).Add(TimeSpan.FromMinutes(2));

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.True(futureDates!.Count >= 2);
    }

    [Fact, Trait("Category", "Integration")]
    public void DailyRecurrence_ShouldSuccess_WhenVeryLongPeriod() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(30);

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
        Assert.Equal(12, futureDates!.Count);
        Assert.Contains("30 days", result.Value.Description);
    }

    [Fact, Trait("Category", "Integration")]
    public void DailyRecurrence_ShouldSuccess_WhenStartDateEqualsEndDate() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 15, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 15, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 15, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromHours(2);
        schedulerInput.DailyStartTime = TimeSpan.FromHours(8);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(16);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.True(futureDates!.All(d => d.Date == schedulerInput.StartDate.Date));
    }

    [Fact, Trait("Category", "Integration")]
    public void DailyRecurrence_ShouldSuccess_WhenOccursEveryWith1SecondPeriod() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromSeconds(1);
        schedulerInput.DailyStartTime = TimeSpan.FromHours(10);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(10).Add(TimeSpan.FromSeconds(10));

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);
        if (futureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {futureDates.Count}):");
            foreach (var dto in futureDates) {
                output.WriteLine($"{dto:HH:mm:ss}");
            }
        }

        Assert.True(result.IsSuccess);
        Assert.Contains("1 second", result.Value.Description);
    }

    [Fact, Trait("Category", "Integration")]
    public void DailyRecurrence_ShouldFail_WhenBothOccursModesAreEnabled() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = new TimeSpan(10, 0, 0);
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromHours(2);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? "SUCCESS" : result.Error);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorDailyModeConflict, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void DailyRecurrence_ShouldFail_WhenNoOccursModeIsSpecified() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? "SUCCESS" : result.Error);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorDailyModeRequired, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void DailyRecurrence_ShouldFail_WhenStartTimeIsAfterEndTime() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromHours(1);
        schedulerInput.DailyStartTime = TimeSpan.FromHours(18);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(8);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? "SUCCESS" : result.Error);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorDailyStartAfterEnd, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void DailyRecurrence_ShouldFail_WhenOccursOnceAtIsNull() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = null;

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? "SUCCESS" : result.Error);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorOccursOnceAtNull, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void DailyRecurrence_ShouldFail_WhenOccursEveryWithNullPeriod() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = null;

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? "SUCCESS" : result.Error);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorPositiveOffsetRequired, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void DailyRecurrence_ShouldFail_WhenOccursEveryWithNegativePeriod() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromHours(-1);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? "SUCCESS" : result.Error);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorPositiveOffsetRequired, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void DailyRecurrence_ShouldFail_WhenOccursEveryWithZeroPeriod() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.Zero;

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? "SUCCESS" : result.Error);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorPositiveOffsetRequired, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void DailyRecurrence_ShouldFail_WhenOccursOnceAtAfterEndDate() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 10, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = new TimeSpan(14, 30, 0);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? "SUCCESS" : result.Error);
        Assert.True(result.IsSuccess);
    }

    [Fact, Trait("Category", "Integration")]
    public void DailyRecurrence_ShouldFail_WhenOccursOnceAtBeforeStartDate() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 10, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 10, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 20, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = new TimeSpan(14, 30, 0);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine(result.IsSuccess ? "SUCCESS" : result.Error);
        Assert.True(result.IsSuccess);
    }
}
