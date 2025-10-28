namespace Scheduler_Lib.Core.Model;

public class SchedulerInput {
    public DateTimeOffset CurrentDate { get; set; }
    public EnumRecurrency Recurrency { get; set; }
    public bool EnabledChk { get; set; }
    public DateTimeOffset? TargetDate { get; set; }
    public EnumConfiguration Periodicity { get; set; }
    public int? WeeklyPeriod { get; set; }
    public List<DayOfWeek>? DaysOfWeek { get; set; }
    public bool OccursOnceChk { get; set; }
    public DateTimeOffset? OccursOnceAt { get; set; }
    public bool OccursEveryChk { get; set; }
    public TimeSpan? DailyPeriod { get; set; }
    public TimeSpan? DailyStartTime { get; set; }
    public TimeSpan? DailyEndTime { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
}