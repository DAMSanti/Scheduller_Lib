using System.Globalization;

namespace Scheduler_Lib.Core.Services;

public static class LocalizationService {
    private static readonly Dictionary<string, CultureInfo> SupportedCultures = new() {
        { "es_ES", new CultureInfo("es_ES") },
        { "en_US", new CultureInfo("en_US") },
        { "en_GB", new CultureInfo("en_GB") },
    };

    public static CultureInfo GetCulture(string language) {
        return SupportedCultures.TryGetValue(language, out var culture)
            ? culture
            : SupportedCultures["es_ES"];
    }

    public static string FormatDate(DateTimeOffset date, string language) {
        var culture = GetCulture(language);

        return language switch {
            "es_ES" => date.DateTime.ToString("dddd, d 'de' MMMM 'de' yyyy HH:mm:ss", culture),
            "en_GB" => date.DateTime.ToString("dddd, d MMMM yyyy HH:mm:ss", culture),
            "en_US" => date.DateTime.ToString("dddd, MMMM d, yyyy HH:mm:ss", culture),
            _ => date.DateTime.ToString("dddd, d MMMM yyyy HH:mm:ss", culture),
        };
    }

    public static string FormatTime(TimeSpan time) {
        return time.ToString(@"hh\:mm\:ss");
    }

    public static bool IsSupportedLanguage(string language) {
        return SupportedCultures.ContainsKey(language);
    }

    public static IEnumerable<string> GetSupportedLanguages() {
        return SupportedCultures.Keys;
    }
}