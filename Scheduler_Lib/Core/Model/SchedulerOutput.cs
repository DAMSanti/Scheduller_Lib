namespace Scheduler_Lib.Core.Model;

public struct SchedulerOutput() {
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset NextDate { get; set; }
    public List<DateTimeOffset>? FutureDates { get; set; }
}