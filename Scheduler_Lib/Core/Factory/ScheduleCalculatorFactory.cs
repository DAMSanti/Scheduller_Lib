using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Core.Services.Strategies;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Factory;

public static class ScheduleCalculatorFactory {
    public static ResultPattern<SchedulerOutput> CreateAndExecute(SchedulerInput schedulerInput) {
        return schedulerInput.Periodicity switch {
            EnumConfiguration.Once => CalculateOneTime.CalculateDate(schedulerInput),
            EnumConfiguration.Recurrent => CalculateRecurrent.CalculateDate(schedulerInput),
            _ => ResultPattern<SchedulerOutput>.Failure(Messages.ErrorUnsupportedPeriodicity)
        };
    }

    public static string GetStrategyType(EnumConfiguration periodicity) {
        return periodicity switch {
            EnumConfiguration.Once => nameof(CalculateOneTime),
            EnumConfiguration.Recurrent => nameof(CalculateRecurrent),
            _ => "Unknown"
        };
    }
}
