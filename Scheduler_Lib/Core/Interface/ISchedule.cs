using Scheduler_Lib.Core.Model;

namespace Scheduler_Lib.Core.Interface;
public interface ISchedule {
    SolvedDate CalcDate(RequestedDate requestedDate);
}

