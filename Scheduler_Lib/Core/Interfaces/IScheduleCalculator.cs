using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;

namespace Scheduler_Lib.Core.Interfaces;

public interface IScheduleCalculator {
    ResultPattern<SchedulerOutput> Calculate(SchedulerInput input);
}
