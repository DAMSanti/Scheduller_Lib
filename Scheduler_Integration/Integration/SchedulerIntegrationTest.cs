using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Resources;
using Xunit;
using Xunit.Abstractions;
// ReSharper disable UseObjectOrCollectionInitializer

namespace Scheduler_IntegrationTests.Integration;

public class CalculateDateIntegrationTests(ITestOutputHelper output) {
    [Fact, Trait("Category", "Integration")]
    public void WeeklyRecurrent_EndToEnd_ShouldReturnFutureDates() {

        var schedulerInput = new SchedulerInput();

        schedulerInput.Enabled = true;
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 03, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);

        if (result.Value.FutureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {result.Value.FutureDates.Count}):");
            foreach (var dto in result.Value.FutureDates) {
                output.WriteLine(dto.ToString());
            }
        }

        Assert.True(result.IsSuccess);
        Assert.False(string.IsNullOrEmpty(result.Value.Description));
        Assert.True(result.Value.FutureDates is { Count: > 0 });
    }

    [Fact, Trait("Category", "Integration")]
    public void OneTime_WithTargetDate_ShouldReturnNextDateEqualTarget() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput();

        schedulerInput.Enabled = true;
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01)));
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 12, 31)));
        schedulerInput.TargetDate = new DateTimeOffset(2025, 10, 05, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 05)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 10, 03, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 03)));

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? result.Value.Description : result.Error);
        Assert.True(result.IsSuccess);
        Assert.Equal(schedulerInput.TargetDate, result.Value.NextDate);
    }

    [Fact, Trait("Category", "Integration")]
    public void DisabledScheduler_ShouldFailWithApplicationDisabledMessage() {
        var schedulerInput = new SchedulerInput();

        schedulerInput.Enabled = false;
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);

        var result = SchedulerService.CalculateDate(schedulerInput);

        output.WriteLine(result.IsSuccess ? "NO ERROR" : result.Error);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorApplicationDisabled, result.Error ?? string.Empty);
    }
}