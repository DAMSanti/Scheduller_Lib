namespace Scheduler_Lib.Resources;

public static class Messages {
    public const string ErrorPositiveOffsetRequired = "ERROR: Positive Period required.";
    public const string ErrorDateOutOfRange = "The date should be between start and end date.";
    public const string ErrorRequestNull = "Error: The request shouldn't be null.";
    public const string ErrorUnsupportedPeriodicity = "Unsupported periodicity.";
    public const string ErrorOnceMode = "New date time required in Once mode.";
    public const string ErrorChangeDateAfterEndDate = "ERROR: The given date must be before the end date.";
    public const string ErrorStartDatePostEndDate = "ERROR: Start date must be before or equal to end date.";
    public const string ErrorChangeDateNull = "ERROR: ChangeDate must have a value";
    public const string ErrorEndDateNull = "ERROR: EndDate must have a value";
    public const string ErrorWeeklyPeriodRequired = "ERROR: Weekly Period is required";
    public const string ErrorDaysOfWeekRequired = "ERROR: You need to select some days of the week";
    public const string ErrorDailyTimeWindowIncomplete = "ERROR: You lack some of the required dates.";
    public const string ErrorDailyStartAfterEnd = "ERROR: Your StartTime is after your EndTime";
}