using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Core.Services.Localization;
using Scheduler_Lib.Core.Services.Utilities;
using Scheduler_Lib.Resources;
using Xunit;

namespace Scheduler_IntegrationTests.Integration;

public class MonthlyFormatCoverageTests {
    [Fact]
    public void MonthlyRecurrence_FormatMonthlyFrequency_AllFrequencies() {
        var tz = TimeZoneConverter.GetTimeZone();
        foreach (EnumMonthlyFrequency freq in Enum.GetValues(typeof(EnumMonthlyFrequency))) {
            var schedulerInput = new SchedulerInput {
                EnabledChk = true,
                Periodicity = EnumConfiguration.Recurrent,
                Recurrency = EnumRecurrency.Monthly,
                Language = "en_US",
                StartDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01))),
                CurrentDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01))),
                EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 12, 31))),
                MonthlyTheChk = true,
                MonthlyFrequency = freq,
                MonthlyDateType = EnumMonthlyDateType.Monday,
                MonthlyThePeriod = 1,
                OccursOnceChk = true,
                OccursOnceAt = new TimeSpan(10, 0, 0)
            };

            var result = SchedulerService.InitialHandler(schedulerInput);
            Assert.True(result.IsSuccess);

            var expected = freq switch {
                EnumMonthlyFrequency.First => "first",
                EnumMonthlyFrequency.Second => "second",
                EnumMonthlyFrequency.Third => "third",
                EnumMonthlyFrequency.Fourth => "fourth",
                EnumMonthlyFrequency.Last => "last",
                _ => freq.ToString().ToLower()
            };
            Assert.Contains(expected, result.Value.Description);
        }
    }

    [Fact]
    public void MonthlyRecurrence_FormatMonthlyDateType_AllDateTypes() {
        var tz = TimeZoneConverter.GetTimeZone();
        foreach (EnumMonthlyDateType dt in Enum.GetValues(typeof(EnumMonthlyDateType))) {
            var schedulerInput = new SchedulerInput {
                EnabledChk = true,
                Periodicity = EnumConfiguration.Recurrent,
                Recurrency = EnumRecurrency.Monthly,
                Language = "en_US",
                StartDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01))),
                CurrentDate = new DateTimeOffset(2025, 10, 01, 10, 0, 0, tz.GetUtcOffset(new DateTime(2025, 10, 01))),
                EndDate = new DateTimeOffset(2025, 12, 31, 23, 59, 59, tz.GetUtcOffset(new DateTime(2025, 12, 31))),
                MonthlyTheChk = true,
                MonthlyFrequency = EnumMonthlyFrequency.First,
                MonthlyDateType = dt,
                MonthlyThePeriod = 1,
                OccursOnceChk = true,
                OccursOnceAt = new TimeSpan(10, 0, 0)
            };

            var result = SchedulerService.InitialHandler(schedulerInput);
            Assert.True(result.IsSuccess);

            string expected;
            switch (dt) {
                case EnumMonthlyDateType.Day:
                    expected = "day";
                    break;
                case EnumMonthlyDateType.Weekday:
                    expected = "weekday";
                    break;
                case EnumMonthlyDateType.WeekendDay:
                    expected = "weekend day";
                    break;
                default:
                    var dow = dt switch {
                        EnumMonthlyDateType.Monday => DayOfWeek.Monday,
                        EnumMonthlyDateType.Tuesday => DayOfWeek.Tuesday,
                        EnumMonthlyDateType.Wednesday => DayOfWeek.Wednesday,
                        EnumMonthlyDateType.Thursday => DayOfWeek.Thursday,
                        EnumMonthlyDateType.Friday => DayOfWeek.Friday,
                        EnumMonthlyDateType.Saturday => DayOfWeek.Saturday,
                        EnumMonthlyDateType.Sunday => DayOfWeek.Sunday,
                        _ => DayOfWeek.Monday
                    };
                    expected = LocalizationService.FormatDayOfWeek(dow, "en_US");
                    break;
            }

            Assert.Contains(expected, result.Value.Description);
        }
    }
}
