using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Model.Enum;
using Scheduler_Lib.Core.Services.Description.Formatters;

namespace Scheduler_Lib.Core.Services.Description {
    public class WeeklyDescriptionBuilder {
        private readonly PeriodFormatter _periodFormatter;
        private readonly TimeFormatter _timeFormatter;

        public WeeklyDescriptionBuilder(PeriodFormatter periodFormatter, TimeFormatter timeFormatter) {
            _periodFormatter = periodFormatter;
            _timeFormatter = timeFormatter;
        }

        public string Build(SchedulerInput input, TimeZoneInfo tz, DateTimeOffset nextLocal) {
            var daysOfWeek = input.DaysOfWeek is { Count: > 0 }
                ? string.Join(", ", input.DaysOfWeek.Select(d => d.ToString()))
                : nextLocal.DayOfWeek.ToString();

            var period = input.DailyPeriod.HasValue 
                ? _periodFormatter.Format(input.DailyPeriod.Value) 
                : "1 week";

            var startDateStr = _timeFormatter.FormatDate(input.StartDate, tz);
            var weeklyPeriod = input.WeeklyPeriod ?? 1;

            if (input is { Periodicity: EnumConfiguration.Recurrent, DailyStartTime: not null, DailyEndTime: not null }) {
                var startTime = _timeFormatter.FormatTime(input.DailyStartTime.Value);
                var endTime = _timeFormatter.FormatTime(input.DailyEndTime.Value);
                return $"Occurs every {weeklyPeriod} week(s) on {daysOfWeek} every {period} between {startTime} and {endTime} starting on {startDateStr}";
            }

            return $"Occurs every {weeklyPeriod} week(s) on {daysOfWeek} every {period} starting on {startDateStr}";
        }
    }
}
