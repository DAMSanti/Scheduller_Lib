using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;


namespace Scheduler_Lib.Core.Interface;
public interface ISchedule {
    ResultPattern<SolvedDate> CalcDate(RequestedDate requestedDate);
}

