using Scheduler_Lib.Core.Classes;

namespace Scheduler_Lib.Core.Interface;
public interface ISchedule {
    SolvedDate CalcDate(RequestedDate requestedDate);
}

