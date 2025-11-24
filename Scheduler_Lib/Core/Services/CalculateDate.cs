using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Facades;

namespace Scheduler_Lib.Core.Services;

/// <summary>
/// Main scheduler service - Uses Facade Pattern to simplify complex operations
/// This is the public entry point for scheduler calculations
/// </summary>
public class SchedulerService {
    private static readonly SchedulerServiceFacade _facade = new();

    /// <summary>
    /// Calculates the next execution date for the given scheduler configuration
    /// This method uses the refactored architecture with SOLID principles and design patterns
    /// </summary>
    public static ResultPattern<SchedulerOutput> InitialOrchestator(SchedulerInput schedulerInput) {
        // Delegate to Facade which handles all complexity
        return _facade.CalculateNextExecution(schedulerInput);
    }
}