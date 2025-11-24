using Scheduler_Lib.Core.Interfaces;
using Scheduler_Lib.Core.Model;

namespace Scheduler_Lib.Core.Adapters;

/// <summary>
/// Adapter to make SchedulerInput compatible with ISchedulerConfiguration (Adapter Pattern)
/// </summary>
public class SchedulerInputAdapter : ISchedulerConfiguration
{
    private readonly SchedulerInput _schedulerInput;

    public SchedulerInputAdapter(SchedulerInput schedulerInput)
    {
        _schedulerInput = schedulerInput ?? throw new ArgumentNullException(nameof(schedulerInput));
    }

    public DateTimeOffset CurrentDate => _schedulerInput.CurrentDate;
    public EnumRecurrency Recurrency => _schedulerInput.Recurrency;
    public bool EnabledChk => _schedulerInput.EnabledChk;
    public DateTimeOffset? TargetDate => _schedulerInput.TargetDate;
    public EnumConfiguration Periodicity => _schedulerInput.Periodicity;
    public int? WeeklyPeriod => _schedulerInput.WeeklyPeriod;
    public List<DayOfWeek>? DaysOfWeek => _schedulerInput.DaysOfWeek;
    public bool OccursOnceChk => _schedulerInput.OccursOnceChk;
    public TimeSpan? OccursOnceAt => _schedulerInput.OccursOnceAt;
    public bool OccursEveryChk => _schedulerInput.OccursEveryChk;
    public TimeSpan? DailyPeriod => _schedulerInput.DailyPeriod;
    public TimeSpan? DailyStartTime => _schedulerInput.DailyStartTime;
    public TimeSpan? DailyEndTime => _schedulerInput.DailyEndTime;
    public DateTimeOffset StartDate => _schedulerInput.StartDate;
    public DateTimeOffset? EndDate => _schedulerInput.EndDate;
    public bool MonthlyDayChk => _schedulerInput.MonthlyDayChk;
    public int? MonthlyDay => _schedulerInput.MonthlyDay;
    public int? MonthlyDayPeriod => _schedulerInput.MonthlyDayPeriod;
    public bool MonthlyTheChk => _schedulerInput.MonthlyTheChk;
    public EnumMonthlyFrequency? MonthlyFrequency => _schedulerInput.MonthlyFrequency;
    public EnumMonthlyDateType? MonthlyDateType => _schedulerInput.MonthlyDateType;
    public int? MonthlyThePeriod => _schedulerInput.MonthlyThePeriod;
    public string Language => _schedulerInput.Language;
    public string? TimeZoneId => _schedulerInput.TimeZoneId;
}
