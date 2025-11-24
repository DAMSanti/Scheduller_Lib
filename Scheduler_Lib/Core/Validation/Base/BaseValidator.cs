using Scheduler_Lib.Core.Interfaces;
using Scheduler_Lib.Core.Services;

namespace Scheduler_Lib.Core.Validation.Base;

/// <summary>
/// Base validator class for Chain of Responsibility pattern
/// </summary>
public abstract class BaseValidator : IValidator
{
    private IValidator? _nextValidator;

    public void SetNext(IValidator nextValidator)
    {
        _nextValidator = nextValidator;
    }

    public virtual ResultPattern<bool> Validate(ISchedulerConfiguration configuration)
    {
        var result = DoValidate(configuration);
        
        if (!result.IsSuccess)
            return result;
        
        return _nextValidator?.Validate(configuration) ?? ResultPattern<bool>.Success(true);
    }

    protected abstract ResultPattern<bool> DoValidate(ISchedulerConfiguration configuration);
}
