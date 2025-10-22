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

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Value.Description);

        if (result.Value.FutureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {result.Value.FutureDates.Count}):");
            foreach (var dto in result.Value.FutureDates) {
                output.WriteLine(dto.ToString());
            }
        }

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(data: ["2025-10-03", "2025-10-03", EnumConfiguration.Once, null, Messages.ErrorUnsupportedRecurrency])]
    [InlineData(data: ["2025-10-03", "2025-10-03", null, EnumRecurrency.Daily, Messages.ErrorUnsupportedPeriodicity])]
    [InlineData(data: ["2025-10-03", null, EnumConfiguration.Once, EnumRecurrency.Daily, Messages.ErrorCurrentDateNull])]
    [InlineData(data: [null, "2025-10-03", EnumConfiguration.Once, EnumRecurrency.Daily, Messages.ErrorStartDateMissing])]
    public void ValidateCalculateDate_ShouldFail_WhenInvalidInputs(string? startDate, string? currentDate, EnumConfiguration periodicity, EnumRecurrency recurrency, string expectedError) { 
        var schedulerInput = new SchedulerInput();

        schedulerInput.CurrentDate = currentDate != null ? DateTimeOffset.Parse(currentDate) : default;
        schedulerInput.StartDate = startDate != null ? DateTimeOffset.Parse(startDate) : default;
        schedulerInput.Periodicity = periodicity;
        schedulerInput.Recurrency = recurrency;

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "Success");

        Assert.False(result.IsSuccess);
        Assert.Contains(expectedError, result.Error);
    }

    [Fact]
    public void ValidateCalculateDate_ShouldFail_WhenMissingFields() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.CurrentDate = DateTimeOffset.Parse("2025-10-03");
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = DateTimeOffset.Parse("2025-12-31");
        schedulerInput.TargetDate = DateTimeOffset.Parse("2025-10-05");
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.DaysOfWeek = null;
        schedulerInput.WeeklyPeriod = null;
        schedulerInput.DailyPeriod = null;

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "Success");

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorOnceWeekly, result.Error);
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
        var schedulerInput = new SchedulerInput();

        schedulerInput!.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.DailyPeriod = new TimeSpan(1, 0, 0, 0);

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorStartDateMissing, result.Error);
    }
}