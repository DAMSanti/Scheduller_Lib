using Scheduler_Lib.Core.Enum;
using Scheduler_Lib.Core.Interface;
using Scheduler_Lib.Core.Services;

namespace Scheduler_Lib.Core.Factory;
public static class ScheduleCalculator {
    public static ISchedule GetScheduleCalculator(Periodicity? periodicity) {
        switch (periodicity) {
            case Periodicity.OneTime:
                return new CalcOneTime();
            case Periodicity.Recurrent:
                return new CalcRecurrent();
            default:
                throw new Exception("Unsupported periodicity.");
        }
    }
}
