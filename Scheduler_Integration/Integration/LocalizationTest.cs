using Microsoft.VisualStudio.TestPlatform.Utilities;
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Core.Services.Utilities;
using Xunit;
using Xunit.Abstractions;
#pragma warning disable IDE0017

namespace Scheduler_IntegrationTests.Integration;

public class LocalizationIntegrationTests(ITestOutputHelper output) {
    [Theory, Trait("Category", "Localization")]
    [InlineData("es_ES", "cada semana en Lunes, Miércoles, Viernes a las 10:30:00. Próxima ejecución: lunes, 6 de octubre de 2025 10:30:00")]
    [InlineData("en_US", "every week on Monday, Wednesday, Friday at 10:30:00. Next execution: Monday, October 6, 2025 10:30:00")]
    [InlineData("en_GB", "every week on Monday, Wednesday, Friday at 10:30:00. Next execution: Monday, 6 October 2025 10:30:00")]
    public void WeeklyRecurrence_ShouldGenerateLocalizedDescription_WhenLanguageIsSupported(string language, string expectedDescription) {
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
        Assert.Equal(expectedDescription, result.Value.Description);
    }

    [Theory, Trait("Category", "Localization")]
    [InlineData("es_ES", "cada semana en Lunes ocurre cada 2 horas. Próxima ejecución: lunes, 6 de octubre de 2025 10:00:00")]
    [InlineData("en_US", "every week on Monday occurs every 2 hours. Next execution: Monday, October 6, 2025 10:00:00")]
    [InlineData("en_GB", "every week on Monday occurs every 2 hours. Next execution: Monday, 6 October 2025 10:00:00")]
    public void WeeklyRecurrence_WithOccursEvery_ShouldGenerateLocalizedDescription(string language, string expectedDescription) {
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
        Assert.Equal(expectedDescription, result.Value.Description);
    }

