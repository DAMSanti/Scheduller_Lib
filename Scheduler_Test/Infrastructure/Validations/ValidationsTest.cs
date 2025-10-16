using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Resources;
using Xunit.Abstractions;

namespace Scheduler_Lib.Infrastructure.Validations;

public class ValidationsTest(ITestOutputHelper output) {
    [Fact]
    public void CalculateDate_ShouldFail_WhenNoRecurrency() {
        var requestedDate = new SchedulerInput();

        requestedDate!.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        requestedDate.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Periodicity = EnumConfiguration.Once;

        var result = Service.CalculateDate(requestedDate);

        output.WriteLine(result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorUnsupportedRecurrency, result.Error);
    }

    [Fact]
    public void CalculateDate_ShouldFail_WhenNoPeriodicity() {
        var requestedDate = new SchedulerInput();

        requestedDate!.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        requestedDate.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Recurrency = EnumRecurrency.Daily;

        var result = Service.CalculateDate(requestedDate);

        output.WriteLine(result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorUnsupportedPeriodicity, result.Error);
    }

    [Fact]
    public void CalculateDate_ShouldFail_WhenNullRequest() {
        SchedulerInput? requestedDate = null;
        var result = Service.CalculateDate(requestedDate!);

        output.WriteLine(result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorRequestNull, result.Error);
    }

    [Fact]
    public void CalculateDate_ShouldFail_WhenNoStartDate() {
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

    [Fact]
    public void CalculateDate_ShouldFail_WhenNoCurrentDate() {
        var requestedDate = new SchedulerInput();

        requestedDate.StartDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        requestedDate.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        requestedDate.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Periodicity = EnumConfiguration.Once;
        requestedDate.Recurrency = EnumRecurrency.Daily;
        requestedDate.Period = new TimeSpan(1, 0, 0, 0);

        var result = Service.CalculateDate(requestedDate);

        output.WriteLine(result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorCurrentDateNull, result.Error);
    }

    [Fact]
    public void CalculateDateWeekly_ShouldFail_WhenEmptyOrNullDaysOfWeek() {
        var requestedDate = new SchedulerInput();

        requestedDate!.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        requestedDate.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Periodicity = EnumConfiguration.Once;
        requestedDate.Recurrency = EnumRecurrency.Weekly;
        requestedDate.WeeklyPeriod = 2;

        var result = Service.CalculateDate(requestedDate);

        output.WriteLine(result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorDaysOfWeekRequired, result.Error);
    }

    [Fact]
    public void CalculateDateWeekly_ShouldFail_WhenEmptyOrNullWeeklyPeriod() {
        var requestedDate = new SchedulerInput();

        requestedDate!.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        requestedDate.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Periodicity = EnumConfiguration.Once;
        requestedDate.Recurrency = EnumRecurrency.Weekly;
        requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };

        var result = Service.CalculateDate(requestedDate);

        output.WriteLine(result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorWeeklyPeriodRequired, result.Error);
    }

    [Fact]
    public void CalculateDateDaily_ShouldFail_WhenEmptyOrNullPeriod() {
        var requestedDate = new SchedulerInput();

        requestedDate!.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Periodicity = EnumConfiguration.Once;
        requestedDate.Recurrency = EnumRecurrency.Daily;

        var result = Service.CalculateDate(requestedDate);

        output.WriteLine(result.Error);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorPositiveOffsetRequired, result.Error);
    }
}