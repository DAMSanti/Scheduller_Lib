using Scheduler_Lib.Core.Interfaces;
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;

namespace Scheduler_Lib.Core.Factory;

public class ScheduleCalculatorFactory : ICalculatorFactory {
    public IScheduleCalculator GetCalculator(SchedulerInput input) {
        return input.Periodicity switch {
            _ when input.Periodicity == EnumConfiguration.Once => new OneTimeCalculatorAdapter(),
            _ when input.Periodicity == EnumConfiguration.Recurrent => new RecurrentCalculatorAdapter(),
            _ => new UnsupportedCalculator()
        };
    }
}

internal class OneTimeCalculatorAdapter : IScheduleCalculator {
    public ResultPattern<SchedulerOutput> Calculate(SchedulerInput input) => CalculateOneTime.CalculateDate(input);
}

internal class RecurrentCalculatorAdapter : IScheduleCalculator {
    public ResultPattern<SchedulerOutput> Calculate(SchedulerInput input) => CalculateRecurrent.CalculateDate(input);
}

internal class UnsupportedCalculator : IScheduleCalculator {
    public ResultPattern<SchedulerOutput> Calculate(SchedulerInput input) => ResultPattern<SchedulerOutput>.Failure("Unsupported periodicity");
}
