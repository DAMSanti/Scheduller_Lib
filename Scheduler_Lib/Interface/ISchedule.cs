using Scheduler_Lib.Classes;

namespace Scheduler_Lib.Interface;
public interface ISchedule {
    SolvedDate CalcDate(RequestedDate requestedDate);
}

