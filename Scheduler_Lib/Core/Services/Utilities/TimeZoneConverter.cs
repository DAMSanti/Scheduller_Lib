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

    public static string GetTimeZoneId(TimeZoneInfo localZone) {
        if (localZone.Id == "Central European Standard Time" || localZone.Id == "Europe/Madrid")
            return "Europe/Madrid";

        if (localZone.Id == "Atlantic/Canary")
            return "Atlantic/Canary";

        if (localZone.Id == "GMT Standard Time" || localZone.Id == "Europe/London")
            return "Europe/London";

        return localZone.Id switch {
            "Eastern Standard Time" or "America/New_York" => "America/New_York",
            "Central Standard Time" or "America/Chicago" => "America/Chicago",
            "Mountain Standard Time" or "America/Denver" => "America/Denver",
            "Pacific Standard Time" or "America/Los_Angeles" => "America/Los_Angeles",
            "Alaskan Standard Time" or "America/Anchorage" => "America/Anchorage",
            "Hawaiian Standard Time" or "Pacific/Honolulu" => "Pacific/Honolulu",
            _ => "Europe/Madrid",
        };

        return "Europe/Madrid";
    }
}
