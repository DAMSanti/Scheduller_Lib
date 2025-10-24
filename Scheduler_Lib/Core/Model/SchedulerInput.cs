namespace Scheduler_Lib.Core.Model;

public class SchedulerInput {
    public DateTimeOffset CurrentDate { get; set; }
    public EnumRecurrency Recurrency { get; set; }
    public bool Enabled { get; set; } = true;
    public DateTimeOffset? TargetDate { get; set; }
    public EnumConfiguration Periodicity { get; set; }
    public int? WeeklyPeriod { get; set; }
    public List<DayOfWeek>? DaysOfWeek { get; set; }
    public bool OccursOnce { get; set; } = false;
    public DateTimeOffset? OccursOnceAt { get; set; }
    public bool OccursEvery { get; set; } = true;
    public TimeSpan? DailyPeriod { get; set; }
    public TimeSpan? DailyStartTime { get; set; }
    public TimeSpan? DailyEndTime { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
}