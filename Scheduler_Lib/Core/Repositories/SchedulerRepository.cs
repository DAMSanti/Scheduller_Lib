using Scheduler_Lib.Core.Builders;
using Scheduler_Lib.Core.Commands;
using Scheduler_Lib.Core.Factories;
using Scheduler_Lib.Core.Interfaces;
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Core.Services.Utilities;
using Scheduler_Lib.Core.Validation;

namespace Scheduler_Lib.Core.Repositories;

/// <summary>
/// Repository for scheduler operations (Repository Pattern)
/// </summary>
public class SchedulerRepository : ISchedulerRepository
{
    private readonly IRecurrenceStrategyFactory _strategyFactory;
    private readonly IValidator _validator;

    public SchedulerRepository()
    {
        _strategyFactory = new RecurrenceStrategyFactory();
        _validator = ValidationChainFactory.CreateValidationChain();
    }

    public SchedulerRepository(IRecurrenceStrategyFactory strategyFactory, IValidator validator)
    {
        _strategyFactory = strategyFactory ?? throw new ArgumentNullException(nameof(strategyFactory));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    public ResultPattern<SchedulerOutput> ValidateAndCalculate(ISchedulerConfiguration configuration)
    {
        var validationResult = _validator.Validate(configuration);
        
        if (!validationResult.IsSuccess)
            return ResultPattern<SchedulerOutput>.Failure(validationResult.Error!);

        var output = Calculate(configuration);
        return ResultPattern<SchedulerOutput>.Success(output);
    }

    public SchedulerOutput Calculate(ISchedulerConfiguration configuration)
    {
        var timeZone = TimeZoneConverter.GetTimeZone(configuration.TimeZoneId);
        var strategy = _strategyFactory.CreateStrategy(configuration);
        var descriptionBuilder = new SchedulerDescriptionBuilder();

        var command = new CalculateSchedulerCommand(
            configuration,
            strategy,
            descriptionBuilder,
            timeZone);

        if (!command.CanExecute())
            throw new InvalidOperationException("Cannot execute calculation with current configuration");

        return command.Execute();
    }
}
