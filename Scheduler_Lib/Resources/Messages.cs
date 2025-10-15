namespace Scheduler_Lib.Resources;

public static class Messages {
    public const string ErrorPositiveOffsetRequired = "ERROR: Positive Period required.";
    public const string ErrorDateOutOfRange = "ERROR: The date should be between start and end date.";
    public const string ErrorRequestNull = "ERROR: The request shouldn't be null.";
    public const string ErrorUnsupportedPeriodicity = "ERROR: Unsupported periodicity.";
    public const string ErrorOnceMode = "ERROR: New date time required in Once mode.";
    public const string ErrorTargetDateAfterEndDate = "ERROR: The target date must be before the end date and after the start date.";
    public const string ErrorStartDatePostEndDate = "ERROR: Start date must be before or equal to end date.";
    public const string ErrorTargetDateNull = "ERROR: TargetDate must have a value";
    public const string ErrorEndDateNull = "ERROR: EndDate must have a value";
    public const string ErrorWeeklyPeriodRequired = "ERROR: Weekly Period is required";
    public const string ErrorDaysOfWeekRequired = "ERROR: You need to select some days of the week";
    public const string ErrorDailyTimeWindowIncomplete = "ERROR: You lack some of the required dates.";
    public const string ErrorDailyStartAfterEnd = "ERROR: Your StartTime is after your EndTime";
}