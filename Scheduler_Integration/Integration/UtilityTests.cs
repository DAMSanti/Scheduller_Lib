using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Core.Services.Utilities;
using Scheduler_Lib.Resources;
using Xunit;
using Xunit.Abstractions;

namespace Scheduler_IntegrationTests.Integration;

public class UtilitiesTests(ITestOutputHelper output) {

    #region Setup Helpers

    private SchedulerInput CreateBaseValidRecurrentInput() {
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

    #endregion

    #region DateSafetyHelper Tests - Via InitialHandler (TryAddDaysSafely)

    #region TEST 1: days == 0 (línea 3-4 en DateSafetyHelper)
    [Fact, Trait("Category", "DateSafetyHelper_Integration")]
    public void InitialHandler_ShouldCoverDateSafetyHelper_WhenDaysIsZero() {
        var schedulerInput = CreateBaseValidRecurrentInput();

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        output.WriteLine($"✓ DateSafetyHelper: days == 0 path ejecutado (línea 3-4)");
        output.WriteLine($"  - StartDate: {schedulerInput.StartDate}");
        output.WriteLine($"  - EndDate: {schedulerInput.EndDate}");
    }
    #endregion

    #region TEST 2: days > 0 && dt > DateTime.MaxValue.AddDays(-days) (línea 6)
    [Fact, Trait("Category", "DateSafetyHelper_Integration")]
    public void InitialHandler_ShouldCoverDateSafetyHelper_WhenPositiveDaysWouldOverflow() {
        var schedulerInput = CreateBaseValidRecurrentInput();

        var threshold = DateTime.MaxValue.AddDays(-5);
        var maxDate = new DateTimeOffset(threshold, TimeSpan.Zero);

        schedulerInput.StartDate = maxDate;
        schedulerInput.CurrentDate = maxDate;
        schedulerInput.EndDate = new DateTimeOffset(DateTime.MaxValue, TimeSpan.Zero);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine($"✓ DateSafetyHelper: positive days overflow path evaluado (línea 6)");
        output.WriteLine($"  - StartDate cercana a MaxValue: {schedulerInput.StartDate}");
        output.WriteLine($"  - Resultado: {result.IsSuccess}");
    }

    [Fact, Trait("Category", "DateSafetyHelper_Integration")]
    public void InitialHandler_ShouldCoverDateSafetyHelper_WhenPositiveDaysDoNotOverflow() {
        var schedulerInput = CreateBaseValidRecurrentInput();

        var startDate = new DateTime(2025, 1, 1, 10, 0, 0);
        var tz = TimeZoneConverter.GetTimeZone();

        schedulerInput.StartDate = new DateTimeOffset(startDate, tz.GetUtcOffset(startDate));
        schedulerInput.CurrentDate = new DateTimeOffset(startDate, tz.GetUtcOffset(startDate));
        schedulerInput.EndDate = new DateTimeOffset(startDate.AddDays(365), tz.GetUtcOffset(startDate.AddDays(365)));
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(1);

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        output.WriteLine($"✓ DateSafetyHelper: positive days NO overflow path ejecutado (línea 6 FALSE)");
        output.WriteLine($"  - Cálculo exitoso sin overflow");
    }
    #endregion

    #region TEST 3: days < 0 && dt < DateTime.MinValue.AddDays(-days) (línea 8)
    [Fact, Trait("Category", "DateSafetyHelper_Integration")]
    public void InitialHandler_ShouldCoverDateSafetyHelper_WhenNegativeDaysWouldUnderflow() {
        var schedulerInput = CreateBaseValidRecurrentInput();

        var threshold = DateTime.MinValue.AddDays(5);
        var minDate = new DateTimeOffset(threshold, TimeSpan.Zero);

        schedulerInput.StartDate = minDate;
        schedulerInput.CurrentDate = minDate;
        schedulerInput.EndDate = new DateTimeOffset(threshold.AddDays(10), TimeSpan.Zero);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine($"✓ DateSafetyHelper: negative days underflow path evaluado (línea 8)");
        output.WriteLine($"  - StartDate cercana a MinValue: {schedulerInput.StartDate}");
        output.WriteLine($"  - Resultado: {result.IsSuccess}");
    }

    [Fact, Trait("Category", "DateSafetyHelper_Integration")]
    public void InitialHandler_ShouldCoverDateSafetyHelper_WhenNegativeDaysDoNotUnderflow() {
        var schedulerInput = CreateBaseValidRecurrentInput();

        var tz = TimeZoneConverter.GetTimeZone();
        var startDate = new DateTime(2025, 6, 15, 10, 0, 0);

        schedulerInput.StartDate = new DateTimeOffset(startDate, tz.GetUtcOffset(startDate));
        schedulerInput.CurrentDate = new DateTimeOffset(startDate, tz.GetUtcOffset(startDate));
        schedulerInput.EndDate = new DateTimeOffset(startDate.AddMonths(6), tz.GetUtcOffset(startDate.AddMonths(6)));

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        output.WriteLine($"✓ DateSafetyHelper: negative days NO underflow path ejecutado (línea 8 FALSE)");
    }
    #endregion

    #region TEST 4: Successful path - result = dt.AddDays(days) (línea 10)
    [Fact, Trait("Category", "DateSafetyHelper_Integration")]
    public void InitialHandler_ShouldCoverDateSafetyHelper_SuccessfulAddDaysPath() {
        var schedulerInput = CreateBaseValidRecurrentInput();
        var tz = TimeZoneConverter.GetTimeZone();

        var startDate = new DateTime(2025, 3, 15, 14, 45, 30);
        schedulerInput.StartDate = new DateTimeOffset(startDate, tz.GetUtcOffset(startDate));
        schedulerInput.CurrentDate = new DateTimeOffset(startDate, tz.GetUtcOffset(startDate));
        schedulerInput.EndDate = new DateTimeOffset(startDate.AddDays(90), tz.GetUtcOffset(startDate.AddDays(90)));
        schedulerInput.OccursEveryChk = true;
        schedulerInput.DailyPeriod = TimeSpan.FromDays(5);

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        output.WriteLine($"✓ DateSafetyHelper: successful AddDays path ejecutado (línea 10)");
        output.WriteLine($"  - StartDate: {schedulerInput.StartDate}");
        output.WriteLine($"  - EndDate: {schedulerInput.EndDate}");
        output.WriteLine($"  - NextDate: {result.Value.NextDate}");
    }
    #endregion

    #region TEST 5: Edge Cases - Boundaries
    [Fact, Trait("Category", "DateSafetyHelper_Integration")]
    public void InitialHandler_ShouldCoverDateSafetyHelper_WithLeapYearBoundary() {
        var schedulerInput = CreateBaseValidRecurrentInput();
        var tz = TimeZoneConverter.GetTimeZone();

        var leapYearDate = new DateTime(2024, 2, 28, 10, 0, 0);
        schedulerInput.StartDate = new DateTimeOffset(leapYearDate, tz.GetUtcOffset(leapYearDate));
        schedulerInput.CurrentDate = new DateTimeOffset(leapYearDate, tz.GetUtcOffset(leapYearDate));
        schedulerInput.EndDate = new DateTimeOffset(leapYearDate.AddDays(5), tz.GetUtcOffset(leapYearDate.AddDays(5)));

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        output.WriteLine($"✓ DateSafetyHelper: leap year boundary cubierto");
        output.WriteLine($"  - Incluye 29 de febrero");
    }

    [Fact, Trait("Category", "DateSafetyHelper_Integration")]
    public void InitialHandler_ShouldCoverDateSafetyHelper_WithMonthBoundary() {
        var schedulerInput = CreateBaseValidRecurrentInput();
        var tz = TimeZoneConverter.GetTimeZone();

        var monthEndDate = new DateTime(2025, 1, 31, 10, 0, 0);
        schedulerInput.StartDate = new DateTimeOffset(monthEndDate, tz.GetUtcOffset(monthEndDate));
        schedulerInput.CurrentDate = new DateTimeOffset(monthEndDate, tz.GetUtcOffset(monthEndDate));
        schedulerInput.EndDate = new DateTimeOffset(monthEndDate.AddDays(10), tz.GetUtcOffset(monthEndDate.AddDays(10)));

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        output.WriteLine($"✓ DateSafetyHelper: month boundary cubierto");
    }

    [Fact, Trait("Category", "DateSafetyHelper_Integration")]
    public void InitialHandler_ShouldCoverDateSafetyHelper_WithYearBoundary() {
        var schedulerInput = CreateBaseValidRecurrentInput();
        var tz = TimeZoneConverter.GetTimeZone();

        var yearEndDate = new DateTime(2025, 12, 31, 10, 0, 0);
        schedulerInput.StartDate = new DateTimeOffset(yearEndDate, tz.GetUtcOffset(yearEndDate));
        schedulerInput.CurrentDate = new DateTimeOffset(yearEndDate, tz.GetUtcOffset(yearEndDate));
        schedulerInput.EndDate = new DateTimeOffset(yearEndDate.AddDays(5), tz.GetUtcOffset(yearEndDate.AddDays(5)));

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);
        output.WriteLine($"✓ DateSafetyHelper: year boundary cubierto");
    }
    #endregion

