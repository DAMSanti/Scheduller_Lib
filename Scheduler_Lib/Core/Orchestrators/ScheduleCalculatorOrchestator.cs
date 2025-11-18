using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Core.Services.Strategies;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Factory;

internal static class ScheduleCalculatorOrchestator {
    internal static ResultPattern<SchedulerOutput> GetPeriodicityType(SchedulerInput schedulerInput) {
        return schedulerInput.Periodicity switch {
            EnumConfiguration.Once => CalculateOneTime.CalculateOneTimeScheduler(schedulerInput),
            EnumConfiguration.Recurrent => CalculateRecurrent.CalculateRecurrentScheduler(schedulerInput),
            _ => ResultPattern<SchedulerOutput>.Failure(Messages.ErrorUnsupportedPeriodicity)
        };
    }
}
