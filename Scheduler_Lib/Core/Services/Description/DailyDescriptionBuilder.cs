using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Model.Enum;
using Scheduler_Lib.Core.Services.Description.Formatters;

namespace Scheduler_Lib.Core.Services.Description {
    public class DailyDescriptionBuilder {
        private readonly PeriodFormatter _periodFormatter;
        private readonly TimeFormatter _timeFormatter;

        public DailyDescriptionBuilder(PeriodFormatter periodFormatter, TimeFormatter timeFormatter) {
            _periodFormatter = periodFormatter;
            _timeFormatter = timeFormatter;
        }

        public string Build(SchedulerInput input, TimeZoneInfo tz, DateTimeOffset nextLocal) {
            var startDateStr = _timeFormatter.FormatDate(input.StartDate, tz);

            if (input.Periodicity != EnumConfiguration.Recurrent) {
                var dateStr = nextLocal.Date.ToShortDateString();
                var timeStr = nextLocal.DateTime.ToShortTimeString();
                return $"Occurs once: Schedule will be used on {dateStr} at {timeStr} starting on {startDateStr}";
            }

            var periodStr = input.DailyPeriod.HasValue
                ? _periodFormatter.Format(input.DailyPeriod.Value)
                : "1 day";

            if (input.DailyStartTime.HasValue && input.DailyEndTime.HasValue) {
                var startTime = _timeFormatter.FormatTime(input.DailyStartTime.Value);
                var endTime = _timeFormatter.FormatTime(input.DailyEndTime.Value);
                return $"Occurs every {periodStr} between {startTime} and {endTime} at starting on {startDateStr}";
            }

            var dateStr2 = nextLocal.Date.ToShortDateString();
            var timeStr2 = nextLocal.DateTime.ToShortTimeString();
            return $"Occurs every {periodStr}. Schedule will be used on {dateStr2} at {timeStr2} starting on {startDateStr}";
        }
    }
}
