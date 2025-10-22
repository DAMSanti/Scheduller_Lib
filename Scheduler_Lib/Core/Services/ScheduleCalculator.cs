using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Services;
public static class ScheduleCalculator {
    public static ResultPattern<SchedulerOutput> GetScheduleCalculator(SchedulerInput schedulerInput) {
        return schedulerInput.Periodicity switch {
            EnumConfiguration.Once => CalculateOneTime.CalculateDate(schedulerInput),
            EnumConfiguration.Recurrent => CalculateRecurrent.CalculateDate(schedulerInput),
            _ => ResultPattern<SchedulerOutput>.Failure(Messages.ErrorUnsupportedPeriodicity)
        };
    }
}
