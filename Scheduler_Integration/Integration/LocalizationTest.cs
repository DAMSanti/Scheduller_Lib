using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Core.Services.Localization;
using Scheduler_Lib.Core.Services.Utilities;
using Xunit;
using Xunit.Abstractions;
#pragma warning disable IDE0017

namespace Scheduler_IntegrationTests.Integration;

public class LocalizationIntegrationTests() {
    [Theory, Trait("Category", "Localization")]
    [InlineData("es_ES")]
    [InlineData("en_US")]
    [InlineData("en_GB")]
    public void WeeklyRecurrence_ShouldGenerateLocalizedDescription_WhenLanguageIsSupported(string language) {
        var tz = TimeZoneConverter.GetTimeZone();
        var schedulerInput = new SchedulerInput();
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.Language = language;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 12, 31)));
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday];
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = new TimeSpan(10, 30, 0);

        var result = SchedulerService.InitialOrchestator(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Description);
    }

    [Theory, Trait("Category", "Localization")]
    [InlineData("es_ES")]
    [InlineData("en_US")]
    [InlineData("en_GB")]
    public void WeeklyRecurrence_WithOccursEvery_ShouldGenerateLocalizedDescription(string language) {
        var tz = TimeZoneConverter.GetTimeZone();
        var schedulerInput = new SchedulerInput();
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.Language = language;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 15, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 10, 15)));
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromHours(2);
        schedulerInput.DailyStartTime = TimeSpan.FromHours(9);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(17);

        var result = SchedulerService.InitialOrchestator(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Description);
    }

    [Theory, Trait("Category", "Localization")]
    [InlineData("es_ES")]
    [InlineData("en_US")]
    [InlineData("en_GB")]    public void DailyRecurrence_ShouldGenerateLocalizedDescription_WhenLanguageIsSupported(string language) {
        var tz = TimeZoneConverter.GetTimeZone();
        var schedulerInput = new SchedulerInput();
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.Language = language;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 31, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 10, 31)));
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = new TimeSpan(14, 30, 0);

        var result = SchedulerService.InitialOrchestator(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Description);
    }

    [Theory, Trait("Category", "Localization")]
    [InlineData("es_ES")]
    [InlineData("en_US")]
    [InlineData("en_GB")]
    public void DailyRecurrence_WithOccursEvery_ShouldGenerateLocalizedDescription(string language) {
        var tz = TimeZoneConverter.GetTimeZone();
        var schedulerInput = new SchedulerInput();
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.Language = language;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 15, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 10, 15)));
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromHours(3);
        schedulerInput.DailyStartTime = TimeSpan.FromHours(8);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(20);

        var result = SchedulerService.InitialOrchestator(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Description);
    }

    [Theory, Trait("Category", "Localization")]
    [InlineData("es_ES")]
    [InlineData("en_US")]
    [InlineData("en_GB")]
    public void MonthlyRecurrence_ByDay_ShouldGenerateLocalizedDescription(string language) {
        var tz = TimeZoneConverter.GetTimeZone();
        var schedulerInput = new SchedulerInput();
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.Language = language;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 12, 31)));
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 15;
        schedulerInput.MonthlyDayPeriod = 1;
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = new TimeSpan(10, 0, 0);

        var result = SchedulerService.InitialOrchestator(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Description);
    }

    [Theory, Trait("Category", "Localization")]
    [InlineData("es_ES")]
    [InlineData("en_US")]
    [InlineData("en_GB")]
    public void MonthlyRecurrence_ByFrequencyAndDateType_ShouldGenerateLocalizedDescription(string language) {
        var tz = TimeZoneConverter.GetTimeZone();
        var schedulerInput = new SchedulerInput();
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.Language = language;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 12, 31)));
        schedulerInput.MonthlyTheChk = true;
        schedulerInput.MonthlyFrequency = EnumMonthlyFrequency.Last;
        schedulerInput.MonthlyDateType = EnumMonthlyDateType.Friday;
        schedulerInput.MonthlyThePeriod = 1;
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = new TimeSpan(14, 0, 0);

        var result = SchedulerService.InitialOrchestator(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Description);
    }

    [Theory, Trait("Category", "Localization")]
    [InlineData("es_ES")]
    [InlineData("en_US")]
    [InlineData("en_GB")]
    public void MonthlyRecurrence_WithOccursEvery_ShouldGenerateLocalizedDescription(string language) {
        var tz = TimeZoneConverter.GetTimeZone();
        var schedulerInput = new SchedulerInput();
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.Language = language;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 12, 31)));
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 10;
        schedulerInput.MonthlyDayPeriod = 1;
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromHours(4);
        schedulerInput.DailyStartTime = TimeSpan.FromHours(8);
        schedulerInput.DailyEndTime = TimeSpan.FromHours(20);

        var result = SchedulerService.InitialOrchestator(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Description);
    }

    [Theory, Trait("Category", "Localization")]
    [InlineData("es_ES")]
    [InlineData("en_US")]
    [InlineData("en_GB")]
    public void Once_Configuration_ShouldGenerateLocalizedDescription(string language) {
        var tz = TimeZoneConverter.GetTimeZone();
        var schedulerInput = new SchedulerInput();
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.Language = language;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.TargetDate = new DateTimeOffset(2025, 10, 15, 14, 30, 0, tz.GetUtcOffset(new DateTime(2025, 10, 15, 14, 30, 0)));

        var result = SchedulerService.InitialOrchestator(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Description);
    }

    [Fact, Trait("Category", "Localization")]
    public void Validation_ShouldFail_WhenLanguageIsNotSupported() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.Language = "ja-JP";
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];

        var result = SchedulerService.InitialOrchestator(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains("ERROR: Language not supported. Available ", result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Localization")]
    public void Validation_ShouldFail_WhenLanguageIsNull() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.Language = null!;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];

        var result = SchedulerService.InitialOrchestator(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains("ERROR: The language is mandatory.", result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Localization")]
    public void Validation_ShouldFail_WhenLanguageIsEmpty() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.Language = "";
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];

        var result = SchedulerService.InitialOrchestator(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains("ERROR: The language is mandatory.", result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Localization")]
    public void LocalizationService_ShouldReturnAllSupportedLanguages() {
        var supportedLanguages = LocalizationService.GetSupportedLanguages().ToList();

        Assert.NotEmpty(supportedLanguages);
        Assert.Equal(3, supportedLanguages.Count);

        foreach (var lang in supportedLanguages) {
            Assert.True(LocalizationService.IsSupportedLanguage(lang));
        }
    }

    [Theory, Trait("Category", "Localization")]
    [InlineData("es_ES")]
    [InlineData("en_US")]
    [InlineData("en_GB")]
    public void DateFormatting_ShouldBeLocalizedCorrectly(string language) {
        var date = new DateTimeOffset(2025, 10, 15, 14, 30, 45, TimeSpan.Zero);
        var formattedDate = LocalizationService.FormatDate(date, language);

        Assert.NotEmpty(formattedDate);
    }

    [Theory, Trait("Category", "Localization")]
    [InlineData("es_ES")]
    [InlineData("en_US")]
    [InlineData("en_GB")]
    public void DayOfWeekFormatting_ShouldBeLocalizedCorrectly(string language) {
        var daysOfWeek = new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday };

        foreach (var day in daysOfWeek) {
            var formattedDay = LocalizationService.FormatDayOfWeek(day, language);
            Assert.NotEmpty(formattedDay);
        }
    }
}