using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;
using Xunit.Abstractions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Scheduler_Lib.Core.Services;
public class CalcOneTimeTest(ITestOutputHelper output) {

    [Fact]
    public void ChangeDate_OneTime() {
        var requestedDate = new SchedulerInput();

        requestedDate!.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        requestedDate.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Periodicity = EnumConfiguration.Once;
        requestedDate.Recurrency = EnumRecurrency.None;

        var result = new CalculateOneTime().CalculateDate(requestedDate);

        var expectedNewDate = new DateTimeOffset(requestedDate.TargetDate.Value.DateTime, TimeSpan.Zero);
        Assert.Equal(expectedNewDate, result.Value!.NextDate);
        var expectedResult = $"Occurs once: Schedule will be used on {expectedNewDate.Date.ToShortDateString()} at {expectedNewDate.Date.ToShortTimeString()} starting on {requestedDate.StartDate.Date.ToShortDateString()}";
        Assert.Equal(expectedResult, result.Value.Description);
    }

    [Fact]
    public void CalculateWeeklyRecurrence_GeneratesCorrectDates() {
        var requestedDate = new SchedulerInput();

        requestedDate!.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate.StartDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate.EndDate = new DateTimeOffset(2025, 10, 21, 0, 0, 0, TimeSpan.Zero);
        requestedDate.WeeklyPeriod = 1;
        requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday };
        requestedDate.DailyStartTime = TimeSpan.Zero;
        requestedDate.DailyEndTime = TimeSpan.MaxValue;

        var result = new CalculateOneTime().CalculateDate(requestedDate); 

        var expected = new List<DateTimeOffset>();
        expected.Add(new DateTimeOffset(2025, 10, 6, 0, 0, 0, TimeSpan.Zero));
        expected.Add(new DateTimeOffset(2025, 10, 8, 0, 0, 0, TimeSpan.Zero));
        expected.Add(new DateTimeOffset(2025, 10, 13, 0, 0, 0, TimeSpan.Zero));
        expected.Add(new DateTimeOffset(2025, 10, 15, 0, 0, 0, TimeSpan.Zero));
        expected.Add(new DateTimeOffset(2025, 10, 20, 0, 0, 0, TimeSpan.Zero));

