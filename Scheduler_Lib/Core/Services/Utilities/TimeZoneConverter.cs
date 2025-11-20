using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Services.Utilities;

public static class TimeZoneConverter {
    public static TimeZoneInfo GetTimeZone() {
        return TimeZoneInfo.FindSystemTimeZoneById(Config.TimeZoneId);
    }

    public static DateTimeOffset CreateDateTimeOffset(DateTime localWallClock, TimeZoneInfo tz) {
        return new DateTimeOffset(localWallClock, tz.GetUtcOffset(localWallClock));
    }

    public static DateTime ConvertFromUtc(DateTime utcTime, TimeZoneInfo tz) {
        return TimeZoneInfo.ConvertTimeFromUtc(utcTime, tz);
    }

    public static TimeSpan GetUtcOffset(DateTime dateTime, TimeZoneInfo tz) {
        return tz.GetUtcOffset(dateTime);
    }

    public static string GetTimeZoneId() {
        TimeZoneInfo localZone = TimeZoneInfo.Local;
        return GetTimeZoneId(localZone);
    }

    // Overload for testability: resolves timezone mapping from a provided TimeZoneInfo
    public static string GetTimeZoneId(TimeZoneInfo localZone) {
        if (localZone.Id == "Central European Standard Time" || localZone.Id == "Europe/Madrid")
            return "Europe/Madrid";

        if (localZone.Id == "Atlantic/Canary")
            return "Atlantic/Canary";

        if (localZone.Id == "GMT Standard Time" || localZone.Id == "Europe/London")
            return "Europe/London";

        switch (localZone.Id) {
            case "Eastern Standard Time":
            case "America/New_York":
                return "America/New_York";
            case "Central Standard Time":
            case "America/Chicago":
                return "America/Chicago";
            case "Mountain Standard Time":
            case "America/Denver":
                return "America/Denver";
            case "Pacific Standard Time":
            case "America/Los_Angeles":
                return "America/Los_Angeles";
            case "Alaskan Standard Time":
            case "America/Anchorage":
                return "America/Anchorage";
            case "Hawaiian Standard Time":
            case "Pacific/Honolulu":
                return "Pacific/Honolulu";
        }

        return localZone.Id;
    }
}
