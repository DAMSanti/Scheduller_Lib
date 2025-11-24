namespace Scheduler_Lib.Core.Interfaces;

/// <summary>
/// Command interface for execution operations (Command Pattern)
/// </summary>
public interface ISchedulerCommand<out T>
{
    T Execute();
    bool CanExecute();
}
