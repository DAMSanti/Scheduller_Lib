using Scheduler_Lib.Core.Services.Utilities;

namespace Scheduler_Lib.Resources;

public static class Config {
    public const int MaxIterations = int.MaxValue;
    public const int EffectiveEndDateMultiplier = 1000;
    public static readonly string TimeZoneId = TimeZoneConverter.GetTimeZoneId(TimeZoneInfo.Local);
}   