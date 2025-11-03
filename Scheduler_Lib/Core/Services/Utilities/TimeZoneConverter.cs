using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Services.Utilities;

public static class TimeZoneConverter {
    public static TimeZoneInfo GetTimeZone() {
        return TimeZoneInfo.FindSystemTimeZoneById(Config.TimeZoneId);
    }

    public static DateTimeOffset CreateDateTimeOffset(DateTime localWallClock, TimeZoneInfo tz) {
        return new DateTimeOffset(localWallClock, tz.GetUtcOffset(localWallClock));
    }

    public static TimeSpan GetUtcOffset(DateTime dateTime, TimeZoneInfo tz) {
        return tz.GetUtcOffset(dateTime);
    }

    public static DateTime ConvertFromUtc(DateTime utcTime, TimeZoneInfo tz) {
        return TimeZoneInfo.ConvertTimeFromUtc(utcTime, tz);
    }

    public static DateTimeOffset ConvertToTimeZone(DateTimeOffset dateTimeOffset, TimeZoneInfo tz) {
        var utcTime = dateTimeOffset.UtcDateTime;
        var localInTz = ConvertFromUtc(utcTime, tz);
        return CreateDateTimeOffset(localInTz, tz);
    }
}
