using Scheduler_Lib.Classes;
using Scheduler_Lib.Factory;

namespace Scheduler_Lib.Services;
public class Service {
    public static SolvedDate CalcDate(RequestedDate requestedDate) {
        Validations.Validations.ValidateCalc(requestedDate);
        requestedDate.InitializeEndDate();

        if (!requestedDate.Enabled) {
            return new SolvedDate
            {
                NewDate = requestedDate.Date,
                Description = "Disabled: No changes performed."
            };
        }

        var calcDate = ScheduleCalculator.GetScheduleCalculator(requestedDate.Periodicity);
        return calcDate.CalcDate(requestedDate);
    }
}