    [Theory, Trait("Category", "Localization")]
    [InlineData("es_ES", "America/New_York", "ocurre cada día a las 14:30:00. Próxima ejecución: miércoles, 1 de octubre de 2025 14:30:00")]
    [InlineData("en_US", "America/New_York", "occurs every day at 14:30:00. Next execution: Wednesday, October 1, 2025 14:30:00")]
    [InlineData("en_GB", "Europe/London", "occurs every day at 14:30:00. Next execution: Wednesday, 1 October 2025 14:30:00")]
    [InlineData("es_ES", "America/Los_Angeles", "ocurre cada día a las 14:30:00. Próxima ejecución: miércoles, 1 de octubre de 2025 14:30:00")]
    [InlineData("en_US", "Pacific/Honolulu", "occurs every day at 14:30:00. Next execution: Wednesday, October 1, 2025 14:30:00")]
    public void DailyRecurrence_ShouldGenerateLocalizedDescription_WithDifferentTimeZones(string language, string timeZoneId, string expectedDescription) {
        var tz = TimeZoneConverter.GetTimeZone(timeZoneId);
        var schedulerInput = new SchedulerInput();
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.Language = language;
        schedulerInput.TimeZoneId = timeZoneId;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 31, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 10, 31)));
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = new TimeSpan(14, 30, 0);

        var result = SchedulerService.InitialOrchestator(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Description);
        Assert.Equal(expectedDescription, result.Value.Description);
        
        // Verify the timezone was correctly applied
        output.WriteLine($"TimeZone: {timeZoneId}, Language: {language}");
        output.WriteLine($"NextDate: {result.Value.NextDate}");
        output.WriteLine($"Description: {result.Value.Description}");
    }

    [Theory, Trait("Category", "Localization")]
    [InlineData("es_ES", "ocurre cada día a las 14:30:00. Próxima ejecución: miércoles, 1 de octubre de 2025 14:30:00")]
    [InlineData("en_US", "occurs every day at 14:30:00. Next execution: Wednesday, October 1, 2025 14:30:00")]
    [InlineData("en_GB", "occurs every day at 14:30:00. Next execution: Wednesday, 1 October 2025 14:30:00")]
    public void DailyRecurrence_ShouldGenerateLocalizedDescription_WhenLanguageIsSupported(string language, string expectedDescription) {
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
        Assert.Equal(expectedDescription, result.Value.Description);
    }

    [Theory, Trait("Category", "Localization")]
    [InlineData("es_ES", "ocurre cada 3 horas. Próxima ejecución: miércoles, 1 de octubre de 2025 10:00:00")]
    [InlineData("en_US", "occurs every 3 hours. Next execution: Wednesday, October 1, 2025 10:00:00")]
    [InlineData("en_GB", "occurs every 3 hours. Next execution: Wednesday, 1 October 2025 10:00:00")]
    public void DailyRecurrence_WithOccursEvery_ShouldGenerateLocalizedDescription(string language, string expectedDescription) {
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
        Assert.Equal(expectedDescription, result.Value.Description);
    }

    [Theory, Trait("Category", "Localization")]
    [InlineData("es_ES", "Europe/Madrid", "cada mes el día 15. Próxima ejecución: miércoles, 15 de octubre de 2025 10:00:00")]
    [InlineData("en_US", "America/Chicago", "every month on day 15. Next execution: Wednesday, October 15, 2025 10:00:00")]
    [InlineData("en_GB", "Europe/London", "every month on day 15. Next execution: Wednesday, 15 October 2025 10:00:00")]
    [InlineData("es_ES", "America/Mexico_City", "cada mes el día 15. Próxima ejecución: miércoles, 15 de octubre de 2025 10:00:00")]
    public void MonthlyRecurrence_ByDay_ShouldGenerateLocalizedDescription_WithDifferentTimeZones(string language, string timeZoneId, string expectedDescription) {
        var tz = TimeZoneConverter.GetTimeZone(timeZoneId);
        var schedulerInput = new SchedulerInput();
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.Language = language;
        schedulerInput.TimeZoneId = timeZoneId;
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
        Assert.Equal(expectedDescription, result.Value.Description);
        
        output.WriteLine($"TimeZone: {timeZoneId}, Language: {language}");
        output.WriteLine($"NextDate: {result.Value.NextDate}");
        output.WriteLine($"NextDate Offset: {result.Value.NextDate.Offset}");
    }

    [Theory, Trait("Category", "Localization")]
    [InlineData("es_ES", "cada mes el día 15. Próxima ejecución: miércoles, 15 de octubre de 2025 10:00:00")]
    [InlineData("en_US", "every month on day 15. Next execution: Wednesday, October 15, 2025 10:00:00")]
    [InlineData("en_GB", "every month on day 15. Next execution: Wednesday, 15 October 2025 10:00:00")]
    public void MonthlyRecurrence_ByDay_ShouldGenerateLocalizedDescription(string language, string expectedDescription) {
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
        Assert.Equal(expectedDescription, result.Value.Description);
    }

    [Theory, Trait("Category", "Localization")]
    [InlineData("es_ES", "cada mes el último Viernes. Próxima ejecución: viernes, 31 de octubre de 2025 14:00:00")]
    [InlineData("en_US", "every month the last Friday. Next execution: Friday, October 31, 2025 14:00:00")]
    [InlineData("en_GB", "every month the last Friday. Next execution: Friday, 31 October 2025 14:00:00")]
    public void MonthlyRecurrence_ByFrequencyAndDateType_ShouldGenerateLocalizedDescription(string language, string expectedDescription) {
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
        Assert.Equal(expectedDescription, result.Value.Description);
    }

    [Theory, Trait("Category", "Localization")]
    [InlineData("es_ES", "America/New_York", "cada semana en Martes, Jueves a las 15:00:00. Próxima ejecución: jueves, 2 de octubre de 2025 15:00:00")]
    [InlineData("en_US", "America/Los_Angeles", "every week on Tuesday, Thursday at 15:00:00. Next execution: Thursday, October 2, 2025 15:00:00")]
    [InlineData("en_GB", "Asia/Tokyo", "every week on Tuesday, Thursday at 15:00:00. Next execution: Thursday, 2 October 2025 15:00:00")]
    public void WeeklyRecurrence_ShouldGenerateLocalizedDescription_WithDifferentTimeZones(string language, string timeZoneId, string expectedDescription) {
        var tz = TimeZoneConverter.GetTimeZone(timeZoneId);
        var schedulerInput = new SchedulerInput();
        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.Language = language;
        schedulerInput.TimeZoneId = timeZoneId;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 12, 31)));
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Tuesday, DayOfWeek.Thursday];
        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = new TimeSpan(15, 0, 0);

        var result = SchedulerService.InitialOrchestator(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Description);
        Assert.Equal(expectedDescription, result.Value.Description);
        
        output.WriteLine($"TimeZone: {timeZoneId}, Language: {language}");
        output.WriteLine($"NextDate: {result.Value.NextDate}");
        output.WriteLine($"NextDate UTC: {result.Value.NextDate.UtcDateTime}");
    }

    [Theory, Trait("Category", "Localization")]
    [InlineData("es_ES", "cada mes el día 10. Próxima ejecución: viernes, 10 de octubre de 2025 08:00:00")]
    [InlineData("en_US", "every month on day 10. Next execution: Friday, October 10, 2025 08:00:00")]
    [InlineData("en_GB", "every month on day 10. Next execution: Friday, 10 October 2025 08:00:00")]
    public void MonthlyRecurrence_WithOccursEvery_ShouldGenerateLocalizedDescription(string language, string expectedDescription) {
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
        Assert.Equal(expectedDescription, result.Value.Description);
    }

    [Theory, Trait("Category", "Localization")]
    [InlineData("es_ES", "Ocurre una vez el miércoles, 15 de octubre de 2025 14:30:00 a partir de 01/10/2025")]
    [InlineData("en_US", "Occurs once on Wednesday, October 15, 2025 14:30:00 starting from 10/1/2025")]
    [InlineData("en_GB", "Occurs once on Wednesday, 15 October 2025 14:30:00 starting from 01/10/2025")]
    public void Once_Configuration_ShouldGenerateLocalizedDescription(string language, string expectedDescription) {
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
        Assert.Equal(expectedDescription, result.Value.Description);
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
    [InlineData("es_ES", "miércoles, 15 de octubre de 2025 14:30:45")]
    [InlineData("en_US", "Wednesday, October 15, 2025 14:30:45")]
    [InlineData("en_GB", "Wednesday, 15 October 2025 14:30:45")]
    public void DateFormatting_ShouldBeLocalizedCorrectly(string language, string expectedFormat) {
        var date = new DateTimeOffset(2025, 10, 15, 14, 30, 45, TimeSpan.Zero);
        var formattedDate = LocalizationService.FormatDate(date, language);

        Assert.NotEmpty(formattedDate);
        Assert.Equal(expectedFormat, formattedDate);
    }
}