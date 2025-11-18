using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Factory;
using Scheduler_Lib.Infrastructure.Validations;

namespace Scheduler_Lib.Core.Services;

public class SchedulerService {
    public static ResultPattern<SchedulerOutput> InitialHandler(SchedulerInput schedulerInput) {

        var validation = Validations.ValidateCalculateDate(schedulerInput);
        if (!validation.IsSuccess) 
            return ResultPattern<SchedulerOutput>.Failure(validation.Error!);

        return ScheduleCalculatorOrchestator.GetPeriodicityType(schedulerInput);
    }
}