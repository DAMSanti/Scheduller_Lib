using System;
using System.Collections.Generic;
using Scheduler_Lib.Core.Services.Utilities;
using Xunit;

namespace Scheduler_IntegrationTests.Integration;

public class TimeZoneConverterIntegrationTests {
    public static IEnumerable<object[]> GetTimezoneMappings() {
        yield return new object[] { "Central European Standard Time", "Europe/Madrid" };
        yield return new object[] { "Europe/Madrid", "Europe/Madrid" };
    yield return new object[] { "GMT Standard Time", "Europe/London" };
        yield return new object[] { "Atlantic/Canary", "Atlantic/Canary" };
        yield return new object[] { "Europe/London", "Europe/London" };
        yield return new object[] { "Eastern Standard Time", "America/New_York" };
        yield return new object[] { "America/New_York", "America/New_York" };
        yield return new object[] { "Central Standard Time", "America/Chicago" };
        yield return new object[] { "America/Chicago", "America/Chicago" };
        yield return new object[] { "Mountain Standard Time", "America/Denver" };
        yield return new object[] { "America/Denver", "America/Denver" };
        yield return new object[] { "Pacific Standard Time", "America/Los_Angeles" };
        yield return new object[] { "America/Los_Angeles", "America/Los_Angeles" };
        yield return new object[] { "Alaskan Standard Time", "America/Anchorage" };
        yield return new object[] { "America/Anchorage", "America/Anchorage" };
        yield return new object[] { "Hawaiian Standard Time", "Pacific/Honolulu" };
        yield return new object[] { "Pacific/Honolulu", "Pacific/Honolulu" };
        yield return new object[] { "My/Unknown_Timezone", "My/Unknown_Timezone" };
    }

    [Theory, MemberData(nameof(GetTimezoneMappings))]
    public void GetTimeZoneId_ShouldReturnExpectedMapping(string inputId, string expected) {
        var tz = TimeZoneInfo.CreateCustomTimeZone(inputId, TimeSpan.Zero, inputId, inputId);
        var result = TimeZoneConverter.GetTimeZoneId(tz);
        Assert.Equal(expected, result);
    }
}
