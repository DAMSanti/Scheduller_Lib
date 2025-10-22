using Scheduler_Lib.Core.Model;

namespace Scheduler_Lib.Core.Interfaces;

public interface ICalculatorFactory {
    IScheduleCalculator GetCalculator(SchedulerInput input);
}
