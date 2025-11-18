using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Core.Services.Utilities;
using Scheduler_Lib.Resources;
using Xunit;
using Xunit.Abstractions;
// ReSharper disable UseObjectOrCollectionInitializer

namespace Scheduler_IntegrationTests.Integration;

public class CalculateDateIntegrationTests(ITestOutputHelper output) {

    #region Setup Helpers

    private SchedulerInput CreateBaseValidOnceInput() {
        var tz = TimeZoneConverter.GetTimeZone();
        return new SchedulerInput {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Once,
            Recurrency = EnumRecurrency.Daily,
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1))),
            CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1))),
            EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 12, 31))),
            TargetDate = new DateTimeOffset(2025, 6, 15, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 6, 15))),
        };
    }

    private SchedulerInput CreateBaseValidOnceInputWithoutEndDate() {
        var tz = TimeZoneConverter.GetTimeZone();
        return new SchedulerInput {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Once,
            Recurrency = EnumRecurrency.Daily,
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1))),
            CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1))),
            EndDate = null,
            TargetDate = new DateTimeOffset(2025, 6, 15, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 6, 15))),
        };
    }

    private SchedulerInput CreateDailyInput() {
        var tz = TimeZoneConverter.GetTimeZone();
        return new SchedulerInput {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Daily,
            StartDate = new DateTimeOffset(2025, 1, 1, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1))),
            CurrentDate = new DateTimeOffset(2025, 1, 1, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1))),
            EndDate = new DateTimeOffset(2025, 1, 31, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 1, 31))),
            OccursEveryChk = true,
            DailyPeriod = TimeSpan.FromDays(1)
        };
    }

    private SchedulerInput CreateWeeklyInput() {
        var tz = TimeZoneConverter.GetTimeZone();
        return new SchedulerInput {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            StartDate = new DateTimeOffset(2025, 1, 1, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1))),
            CurrentDate = new DateTimeOffset(2025, 1, 1, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1))),
            EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 12, 31))),
            WeeklyPeriod = 1,
            DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday]
        };
    }

    private SchedulerInput CreateMonthlyInput() {
        var tz = TimeZoneConverter.GetTimeZone();
        return new SchedulerInput {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Monthly,
            StartDate = new DateTimeOffset(2025, 1, 1, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1))),
            CurrentDate = new DateTimeOffset(2025, 1, 1, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 1, 1))),
            EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 12, 31))),
            MonthlyDayChk = true,
            MonthlyDay = 15,
            MonthlyDayPeriod = 1
        };
    }

    #endregion

    #region Global Validations

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldFail_WhenApplicationIsDisabled() {
        var schedulerInput = new SchedulerInput {
            EnabledChk = false,
            Periodicity = EnumConfiguration.Once,
            Recurrency = EnumRecurrency.Daily,
            StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero),
            CurrentDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero),
        };

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorApplicationDisabled, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldFail_WhenStartDateIsAfterEndDate() {
        var schedulerInput = new SchedulerInput {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Daily,
            StartDate = new DateTimeOffset(2025, 10, 20, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 10, 10, 0, 0, 0, TimeSpan.Zero),
            CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero),
            OccursOnceChk = true,
            OccursOnceAt = new TimeSpan(10, 0, 0),
        };

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorStartDatePostEndDate, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldFail_WhenStartDateIsDefault() {
        var schedulerInput = new SchedulerInput {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Daily,
            StartDate = default,
            CurrentDate = new DateTimeOffset(2025, 10, 01, 0, 0, 0, TimeSpan.Zero),
            OccursEveryChk = true,
            DailyPeriod = TimeSpan.FromDays(1),
        };

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
    }

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldFail_WhenUnsupportedPeriodicity() {
        var schedulerInput = new SchedulerInput {
            EnabledChk = true,
            StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero),
            CurrentDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero),
            Periodicity = (EnumConfiguration)999,
            Recurrency = EnumRecurrency.Daily,
        };

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorUnsupportedPeriodicity, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldFail_WhenUnsupportedRecurrency() {
        var schedulerInput = new SchedulerInput {
            EnabledChk = true,
            StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero),
            CurrentDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero),
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = (EnumRecurrency)999,
        };

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorUnsupportedRecurrency, result.Error ?? string.Empty);
    }

    #endregion

    #region Once Configuration Validation

    [Fact, Trait("Category", "ValidateOnce")]
    public void ValidateOnce_ShouldFail_WhenPeriodicityOnceAndRecurrencyWeekly() {
        var schedulerInput = CreateBaseValidOnceInput();
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];
        schedulerInput.WeeklyPeriod = 1;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorOnceWeekly, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidateOnce")]
    public void ValidateOnce_ShouldFail_WhenOnceWeeklyWithMultipleDaysOfWeek() {
        var schedulerInput = CreateBaseValidOnceInput();
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday];
        schedulerInput.WeeklyPeriod = 1;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorOnceWeekly, result.Error ?? string.Empty);
    }

    #endregion

    #region TargetDate Validation

    [Fact, Trait("Category", "ValidateOnce")]
    public void ValidateOnce_ShouldFail_WhenTargetDateLessThanStartDate() {
        var schedulerInput = CreateBaseValidOnceInput();
        schedulerInput.TargetDate = new DateTimeOffset(2024, 12, 31, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero);

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorTargetDateAfterEndDate, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidateOnce")]
    public void ValidateOnce_ShouldFail_WhenTargetDateGreaterThanEndDate() {
        var schedulerInput = CreateBaseValidOnceInput();
        schedulerInput.TargetDate = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero);

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorTargetDateAfterEndDate, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidateOnce")]
    public void ValidateOnce_ShouldSuccess_WhenTargetDateIsBetweenStartAndEnd() {
        var schedulerInput = CreateBaseValidOnceInput();
        schedulerInput.TargetDate = new DateTimeOffset(2025, 6, 15, 14, 30, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero);

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
    }

    [Fact, Trait("Category", "ValidateOnce")]
    public void ValidateOnce_ShouldFail_WhenTargetDateJustOutOfRange_LowerBound() {
        var schedulerInput = CreateBaseValidOnceInput();
        schedulerInput.TargetDate = new DateTimeOffset(2024, 12, 31, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero);

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorTargetDateAfterEndDate, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidateOnce")]
    public void ValidateOnce_ShouldFail_WhenTargetDateJustOutOfRange_UpperBound() {
        var schedulerInput = CreateBaseValidOnceInput();
        schedulerInput.TargetDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero);

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorTargetDateAfterEndDate, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidateOnce")]
    public void ValidateOnce_ShouldSuccess_WhenTargetDateEqualsStartDateWithoutEndDate() {
        var schedulerInput = CreateBaseValidOnceInputWithoutEndDate();
        schedulerInput.TargetDate = schedulerInput.StartDate;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
    }

    [Fact, Trait("Category", "ValidateOnce")]
    public void ValidateOnce_ShouldFail_WhenTargetDateNullAndRecurrencyDaily() {
        var schedulerInput = CreateBaseValidOnceInput();
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.TargetDate = null;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorTargetDateNull, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidateOnce")]
    public void ValidateOnce_ShouldFail_WhenTargetDateNullAndRecurrencyMonthly() {
        var schedulerInput = CreateBaseValidOnceInput();
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 15;
        schedulerInput.MonthlyDayPeriod = 1;
        schedulerInput.TargetDate = null;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorTargetDateNull, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidateOnce")]
    public void ValidateOnce_ShouldSuccess_WhenEndDateNullAndTargetDateValid() {
        var schedulerInput = CreateBaseValidOnceInputWithoutEndDate();
        schedulerInput.TargetDate = new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero);

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
    }

    #endregion

    #region StartDate Validation

    [Fact, Trait("Category", "ValidateOnce")]
    public void ValidateOnce_ShouldFail_WhenStartDateGreaterThanEndDate() {
        var schedulerInput = CreateBaseValidOnceInput();
        schedulerInput.StartDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero);

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorStartDatePostEndDate, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidateOnce")]
    public void ValidateOnce_ShouldFail_WhenStartDateGreaterThanEndDateWithoutTargetDate() {
        var schedulerInput = CreateBaseValidOnceInput();
        schedulerInput.StartDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.TargetDate = null;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorStartDatePostEndDate, result.Error ?? string.Empty);
    }

    #endregion

    #region Multiple Errors Validation

    [Fact, Trait("Category", "ValidateOnce")]
    public void ValidateOnce_ShouldFail_WithMultipleErrors_OnceWeeklyAndStartGreaterThanEnd() {
        var schedulerInput = CreateBaseValidOnceInput();
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.StartDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorOnceWeekly, result.Error ?? string.Empty);
        Assert.Contains(Messages.ErrorStartDatePostEndDate, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidateOnce")]
    public void ValidateOnce_ShouldFail_WithMultipleErrors_TargetDateOutOfRangeAndStartGreaterThanEnd() {
        var schedulerInput = CreateBaseValidOnceInput();
        schedulerInput.TargetDate = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorTargetDateAfterEndDate, result.Error ?? string.Empty);
        Assert.Contains(Messages.ErrorStartDatePostEndDate, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidateOnce")]
    public void ValidateOnce_ShouldFail_WithAllThreeErrors() {
        var schedulerInput = CreateBaseValidOnceInput();
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.TargetDate = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        var error = result.Error ?? string.Empty;
        Assert.Contains(Messages.ErrorOnceWeekly, error);
        Assert.Contains(Messages.ErrorTargetDateAfterEndDate, error);
        Assert.Contains(Messages.ErrorStartDatePostEndDate, error);
    }

    #endregion

    #region Edge Cases

    [Fact, Trait("Category", "ValidateOnce")]
    public void ValidateOnce_ShouldSuccess_WhenAllConditionsAreValid() {
        var schedulerInput = CreateBaseValidOnceInput();

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
    }

    [Fact, Trait("Category", "ValidateOnce")]
    public void ValidateOnce_ShouldSuccess_WhenOnceDaily() {
        var schedulerInput = CreateBaseValidOnceInput();
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
    }

    #endregion

    #region Recurrent StartDate And EndDate Validation

    [Fact, Trait("Category", "ValidationRecurrent")]
    public void ValidateRecurrent_ShouldFail_WhenEndDatePresentAndStartGreaterThanEnd() {
        var schedulerInput = CreateDailyInput();
        schedulerInput.StartDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorStartDatePostEndDate, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidationRecurrent")]
    public void ValidateRecurrent_ShouldNotValidateStartEndDateOrder_WhenEndDateIsNull() {
        var schedulerInput = CreateDailyInput();
        schedulerInput.StartDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.EndDate = null;
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.DoesNotContain(Messages.ErrorStartDatePostEndDate, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidationRecurrent")]
    public void ValidateRecurrent_ShouldSuccess_WhenStartDateJustBeforeEndDate() {
        var schedulerInput = CreateDailyInput();
        var endDate = new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero);
        var startDate = endDate.AddTicks(-1);

        schedulerInput.StartDate = startDate;
        schedulerInput.EndDate = endDate;
        schedulerInput.CurrentDate = startDate;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.DoesNotContain(Messages.ErrorStartDatePostEndDate, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidationRecurrent")]
    public void ValidateRecurrent_ShouldFail_WhenStartDateJustAfterEndDate() {
        var schedulerInput = CreateDailyInput();
        var endDate = new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero);
        var startDate = endDate.AddTicks(1);

        schedulerInput.StartDate = startDate;
        schedulerInput.EndDate = endDate;
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorStartDatePostEndDate, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidationRecurrent")]
    public void ValidateRecurrent_ShouldSuccess_WhenStartDateEqualsEndDate() {
        var schedulerInput = CreateDailyInput();
        var sameDate = new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero);
        schedulerInput.StartDate = sameDate;
        schedulerInput.EndDate = sameDate;
        schedulerInput.CurrentDate = sameDate;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.DoesNotContain(Messages.ErrorStartDatePostEndDate, result.Error ?? string.Empty);
    }

    #endregion

    #region Recurrency Type Validation

    [Fact, Trait("Category", "ValidationRecurrent")]
    public void ValidateRecurrent_ShouldFail_WhenRecurrencyIsNotSupported() {
        var schedulerInput = CreateDailyInput();
        schedulerInput.Recurrency = (EnumRecurrency)999;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorUnsupportedRecurrency, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidationRecurrent")]
    public void ValidateRecurrent_ShouldSuccess_WhenRecurrencyIsWeeklyValid() {
        var schedulerInput = CreateDailyInput();
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];
        schedulerInput.WeeklyPeriod = 1;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.DoesNotContain(Messages.ErrorUnsupportedRecurrency, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidationRecurrent")]
    public void ValidateRecurrent_ShouldSuccess_WhenRecurrencyIsDailyValid() {
        var schedulerInput = CreateDailyInput();
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.DoesNotContain(Messages.ErrorUnsupportedRecurrency, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidationRecurrent")]
    public void ValidateRecurrent_ShouldSuccess_WhenRecurrencyIsMonthlyValid() {
        var schedulerInput = CreateDailyInput();
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 15;
        schedulerInput.MonthlyDayPeriod = 1;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.DoesNotContain(Messages.ErrorUnsupportedRecurrency, result.Error ?? string.Empty);
    }

    #endregion

    #region Daily Time Window Validation

    [Fact, Trait("Category", "ValidationRecurrent")]
    public void ValidateRecurrent_ShouldFail_WhenDailyStartTimeGreaterThanEndTime() {
        var schedulerInput = CreateDailyInput();
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromHours(1);
        schedulerInput.DailyStartTime = new TimeSpan(17, 0, 0);
        schedulerInput.DailyEndTime = new TimeSpan(9, 0, 0);

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorDailyStartAfterEnd, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidationRecurrent")]
    public void ValidateRecurrent_ShouldNotValidateDailyTimes_WhenStartTimeIsNull() {
        var schedulerInput = CreateDailyInput();
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromHours(1);
        schedulerInput.DailyStartTime = null;
        schedulerInput.DailyEndTime = new TimeSpan(9, 0, 0);

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.DoesNotContain(Messages.ErrorDailyStartAfterEnd, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidationRecurrent")]
    public void ValidateRecurrent_ShouldNotValidateDailyTimes_WhenEndTimeIsNull() {
        var schedulerInput = CreateDailyInput();
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromHours(1);
        schedulerInput.DailyStartTime = new TimeSpan(17, 0, 0);
        schedulerInput.DailyEndTime = null;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.DoesNotContain(Messages.ErrorDailyStartAfterEnd, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidationRecurrent")]
    public void ValidateRecurrent_ShouldNotValidateDailyTimes_WhenBothTimesAreNull() {
        var schedulerInput = CreateDailyInput();
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromHours(1);
        schedulerInput.DailyStartTime = null;
        schedulerInput.DailyEndTime = null;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.DoesNotContain(Messages.ErrorDailyStartAfterEnd, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidationRecurrent")]
    public void ValidateRecurrent_ShouldSuccess_WhenDailyStartTimeJustBeforeEndTime() {
        var schedulerInput = CreateDailyInput();
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromHours(1);
        var endTime = new TimeSpan(12, 0, 0);
        var startTime = endTime.Add(TimeSpan.FromTicks(-1));
        schedulerInput.DailyStartTime = startTime;
        schedulerInput.DailyEndTime = endTime;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.DoesNotContain(Messages.ErrorDailyStartAfterEnd, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidationRecurrent")]
    public void ValidateRecurrent_ShouldFail_WhenDailyStartTimeJustAfterEndTime() {
        var schedulerInput = CreateDailyInput();
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromHours(1);
        var endTime = new TimeSpan(12, 0, 0);
        var startTime = endTime.Add(TimeSpan.FromTicks(1));
        schedulerInput.DailyStartTime = startTime;
        schedulerInput.DailyEndTime = endTime;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorDailyStartAfterEnd, result.Error ?? string.Empty);
    }

    #endregion

    #region Daily Period Validation

    [Fact, Trait("Category", "ValidationRecurrent")]
    public void ValidateRecurrent_ShouldFail_WhenDailyPeriodIsExactlyZero() {
        var schedulerInput = CreateDailyInput();
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 15;
        schedulerInput.MonthlyDayPeriod = 1;
        schedulerInput.DailyPeriod = TimeSpan.Zero;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorPositiveOffsetRequired, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidationRecurrent")]
    public void ValidateRecurrent_ShouldFail_WhenDailyPeriodIsNegative() {
        var schedulerInput = CreateDailyInput();
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 15;
        schedulerInput.MonthlyDayPeriod = 1;
        schedulerInput.DailyPeriod = TimeSpan.FromHours(-1);

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorPositiveOffsetRequired, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidationRecurrent")]
    public void ValidateRecurrent_ShouldNotValidateDailyPeriod_WhenDailyPeriodIsNull() {
        var schedulerInput = CreateDailyInput();
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 15;
        schedulerInput.MonthlyDayPeriod = 1;
        schedulerInput.DailyPeriod = null;

        var result = SchedulerService.InitialHandler(schedulerInput);

        if (result.IsSuccess) {
            Assert.DoesNotContain(Messages.ErrorPositiveOffsetRequired, result.Error ?? string.Empty);
        }
    }

    [Fact, Trait("Category", "ValidationRecurrent")]
    public void ValidateRecurrent_ShouldSuccess_WhenDailyPeriodIsOneTickAboveZero() {
        var schedulerInput = CreateDailyInput();
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 15;
        schedulerInput.MonthlyDayPeriod = 1;
        schedulerInput.DailyPeriod = TimeSpan.FromTicks(1);

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.DoesNotContain(Messages.ErrorPositiveOffsetRequired, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "ValidationRecurrent")]
    public void ValidateRecurrent_ShouldSuccess_WhenDailyPeriodIsPositive() {
        var schedulerInput = CreateDailyInput();
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 15;
        schedulerInput.MonthlyDayPeriod = 1;
        schedulerInput.DailyPeriod = TimeSpan.FromHours(2);

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.DoesNotContain(Messages.ErrorPositiveOffsetRequired, result.Error ?? string.Empty);
    }

    #endregion

    #region Combined Validations

    [Fact, Trait("Category", "ValidationRecurrent")]
    public void ValidateRecurrent_ShouldFail_WhenMultipleConditionsFail() {
        var schedulerInput = CreateDailyInput();
        schedulerInput.StartDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        schedulerInput.Recurrency = EnumRecurrency.Monthly;
        schedulerInput.MonthlyDayChk = true;
        schedulerInput.MonthlyDay = 15;
        schedulerInput.MonthlyDayPeriod = 1;
        schedulerInput.DailyStartTime = new TimeSpan(17, 0, 0);
        schedulerInput.DailyEndTime = new TimeSpan(9, 0, 0);
        schedulerInput.DailyPeriod = TimeSpan.Zero;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        var error = result.Error ?? string.Empty;
        Assert.Contains(Messages.ErrorStartDatePostEndDate, error);
        Assert.Contains(Messages.ErrorDailyStartAfterEnd, error);
        Assert.Contains(Messages.ErrorPositiveOffsetRequired, error);
    }

    #endregion

    #region Once Configuration Tests

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldSuccess_WhenOnceConfigurationIsValid() {
        var schedulerInput = new SchedulerInput();
        var tz = TimeZoneConverter.GetTimeZone();

        schedulerInput.EnabledChk = true;
        schedulerInput.Periodicity = EnumConfiguration.Once;
        schedulerInput.Recurrency = EnumRecurrency.Daily;
        schedulerInput.StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 01, 01)));
        schedulerInput.CurrentDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 01, 01)));
        schedulerInput.TargetDate = new DateTimeOffset(2025, 01, 15, 14, 30, 0, tz.GetUtcOffset(new DateTime(2025, 01, 15, 14, 30, 0)));

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.Equal(schedulerInput.TargetDate!.Value.DateTime, result.Value.NextDate.DateTime);
    }

    [Fact, Trait("Category", "Integration")]
    public void SchedulerCalculation_ShouldFail_WhenOnceConfigurationMissingTargetDate() {
        var schedulerInput = new SchedulerInput {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Once,
            Recurrency = EnumRecurrency.Daily,
            StartDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero),
            CurrentDate = new DateTimeOffset(2025, 01, 01, 0, 0, 0, TimeSpan.Zero),
            TargetDate = null,
        };

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
    }

    #endregion

    #region Periodicity and Recurrency Validation

    [Fact, Trait("Category", "Validations")]
    public void ValidateCalculateDate_ShouldPass_WhenPeriodicityIsOnce() {
        var schedulerInput = new SchedulerInput {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Once,
            Recurrency = EnumRecurrency.Daily,
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            TargetDate = new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero)
        };

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.DoesNotContain(Messages.ErrorUnsupportedPeriodicity, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Validations")]
    public void ValidateCalculateDate_ShouldPass_WhenPeriodicityIsRecurrent() {
        var schedulerInput = new SchedulerInput {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Daily,
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero),
            OccursEveryChk = true,
            DailyPeriod = TimeSpan.FromDays(1)
        };

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.DoesNotContain(Messages.ErrorUnsupportedPeriodicity, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Validations")]
    public void ValidateCalculateDate_ShouldFail_WhenPeriodicityIsNotSupported() {
        var schedulerInput = new SchedulerInput {
            EnabledChk = true,
            Periodicity = (EnumConfiguration)999,
            Recurrency = EnumRecurrency.Daily,
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)
        };

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorUnsupportedPeriodicity, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Validations")]
    public void ValidateCalculateDate_ShouldPass_WhenRecurrencyIsDaily() {
        var schedulerInput = new SchedulerInput {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Daily,
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero),
            OccursEveryChk = true,
            DailyPeriod = TimeSpan.FromDays(1)
        };

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.DoesNotContain(Messages.ErrorUnsupportedRecurrency, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Validations")]
    public void ValidateCalculateDate_ShouldPass_WhenRecurrencyIsWeekly() {
        var schedulerInput = new SchedulerInput {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero),
            WeeklyPeriod = 1,
            DaysOfWeek = [DayOfWeek.Monday]
        };

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.DoesNotContain(Messages.ErrorUnsupportedRecurrency, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Validations")]
    public void ValidateCalculateDate_ShouldPass_WhenRecurrencyIsMonthly() {
        var schedulerInput = new SchedulerInput {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Monthly,
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero),
            MonthlyDayChk = true,
            MonthlyDay = 15,
            MonthlyDayPeriod = 1
        };

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.DoesNotContain(Messages.ErrorUnsupportedRecurrency, result.Error ?? string.Empty);
    }

    #endregion

    #region Current Date and Enabled Validation

    [Fact, Trait("Category", "Validations")]
    public void ValidateCalculateDate_ShouldFail_WhenCurrentDateIsDefault() {
        var schedulerInput = new SchedulerInput {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Once,
            Recurrency = EnumRecurrency.Daily,
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CurrentDate = default,
            TargetDate = new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero)
        };

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorCurrentDateNull, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Validations")]
    public void ValidateCalculateDate_ShouldPass_WhenCurrentDateIsValid() {
        var schedulerInput = new SchedulerInput {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Once,
            Recurrency = EnumRecurrency.Daily,
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            TargetDate = new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero)
        };

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.DoesNotContain(Messages.ErrorCurrentDateNull, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Validations")]
    public void ValidateCalculateDate_ShouldFail_WhenStartDateIsDefaultValue() {
        var schedulerInput = new SchedulerInput {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Once,
            Recurrency = EnumRecurrency.Daily,
            StartDate = default,
            CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            TargetDate = new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero)
        };

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorStartDateMissing, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Validations")]
    public void ValidateCalculateDate_ShouldSuccess_WhenAllValidationsPass() {
        var schedulerInput = new SchedulerInput {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Once,
            Recurrency = EnumRecurrency.Daily,
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            TargetDate = new DateTimeOffset(2025, 6, 15, 10, 0, 0, TimeSpan.Zero)
        };

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.DoesNotContain(Messages.ErrorStartDateMissing, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Validations")]
    public void ValidateCalculateDate_ShouldFail_WhenEnabledChkIsFalse() {
        var schedulerInput = new SchedulerInput {
            EnabledChk = false,
            Periodicity = EnumConfiguration.Once,
            Recurrency = EnumRecurrency.Daily,
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CurrentDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)
        };

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorApplicationDisabled, result.Error ?? string.Empty);
    }

    [Fact, Trait("Category", "Validations")]
    public void ValidateCalculateDate_ShouldFail_WhenSchedulerInputIsNull() {
        var result = SchedulerService.InitialHandler(null!);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorRequestNull, result.Error ?? string.Empty);
    }

    #endregion

    #region Occurs Once Time - Weekly

    [Fact, Trait("Category", "CalculateRecurrent_OccursOnce")]
    public void InitialHandler_ShouldApplyOccursOnceTime_ToWeeklyRecurrence() {
        var schedulerInput = CreateWeeklyInput();
        var occursTime = new TimeSpan(14, 30, 45);

        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = occursTime;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.Equal(occursTime.Hours, result.Value.NextDate.Hour);
        Assert.Equal(occursTime.Minutes, result.Value.NextDate.Minute);
        Assert.Equal(occursTime.Seconds, result.Value.NextDate.Second);
    }

    [Fact, Trait("Category", "CalculateRecurrent_OccursOnce")]
    public void InitialHandler_ShouldPreserveDate_WhenApplyingOccursOnceTime_Weekly() {
        var schedulerInput = CreateWeeklyInput();
        var occursTime = new TimeSpan(9, 15, 30);

        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = occursTime;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value.NextDate.DayOfWeek, schedulerInput.DaysOfWeek!);
        Assert.Equal(occursTime, result.Value.NextDate.TimeOfDay);
    }

    [Fact, Trait("Category", "CalculateRecurrent_OccursOnce")]
    public void InitialHandler_ShouldNotApplyOccursOnceTime_WhenOccursOnceChkIsFalse_Weekly() {
        var schedulerInput = CreateWeeklyInput();
        var originalResult = SchedulerService.InitialHandler(schedulerInput);

        var schedulerInput2 = CreateWeeklyInput();
        var occursTime = new TimeSpan(14, 30, 45);
        schedulerInput2.OccursOnceChk = false;
        schedulerInput2.OccursOnceAt = occursTime;

        var resultWithOccursOnce = SchedulerService.InitialHandler(schedulerInput2);

        Assert.True(resultWithOccursOnce.IsSuccess);
    }

    [Fact, Trait("Category", "CalculateRecurrent_OccursOnce")]
    public void InitialHandler_ShouldPreserveTimeZoneOffset_WhenApplyingOccursOnceTime_Weekly() {
        var schedulerInput = CreateWeeklyInput();
        var tz = TimeZoneConverter.GetTimeZone();
        var occursTime = new TimeSpan(10, 30, 0);

        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = occursTime;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);

        var expectedOffset = tz.GetUtcOffset(result.Value.NextDate.DateTime);
        Assert.Equal(expectedOffset, result.Value.NextDate.Offset);
    }

    [Fact, Trait("Category", "CalculateRecurrent_OccursOnce")]
    public void InitialHandler_ShouldHandleMidnightTime_WhenApplyingOccursOnceTime_Weekly() {
        var schedulerInput = CreateWeeklyInput();
        var midnightTime = new TimeSpan(0, 0, 0);

        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = midnightTime;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.NextDate.Hour);
        Assert.Equal(0, result.Value.NextDate.Minute);
        Assert.Equal(0, result.Value.NextDate.Second);
    }

    [Fact, Trait("Category", "CalculateRecurrent_OccursOnce")]
    public void InitialHandler_ShouldHandleEndOfDayTime_WhenApplyingOccursOnceTime_Weekly() {
        var schedulerInput = CreateWeeklyInput();
        var endOfDayTime = new TimeSpan(23, 59, 59);

        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = endOfDayTime;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.Equal(23, result.Value.NextDate.Hour);
        Assert.Equal(59, result.Value.NextDate.Minute);
        Assert.Equal(59, result.Value.NextDate.Second);
    }

    #endregion

    #region Occurs Once Time - Monthly

    [Fact, Trait("Category", "CalculateRecurrent_OccursOnce")]
    public void InitialHandler_ShouldApplyOccursOnceTime_ToMonthlyRecurrence() {
        var schedulerInput = CreateMonthlyInput();
        var occursTime = new TimeSpan(15, 45, 30);

        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = occursTime;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.Equal(occursTime.Hours, result.Value.NextDate.Hour);
        Assert.Equal(occursTime.Minutes, result.Value.NextDate.Minute);
        Assert.Equal(occursTime.Seconds, result.Value.NextDate.Second);
    }

    [Fact, Trait("Category", "CalculateRecurrent_OccursOnce")]
    public void InitialHandler_ShouldPreserveMonthlyDay_WhenApplyingOccursOnceTime_Monthly() {
        var schedulerInput = CreateMonthlyInput();
        var occursTime = new TimeSpan(10, 0, 0);

        schedulerInput.OccursOnceChk = true;
        schedulerInput.OccursOnceAt = occursTime;
        schedulerInput.MonthlyDay = 15;

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        Assert.Equal(schedulerInput.MonthlyDay, result.Value.NextDate.Day);
        Assert.Equal(occursTime, result.Value.NextDate.TimeOfDay);
    }
    #endregion    
}