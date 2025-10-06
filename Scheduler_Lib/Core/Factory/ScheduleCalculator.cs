using Scheduler_Lib.Core.Interface;
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Factory;
public static class ScheduleCalculator {
    public static ISchedule GetScheduleCalculator(EnumPeriodicity? periodicity) {
        switch (periodicity) {
            case EnumPeriodicity.OneTime:
                return new CalcOneTime();
            case EnumPeriodicity.Recurrent:
                return new CalcRecurrent();
            default:
                throw new UnsupportedPeriodicityException(Messages.ErrorUnsupportedPeriodicity);
        }
    }
}
