namespace Scheduler_Lib.Resources;

public static class Messages {
    public const string ErrorPositiveOffsetRequired = "ERROR: Positive Offset required.";
    public const string ErrorDateOutOfRange = "The date should be between start and end date.";
    public const string ErrorRequestNull = "Error: The request shouldn't be null.";
    public const string ErrorUnsupportedPeriodicity = "Unsupported periodicity.";
    public const string ErrorOnceMode = "New date time required in Once mode.";
    public const string ErrorChangeDateAfterEndDate = "ERROR: The given date must be before the end date.";
    public const string ErrorStartDatePostEndDate = "ERROR: Start date must be before or equal to end date.";
    public const string ErrorChangeDateNull = "ERROR: ChangeDate must have a value";
    public const string ErrorEndDateNull = "ERROR: EndDate must have a value";
}
