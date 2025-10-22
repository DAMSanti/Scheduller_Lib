using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Infrastructure.Validations;

namespace Scheduler_Lib.Core.Services;
public class Service {
    public static ResultPattern<SchedulerOutput> CalculateDate(SchedulerInput schedulerInput) {
        var validation = Validations.ValidateCalculateDate(schedulerInput);

        if (!validation.IsSuccess) return ResultPattern<SchedulerOutput>.Failure(validation.Error!);

        var calculateDate = ScheduleCalculator.GetScheduleCalculator(schedulerInput);

        return calculateDate;
    }
}