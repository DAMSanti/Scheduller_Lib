namespace Scheduler_Lib.Core.Classes;

public class SolvedDate {
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset NewDate { get; set; }
    public List<DateTimeOffset>? FutureDates { get; set; }
}