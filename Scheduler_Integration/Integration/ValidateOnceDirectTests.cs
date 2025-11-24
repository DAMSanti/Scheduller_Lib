using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Adapters;
using Scheduler_Lib.Core.Validation.Validators;
using Scheduler_Lib.Resources;
using System;
using Xunit;

namespace Scheduler_IntegrationTests.Integration;

public class ValidateOnceDirectTests {
    [Fact, Trait("Category", "ValidateOnceDirect")]
    public void ValidateOnce_Direct_ShouldNotFlagOnceWeekly_WhenPeriodicityIsNotOnceAndRecurrencyWeekly() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = schedulerInput.StartDate;
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);

        var validator = new OneTimeExecutionValidator();
        var adapter = new SchedulerInputAdapter(schedulerInput);
        var result = validator.Validate(adapter);

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(Messages.ErrorOnceWeekly, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidateOnceDirect")]
    public void ValidateOnce_Direct_TargetDateNullAndRecurrencyDaily_ShouldErrorTargetDateNull() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = schedulerInput.StartDate;
        schedulerInput.TargetDate = null;

        var validator = new OneTimeExecutionValidator();
        var adapter = new SchedulerInputAdapter(schedulerInput);
        var result = validator.Validate(adapter);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorTargetDateNull, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidateOnceDirect")]
    public void ValidateOnce_Direct_TargetDateNullAndRecurrencyWeekly_ShouldNotErrorTargetDateNull() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = schedulerInput.StartDate;
        schedulerInput.TargetDate = null;
        schedulerInput.DaysOfWeek = new System.Collections.Generic.List<DayOfWeek> { DayOfWeek.Monday };

        var validator = new OneTimeExecutionValidator();
        var adapter = new SchedulerInputAdapter(schedulerInput);
        var result = validator.Validate(adapter);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorOnceWeekly, result.Error ?? string.Empty);
        Assert.DoesNotContain(Messages.ErrorTargetDateNull, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidateOnceDirect")]
    public void ValidateOnce_Direct_PeriodicityNotOnceAndRecurrencyWeeklyWithTargetNull_ShouldNotErrorTargetDateNull() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = schedulerInput.StartDate;
        schedulerInput.TargetDate = null;
        schedulerInput.DaysOfWeek = new System.Collections.Generic.List<DayOfWeek> { DayOfWeek.Monday };

        var validator = new OneTimeExecutionValidator();
        var adapter = new SchedulerInputAdapter(schedulerInput);
        var result = validator.Validate(adapter);

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(Messages.ErrorTargetDateNull, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidateOnceDirect")]
    public void ValidateOnce_Direct_PeriodicityOnceWeeklyAndTargetNull_ShouldIncludeBothErrors() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = schedulerInput.StartDate;
        schedulerInput.TargetDate = null;
        schedulerInput.DaysOfWeek = new System.Collections.Generic.List<DayOfWeek> { DayOfWeek.Monday };

        var validator = new OneTimeExecutionValidator();
        var adapter = new SchedulerInputAdapter(schedulerInput);
        var result = validator.Validate(adapter);

        Assert.False(result.IsSuccess);
        var error = result.Error ?? string.Empty;
        Assert.Contains(Messages.ErrorOnceWeekly, error);
        Assert.DoesNotContain(Messages.ErrorTargetDateNull, error);
    }

    [Fact, Trait("Category", "ValidateOnceDirect")]
    public void ValidateOnce_Direct_TargetDateEqualsEndDate_ShouldBeValid() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = schedulerInput.StartDate;
        schedulerInput.EndDate = new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = schedulerInput.EndDate;

        var validator = new DateRangeValidator();
        var adapter = new SchedulerInputAdapter(schedulerInput);
        var result = validator.Validate(adapter);

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(Messages.ErrorTargetDateAfterEndDate, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidateOnceDirect")]
    public void ValidateOnce_Direct_EndDatePresentAndTargetGreaterThanEndDate_ShouldReturnError() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = schedulerInput.StartDate;
        schedulerInput.EndDate = new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2025, 6, 15, 10, 0, 1, TimeSpan.Zero);

        var validator = new DateRangeValidator();
        var adapter = new SchedulerInputAdapter(schedulerInput);
        var result = validator.Validate(adapter);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorTargetDateAfterEndDate, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidateOnceDirect")]
    public void ValidateOnce_Direct_EndDatePresentAndStartEqualsEndDate_ShouldBeValid() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = schedulerInput.StartDate;
        schedulerInput.EndDate = new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero);

        var validator = new DateRangeValidator();
        var adapter = new SchedulerInputAdapter(schedulerInput);
        var result = validator.Validate(adapter);

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(Messages.ErrorStartDatePostEndDate, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidateOnceDirect")]
    public void ValidateOnce_Direct_StartDateGreaterThanEndDate_ShouldErrorStartDatePostEndDate() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var validator = new DateRangeValidator();
        var adapter = new SchedulerInputAdapter(schedulerInput);
        var result = validator.Validate(adapter);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorStartDatePostEndDate, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidateOnceDirect")]
    public void ValidateOnce_Direct_WeeklyWithTargetGreaterThanEndDate_ShouldIncludeBothErrors() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = schedulerInput.StartDate;
        schedulerInput.EndDate = new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2026, 6, 15, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.DaysOfWeek = new System.Collections.Generic.List<DayOfWeek> { DayOfWeek.Monday };

        var oneTimeValidator = new OneTimeExecutionValidator();
        var dateRangeValidator = new DateRangeValidator();
        var adapter = new SchedulerInputAdapter(schedulerInput);
        
        var oneTimeResult = oneTimeValidator.Validate(adapter);
        var dateRangeResult = dateRangeValidator.Validate(adapter);

        Assert.False(oneTimeResult.IsSuccess || dateRangeResult.IsSuccess);
        var combinedError = $"{oneTimeResult.Error}{dateRangeResult.Error}";
        Assert.Contains(Messages.ErrorOnceWeekly, combinedError);
        Assert.Contains(Messages.ErrorTargetDateAfterEndDate, combinedError);
    }

    [Fact, Trait("Category", "ValidateOnceDirect")]
    public void ValidateOnce_Direct_WeeklyStartAfterEndWithNoTarget_ShouldIncludeStartDateErrorOnly() {
        var schedulerInput = new SchedulerInput();
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = null;
        schedulerInput.DaysOfWeek = new System.Collections.Generic.List<DayOfWeek> { DayOfWeek.Monday };

        var oneTimeValidator = new OneTimeExecutionValidator();
        var dateRangeValidator = new DateRangeValidator();
        var adapter = new SchedulerInputAdapter(schedulerInput);
        
        var oneTimeResult = oneTimeValidator.Validate(adapter);
        var dateRangeResult = dateRangeValidator.Validate(adapter);

        Assert.False(oneTimeResult.IsSuccess || dateRangeResult.IsSuccess);
        var combinedError = $"{oneTimeResult.Error}{dateRangeResult.Error}";
        Assert.Contains(Messages.ErrorOnceWeekly, combinedError);
        Assert.Contains(Messages.ErrorStartDatePostEndDate, combinedError);
    }
}
