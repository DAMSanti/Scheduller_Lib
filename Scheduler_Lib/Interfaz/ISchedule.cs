using Scheduler_Lib.Classes;

namespace Scheduler_Lib.Interfaz {
    public interface ISchedule {
        SolvedDate CalcDate(RequestedDate requestedDate);
    }
}
