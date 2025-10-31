using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services.Calculation.Helpers;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Services.Calculation;

public class WeeklyCalculator {
    private readonly DateTimeHelper _dateTimeHelper;

    public WeeklyCalculator(DateTimeHelper dateTimeHelper) {
        _dateTimeHelper = dateTimeHelper;
    }

    public List<DateTimeOffset> Calculate(SchedulerInput schedulerInput, TimeZoneInfo tz) {
        var dates = new List<DateTimeOffset>();
        var baseLocal = _dateTimeHelper.GetBaseLocal(schedulerInput);
        var nextEligible = SelectNextEligibleDate(
            new DateTimeOffset(baseLocal, tz.GetUtcOffset(baseLocal)),
            schedulerInput.DaysOfWeek!,
            tz
        );

        var weekStart = baseLocal.Date;
        var endLocal = schedulerInput.EndDate ?? throw new ArgumentException("EndDate is required");

        var iteration = 0;
        const int maxIterations = Config.MaxIterations;

        while (weekStart <= endLocal && iteration < maxIterations) {
            GenerateWeeklySlotsForWeek(weekStart, schedulerInput, tz, nextEligible, dates);

            if (dates.Count >= maxIterations)
                return dates;

            var stepDays = 7 * schedulerInput.WeeklyPeriod!.Value;
            if (!_dateTimeHelper.TryAddDaysSafely(weekStart, stepDays, out var nextWeekStart))
                break;

            weekStart = nextWeekStart;
            iteration++;
        }

        dates.Sort();
        return dates;
    }

    public DateTimeOffset SelectNextEligibleDate(DateTimeOffset targetDate, List<DayOfWeek> daysOfWeek, TimeZoneInfo tz) {
        if (targetDate == DateTimeOffset.MinValue)
            return DateTimeOffset.MinValue;

        var candidates = daysOfWeek
            .Select(day => _dateTimeHelper.NextWeekday(targetDate.DateTime, day, tz))
            .OrderBy(d => d.DateTime)
            .ToList();

        return candidates.Count > 0
            ? candidates.First()
            : new DateTimeOffset(targetDate.DateTime, tz.GetUtcOffset(targetDate.DateTime));
    }

    private void GenerateWeeklySlotsForWeek(DateTime weekStart, SchedulerInput schedulerInput, TimeZoneInfo tz, DateTimeOffset nextEligible, List<DateTimeOffset> accumulator) {
        foreach (var day in schedulerInput.DaysOfWeek!) {
            var timeOfDay = schedulerInput.TargetDate?.TimeOfDay ?? schedulerInput.StartDate.TimeOfDay;
            var candidateLocal = _dateTimeHelper.GetCandidateLocalForWeekAndDay(weekStart, day, timeOfDay);

            if (candidateLocal == null) continue;

            var candidate = _dateTimeHelper.CreateDateTimeOffset(candidateLocal.Value, tz);

            if (candidate <= nextEligible) continue;
            if (!accumulator.Contains(candidate))
                accumulator.Add(candidate);
        }
    }
}