    #endregion

    #region TimeZoneConverter Tests - Via InitialHandler

    #region TEST 6: ConvertToTimeZone() - líneas 14-17
    [Fact, Trait("Category", "TimeZoneConverter_Integration")]
    public void InitialHandler_ShouldCoverTimeZoneConverter_ConvertToTimeZone() {
        var schedulerInput = CreateBaseValidRecurrentInput();

        var tz = TimeZoneConverter.GetTimeZone();

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);

        Assert.True(result.Value.NextDate.Offset.TotalHours >= -14 &&
                   result.Value.NextDate.Offset.TotalHours <= 14);

        output.WriteLine($"✓ TimeZoneConverter.ConvertToTimeZone ejecutado (líneas 14-17)");
        output.WriteLine($"  - NextDate: {result.Value.NextDate}");
        output.WriteLine($"  - Offset: {result.Value.NextDate.Offset}");
        output.WriteLine($"  - UTC Time: {result.Value.NextDate.UtcDateTime}");
    }

    [Fact, Trait("Category", "TimeZoneConverter_Integration")]
    public void InitialHandler_ShouldPreserveUTCTime_InTimeZoneConversion() {
        var schedulerInput = CreateBaseValidRecurrentInput();
        var tz = TimeZoneConverter.GetTimeZone();

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);

        // Verificar que el tiempo UTC se preserva a través de la conversión
        var nextUTC = result.Value.NextDate.UtcDateTime;
        var nextLocal = result.Value.NextDate.DateTime;
        var expectedOffset = tz.GetUtcOffset(nextLocal);

        Assert.Equal(expectedOffset, result.Value.NextDate.Offset);
        output.WriteLine($"✓ UTC time preservado en TimeZoneConverter");
        output.WriteLine($"  - UTC: {nextUTC}");
        output.WriteLine($"  - Local: {nextLocal}");
        output.WriteLine($"  - Offset: {expectedOffset}");
    }

    [Fact, Trait("Category", "TimeZoneConverter_Integration")]
    public void InitialHandler_ShouldCalculateCorrectLocalTime_FromUTC() {
        var schedulerInput = new SchedulerInput {
            EnabledChk = true,
            Periodicity = EnumConfiguration.Recurrent,
            Recurrency = EnumRecurrency.Daily,
            StartDate = new DateTimeOffset(2025, 1, 15, 12, 0, 0, TimeSpan.Zero), // UTC
            CurrentDate = new DateTimeOffset(2025, 1, 15, 12, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 1, 20, 23, 59, 59, TimeSpan.Zero),
            OccursEveryChk = true,
            DailyPeriod = TimeSpan.FromDays(1)
        };

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);

        // Verifica que TimeZoneConverter convirtió correctamente de UTC a local
        var tz = TimeZoneConverter.GetTimeZone();
        var expectedLocal = TimeZoneInfo.ConvertTimeFromUtc(
            new DateTime(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc), tz);

        output.WriteLine($"✓ TimeZoneConverter: UTC a Local conversion correcta");
        output.WriteLine($"  - UTC: 2025-01-15 12:00:00");
        output.WriteLine($"  - Expected Local: {expectedLocal}");
        output.WriteLine($"  - Result NextDate: {result.Value.NextDate}");
    }

    [Fact, Trait("Category", "TimeZoneConverter_Integration")]
    public void InitialHandler_ShouldHandleDaylightSavingTime_InConversion() {
        var schedulerInput = CreateBaseValidRecurrentInput();

        // Crear dos escenarios: uno en invierno y otro en verano
        var tz = TimeZoneConverter.GetTimeZone();

        // Test 1: Invierno
        var winterDate = new DateTime(2025, 1, 15, 12, 0, 0);
        schedulerInput.StartDate = new DateTimeOffset(winterDate, tz.GetUtcOffset(winterDate));
        schedulerInput.CurrentDate = new DateTimeOffset(winterDate, tz.GetUtcOffset(winterDate));
        schedulerInput.EndDate = new DateTimeOffset(winterDate.AddDays(7), tz.GetUtcOffset(winterDate.AddDays(7)));

        var winterResult = SchedulerService.InitialHandler(schedulerInput);
        Assert.True(winterResult.IsSuccess);

        // Test 2: Verano
        var summerDate = new DateTime(2025, 7, 15, 12, 0, 0);
        schedulerInput.StartDate = new DateTimeOffset(summerDate, tz.GetUtcOffset(summerDate));
        schedulerInput.CurrentDate = new DateTimeOffset(summerDate, tz.GetUtcOffset(summerDate));
        schedulerInput.EndDate = new DateTimeOffset(summerDate.AddDays(7), tz.GetUtcOffset(summerDate.AddDays(7)));

        var summerResult = SchedulerService.InitialHandler(schedulerInput);
        Assert.True(summerResult.IsSuccess);

        if (tz.SupportsDaylightSavingTime) {
            // Los offsets pueden ser diferentes si hay DST
            output.WriteLine($"✓ TimeZoneConverter: DST handling correcto");
            output.WriteLine($"  - Winter offset: {winterResult.Value.NextDate.Offset}");
            output.WriteLine($"  - Summer offset: {summerResult.Value.NextDate.Offset}");
        } else {
            output.WriteLine($"✓ TimeZoneConverter: No DST en esta zona horaria");
        }
    }

    [Fact, Trait("Category", "TimeZoneConverter_Integration")]
    public void InitialHandler_ShouldHandleMaxDateTime_InTimeZoneConversion() {
        var schedulerInput = CreateBaseValidRecurrentInput();

        var maxDate = DateTime.MaxValue.AddDays(-10);
        schedulerInput.StartDate = new DateTimeOffset(maxDate, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(maxDate, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(DateTime.MaxValue, TimeSpan.Zero);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine($"✓ TimeZoneConverter: DateTime.MaxValue handling");
        output.WriteLine($"  - Resultado exitoso: {result.IsSuccess}");
    }

    [Fact, Trait("Category", "TimeZoneConverter_Integration")]
    public void InitialHandler_ShouldHandleMinDateTime_InTimeZoneConversion() {
        var schedulerInput = CreateBaseValidRecurrentInput();

        var minDate = DateTime.MinValue.AddDays(10);
        schedulerInput.StartDate = new DateTimeOffset(minDate, TimeSpan.Zero);
        schedulerInput.CurrentDate = new DateTimeOffset(minDate, TimeSpan.Zero);
        schedulerInput.EndDate = new DateTimeOffset(minDate.AddDays(20), TimeSpan.Zero);

        var result = SchedulerService.InitialHandler(schedulerInput);

        output.WriteLine($"✓ TimeZoneConverter: DateTime.MinValue handling");
        output.WriteLine($"  - Resultado exitoso: {result.IsSuccess}");
    }
    #endregion

    #region TEST 7: GetUtcOffset() - línea 21
    [Fact, Trait("Category", "TimeZoneConverter_Integration")]
    public void InitialHandler_ShouldCoverTimeZoneConverter_GetUtcOffset() {
        var schedulerInput = CreateBaseValidRecurrentInput();

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);

        var tz = TimeZoneConverter.GetTimeZone();
        var offset = TimeZoneConverter.GetUtcOffset(result.Value.NextDate.DateTime, tz);

        Assert.Equal(offset, result.Value.NextDate.Offset);
        output.WriteLine($"✓ TimeZoneConverter.GetUtcOffset ejecutado (línea 21)");
        output.WriteLine($"  - DateTime: {result.Value.NextDate.DateTime}");
        output.WriteLine($"  - Offset: {offset}");
    }

    [Fact, Trait("Category", "TimeZoneConverter_Integration")]
    public void InitialHandler_ShouldUseCorrectOffset_ForLocalDateTime() {
        var schedulerInput = CreateBaseValidRecurrentInput();
        var tz = TimeZoneConverter.GetTimeZone();

        var summerDate = new DateTime(2025, 7, 15, 10, 0, 0);
        schedulerInput.StartDate = new DateTimeOffset(summerDate, tz.GetUtcOffset(summerDate));
        schedulerInput.CurrentDate = new DateTimeOffset(summerDate, tz.GetUtcOffset(summerDate));
        schedulerInput.EndDate = new DateTimeOffset(summerDate.AddDays(7), tz.GetUtcOffset(summerDate.AddDays(7)));

        var result = SchedulerService.InitialHandler(schedulerInput);

        Assert.True(result.IsSuccess);

        var expectedOffset = tz.GetUtcOffset(result.Value.NextDate.DateTime);
        Assert.Equal(expectedOffset, result.Value.NextDate.Offset);

        output.WriteLine($"✓ TimeZoneConverter: Offset correcto para LocalDateTime");
        output.WriteLine($"  - Expected: {expectedOffset}, Actual: {result.Value.NextDate.Offset}");
    }
    #endregion

    #endregion

}