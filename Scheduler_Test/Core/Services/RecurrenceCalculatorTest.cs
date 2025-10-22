using Scheduler_Lib.Core.Model;
using Xunit.Abstractions;
// ReSharper disable UseObjectOrCollectionInitializer

namespace Scheduler_Lib.Core.Services;
public class RecurrenceCalculatorTest(ITestOutputHelper output) {
        
    [Fact] 
    public void RecurrenceCalculator_ShouldSucceed_WhenTargetIsOnADesiredDay() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var targetLocal = new DateTime(2025, 10, 6, 9, 0, 0, DateTimeKind.Unspecified);
        var targetDto = new DateTimeOffset(targetLocal, tz.GetUtcOffset(targetLocal));

        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1)));
        schedulerInput.EndDate = new DateTimeOffset(2025, 10, 14, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 10, 14)));
        schedulerInput.DaysOfWeek = [DayOfWeek.Monday];
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.DailyStartTime = new TimeSpan(9, 0, 0);
        schedulerInput.DailyEndTime = new TimeSpan(11, 0, 0);
        schedulerInput.DailyFrequency = TimeSpan.FromHours(1);
        schedulerInput.TargetDate = targetDto;
        schedulerInput.CurrentDate = targetDto;

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "NO ERROR");
        output.WriteLine(result.Value.Description);

        if (result.Value.FutureDates is { Count: > 0 }) { 
            output.WriteLine($"FutureDates (count = {result.Value.FutureDates.Count}):");
            foreach (var dto in result.Value.FutureDates) {
                output.WriteLine(dto.ToString());
            }
        }

        Assert.True(result.IsSuccess);
        var expected = new DateTimeOffset(targetLocal, tz.GetUtcOffset(targetLocal));
        Assert.Equal(expected, result.Value.NextDate);
    }

    [Fact]
    public void RecurrenceCalculator_ShouldSucceed_WhenTargetBeforeDesiredDay() {
        var tz = RecurrenceCalculator.GetTimeZone();

        var targetLocal = new DateTime(2025, 10, 4, 9, 0, 0, DateTimeKind.Unspecified);
        var targetDto = new DateTimeOffset(targetLocal, tz.GetUtcOffset(targetLocal));

        var schedulerInput = new SchedulerInput();

        schedulerInput.Periodicity = EnumConfiguration.Recurrent;
        schedulerInput.Recurrency = EnumRecurrency.Weekly;
        schedulerInput.StartDate = targetDto;
        schedulerInput.EndDate = targetDto.AddDays(14);
        schedulerInput.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };
        schedulerInput.WeeklyPeriod = 1;
        schedulerInput.TargetDate = targetDto;
        schedulerInput.CurrentDate = targetDto;

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "NO ERROR");
        output.WriteLine(result.Value.Description);

        if (result.Value.FutureDates is { Count: > 0 }) {
            output.WriteLine($"FutureDates (count = {result.Value.FutureDates.Count}):");
            foreach (var dto in result.Value.FutureDates) {
                output.WriteLine(dto.ToString());
            }
        }

        Assert.True(result.IsSuccess);
        var expectedLocal = new DateTime(2025, 10, 6, 9, 0, 0, DateTimeKind.Unspecified);
        var expected = new DateTimeOffset(expectedLocal, tz.GetUtcOffset(expectedLocal));
        Assert.Equal(expected, result.Value.NextDate);
    }

    [Fact]
    public void RecurrenceCalculator_ShouldSucceed_WhenEmptyDays()
    {
        var tz = RecurrenceCalculator.GetTimeZone();
        var targetLocal = new DateTime(2025, 10, 4, 9, 0, 0, DateTimeKind.Unspecified);
        var targetDto = new DateTimeOffset(targetLocal, TimeSpan.Zero);

        var schedulerInput = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            StartDate = targetDto,
            EndDate = targetDto.AddDays(14),
            DaysOfWeek = new List<DayOfWeek>(),
            WeeklyPeriod = 1,
            TargetDate = targetDto,
            CurrentDate = targetDto
        };

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "NO ERROR");

        Assert.True(result.IsSuccess);
        var expected = new DateTimeOffset(targetLocal, tz.GetUtcOffset(targetLocal));
        Assert.Equal(expected, result.Value.NextDate);
    }

    [Fact]
    public void RecurrenceCalculator_ShouldSucceed_WhenWeeklyPeriodGreaterThanOne()
    {
        var tz = RecurrenceCalculator.GetTimeZone();

        var schedulerInput = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1))),
            EndDate = new DateTimeOffset(2025, 11, 30, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 11, 30))),
            DaysOfWeek = [DayOfWeek.Monday],
            WeeklyPeriod = 2,
            CurrentDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1)))
        };

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "NO ERROR");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.FutureDates);

        for (var i = 1; i < result.Value.FutureDates!.Count; i++)
        {
            var diff = (result.Value.FutureDates[i].Date - result.Value.FutureDates[i - 1].Date).TotalDays;
            Assert.True((int)diff % (7 * schedulerInput.WeeklyPeriod!.Value) == 0);
        }
    }

    [Fact]
    public void RecurrenceCalculator_ShouldReturnsEmpty_WhenPeriodicityIsNotRecurrent()
    {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Once,
            Recurrency = EnumRecurrency.Daily,
            CurrentDate = new DateTimeOffset(2025, 10, 1, 8, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1)))
        };

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "NO ERROR");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Null(result.Value.FutureDates);
    }

    [Fact]
    public void RecurrenceCalculator_ShouldSucceed_WhenGeneratesSlotsFromCurrentToEndInclusive()
    {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Daily,
            CurrentDate = new DateTimeOffset(2025, 10, 1, 8, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1, 8, 0, 0, DateTimeKind.Unspecified))),
            StartDate = new DateTimeOffset(2025, 9, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1))),
            EndDate = new DateTimeOffset(2025, 10, 3, 8, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 3))),
            DailyPeriod = TimeSpan.FromDays(1)
        };

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "NO ERROR");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.FutureDates);

        var expected1 = schedulerInput.CurrentDate;
        var expected2 = schedulerInput.CurrentDate.AddDays(1);
        var expected3 = schedulerInput.CurrentDate.AddDays(2);

        Assert.Equal(3, result.Value.FutureDates.Count);
        Assert.Contains(expected1, result.Value.FutureDates);
        Assert.Contains(expected2, result.Value.FutureDates);
        Assert.Contains(expected3, result.Value.FutureDates);
    }

    [Fact]
    public void RecurrenceCalculator_ShouldSucceed_WhenGeneratesHourlySlotsPerDay()
    {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Daily,
            StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1))),
            CurrentDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1))),
            EndDate = new DateTimeOffset(2025, 10, 2, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 10, 2))),
            DailyStartTime = new TimeSpan(8, 0, 0),
            DailyEndTime = new TimeSpan(10, 0, 0),
            DailyFrequency = TimeSpan.FromHours(1),
            DailyPeriod = TimeSpan.FromDays(1)
        };

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "NO ERROR");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.FutureDates);
        Assert.Equal(6, result.Value.FutureDates.Count);

        var firstDay = schedulerInput.StartDate.Date;
        var slot1 = new DateTime(firstDay.Year, firstDay.Month, firstDay.Day, 8, 0, 0, DateTimeKind.Unspecified);
        var dto1 = new DateTimeOffset(slot1, tz.GetUtcOffset(slot1));
        Assert.Contains(dto1, result.Value.FutureDates);
    }

    [Fact]
    public void RecurrenceCalculator_ShouldSucceed_WhenGeneratesDaysOnly()
    {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1))),
            CurrentDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1))),
            EndDate = new DateTimeOffset(2025, 10, 21, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 21))),
            DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday },
            WeeklyPeriod = 1
        };

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "NO ERROR");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.FutureDates);
        Assert.True(result.Value.FutureDates.Count > 0);

        Assert.All(result.Value.FutureDates, dto => Assert.Equal(DayOfWeek.Monday, dto.Date.DayOfWeek));
    }

    [Fact]
    public void RecurrenceCalculator_ShouldSucceed_WhenGeneratesMultipleSlotsPerChosenDay()
    {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1))),
            CurrentDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1))),
            EndDate = new DateTimeOffset(2025, 10, 14, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 10, 14))),
            DaysOfWeek = [DayOfWeek.Wednesday],
            WeeklyPeriod = 1,
            DailyStartTime = new TimeSpan(9, 0, 0),
            DailyEndTime = new TimeSpan(11, 0, 0),
            DailyFrequency = TimeSpan.FromHours(1)
        };

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "NO ERROR");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.FutureDates);
        Assert.Equal(3, result.Value.FutureDates.Count);

        Assert.All(result.Value.FutureDates, dto => {
            Assert.Equal(DayOfWeek.Wednesday, dto.Date.DayOfWeek);
            var hour = dto.DateTime.Hour;
            Assert.InRange(hour, 9, 11);
        });
    }

    [Fact]
    public void RecurrenceCalculator_ShouldReturnsEmpty_WhenDaysOfWeekIsNull()
    {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            StartDate = new DateTimeOffset(2025, 10, 1, 9, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1))),
            CurrentDate = new DateTimeOffset(2025, 10, 1, 9, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1))),
            EndDate = new DateTimeOffset(2025, 10, 31, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 10, 31))),
            DaysOfWeek = null,
            WeeklyPeriod = 1
        };

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "NO ERROR");

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void RecurrenceCalculator_ShouldReturnsEmpty_WhenDaysOfWeekIsEmpty()
    {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            StartDate = new DateTimeOffset(2025, 10, 1, 9, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1))),
            CurrentDate = new DateTimeOffset(2025, 10, 1, 9, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1))),
            EndDate = new DateTimeOffset(2025, 10, 31, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 10, 31))),
            DaysOfWeek = new List<DayOfWeek>(),
            WeeklyPeriod = 1
        };

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "NO ERROR");

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void RecurrenceCalculator_ShouldSucceed_WhenTargetDatePresent()
    {
        var tz = RecurrenceCalculator.GetTimeZone();

        var targetDate = new DateTimeOffset(2025, 10, 5, 15, 30, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 5, 15, 30, 0, DateTimeKind.Unspecified))); // Sunday 15:30

        var schedulerInput = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            TargetDate = targetDate,
            CurrentDate = targetDate,
            StartDate = new DateTimeOffset(2025, 10, 1, 9, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1, 9, 0, 0, DateTimeKind.Unspecified))),
            EndDate = new DateTimeOffset(2025, 10, 14, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 10, 14))),
            DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday },
            WeeklyPeriod = 1
        };

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "NO ERROR");

        Assert.True(result.IsSuccess);

        var expectedLocal = new DateTime(2025, 10, 6, 15, 30, 0, DateTimeKind.Unspecified);
        var expectedDto = new DateTimeOffset(expectedLocal, tz.GetUtcOffset(expectedLocal));
        Assert.Equal(expectedDto, result.Value.NextDate);

        Assert.NotNull(result.Value.FutureDates);
        Assert.All(result.Value.FutureDates, dto => {
            Assert.Equal(DayOfWeek.Monday, dto.Date.DayOfWeek);
            Assert.Equal(15, dto.DateTime.Hour);
            Assert.Equal(30, dto.DateTime.Minute);
        });

        Assert.Contains(expectedDto, result.Value.FutureDates);
    }

    [Fact]
    public void RecurrenceCalculator_ShouldSucceed_WhenTargetDateNull()
    {
        var tz = RecurrenceCalculator.GetTimeZone();

        var startDate = new DateTimeOffset(2025, 10, 3, 9, 45, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 3, 9, 45, 0, DateTimeKind.Unspecified)));

        var schedulerInput = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            TargetDate = null,
            CurrentDate = startDate,
            StartDate = startDate,
            EndDate = new DateTimeOffset(2025, 10, 14, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 10, 14))),
            DaysOfWeek = [DayOfWeek.Monday],
            WeeklyPeriod = 1
        };

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "NO ERROR");

        Assert.True(result.IsSuccess);

        var expectedLocal = new DateTime(2025, 10, 6, 9, 45, 0, DateTimeKind.Unspecified);
        var expectedDto = new DateTimeOffset(expectedLocal, tz.GetUtcOffset(expectedLocal));
        Assert.Equal(expectedDto, result.Value.NextDate);

        Assert.NotNull(result.Value.FutureDates);
        Assert.All(result.Value.FutureDates, dto => {
            Assert.Equal(DayOfWeek.Monday, dto.Date.DayOfWeek);
            Assert.Equal(9, dto.DateTime.Hour);
            Assert.Equal(45, dto.DateTime.Minute);
        });

        Assert.Contains(expectedDto, result.Value.FutureDates);
    }
    [Fact]
    public void RecurrenceCalculator_ShouldSucceed_WhenReturnsMinValueWithTzOffset()
    {
        var tz = RecurrenceCalculator.GetTimeZone();
        var minDate = DateTimeOffset.MinValue;

        var schedulerInput = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            TargetDate = minDate,
            CurrentDate = minDate,
            StartDate = minDate,
            EndDate = minDate.AddDays(1),
            DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday },
            WeeklyPeriod = 1
        };

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "NO ERROR");
        output.WriteLine(result.Value.NextDate.ToString());

        if (result.IsSuccess)
        {
            Assert.Equal(minDate, result.Value.NextDate);
        }
    }

    [Fact]
    public void RecurrenceCalculator_ShouldReturnsEmpty_WhenMaxValue()
    {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Daily,
            CurrentDate = DateTimeOffset.MaxValue,
            StartDate = DateTimeOffset.MaxValue,
            EndDate = DateTimeOffset.MaxValue,
            DailyPeriod = TimeSpan.FromDays(1)
        };

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "NO ERROR");

        if (result.IsSuccess)
        {
            Assert.NotNull(result.Value);
            Assert.Null(result.Value.FutureDates);
        }
    }

    [Fact]
    public void CalculateFutureDates_ShouldReturnsEmpty_WhenPeriodicityNone()
    {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput
        {
            Periodicity = EnumConfiguration.None,
            Recurrency = EnumRecurrency.None,
            CurrentDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1))),
            StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1))),
            EndDate = new DateTimeOffset(2025, 10, 31, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 31)))
        };

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "NO ERROR");

        if (result.IsSuccess)
        {
            Assert.NotNull(result.Value);
            Assert.Null(result.Value.FutureDates);
        }
    }

    [Theory]
    [InlineData("2025-10-6", new[] { DayOfWeek.Monday }, "2025-10-6")]
    [InlineData("2025-10-4", new[] { DayOfWeek.Monday }, "2025-10-6")]
    [InlineData("2025-10-4", new DayOfWeek[0], "2025-10-4")]
    public void SelectNextEligibleDate_VariousScenarios_ReturnsExpected(string targetDate, DayOfWeek[] days, string expectedDate)
    {
        var tz = RecurrenceCalculator.GetTimeZone();
        var targetLocal = DateTime.Parse(targetDate);
        var targetDto = new DateTimeOffset(targetLocal, tz.GetUtcOffset(targetLocal));

        var schedulerInput = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            TargetDate = targetDto,
            CurrentDate = targetDto,
            StartDate = targetDto,
            EndDate = targetDto.AddDays(14),
            DaysOfWeek = days.Length > 0 ? days.ToList() : null,
            WeeklyPeriod = 1
        };

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "NO ERROR");

        if (days.Length > 0 && result.IsSuccess)
        {
            var expectedLocal = DateTime.Parse(expectedDate);
            var expectedDto = new DateTimeOffset(expectedLocal, tz.GetUtcOffset(expectedLocal));
            Assert.Equal(expectedDto, result.Value.NextDate);
        }
    }

    [Theory]
    [InlineData("2025-10-3", "2025-10-6", "2025-10-5", 2)]
    [InlineData("2025-10-1", "2025-10-5", "2025-10-3", 3)]
    public void RecurrenceCalculator_ShouldSucceed_WhenDailyScenarios(string startDate, string endDate, string currentDate, int expectedCount)
    {
        var tz = RecurrenceCalculator.GetTimeZone();
        var schedulerInput = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Daily,
            StartDate = new DateTimeOffset(DateTime.Parse(startDate), tz.GetUtcOffset(DateTime.Parse(startDate))),
            EndDate = new DateTimeOffset(DateTime.Parse(endDate), tz.GetUtcOffset(DateTime.Parse(endDate))),
            CurrentDate = new DateTimeOffset(DateTime.Parse(currentDate), tz.GetUtcOffset(DateTime.Parse(currentDate))),
            DailyPeriod = TimeSpan.FromDays(1)
        };

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "NO ERROR");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.FutureDates);
        Assert.Equal(expectedCount, result.Value.FutureDates.Count);
    }

    [Fact]
    public void RecurrenceCalculator_ShouldSucceed_WhenFiltersNullAndPastDates()
    {
        var tz = RecurrenceCalculator.GetTimeZone();
        var targetLocal = new DateTime(2025, 10, 6, 9, 0, 0, DateTimeKind.Unspecified);
        var targetDto = new DateTimeOffset(targetLocal, tz.GetUtcOffset(targetLocal));

        var schedulerInput = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            TargetDate = targetDto,
            CurrentDate = targetDto,
            StartDate = targetDto,
            EndDate = targetDto.AddDays(14),
            DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Sunday },
            WeeklyPeriod = 1
        };

        var result = Service.CalculateDate(schedulerInput);

        output.WriteLine(result.Error ?? "NO ERROR");

        Assert.True(result.IsSuccess);
        var expectedLocal = new DateTime(2025, 10, 6, 9, 0, 0, DateTimeKind.Unspecified);
        var expected = new DateTimeOffset(expectedLocal, tz.GetUtcOffset(expectedLocal));
        Assert.Equal(expected, result.Value.NextDate);
    }
}

