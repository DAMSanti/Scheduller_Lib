using Scheduler_Lib.Classes;
using Scheduler_Lib.Factory;

namespace Scheduler_Lib.Services;
public class Service {
    public static SolvedDate CalcDate(RequestedDate requestedDate) {
        Validations.Validations.ValidateCalc(requestedDate);

        var calcDate = ScheduleCalculator.GetScheduleCalculator(requestedDate.Periodicity);
        return calcDate.CalcDate(requestedDate);
    }
}