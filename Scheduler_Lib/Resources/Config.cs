using Scheduler_Lib.Core.Services.Utilities;

namespace Scheduler_Lib.Resources;

public static class Config {
    public const int MaxIterations = int.MaxValue;
    /// <summary>
    /// Multiplier used to compute an effective end date when <c>EndDate</c> is not provided.
    /// Effective end date = baseDate + (period * EffectiveEndDateMultiplier).
    /// Default value 1000 means the window is period * 1000.
    /// </summary>
    public const int EffectiveEndDateMultiplier = 1000;
    public static readonly string TimeZoneId = TimeZoneConverter.GetTimeZoneId();
}   