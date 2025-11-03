namespace Scheduler_Lib.Core.Services.Utilities;
public static class DateSafetyHelper {
    public static bool TryAddDaysSafely(DateTime dt, int days, out DateTime result) {
        result = dt;
        if (days == 0) return true;
        
        // Check overflow for positive days
        if (days > 0 && dt > DateTime.MaxValue.AddDays(-days)) return false;
        
        // Check overflow for negative days
        if (days < 0 && dt < DateTime.MinValue.AddDays(-days)) return false;
        
        result = dt.AddDays(days);
        return true;
    }
}
