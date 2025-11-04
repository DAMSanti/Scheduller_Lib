using Scheduler_Lib.Core.Model;
using Xunit.Abstractions;
// ReSharper disable UseObjectOrCollectionInitializer

namespace Scheduler_Lib.Core.Services;

public class ResultPatternTest(ITestOutputHelper output) {
    [Fact]
    public void Success_ShouldSuccess_WhenValueIsNull() {
        string? nullValue = null;
        var result = ResultPattern<string>.Success(nullValue!);

        output.WriteLine($"Result: {result.IsSuccess}, Value is null: {result.Value == null}");
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Success_ShouldSuccess_WhenValueIsObject() {
        var schedulerOutput = new SchedulerOutput();
        schedulerOutput.NextDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        schedulerOutput.Description = "Test description";

        var result = ResultPattern<SchedulerOutput>.Success(schedulerOutput);

        output.WriteLine($"Result: {result.IsSuccess}, Description: {result.Value.Description}");
        Assert.True(result.IsSuccess);
        Assert.Equal(schedulerOutput, result.Value);
        Assert.Null(result.Error);
        Assert.Equal("Test description", result.Value.Description);
    }

    [Fact]
    public void Failure_ShouldSuccess_WhenErrorMessageIsEmpty() {
        var result = ResultPattern<string>.Failure(string.Empty);

        output.WriteLine($"Result: {result.IsSuccess}, Error is empty: {string.IsNullOrEmpty(result.Error)}");
        Assert.False(result.IsSuccess);
        Assert.Equal(string.Empty, result.Error);
        Assert.Null(result.Value);
    }
    
    [Fact]
    public void Failure_ShouldSuccess_WhenErrorMessageIsProvided() {
        const string errorMessage = "An error occurred";
        var result = ResultPattern<int>.Failure(errorMessage);
        
        output.WriteLine($"Result: {result.IsSuccess}, Error: {result.Error}");
        Assert.False(result.IsSuccess);
        Assert.Equal(errorMessage, result.Error);
        Assert.Equal(0, result.Value);
    }
    
    [Fact]
    public void Failure_ShouldSuccess_WhenErrorMessageIsNull() {
        string? nullError = null;
        var result = ResultPattern<bool>.Failure(nullError!);
        
        output.WriteLine($"Result: {result.IsSuccess}, Error is null: {result.Error == null}");
        Assert.False(result.IsSuccess);
        Assert.Null(result.Error);
        Assert.False(result.Value);
    }
    
    [Fact]
    public void Success_ShouldSuccess_WhenValueIsValueType() {
        const int value = 42;
        var result = ResultPattern<int>.Success(value);
        
        output.WriteLine($"Result: {result.IsSuccess}, Value: {result.Value}");
        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value);
        Assert.Null(result.Error);
    }
    
    [Fact]
    public void Success_ShouldSuccess_WhenValueIsDefaultValueType() {
        const int defaultValue = 0;
        var result = ResultPattern<int>.Success(defaultValue);
        
        output.WriteLine($"Result: {result.IsSuccess}, Value: {result.Value}");
        Assert.True(result.IsSuccess);
        Assert.Equal(defaultValue, result.Value);
        Assert.Null(result.Error);
    }
    
    [Fact]
    public void Success_ShouldSuccess_WhenValueIsFalse() {
        const bool falseValue = false;
        var result = ResultPattern<bool>.Success(falseValue);
        
        output.WriteLine($"Result: {result.IsSuccess}, Value: {result.Value}");
        Assert.True(result.IsSuccess);
        Assert.False(result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Success_ShouldSuccess_WhenValueIsComplexObject() {
        var schedulerOutput = new SchedulerOutput();
        schedulerOutput.NextDate = new DateTimeOffset(2025, 10, 5, 14, 30, 0, TimeSpan.Zero);
        schedulerOutput.Description = "Complex test description with detailed information";

        var result = ResultPattern<SchedulerOutput>.Success(schedulerOutput);

        output.WriteLine($"Result: {result.IsSuccess}, Description: {result.Value.Description}, NextDate: {result.Value.NextDate}");

        Assert.True(result.IsSuccess);
        Assert.Equal(schedulerOutput, result.Value);
        Assert.Equal("Complex test description with detailed information", result.Value.Description);
        Assert.Equal(new DateTimeOffset(2025, 10, 5, 14, 30, 0, TimeSpan.Zero), result.Value.NextDate);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Failure_ShouldSuccess_WhenErrorMessageIsLong() {
        var longError = new string('A', 1000);
        var result = ResultPattern<object>.Failure(longError);
        
        output.WriteLine($"Result: {result.IsSuccess}, Error length: {result.Error?.Length}");
        Assert.False(result.IsSuccess);
        Assert.Equal(longError, result.Error);
        Assert.Equal(1000, result.Error?.Length);
        Assert.Null(result.Value);
    }
}