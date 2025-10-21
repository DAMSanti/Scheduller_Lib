namespace Scheduler_Lib.Core.Model;

public class SchedulerInput {
    public DateTimeOffset CurrentDate { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public DateTimeOffset? TargetDate { get; set; }
    public EnumConfiguration Periodicity { get; set; }
    public TimeSpan? DailyPeriod { get; set; }
    public EnumRecurrency Recurrency { get; set; }
    public TimeSpan? DailyFrequency { get; set; }
    public TimeSpan? DailyStartTime { get; set; }
    public TimeSpan? DailyEndTime { get; set; }
    public int? WeeklyPeriod { get; set; }
    public List<DayOfWeek>? DaysOfWeek { get; set; }
}    