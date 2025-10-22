using Scheduler_Lib.Core.Interfaces;
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Factory;

namespace Scheduler_Lib.Core.Services;

public static class Service {
    public static ResultPattern<SchedulerOutput> CalculateDate(SchedulerInput? schedulerInput) {
        var validator = new SchedulerValidator();
        var factory = new ScheduleCalculatorFactory();
        var service = new SchedulerService(validator, factory);
        return service.Calculate(schedulerInput);
    }
}

public class SchedulerService(ISchedulerValidator validator, ICalculatorFactory calculatorFactory) {
    public ResultPattern<SchedulerOutput> Calculate(SchedulerInput? schedulerInput) {
        var validation = validator.Validate(schedulerInput);
        if (!validation.IsSuccess)
            return ResultPattern<SchedulerOutput>.Failure(validation.Error!);

        var calculator = calculatorFactory.GetCalculator(schedulerInput!);
        return calculator.Calculate(schedulerInput!);
    }

    public static ResultPattern<SchedulerOutput> CalculateDateStatic(SchedulerInput? schedulerInput) {
        var validator = new SchedulerValidator();
        var factory = new ScheduleCalculatorFactory();
        var service = new SchedulerService(validator, factory);
        return service.Calculate(schedulerInput);
    }

    public static ResultPattern<SchedulerOutput> CalculateDate(SchedulerInput? schedulerInput) {
        return CalculateDateStatic(schedulerInput);
    }
}