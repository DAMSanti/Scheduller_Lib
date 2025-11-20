using System;
using System.Collections.Generic;
using Scheduler_Lib.Core.Services.Utilities;
using Xunit;

namespace Scheduler_IntegrationTests.Integration;

public class TimeZoneConverterIntegrationTests {
    public static TheoryData<string, string> GetTimezoneMappings() {
        return new TheoryData<string, string> {
            { "Central European Standard Time", "Europe/Madrid" },
            { "Europe/Madrid", "Europe/Madrid" },
            { "GMT Standard Time", "Europe/London" },
            { "Atlantic/Canary", "Atlantic/Canary" },
            { "Europe/London", "Europe/London" },
            { "Eastern Standard Time", "America/New_York" },
            { "America/New_York", "America/New_York" },
            { "Central Standard Time", "America/Chicago" },
            { "America/Chicago", "America/Chicago" },
            { "Mountain Standard Time", "America/Denver" },
            { "America/Denver", "America/Denver" },
            { "Pacific Standard Time", "America/Los_Angeles" },
            { "America/Los_Angeles", "America/Los_Angeles" },
            { "Alaskan Standard Time", "America/Anchorage" },
            { "America/Anchorage", "America/Anchorage" },
            { "Hawaiian Standard Time", "Pacific/Honolulu" },
            { "Pacific/Honolulu", "Pacific/Honolulu" },
            { "My/Unknown_Timezone", "Europe/Madrid" }
        };
    }

    [Theory, MemberData(nameof(GetTimezoneMappings))]
    public void GetTimeZoneId_ShouldReturnExpectedMapping(string inputId, string expected) {
        var tz = TimeZoneInfo.CreateCustomTimeZone(inputId, TimeSpan.Zero, inputId, inputId);
        var result = TimeZoneConverter.GetTimeZoneId(tz);
        Assert.Equal(expected, result);
    }
}
