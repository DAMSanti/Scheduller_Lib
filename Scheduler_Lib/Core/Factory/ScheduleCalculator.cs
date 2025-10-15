using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Factory;
public static class ScheduleCalculator {
    public static ResultPattern<SchedulerOutput> GetScheduleCalculator(SchedulerInput requestedDate) {
        return requestedDate.Periodicity switch {
            EnumConfiguration.Once => new CalculateOneTime().CalculateDate(requestedDate),
            EnumConfiguration.Recurrent => new CalculateRecurrent().CalculateDate(requestedDate),
            _ => ResultPattern<SchedulerOutput>.Failure(Messages.ErrorUnsupportedPeriodicity)
        };
    }
}
