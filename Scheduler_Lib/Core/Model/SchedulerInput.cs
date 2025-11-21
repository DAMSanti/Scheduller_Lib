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
    public TimeSpan? OccursOnceAt { get; set; }
    public bool OccursEveryChk { get; set; }
    public TimeSpan? DailyPeriod { get; set; }
    public TimeSpan? DailyStartTime { get; set; }
    public TimeSpan? DailyEndTime { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public bool MonthlyDayChk { get; set; }
    public int? MonthlyDay { get; set; }
    public int? MonthlyDayPeriod { get; set; }
    public bool MonthlyTheChk { get; set; }
    public EnumMonthlyFrequency? MonthlyFrequency { get; set; }
    public EnumMonthlyDateType? MonthlyDateType { get; set; }
    public int? MonthlyThePeriod { get; set; }
    public string Language { get; set; } = "en_US";
    public string? TimeZoneId { get; set; }
}