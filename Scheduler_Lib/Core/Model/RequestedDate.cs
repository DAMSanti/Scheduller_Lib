namespace Scheduler_Lib.Core.Model;

public class RequestedDate {
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public DateTimeOffset? ChangeDate { get; set; }
    public EnumPeriodicity Periodicity { get; set; }
    public TimeSpan? Period { get; set; }
    public EnumOcurrence Ocurrence { get; set; }
    public TimeSpan? DailyFrequency { get; set; }
    public TimeSpan? DailyStartTime { get; set; }
    public TimeSpan? DailyEndTime { get; set; }
    public int? WeeklyPeriod { get; set; }
    public List<DayOfWeek>? DaysOfWeek { get; set; }
}