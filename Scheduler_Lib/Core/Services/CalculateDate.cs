using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Factory;
using Scheduler_Lib.Infrastructure.Validations;

namespace Scheduler_Lib.Core.Services;
public class Service {
    public static ResultPattern<SchedulerOutput> CalculateDate(SchedulerInput requestedDate) {
        var validation = Validations.ValidateCalc(requestedDate);
        if (!validation.IsSuccess) {
            return ResultPattern<SchedulerOutput>.Failure(validation.Error!);
        }

        var calcDate = ScheduleCalculator.GetScheduleCalculator(requestedDate);
        return calcDate;
    }
}