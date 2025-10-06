using Scheduler_Lib.Core.Enum;

namespace Scheduler_Lib.Core.Classes;

public class RequestedDate {
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public DateTimeOffset? ChangeDate { get; set; }
    public Periodicity Periodicity { get; set; }
    public int? Offset { get; set; }
}