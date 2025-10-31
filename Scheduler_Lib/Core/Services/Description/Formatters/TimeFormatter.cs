using Scheduler_Lib.Core.Model;

namespace Scheduler_Lib.Core.Services.Description.Formatters;

public class TimeFormatter {
    public string FormatTime(TimeSpan timeSpan) {
        var dateTime = DateTime.Today.Add(timeSpan);
        var hour12 = dateTime.Hour % 12;
        if (hour12 == 0) hour12 = 12;

        var period = dateTime.Hour < 12 ? "AM" : "PM";
        return $"{hour12:D2}:{dateTime.Minute:D2} {period}";
    }

    public string FormatDate(DateTimeOffset dto, TimeZoneInfo tz) {
        var converted = TimeZoneInfo.ConvertTime(dto, tz);
        return converted.Date.ToShortDateString();
    }
}