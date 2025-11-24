using Scheduler_Lib.Core.Services;

namespace Scheduler_Lib.Core.Interfaces;

/// <summary>
/// Validation interface for Chain of Responsibility pattern
/// </summary>
public interface IValidator
{
    ResultPattern<bool> Validate(ISchedulerConfiguration configuration);
    void SetNext(IValidator nextValidator);
}
