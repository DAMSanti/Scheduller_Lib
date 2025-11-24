using Scheduler_Lib.Core.Adapters;
using Scheduler_Lib.Core.Interfaces;
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Repositories;
using Scheduler_Lib.Core.Services;

namespace Scheduler_Lib.Core.Facades;

/// <summary>
/// Facade for scheduler operations (Facade Pattern)
/// Simplifies the complex subsystem of validation, calculation, and description building
/// </summary>
public class SchedulerServiceFacade
{
    private readonly ISchedulerRepository _repository;

    public SchedulerServiceFacade()
    {
        _repository = new SchedulerRepository();
    }

    public SchedulerServiceFacade(ISchedulerRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Calculates the next execution date for a scheduler configuration
    /// </summary>
    public ResultPattern<SchedulerOutput> CalculateNextExecution(SchedulerInput schedulerInput)
    {
        if (schedulerInput == null)
            return ResultPattern<SchedulerOutput>.Failure("Scheduler input cannot be null");

        // Adapter Pattern: Convert SchedulerInput to ISchedulerConfiguration
        var configuration = new SchedulerInputAdapter(schedulerInput);

        // Delegate to repository which orchestrates validation and calculation
        return _repository.ValidateAndCalculate(configuration);
    }

    /// <summary>
    /// Calculates the next execution date without validation (use with caution)
    /// </summary>
    public SchedulerOutput CalculateNextExecutionUnsafe(SchedulerInput schedulerInput)
    {
        var configuration = new SchedulerInputAdapter(schedulerInput);
        return _repository.Calculate(configuration);
    }
}
