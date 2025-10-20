using System.Diagnostics.CodeAnalysis;
using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Resources;
using Xunit.Abstractions;
// ReSharper disable UseObjectOrCollectionInitializer

namespace Scheduler_Lib.Infrastructure.Validations;

public class ValidationsTest(ITestOutputHelper output) {
    [Fact]
    public void ValidateCalculateDate_ShouldSucceed_WhenWeeklyConfigurationIsValid() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];

        var result = Validations.ValidateCalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "Success");

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Theory]
    [InlineData("2025-10-03", EnumConfiguration.Once, null, Messages.ErrorUnsupportedRecurrency)]
    [InlineData("2025-10-03", null, EnumRecurrency.Daily, Messages.ErrorUnsupportedPeriodicity)]
    [InlineData(null, EnumConfiguration.Once, EnumRecurrency.Daily, Messages.ErrorCurrentDateNull)]
    public void ValidateCalculateDate_ShouldFail_WithInvalidInputs(string? currentDate, EnumConfiguration periodicity, EnumRecurrency recurrency, string expectedError) { 
        var schedulerInput = new SchedulerInput();

        schedulerInput.CurrentDate = currentDate != null ? DateTimeOffset.Parse(currentDate) : default;
        schedulerInput.StartDate = periodicity == EnumConfiguration.Once
                ? new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)
                : default;
        schedulerInput.Periodicity = periodicity;
        schedulerInput.Recurrency = recurrency;

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "Success");

        Assert.False(result.IsSuccess);
        Assert.Contains(expectedError, result.Error);
    }

    [Theory]
    [InlineData("2025-10-03", "2025-12-31", "2025-10-05", EnumConfiguration.Once, EnumRecurrency.Weekly, Messages.ErrorDaysOfWeekRequired, null, null, null)]
    [InlineData("2025-10-03", "2025-12-31", "2025-10-05", EnumConfiguration.Once, EnumRecurrency.Weekly, Messages.ErrorWeeklyPeriodRequired, new object[] { DayOfWeek.Monday }, null, null)]
    [InlineData("2025-10-03", "2025-12-31", "2025-10-05", EnumConfiguration.Once, EnumRecurrency.Daily, Messages.ErrorPositiveOffsetRequired, null, null, null)]
    public void ValidateCalculateDate_ShouldFail_WithMissingFields(string? currentDate, string? endDate, string? targetDate, EnumConfiguration periodicity, EnumRecurrency recurrency, string expectedError, object? daysOfWeek = null, int? weeklyPeriod = null, TimeSpan? period = null) {
        var schedulerInput = new SchedulerInput();

        schedulerInput.CurrentDate = currentDate != null ? DateTimeOffset.Parse(currentDate) : default;
        schedulerInput.StartDate = currentDate != null ? new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero) : default;
        schedulerInput.EndDate = endDate != null ? DateTimeOffset.Parse(endDate) : default;
        schedulerInput.TargetDate = targetDate != null ? DateTimeOffset.Parse(targetDate) : default;
        schedulerInput.Periodicity = periodicity;
        schedulerInput.Recurrency = recurrency;
        schedulerInput.DaysOfWeek = daysOfWeek is object[] arr ? arr.Cast<DayOfWeek>().ToList() : null;
        schedulerInput.WeeklyPeriod = weeklyPeriod;
        schedulerInput.Period = period;

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "Success");

        Assert.False(result.IsSuccess);
        Assert.Contains(expectedError, result.Error);
    }

    [Fact]
    public void ValidateCalculateDate_ShouldFail_WhenNullRequest() {
        SchedulerInput? schedulerInput = null;
        var result = Service.CalculateDate(schedulerInput!);

        output.WriteLine(result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorRequestNull, result.Error);
    }

    
    [Fact]
    public void ValidateCalculateDate_ShouldFail_WhenNoStartDate() {
        var requestedDate = new SchedulerInput();

        requestedDate!.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        requestedDate.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        requestedDate.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Periodicity = EnumConfiguration.Once;
        requestedDate.Recurrency = EnumRecurrency.Daily;
        requestedDate.Period = new TimeSpan(1, 0, 0, 0);

        var result = Service.CalculateDate(requestedDate);

        output.WriteLine(result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorStartDateMissing, result.Error);
    }
}