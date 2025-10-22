using Scheduler_Lib.Core.Interfaces;
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;

namespace Scheduler_Lib.Infrastructure.Validations;

public class SchedulerValidator : ISchedulerValidator {
    public ResultPattern<bool> Validate(SchedulerInput? input) => Validations.ValidateCalculateDate(input);
}
