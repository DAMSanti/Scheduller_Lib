using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;

namespace Scheduler_Lib.Core.Interfaces;

/// <summary>
/// Repository for scheduler configurations (Repository Pattern)
/// </summary>
public interface ISchedulerRepository
{
    SchedulerOutput Calculate(ISchedulerConfiguration configuration);
    ResultPattern<SchedulerOutput> ValidateAndCalculate(ISchedulerConfiguration configuration);
}
