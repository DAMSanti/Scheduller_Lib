using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;
using Xunit.Abstractions;

namespace Scheduler_Lib.Core.Services;
    public class RecurrenceCalculatorTest(ITestOutputHelper output) {

        [Fact]
        public void SelectNextEligibleDate_WhenTargetIsOnADesiredDay_ReturnsSameInstantWithTzOffset()
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");
            var targetLocal = new DateTime(2025, 10, 6, 9, 0, 0, DateTimeKind.Unspecified);
            var targetDto = new DateTimeOffset(targetLocal, tz.GetUtcOffset(targetLocal));

            var result = RecurrenceCalculator.SelectNextEligibleDate(targetDto, new List<DayOfWeek> { DayOfWeek.Monday }, tz);

            var expected = new DateTimeOffset(targetLocal, tz.GetUtcOffset(targetLocal));
            Assert.Equal(expected, result);
        }

        [Fact]
        public void SelectNextEligibleDate_WhenTargetBeforeDesiredDay_ReturnsNextDesiredDay()
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");
            var targetLocal = new DateTime(2025, 10, 4, 9, 0, 0, DateTimeKind.Unspecified);
            var targetDto = new DateTimeOffset(targetLocal, tz.GetUtcOffset(targetLocal));

            var result = RecurrenceCalculator.SelectNextEligibleDate(targetDto, new List<DayOfWeek> { DayOfWeek.Monday }, tz);

            var expectedLocal = new DateTime(2025, 10, 6, 9, 0, 0, DateTimeKind.Unspecified);
            var expected = new DateTimeOffset(expectedLocal, tz.GetUtcOffset(expectedLocal));
            Assert.Equal(expected, result);
        }

        [Fact]
        public void SelectNextEligibleDate_WithEmptyDays_ReturnsTargetWithTzOffset()
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");
            var targetLocal = new DateTime(2025, 10, 4, 9, 0, 0, DateTimeKind.Unspecified);
            var targetDto = new DateTimeOffset(targetLocal, TimeSpan.Zero);

            var result = RecurrenceCalculator.SelectNextEligibleDate(targetDto, new List<DayOfWeek>(), tz);

            var expected = new DateTimeOffset(targetLocal, tz.GetUtcOffset(targetLocal));
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateWeeklyRecurrence_WithWeeklyPeriodGreaterThanOne_SkipsWeeksCorrectly()
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");
            var requested = new SchedulerInput
            {
                StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1))),
                TargetDate = null,
                EndDate = new DateTimeOffset(2025, 11, 30, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 11, 30))),
                DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday },
                WeeklyPeriod = 2,
            };

            var result = RecurrenceCalculator.CalculateWeeklyRecurrence(requested, tz);

            Assert.NotNull(result);

            for (int i = 1; i < result!.Count; i++)
            {
                var diff = (result[i].Date - result[i - 1].Date).TotalDays;
                Assert.True((int)diff % (7 * requested.WeeklyPeriod!.Value) == 0);
            }
        }

        [Fact]
        public void CalculateWeeklyRecurrence_WithNullDaysOfWeek_Throws() {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");
            var requested = new SchedulerInput {
                StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1))),
                TargetDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1))),
                EndDate = new DateTimeOffset(2025, 11, 30, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 11, 30))),
                DaysOfWeek = null!,
                WeeklyPeriod = 1,
            };

            Assert.ThrowsAny<NullReferenceException>(() => RecurrenceCalculator.CalculateWeeklyRecurrence(requested, tz));
        }

        [Fact]
        public void CalculateFutureDates_ReturnsEmpty_WhenPeriodicityIsNotRecurrent()
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");
            var requested = new SchedulerInput
            {
                Periodicity = EnumConfiguration.Once,
                Recurrency = EnumRecurrency.Daily,
                CurrentDate = new DateTimeOffset(2025, 10, 1, 8, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1)))
            };

            var result = RecurrenceCalculator.CalculateFutureDates(requested, tz);
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void CalculateFutureDates_DailyWithoutWindow_GeneratesSlotsFromCurrentToEndInclusive()
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");
            var requested = new SchedulerInput
            {
                Periodicity = EnumConfiguration.Recurrent,
                Recurrency = EnumRecurrency.Daily,
                CurrentDate = new DateTimeOffset(2025, 10, 1, 8, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1, 8, 0, 0, DateTimeKind.Unspecified))),
                StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1))),
                EndDate = new DateTimeOffset(2025, 10, 3, 8, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 3))),
                Period = TimeSpan.FromDays(1)
            };

            var result = RecurrenceCalculator.CalculateFutureDates(requested, tz);

            var expected1 = requested.CurrentDate;
            var expected2 = requested.CurrentDate.AddDays(1);
            var expected3 = requested.CurrentDate.AddDays(2);

            Assert.Equal(3, result.Count);
            Assert.Equal(expected1, result[0]);
            Assert.Equal(expected2, result[1]);
            Assert.Equal(expected3, result[2]);
        }

        [Fact]
        public void CalculateFutureDates_DailyWithWindow_GeneratesHourlySlotsPerDay()
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");
            var requested = new SchedulerInput
            {
                Periodicity = EnumConfiguration.Recurrent,
                Recurrency = EnumRecurrency.Daily,
                StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1))),
                EndDate = new DateTimeOffset(2025, 10, 2, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 10, 2))),
                DailyStartTime = new TimeSpan(8, 0, 0),
                DailyEndTime = new TimeSpan(10, 0, 0),
                DailyFrequency = TimeSpan.FromHours(1)
            };

            var result = RecurrenceCalculator.CalculateFutureDates(requested, tz);

            Assert.Equal(6, result.Count);

            var firstDay = requested.StartDate.Date;
            var slot1 = new DateTime(firstDay.Year, firstDay.Month, firstDay.Day, 8, 0, 0, DateTimeKind.Unspecified);
            var dto1 = new DateTimeOffset(slot1, tz.GetUtcOffset(slot1));
            Assert.Contains(dto1, result);
        }

        [Fact]
        public void CalculateFutureDates_WeeklyWithoutDailyWindow_GeneratesDaysOnly()
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");
            var requested = new SchedulerInput
            {
                Periodicity = EnumConfiguration.Recurrent,
                Recurrency = EnumRecurrency.Weekly,
                StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1))),
                EndDate = new DateTimeOffset(2025, 10, 21, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 21))),
                DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday },
                WeeklyPeriod = 1,
            };

            var result = RecurrenceCalculator.CalculateFutureDates(requested, tz);

            Assert.True(result.Count > 0);
            Assert.All(result, dto => Assert.Equal(DayOfWeek.Monday, dto.Date.DayOfWeek));
        }

        [Fact]
        public void CalculateFutureDates_WeeklyWithDailyWindow_GeneratesMultipleSlotsPerChosenDay()
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");
            var requested = new SchedulerInput
            {
                Periodicity = EnumConfiguration.Recurrent,
                Recurrency = EnumRecurrency.Weekly,
                StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1))),
                EndDate = new DateTimeOffset(2025, 10, 14, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 10, 14))),
                DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Wednesday },
                WeeklyPeriod = 1,
                DailyStartTime = new TimeSpan(9, 0, 0),
                DailyEndTime = new TimeSpan(11, 0, 0),
                DailyFrequency = TimeSpan.FromHours(1)
            };

            var result = RecurrenceCalculator.CalculateFutureDates(requested, tz);

            Assert.Equal(6, result.Count);

            Assert.All(result, dto => {
                Assert.Equal(DayOfWeek.Wednesday, dto.Date.DayOfWeek);
                var hour = dto.DateTime.Hour;
                Assert.InRange(hour, 9, 11);
            });
        }

    [Fact]
    public void CalculateFutureDates_ReturnsEmpty_When_DaysOfWeek_IsNull()
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");
        var requested = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            StartDate = new DateTimeOffset(2025, 10, 1, 9, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1))),
            EndDate = new DateTimeOffset(2025, 10, 31, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 10, 31))),
            DaysOfWeek = null
        };

        var result = RecurrenceCalculator.CalculateFutureDates(requested, tz);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void CalculateFutureDates_ReturnsEmpty_When_DaysOfWeek_IsEmpty()
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");
        var requested = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            StartDate = new DateTimeOffset(2025, 10, 1, 9, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1))),
            EndDate = new DateTimeOffset(2025, 10, 31, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 10, 31))),
            DaysOfWeek = new List<DayOfWeek>()
        };

        var result = RecurrenceCalculator.CalculateFutureDates(requested, tz);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void CalculateFutureDates_Uses_TargetDate_TimeOfDay_And_BaseLocal_When_TargetDate_Present()
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");

        var targetDate = new DateTimeOffset(2025, 10, 5, 15, 30, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 5, 15, 30, 0, DateTimeKind.Unspecified))); // Sunday 15:30

        var requested = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            TargetDate = targetDate,
            StartDate = new DateTimeOffset(2025, 10, 1, 9, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 1, 9, 0, 0, DateTimeKind.Unspecified))),
            EndDate = new DateTimeOffset(2025, 10, 14, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 10, 14))),
            DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday },
            WeeklyPeriod = 1
        };

        var result = RecurrenceCalculator.CalculateFutureDates(requested, tz);

        Assert.NotNull(result);
        Assert.NotEmpty(result);

        Assert.All(result, dto => {
            Assert.Equal(DayOfWeek.Monday, dto.Date.DayOfWeek);
            Assert.Equal(15, dto.DateTime.Hour);
            Assert.Equal(30, dto.DateTime.Minute);
        });

        var expectedLocal = new DateTime(2025, 10, 6, 15, 30, 0, DateTimeKind.Unspecified);
        var expectedDto = new DateTimeOffset(expectedLocal, tz.GetUtcOffset(expectedLocal));
        Assert.Contains(expectedDto, result);
    }

    [Fact]
    public void CalculateFutureDates_Uses_StartDate_TimeOfDay_When_TargetDate_Null()
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");

        var startDate = new DateTimeOffset(2025, 10, 3, 9, 45, 0,
            tz.GetUtcOffset(new DateTime(2025, 10, 3, 9, 45, 0, DateTimeKind.Unspecified))); // StartDate time 09:45

        var requested = new SchedulerInput
        {
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Weekly,
            TargetDate = null,
            StartDate = startDate,
            EndDate = new DateTimeOffset(2025, 10, 14, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 10, 14))),
            DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday },
            WeeklyPeriod = 1
        };

        var result = RecurrenceCalculator.CalculateFutureDates(requested, tz);

        Assert.NotNull(result);
        Assert.NotEmpty(result);

        Assert.All(result, dto => {
            Assert.Equal(DayOfWeek.Monday, dto.Date.DayOfWeek);
            Assert.Equal(9, dto.DateTime.Hour);
            Assert.Equal(45, dto.DateTime.Minute);
        });

        var expectedLocal = new DateTime(2025, 10, 6, 9, 45, 0, DateTimeKind.Unspecified);
        var expectedDto = new DateTimeOffset(expectedLocal, tz.GetUtcOffset(expectedLocal));
        Assert.Contains(expectedDto, result);
    }
}

