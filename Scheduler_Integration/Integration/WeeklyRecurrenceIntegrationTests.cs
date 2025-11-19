using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Core.Services.Utilities;
using Scheduler_Lib.Resources;
using Xunit;
using Xunit.Abstractions;
// ReSharper disable UseObjectOrCollectionInitializer

namespace Scheduler_IntegrationTests.Integration;

public class WeeklyRecurrenceIntegrationTests(ITestOutputHelper output) {

    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldSuccess_WhenBasicConfigurationIsValid() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 03, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.False(string.IsNullOrEmpty(result.Value.Description));
    }

    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldSuccess_WhenMultipleDaysAreSelected() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.Language = "en_US";
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday];

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.Contains("Monday", result.Value.Description);
        Assert.Contains("Wednesday", result.Value.Description);
        Assert.Contains("Friday", result.Value.Description);
    }
    
    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldSuccess_WhenAllDaysOfWeekAreSelected() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, 
                                      DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday];

        var result = SchedulerService.InitialHandler(schedulerInput);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.True(futureDates!.Count >= 7);
    }

    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldSuccess_WhenBiWeeklyPeriodIsConfigured() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.Language = "en_US";
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 2;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.Contains("every 2 week(s)", result.Value.Description);
    }

    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldSuccess_WhenThreeWeekPeriodIsConfigured() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.Language = "en_US";
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 3;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.Contains("every 3 week(s)", result.Value.Description);
    }
    
    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldSuccess_WhenEndDateIsSpecified() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];

        var result = SchedulerService.InitialHandler(schedulerInput);
        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.All(futureDates!, date => Assert.True(date <= schedulerInput.EndDate));
    }

    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldSuccess_WhenCurrentDateIsAfterStartDate() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 09, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 15, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.NextDate >= schedulerInput.CurrentDate);
    }
    
    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldSuccess_WhenWeekendDaysOnly() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Saturday, DayOfWeek.Sunday];

        var result = SchedulerService.InitialHandler(schedulerInput);
        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.All(futureDates!, date => Assert.True(date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday));
    }
    
    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldSuccess_WhenOccursOnceIsConfigured() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = new TimeSpan(14, 30, 0);

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.Equal(14, result.Value.NextDate.Hour);
        Assert.Equal(30, result.Value.NextDate.Minute);
    }
    
    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldSuccess_WhenOccursOnceWithMultipleDays() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday];
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = new TimeSpan(9, 0, 0);

        var result = SchedulerService.InitialHandler(schedulerInput);
        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.True(futureDates!.Count >= 3);
    }
    
    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldSuccess_WhenOccursOnceWithBiWeekly() {
        var schedulerInput = new SchedulerInput();
        var tz = TimeZoneConverter.GetTimeZone();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.Language = "en_US";
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.EndDate = new DateTimeOffset(2025, 11, 30, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 11, 30)));
        schedulerInput.WeeklyPeriod = 2;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = new TimeSpan(10, 0, 0);

        var result = SchedulerService.InitialHandler(schedulerInput);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.Contains("every 2 week(s)", result.Value.Description);
        Assert.True(futureDates!.Count >= 3); 
    }
    
    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldSuccess_WhenOccursEveryWithTimeWindow() {
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

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.True(futureDates!.Count > 2);
    }

    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldSuccess_WhenOccursEveryHourlyOnSpecificDays() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 15, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromHours(3);
        schedulerInput.DailyStartTime = TimeSpan.FromHours(8);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(20);

        var result = SchedulerService.InitialHandler(schedulerInput);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.All(futureDates!, date => Assert.Equal(DayOfWeek.Monday, date.DayOfWeek));
    }

    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldSuccess_WhenOccursEveryWithShortIntervals() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 08, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Wednesday];
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromMinutes(30);
        schedulerInput.DailyStartTime = TimeSpan.FromHours(9);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(12);

        var result = SchedulerService.InitialHandler(schedulerInput);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.True(futureDates!.Count >= 6);
    }

    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldSuccess_WhenOccursEveryOnAllDaysWithTimeWindow() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 08, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, 
                                      DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday];
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromHours(4);
        schedulerInput.DailyStartTime = TimeSpan.FromHours(8);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(20);

        var result = SchedulerService.InitialHandler(schedulerInput);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.True(futureDates!.Count >= 20);
    }

    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldSuccess_WhenOccursEveryBiWeeklyWithTimeWindow() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 11, 30, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 2;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Friday];
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromHours(2);
        schedulerInput.DailyStartTime = TimeSpan.FromHours(10);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(16);

        var result = SchedulerService.InitialHandler(schedulerInput);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.True(futureDates!.Count >= 10);
    }
    
    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldSuccess_WhenTargetDateIsSpecified() {
        var schedulerInput = new SchedulerInput();
        var tz = TimeZoneConverter.GetTimeZone();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.TargetDate = new DateTimeOffset(2025, 10, 13, 14, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 13, 14, 0, 0)));
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = new TimeSpan(14, 0, 0);

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.Equal(schedulerInput.TargetDate!.Value.DateTime, result.Value.NextDate.DateTime);
    }
    
    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldSuccess_WhenStartAndEndDateAreInSameWeek() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 06, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 06, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 10, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday];
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromHours(2);
        schedulerInput.DailyStartTime = TimeSpan.FromHours(9);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(17);

        var result = SchedulerService.InitialHandler(schedulerInput);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.All(futureDates!, date => Assert.True(date <= schedulerInput.EndDate));
    }

    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldSuccess_WhenVeryLongWeeklyPeriod() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.Language = "es_ES";
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 4;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];

        var result = SchedulerService.InitialHandler(schedulerInput);

        var futureDates = RecurrenceCalculator.GetFutureDates(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.True(futureDates!.Count >= 12);
        Assert.Contains("cada 4 semana(s)", result.Value.Description);
    }

    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldSuccess_WhenOccursEveryWithVeryShortTimeWindow() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.Language = "en_US";
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 15, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromMinutes(15);
        schedulerInput.DailyStartTime = TimeSpan.FromHours(10);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(11);

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.Contains("15 minutes", result.Value.Description);
    }
    
    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldSuccess_WhenDailyTimeRangeGeneratesExpectedSlots() {
        var startDate = new DateTimeOffset(2025, 10, 20, 0, 0, 0, TimeSpan.Zero);
        var endDate = new DateTimeOffset(2025, 10, 29, 23, 59, 59, TimeSpan.Zero);

        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = startDate;
        schedulerInput.CurrentDate = startDate;
        schedulerInput.EndDate = endDate;
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Wednesday];
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromMinutes(30);
        schedulerInput.DailyStartTime = TimeSpan.FromHours(9);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(12);

        var result = RecurrenceCalculator.GetFutureDates(schedulerInput);

        Assert.True(result.Count >= 7, $"Expected at least 7 slots (one Wednesday), but got {result.Count}");

        Assert.All(result, slot => Assert.Equal(DayOfWeek.Wednesday, slot.DayOfWeek));
    }
    
    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldFail_WhenWeeklyPeriodIsNotSpecified() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];

        var result = SchedulerService.InitialHandler(schedulerInput);
;
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorWeeklyPeriodRequired, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldFail_WhenWeeklyPeriodIsZero() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 0;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorWeeklyPeriodRequired, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldFail_WhenWeeklyPeriodIsNegative() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = -1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorWeeklyPeriodRequired, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldFail_WhenDaysOfWeekAreNotSelected() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorDaysOfWeekRequired, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldFail_WhenDaysOfWeekListIsEmpty() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [];

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorDaysOfWeekRequired, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldFail_WhenDuplicateDaysOfWeekExist() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Monday, DayOfWeek.Wednesday];

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorDuplicateDaysOfWeek, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldFail_WhenBothOccursModesAreEnabled() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = new TimeSpan(10, 0, 0);
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromHours(2);

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorDailyModeConflict, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldFail_WhenOccursOnceAtIsNull() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = null;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorOccursOnceAtNull, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldFail_WhenOccursEveryWithNullPeriod() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = null;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorPositiveOffsetRequired, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrence_ShouldFail_WhenStartTimeIsAfterEndTime() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromHours(1);
        schedulerInput.DailyStartTime = TimeSpan.FromHours(18);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(8);

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorDailyStartAfterEnd, result.Error ?? string.Empty);
    }
}
