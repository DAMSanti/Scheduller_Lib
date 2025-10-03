using Scheduler_Lib.Enum;

namespace Scheduler_Lib.Classes;

public class RequestedDate {
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public bool Enabled { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public DateTimeOffset? ChangeDate { get; set; }
    public Periodicity? Periodicity { get; set; }
    public TimeSpan? Offset { get; set; }

    public RequestedDate() {
        if (EndDate == default && Offset.HasValue) {
            EndDate = Date + Offset.Value;
        }
    }
}