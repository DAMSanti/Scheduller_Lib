using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Core.Services.Localization;
using Scheduler_Lib.Core.Services.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Scheduler_IntegrationTests.Integration;

public class LocalizationIntegrationTests(ITestOutputHelper output) {

    #region Weekly Recurrence - All Languages

    [Theory, Trait("Category", "Localization")]
    [InlineData("es-ES")]
    [InlineData("en-US")]
    [InlineData("en-GB")]
    [InlineData("fr-FR")]
    [InlineData("de-DE")]
    [InlineData("pt-BR")]
    public void WeeklyRecurrence_ShouldGenerateLocalizedDescription_WhenLanguageIsSupported(string language) {
        var tz = TimeZoneConverter.GetTimeZone();
        var schedulerInput = new SchedulerInput {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            Language = language,
            StartDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01))),
            CurrentDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01))),
            EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 12, 31))),
            WeeklyPeriod = 1,
            DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday],
            OccursOnceChk = true,
            OccursOnceAt = new TimeSpan(10, 30, 0)
        };

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Description);
        output.WriteLine($"🌐 Idioma: {language}");
        output.WriteLine($"📝 Descripción: {result.Value.Description}");
        output.WriteLine($"📅 Próxima ejecución: {result.Value.NextDate}");
        output.WriteLine("");
    }

    [Theory, Trait("Category", "Localization")]
    [InlineData("es-ES")]
    [InlineData("en-US")]
    [InlineData("en-GB")]
    [InlineData("fr-FR")]
    [InlineData("de-DE")]
    [InlineData("pt-BR")]
    public void WeeklyRecurrence_WithOccursEvery_ShouldGenerateLocalizedDescription(string language) {
        var tz = TimeZoneConverter.GetTimeZone();
        var schedulerInput = new SchedulerInput {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            Language = language,
            StartDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01))),
            CurrentDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01))),
            EndDate = new DateTimeOffset(2025, 10, 15, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 10, 15))),
            WeeklyPeriod = 1,
            DaysOfWeek = [DayOfWeek.Monday],
            OccursEveryChk = true,
            DailyPeriod = TimeSpan.FromHours(2),
            DailyStartTime = TimeSpan.FromHours(9),
            DailyEndTime = TimeSpan.FromHours(17)
        };

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Description);
        output.WriteLine($"🌐 Idioma: {language} (OccursEvery)");
        output.WriteLine($"📝 Descripción: {result.Value.Description}");
        output.WriteLine("");
    }

    #endregion

    #region Daily Recurrence - All Languages

    [Theory, Trait("Category", "Localization")]
    [InlineData("es-ES")]
    [InlineData("en-US")]
    [InlineData("en-GB")]
    [InlineData("fr-FR")]
    [InlineData("de-DE")]
    [InlineData("pt-BR")]
    public void DailyRecurrence_ShouldGenerateLocalizedDescription_WhenLanguageIsSupported(string language) {
        var tz = TimeZoneConverter.GetTimeZone();
        var schedulerInput = new SchedulerInput {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Daily,
            Language = language,
            StartDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01))),
            CurrentDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01))),
            EndDate = new DateTimeOffset(2025, 10, 31, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 10, 31))),
            OccursOnceChk = true,
            OccursOnceAt = new TimeSpan(14, 30, 0)
        };

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Description);
        output.WriteLine($"🌐 Idioma: {language} (Daily)");
        output.WriteLine($"📝 Descripción: {result.Value.Description}");
        output.WriteLine($"📅 Próxima ejecución: {result.Value.NextDate}");
        output.WriteLine("");
    }

    [Theory, Trait("Category", "Localization")]
    [InlineData("es-ES")]
    [InlineData("en-US")]
    [InlineData("en-GB")]
    [InlineData("fr-FR")]
    [InlineData("de-DE")]
    [InlineData("pt-BR")]
    public void DailyRecurrence_WithOccursEvery_ShouldGenerateLocalizedDescription(string language) {
        var tz = TimeZoneConverter.GetTimeZone();
        var schedulerInput = new SchedulerInput {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Daily,
            Language = language,
            StartDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01))),
            CurrentDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01))),
            EndDate = new DateTimeOffset(2025, 10, 15, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 10, 15))),
            OccursEveryChk = true,
            DailyPeriod = TimeSpan.FromHours(3),
            DailyStartTime = TimeSpan.FromHours(8),
            DailyEndTime = TimeSpan.FromHours(20)
        };

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Description);
        output.WriteLine($"🌐 Idioma: {language} (Daily OccursEvery)");
        output.WriteLine($"📝 Descripción: {result.Value.Description}");
        output.WriteLine("");
    }

    #endregion

    #region Monthly Recurrence - All Languages

    [Theory, Trait("Category", "Localization")]
    [InlineData("es-ES")]
    [InlineData("en-US")]
    [InlineData("en-GB")]
    [InlineData("fr-FR")]
    [InlineData("de-DE")]
    [InlineData("pt-BR")]
    public void MonthlyRecurrence_ByDay_ShouldGenerateLocalizedDescription(string language) {
        var tz = TimeZoneConverter.GetTimeZone();
        var schedulerInput = new SchedulerInput {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Monthly,
            Language = language,
            StartDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01))),
            CurrentDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01))),
            EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 12, 31))),
            MonthlyDayChk = true,
            MonthlyDay = 15,
            MonthlyDayPeriod = 1,
            OccursOnceChk = true,
            OccursOnceAt = new TimeSpan(10, 0, 0)
        };

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Description);
        output.WriteLine($"🌐 Idioma: {language} (Monthly by Day)");
        output.WriteLine($"📝 Descripción: {result.Value.Description}");
        output.WriteLine($"📅 Próxima ejecución: {result.Value.NextDate}");
        output.WriteLine("");
    }

    [Theory, Trait("Category", "Localization")]
    [InlineData("es-ES")]
    [InlineData("en-US")]
    [InlineData("en-GB")]
    [InlineData("fr-FR")]
    [InlineData("de-DE")]
    [InlineData("pt-BR")]
    public void MonthlyRecurrence_ByFrequencyAndDateType_ShouldGenerateLocalizedDescription(string language) {
        var tz = TimeZoneConverter.GetTimeZone();
        var schedulerInput = new SchedulerInput {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Monthly,
            Language = language,
            StartDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01))),
            CurrentDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01))),
            EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 12, 31))),
            MonthlyTheChk = true,
            MonthlyFrequency = EnumMonthlyFrequency.Last,
            MonthlyDateType = EnumMonthlyDateType.Friday,
            MonthlyThePeriod = 1,
            OccursOnceChk = true,
            OccursOnceAt = new TimeSpan(14, 0, 0)
        };

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Description);
        output.WriteLine($"🌐 Idioma: {language} (Monthly by Frequency)");
        output.WriteLine($"📝 Descripción: {result.Value.Description}");
        output.WriteLine($"📅 Próxima ejecución: {result.Value.NextDate}");
        output.WriteLine("");
    }

    [Theory, Trait("Category", "Localization")]
    [InlineData("es-ES")]
    [InlineData("en-US")]
    [InlineData("en-GB")]
    [InlineData("fr-FR")]
    [InlineData("de-DE")]
    [InlineData("pt-BR")]
    public void MonthlyRecurrence_WithOccursEvery_ShouldGenerateLocalizedDescription(string language) {
        var tz = TimeZoneConverter.GetTimeZone();
        var schedulerInput = new SchedulerInput {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Monthly,
            Language = language,
            StartDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01))),
            CurrentDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01))),
            EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 12, 31))),
            MonthlyDayChk = true,
            MonthlyDay = 10,
            MonthlyDayPeriod = 1,
            OccursEveryChk = true,
            DailyPeriod = TimeSpan.FromHours(4),
            DailyStartTime = TimeSpan.FromHours(8),
            DailyEndTime = TimeSpan.FromHours(20)
        };

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Description);
        output.WriteLine($"🌐 Idioma: {language} (Monthly OccursEvery)");
        output.WriteLine($"📝 Descripción: {result.Value.Description}");
        output.WriteLine("");
    }

    #endregion

    #region Once Configuration - All Languages

    [Theory, Trait("Category", "Localization")]
    [InlineData("es-ES")]
    [InlineData("en-US")]
    [InlineData("en-GB")]
    [InlineData("fr-FR")]
    [InlineData("de-DE")]
    [InlineData("pt-BR")]
    public void Once_Configuration_ShouldGenerateLocalizedDescription(string language) {
        var tz = TimeZoneConverter.GetTimeZone();
        var schedulerInput = new SchedulerInput {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Once,
            Recurrency = EnumRecurrency.Daily,
            Language = language,
            StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01))),
            CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01))),
            TargetDate = new DateTimeOffset(2025, 10, 15, 14, 30, 0, tz.GetUtcOffset(new DateTime(2025, 10, 15, 14, 30, 0)))
        };

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Description);
        output.WriteLine($"🌐 Idioma: {language} (Once)");
        output.WriteLine($"📝 Descripción: {result.Value.Description}");
        output.WriteLine($"📅 Próxima ejecución: {result.Value.NextDate}");
        output.WriteLine("");
    }

    #endregion

    #region Language Validation Tests

    [Fact, Trait("Category", "Localization")]
    public void Validation_ShouldFail_WhenLanguageIsNotSupported() {
        var schedulerInput = new SchedulerInput {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            Language = "ja-JP", // Idioma no soportado
            StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero),
            CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero),
            WeeklyPeriod = 1,
            DaysOfWeek = [DayOfWeek.Monday]
        };

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains("Idioma no soportado", result.Error ?? string.Empty);
        output.WriteLine($"❌ Error esperado: {result.Error}");
    }

    [Fact, Trait("Category", "Localization")]
    public void Validation_ShouldFail_WhenLanguageIsNull() {
        var schedulerInput = new SchedulerInput {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            Language = null!,
            StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero),
            CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero),
            WeeklyPeriod = 1,
            DaysOfWeek = [DayOfWeek.Monday]
        };

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains("El idioma es obligatorio", result.Error ?? string.Empty);
        output.WriteLine($"❌ Error esperado: {result.Error}");
    }

    [Fact, Trait("Category", "Localization")]
    public void Validation_ShouldFail_WhenLanguageIsEmpty() {
        var schedulerInput = new SchedulerInput {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            Language = "",
            StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero),
            CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero),
            WeeklyPeriod = 1,
            DaysOfWeek = [DayOfWeek.Monday]
        };

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains("El idioma es obligatorio", result.Error ?? string.Empty);
        output.WriteLine($"❌ Error esperado: {result.Error}");
    }

    [Fact, Trait("Category", "Localization")]
    public void LocalizationService_ShouldReturnAllSupportedLanguages() {
        var supportedLanguages = LocalizationService.GetSupportedLanguages().ToList();

        Assert.NotEmpty(supportedLanguages);
        Assert.Equal(6, supportedLanguages.Count);

        output.WriteLine("✅ Idiomas soportados:");
        foreach (var lang in supportedLanguages) {
            output.WriteLine($"   - {lang}");
            Assert.True(LocalizationService.IsSupportedLanguage(lang));
        }
    }

    #endregion

    #region Date Formatting Tests

    [Theory, Trait("Category", "Localization")]
    [InlineData("es-ES")]
    [InlineData("en-US")]
    [InlineData("en-GB")]
    [InlineData("fr-FR")]
    [InlineData("de-DE")]
    [InlineData("pt-BR")]
    public void DateFormatting_ShouldBeLocalizedCorrectly(string language) {
        var date = new DateTimeOffset(2025, 10, 15, 14, 30, 45, TimeSpan.Zero);
        var formattedDate = LocalizationService.FormatDate(date, language);

        Assert.NotEmpty(formattedDate);
        output.WriteLine($"🌐 {language}: {formattedDate}");
    }

    [Theory, Trait("Category", "Localization")]
    [InlineData("es-ES")]
    [InlineData("en-US")]
    [InlineData("en-GB")]
    [InlineData("fr-FR")]
    [InlineData("de-DE")]
    [InlineData("pt-BR")]
    public void DayOfWeekFormatting_ShouldBeLocalizedCorrectly(string language) {
        var daysOfWeek = new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday };

        output.WriteLine($"🌐 {language}:");
        foreach (var day in daysOfWeek) {
            var formattedDay = LocalizationService.FormatDayOfWeek(day, language);
            Assert.NotEmpty(formattedDay);
            output.WriteLine($"   {day} → {formattedDay}");
        }
    }

    #endregion
}