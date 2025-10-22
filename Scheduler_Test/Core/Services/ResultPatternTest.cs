using Scheduler_Lib.Core.Model;
using Xunit.Abstractions;
// ReSharper disable UseObjectOrCollectionInitializer

namespace Scheduler_Lib.Core.Services;

public class ResultPatternTest(ITestOutputHelper output) {
    [Fact]
    public void ResultPattern_Success_WithNullValue_ShouldSetValueToNull() {
        string? nullValue = null;
        var result = ResultPattern<string>.Success(nullValue!);

        output.WriteLine($"Result: {result.IsSuccess}, Value is null: {result.Value == null}");
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void ResultPattern_Success_WithObject_ShouldStoreObject() {
        var schedulerOutput = new SchedulerOutput {
            NextDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero),
            Description = "Test description",
            FutureDates = new List<DateTimeOffset>()
        };

        var result = ResultPattern<SchedulerOutput>.Success(schedulerOutput);

        output.WriteLine($"Result: {result.IsSuccess}, Description: {result.Value.Description}");
        Assert.True(result.IsSuccess);
        Assert.Equal(schedulerOutput, result.Value);
        Assert.Null(result.Error);
        Assert.Equal("Test description", result.Value.Description);
    }

    [Fact]
    public void ResultPattern_Failure_WithEmptyErrorMessage_ShouldStoreEmptyString() {
        var result = ResultPattern<string>.Failure(string.Empty);

        output.WriteLine($"Result: {result.IsSuccess}, Error is empty: {string.IsNullOrEmpty(result.Error)}");
        Assert.False(result.IsSuccess);
        Assert.Equal(string.Empty, result.Error);
        Assert.Null(result.Value);
    }
    
    [Fact]
    public void ResultPattern_Failure_WithErrorMessage_ShouldStoreError() {
        var errorMessage = "An error occurred";
        var result = ResultPattern<int>.Failure(errorMessage);
        
        output.WriteLine($"Result: {result.IsSuccess}, Error: {result.Error}");
        Assert.False(result.IsSuccess);
        Assert.Equal(errorMessage, result.Error);
        Assert.Equal(0, result.Value);
    }
    
    [Fact]
    public void ResultPattern_Failure_WithNullErrorMessage_ShouldStoreNull() {
        string? nullError = null;
        var result = ResultPattern<bool>.Failure(nullError!);
        
        output.WriteLine($"Result: {result.IsSuccess}, Error is null: {result.Error == null}");
        Assert.False(result.IsSuccess);
        Assert.Null(result.Error);
        Assert.False(result.Value);
    }
    
    [Fact]
    public void ResultPattern_Success_WithValueType_ShouldStoreValue() {
        const int value = 42;
        var result = ResultPattern<int>.Success(value);
        
        output.WriteLine($"Result: {result.IsSuccess}, Value: {result.Value}");
        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value);
        Assert.Null(result.Error);
    }
    
    [Fact]
    public void ResultPattern_Success_WithDefaultValueType_ShouldStoreDefaultValue() {
        const int defaultValue = 0;
        var result = ResultPattern<int>.Success(defaultValue);
        
        output.WriteLine($"Result: {result.IsSuccess}, Value: {result.Value}");
        Assert.True(result.IsSuccess);
        Assert.Equal(defaultValue, result.Value);
        Assert.Null(result.Error);
    }
    
    [Fact]
    public void ResultPattern_Success_WithFalse_ShouldStoreFalse() {
        const bool falseValue = false;
        var result = ResultPattern<bool>.Success(falseValue);
        
        output.WriteLine($"Result: {result.IsSuccess}, Value: {result.Value}");
        Assert.True(result.IsSuccess);
        Assert.False(result.Value);
        Assert.Null(result.Error);
    }
    
    [Fact]
    public void ResultPattern_Success_WithComplexObject_ShouldStoreAllProperties() {
        var futureDate1 = new DateTimeOffset(2025, 10, 1, 10, 0, 0, TimeSpan.Zero);
        var futureDate2 = new DateTimeOffset(2025, 10, 2, 10, 0, 0, TimeSpan.Zero);
        
        var schedulerOutput = new SchedulerOutput {
            NextDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero),
            Description = "Complex test description",
            FutureDates = [futureDate1, futureDate2]
        };

        var result = ResultPattern<SchedulerOutput>.Success(schedulerOutput);

        output.WriteLine($"Result: {result.IsSuccess}, Description: {result.Value.Description}, Future dates count: {result.Value.FutureDates!.Count}");
        Assert.True(result.IsSuccess);
        Assert.Equal(schedulerOutput, result.Value);
        Assert.Equal(2, result.Value.FutureDates.Count);
        Assert.Contains(futureDate1, result.Value.FutureDates);
        Assert.Contains(futureDate2, result.Value.FutureDates);
        Assert.Null(result.Error);
    }
    
    [Fact]
    public void ResultPattern_Failure_WithLongErrorMessage_ShouldStoreFullMessage() {
        var longError = new string('A', 1000);
        var result = ResultPattern<object>.Failure(longError);
        
        output.WriteLine($"Result: {result.IsSuccess}, Error length: {result.Error?.Length}");
        Assert.False(result.IsSuccess);
        Assert.Equal(longError, result.Error);
        Assert.Equal(1000, result.Error?.Length);
        Assert.Null(result.Value);
    }
}