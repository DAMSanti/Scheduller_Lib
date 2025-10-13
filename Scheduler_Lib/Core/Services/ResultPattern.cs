using System.Text;

namespace Scheduler_Lib.Core.Services;

public class ResultPattern<T> {
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    private ResultPattern(T value) {
        IsSuccess = true;
        Value = value;
        Error = null;
    }

    private ResultPattern(string error) {
        IsSuccess = false;
        Value = default;
        Error = error;
    }

    public static ResultPattern<T> Success(T value) => new(value);
    public static ResultPattern<T> Failure(string error) => new(error);
}
