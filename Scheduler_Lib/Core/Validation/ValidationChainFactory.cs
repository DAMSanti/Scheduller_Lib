using Scheduler_Lib.Core.Interfaces;
using Scheduler_Lib.Core.Validation.Validators;

namespace Scheduler_Lib.Core.Validation;

/// <summary>
/// Factory for creating validation chains (Factory + Chain of Responsibility patterns)
/// </summary>
public class ValidationChainFactory
{
    public static IValidator CreateValidationChain()
    {
        var basicValidator = new BasicConfigurationValidator();
        var periodicityValidator = new PeriodicityValidator();
        var dateRangeValidator = new DateRangeValidator();
        var oneTimeValidator = new OneTimeExecutionValidator();
        var weeklyValidator = new WeeklyRecurrenceValidator();
        var dailyValidator = new DailyRecurrenceValidator();
        var monthlyValidator = new MonthlyRecurrenceValidator();

        // Build the chain
        basicValidator.SetNext(periodicityValidator);
        periodicityValidator.SetNext(dateRangeValidator);
        dateRangeValidator.SetNext(oneTimeValidator);
        oneTimeValidator.SetNext(weeklyValidator);
        weeklyValidator.SetNext(dailyValidator);
        dailyValidator.SetNext(monthlyValidator);

        return basicValidator;
    }
}
