using System;
using Scheduler_Lib.Core.Services.Utilities;

namespace Scheduler_Lib.Core.Services.Utilities;

internal static class OccursOnceHelper {
    public static DateTimeOffset ApplyOccursOnceAt(DateTimeOffset candidate, TimeSpan? occursOnceAt, TimeZoneInfo tz) {
        if (!occursOnceAt.HasValue)
            return candidate;

        var occursTime = occursOnceAt.Value;
        var nextDate = candidate.DateTime.Date;
        var nextWithTime = new DateTime(nextDate.Year, nextDate.Month, nextDate.Day,
            occursTime.Hours, occursTime.Minutes, occursTime.Seconds, DateTimeKind.Unspecified);

        return TimeZoneConverter.CreateDateTimeOffset(nextWithTime, tz);
    }
}
