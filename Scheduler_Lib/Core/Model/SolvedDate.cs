namespace Scheduler_Lib.Core.Model;

public struct SolvedDate() {
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset NewDate { get; set; }
    public List<DateTimeOffset>? FutureDates { get; set; }
}