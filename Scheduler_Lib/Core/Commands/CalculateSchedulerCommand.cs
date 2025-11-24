using Scheduler_Lib.Core.Interfaces;
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services.Utilities;

namespace Scheduler_Lib.Core.Commands;

/// <summary>
/// Command for calculating scheduler output (Command Pattern)
/// </summary>
public class CalculateSchedulerCommand : ISchedulerCommand<SchedulerOutput>
{
    private readonly ISchedulerConfiguration _configuration;
    private readonly IRecurrenceCalculationStrategy _strategy;
    private readonly IDescriptionBuilder _descriptionBuilder;
    private readonly TimeZoneInfo _timeZone;

    public CalculateSchedulerCommand(
        ISchedulerConfiguration configuration,
        IRecurrenceCalculationStrategy strategy,
        IDescriptionBuilder descriptionBuilder,
        TimeZoneInfo timeZone)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        _descriptionBuilder = descriptionBuilder ?? throw new ArgumentNullException(nameof(descriptionBuilder));
        _timeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
    }

    public bool CanExecute()
    {
        return _strategy.CanHandle(_configuration);
    }

    public SchedulerOutput Execute()
    {
        if (!CanExecute())
            throw new InvalidOperationException("Command cannot be executed with current configuration");

        var nextDate = _strategy.GetNextExecutionDate(_configuration, _timeZone);
        
        if (_configuration.OccursOnceChk && _configuration.OccursOnceAt.HasValue)
        {
            nextDate = OccursOnceHelper.ApplyOccursOnceAt(nextDate, _configuration.OccursOnceAt, _timeZone);
        }

        var description = _descriptionBuilder
            .SetConfiguration(_configuration)
            .SetTimeZone(_timeZone)
            .SetNextDate(nextDate)
            .SetLanguage(_configuration.Language)
            .Build();

        return new SchedulerOutput
        {
            NextDate = nextDate,
            Description = description
        };
    }
}
