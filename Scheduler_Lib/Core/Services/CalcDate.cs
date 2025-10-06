using Scheduler_Lib.Core.Classes;
using Scheduler_Lib.Core.Factory;
using Scheduler_Lib.Infrastructure.Validations;

namespace Scheduler_Lib.Core.Services;
public class Service {
    public static SolvedDate CalcDate(RequestedDate requestedDate) {
        Validations.ValidateCalc(requestedDate);

        var calcDate = ScheduleCalculator.GetScheduleCalculator(requestedDate.Periodicity);
        return calcDate.CalcDate(requestedDate);
    }
}