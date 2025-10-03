using System;
using Scheduler_Lib.Classes;
using Scheduler_Lib.Interfaz;
using Scheduler_Lib.Enum;
using Scheduler_Lib.Services;

namespace Scheduler_Lib.Factory {
    public static class ScheduleCalculator {
        public static ISchedule GetScheduleCalculator(Periodicity periodicity) {
            switch (periodicity) {
                case Periodicity.OneTime:
                    return new CalcOneTime();
                case Periodicity.Recurrent:
                    return new CalcRecurrent();
                default:
                    throw new Exception("Unsupported periodicity");
            }
        }
    }
}
