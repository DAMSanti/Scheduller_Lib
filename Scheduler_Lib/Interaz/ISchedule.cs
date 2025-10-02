using System;
using Scheduler_Lib.Classes;

namespace Scheduler_Lib.Interaz {
    public interface ISchedule {
        SolvedDate CalcDate(RequestedDate requestedDate);
    }
}
