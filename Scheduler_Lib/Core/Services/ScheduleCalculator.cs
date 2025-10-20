using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Services;
public static class ScheduleCalculator {
    public static ResultPattern<SchedulerOutput> GetScheduleCalculator(SchedulerInput requestedDate) {
        return requestedDate.Periodicity switch {
            EnumConfiguration.Once => CalculateOneTime.CalculateDate(requestedDate),
            EnumConfiguration.Recurrent => CalculateRecurrent.CalculateDate(requestedDate),
            _ => ResultPattern<SchedulerOutput>.Failure(Messages.ErrorUnsupportedPeriodicity)
        };
    }
}
