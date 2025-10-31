using System;
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Model.Enum;
using Scheduler_Lib.Core.Services.Description.Formatters;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Services.Description {
    public class DescriptionService {
        private readonly PeriodFormatter _periodFormatter;
        private readonly TimeFormatter _timeFormatter;
        private readonly DailyDescriptionBuilder _dailyBuilder;
        private readonly WeeklyDescriptionBuilder _weeklyBuilder;
        private readonly MonthlyDescriptionBuilder _monthlyBuilder;

        public DescriptionService() {
            _periodFormatter = new PeriodFormatter();
            _timeFormatter = new TimeFormatter();
            _dailyBuilder = new DailyDescriptionBuilder(_periodFormatter, _timeFormatter);
            _weeklyBuilder = new WeeklyDescriptionBuilder(_periodFormatter, _timeFormatter);
            _monthlyBuilder = new MonthlyDescriptionBuilder(_periodFormatter, _timeFormatter);
        }

        public string BuildDescription(SchedulerInput schedulerInput, TimeZoneInfo tz, DateTimeOffset nextLocal) {
            if (schedulerInput is { Recurrency: EnumRecurrency.Weekly, Periodicity: EnumConfiguration.Once })
                return $"ERROR: {Messages.ErrorOnceWeekly}";

            return schedulerInput.Recurrency switch {
                EnumRecurrency.Daily => _dailyBuilder.Build(schedulerInput, tz, nextLocal),
                EnumRecurrency.Weekly => _weeklyBuilder.Build(schedulerInput, tz, nextLocal),
                EnumRecurrency.Monthly => _monthlyBuilder.Build(schedulerInput, tz, nextLocal),
                _ => BuildOnceDescription(schedulerInput, tz, nextLocal)
            };
        }

        private string BuildOnceDescription(SchedulerInput schedulerInput, TimeZoneInfo tz, DateTimeOffset nextLocal) {
            var startDateStr = _timeFormatter.FormatDate(schedulerInput.StartDate, tz);
            var dateStr = nextLocal.Date.ToShortDateString();
            var timeStr = nextLocal.DateTime.ToShortTimeString();

            return $"Occurs once: Schedule will be used on {dateStr} at {timeStr} starting on {startDateStr}";
        }
    }
}