        Assert.Equal(expected, result.Value.FutureDates);
    }

    [Fact]
    public void CalculateWeeklyRecurrence_RespectsWeeklyPeriod() {
        var requestedDate = new SchedulerInput();

        requestedDate!.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate.EndDate = new DateTimeOffset(2025, 10, 26, 0, 0, 0, TimeSpan.Zero);
        requestedDate.WeeklyPeriod = 2;
        requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };
        requestedDate.StartDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate.DailyStartTime = TimeSpan.Zero;
        requestedDate.DailyEndTime = TimeSpan.MaxValue;

        var result = new CalculateOneTime().CalculateDate(requestedDate);

        var expected = new List<DateTimeOffset>();
        expected.Add(new DateTimeOffset(2025, 10, 6, 0, 0, 0, TimeSpan.Zero));
        expected.Add(new DateTimeOffset(2025, 10, 20, 0, 0, 0, TimeSpan.Zero));

        Assert.Equal(expected, result.Value.FutureDates);
    }

    [Fact]
    public void CalculateWeeklyRecurrence_UsesDefaultEndDate_WhenEndDateIsNull(){
        var requestedDate = new SchedulerInput();

        requestedDate.StartDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate.EndDate = null;
        requestedDate.WeeklyPeriod = 1;
        requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };

        var result = new CalculateOneTime().CalculateDate(requestedDate);

        Assert.Contains(Messages.ErrorEndDateNull, result.Error);
    }

    [Fact]
    public void BuildDescriptionForChangeDate_Weekly_PeriodHasValue() {
        var requestedDate = new SchedulerInput();

        requestedDate.Recurrency = EnumRecurrency.Weekly;
        requestedDate.WeeklyPeriod = 2;
        requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Friday };
        requestedDate.TargetDate = new DateTimeOffset(2025, 11, 15, 0, 0, 0, TimeSpan.Zero);
        requestedDate.DailyStartTime = TimeSpan.FromHours(8);
        requestedDate.DailyEndTime = TimeSpan.FromHours(12);
        requestedDate.StartDate = new DateTimeOffset(2025, 11, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.EndDate = new DateTimeOffset(2025, 12, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Period = TimeSpan.FromDays(3);

        var result = new CalculateOneTime().CalculateDate(requestedDate);

        Assert.Contains("every 3 days", result.Value.Description);
    }

    [Fact]
    public void BuildDescriptionForChangeDate_Weekly_PeriodIsNull() {
        var requestedDate = new SchedulerInput();

        requestedDate.Recurrency = EnumRecurrency.Weekly;
        requestedDate.WeeklyPeriod = 1;
        requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday };
        requestedDate.TargetDate = new DateTimeOffset(2025, 11, 15, 0, 0, 0, TimeSpan.Zero);
        requestedDate.DailyStartTime = TimeSpan.FromHours(16);
        requestedDate.DailyEndTime = TimeSpan.FromHours(20);
        requestedDate.StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.EndDate = new DateTimeOffset(2025, 12, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Period = null;

        var result = new CalculateOneTime().CalculateDate(requestedDate); 

        Assert.Contains("every 1 week", result.Value.Description);
    }

    [Fact]
    public void FutureDates_IsNotNull_WhenAllWeeklyConditionsAreMet() {
        var requestedDate = new SchedulerInput();

        requestedDate!.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Recurrency = EnumRecurrency.Weekly;
        requestedDate.EndDate = new DateTimeOffset(2025, 10, 25, 0, 0, 0, TimeSpan.Zero);
        requestedDate.WeeklyPeriod = 1;
        requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };
        requestedDate.DailyStartTime = TimeSpan.FromHours(8);
        requestedDate.DailyEndTime = TimeSpan.FromHours(17);

        var result = new CalculateOneTime().CalculateDate(requestedDate);

        Assert.NotNull(result.Value!.FutureDates);
        Assert.NotEmpty(result.Value!.FutureDates);
    }

    [Fact]
    public void NoChange_MissingData() {
        var requestedDate = new SchedulerInput();

        requestedDate!.CurrentDate = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero);
        requestedDate.StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        requestedDate.EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Periodicity = EnumConfiguration.Once;

        var result = new CalculateOneTime().CalculateDate(requestedDate);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorOnceMode, result.Error);
    }

    [Fact]
    public void FutureDates_IsNull_WhenOcurrenceIsNotWeekly() {
        var requestedDate = new SchedulerInput();

        requestedDate!.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Recurrency = 0;
        requestedDate.WeeklyPeriod = 1;
        requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };

        var result = new CalculateOneTime().CalculateDate(requestedDate);

        Assert.Null(result.Value!.FutureDates);
    }

    [Fact]
    public void FutureDates_IsNull_WhenWeeklyPeriodIsNull() {
        var requestedDate = new SchedulerInput();

        requestedDate!.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate.Recurrency = EnumRecurrency.Weekly;
        requestedDate.WeeklyPeriod = null;
        requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };

        var result = new CalculateOneTime().CalculateDate(requestedDate);

        Assert.Null(result.Value!.FutureDates);
    }

    [Fact]
    public void FutureDates_IsNull_WhenDaysOfWeekIsNullOrEmpty() {
        var requestedDate1 = new SchedulerInput();
        var requestedDate2 = new SchedulerInput();

        requestedDate1.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate1.Recurrency = EnumRecurrency.Weekly;
        requestedDate1.WeeklyPeriod = 1;
        requestedDate1.DaysOfWeek = null;

        requestedDate2.TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate2.Recurrency = EnumRecurrency.Weekly;
        requestedDate2.WeeklyPeriod = 1;
        requestedDate2.DaysOfWeek = new List<DayOfWeek>();

        var result1 = new CalculateOneTime().CalculateDate(requestedDate1);
        var result2 = new CalculateOneTime().CalculateDate(requestedDate2);

        Assert.Null(result1.Value!.FutureDates);
        Assert.Null(result2.Value!.FutureDates);
    }

    [Fact]
    public void CalculateWeeklyRecurrence_NoDates_WhenStartAfterEnd() {
        var requestedDate = new SchedulerInput();

        requestedDate.StartDate = new DateTimeOffset(2025, 10, 10, 0, 0, 0, TimeSpan.Zero);
        requestedDate.EndDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate.WeeklyPeriod = 1;
        requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Monday };

        var result = new CalculateOneTime().CalculateDate(requestedDate);

        Assert.Contains(Messages.ErrorChangeDateAfterEndDate, result.Error);
    }

    [Fact]
    public void CalculateWeeklyRecurrence_NoDuplicates_SameWeek() {
        var requestedDate = new SchedulerInput();

        requestedDate.StartDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        requestedDate.EndDate = new DateTimeOffset(2025, 10, 5, 23, 59, 59, TimeSpan.Zero);
        requestedDate.WeeklyPeriod = 1;
        requestedDate.DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Sunday, DayOfWeek.Sunday };
            
        var result = new CalculateOneTime().CalculateDate(requestedDate);

        Assert.Contains(Messages.ErrorOnceMode, result.Error);
    }

    [Fact]
    public void CalculateWeeklyRecurrence_UseEndDateIfNotNull() {
        var requestedDate = new SchedulerInput
        {
            StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 10, 15, 0, 0, 0, TimeSpan.Zero), // EndDate explícito
            WeeklyPeriod = 1,
            DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Wednesday }
        };

        // Invocación por reflexión porque es private
        var method = typeof(CalculateOneTime).GetMethod("CalculateWeeklyRecurrence", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var result = (List<DateTimeOffset>)method!.Invoke(null, new object[] { requestedDate })!;

        // Debe incluir solo fechas hasta el 15 de octubre
        Assert.All(result, d => Assert.True(d <= requestedDate.EndDate));
        Assert.Contains(new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero), result);
        Assert.Contains(new DateTimeOffset(2025, 10, 8, 0, 0, 0, TimeSpan.Zero), result);
        Assert.Contains(new DateTimeOffset(2025, 10, 15, 0, 0, 0, TimeSpan.Zero), result);
    }

    /*
    [Fact]
    public void CalculateWeeklyRecurrence_UsaStartDateMas21DiasSiEndDateEsNull()
    {
        var requestedDate = new RequestedDate
        {
            StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = null, // EndDate nulo
            WeeklyPeriod = 1,
            DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Wednesday }
        };

        var method = typeof(CalcOneTime).GetMethod("CalculateWeeklyRecurrence", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var result = (List<DateTimeOffset>)method!.Invoke(null, new object[] { requestedDate })!;

        var expectedEnd = requestedDate.StartDate.AddDays(21);
        Assert.All(result, d => Assert.True(d <= expectedEnd));
        Assert.Contains(new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero), result);
        Assert.Contains(new DateTimeOffset(2025, 10, 8, 0, 0, 0, TimeSpan.Zero), result);
        Assert.Contains(new DateTimeOffset(2025, 10, 15, 0, 0, 0, TimeSpan.Zero), result);
        Assert.Contains(new DateTimeOffset(2025, 10, 22, 0, 0, 0, TimeSpan.Zero), result);
    }
    
    [Fact]
    public void CalculateWeeklyRecurrence_UsaChangeDateComoFechaInicialSiNoEsNull()
    {
        var requestedDate = new RequestedDate
        {
            StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero),
            TargetDate = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero), // TargetDate explícito
            EndDate = new DateTimeOffset(2025, 10, 20, 0, 0, 0, TimeSpan.Zero),
            WeeklyPeriod = 1,
            DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Sunday }
        };

        var method = typeof(CalcOneTime).GetMethod("CalculateWeeklyRecurrence", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var result = (List<DateTimeOffset>)method!.Invoke(null, new object[] { requestedDate })!;

        // La primera fecha debe ser el primer domingo igual o después del 5 de octubre
        Assert.Equal(new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero), result[0]);
    }

    [Fact]
    public void CalculateWeeklyRecurrence_UsaStartDateComoFechaInicialSiChangeDateEsNull()
    {
        var requestedDate = new RequestedDate
        {
            StartDate = new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero),
            TargetDate = null,
            EndDate = new DateTimeOffset(2025, 10, 20, 0, 0, 0, TimeSpan.Zero),
            WeeklyPeriod = 1,
            DaysOfWeek = new List<DayOfWeek> { DayOfWeek.Wednesday }
        };

        var method = typeof(CalcOneTime).GetMethod("CalculateWeeklyRecurrence", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var result = (List<DateTimeOffset>)method!.Invoke(null, new object[] { requestedDate })!;

        // La primera fecha debe ser el primer miércoles igual o después del 1 de octubre
        Assert.Equal(new DateTimeOffset(2025, 10, 1, 0, 0, 0, TimeSpan.Zero), result[0]);
    }


    */







    [Fact]
    public void NextWeekday_ReturnsSameDay_WhenStartIsTargetDay() {
        var start = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        var result = InvokeNextWeekday(start, DayOfWeek.Sunday);

        Assert.Equal(start, result);
    }

    [Fact]
    public void NextWeekday_ReturnsNextCorrectDay() {
        var start = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
        var result = InvokeNextWeekday(start, DayOfWeek.Wednesday);

        Assert.Equal(new DateTimeOffset(2025, 10, 8, 0, 0, 0, TimeSpan.Zero), result);
    }

    private static DateTimeOffset InvokeNextWeekday(DateTimeOffset start, DayOfWeek day) {
        var method = typeof(CalculateOneTime).GetMethod("NextWeekday", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        return (DateTimeOffset)method!.Invoke(null, new object[] { start, day })!;
    }
}
