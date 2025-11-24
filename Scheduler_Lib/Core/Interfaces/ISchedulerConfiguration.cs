using Scheduler_Lib.Core.Model;

namespace Scheduler_Lib.Core.Interfaces;

/// <summary>
/// Interface for scheduler configuration (Dependency Inversion Principle)
/// </summary>
public interface ISchedulerConfiguration
{
    DateTimeOffset CurrentDate { get; }
    EnumRecurrency Recurrency { get; }
    bool EnabledChk { get; }
    DateTimeOffset? TargetDate { get; }
    EnumConfiguration Periodicity { get; }
    int? WeeklyPeriod { get; }
    List<DayOfWeek>? DaysOfWeek { get; }
    bool OccursOnceChk { get; }
    TimeSpan? OccursOnceAt { get; }
    bool OccursEveryChk { get; }
    TimeSpan? DailyPeriod { get; }
    TimeSpan? DailyStartTime { get; }
    TimeSpan? DailyEndTime { get; }
    DateTimeOffset StartDate { get; }
    DateTimeOffset? EndDate { get; }
    bool MonthlyDayChk { get; }
    int? MonthlyDay { get; }
    int? MonthlyDayPeriod { get; }
    bool MonthlyTheChk { get; }
    EnumMonthlyFrequency? MonthlyFrequency { get; }
    EnumMonthlyDateType? MonthlyDateType { get; }
    int? MonthlyThePeriod { get; }
    string Language { get; }
    string? TimeZoneId { get; }
}
