namespace Scheduler_Lib.Core.Model;

public class UnsupportedPeriodicityException(string message) : Exception(message);

public class OnceModeException(string message) : Exception(message);

public class NegativeOffsetException(string message) : Exception(message);

public class DateOutOfRangeException(string message) : Exception(message);

public class NullRequestException(string message) : Exception(message);

