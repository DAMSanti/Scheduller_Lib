using Scheduler_Lib.Core.Model;

namespace Scheduler_Lib.Core.Interfaces;

/// <summary>
/// Interface for description building (Builder Pattern)
/// </summary>
public interface IDescriptionBuilder
{
    IDescriptionBuilder SetConfiguration(ISchedulerConfiguration configuration);
    IDescriptionBuilder SetTimeZone(TimeZoneInfo timeZone);
    IDescriptionBuilder SetNextDate(DateTimeOffset nextDate);
    IDescriptionBuilder SetLanguage(string language);
    string Build();
    void Reset();
}
