using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Factory;
public static class ScheduleCalculator {
    public static ResultPattern<SolvedDate> GetScheduleCalculator(RequestedDate requestedDate) {
        return requestedDate.Periodicity switch {
            EnumPeriodicity.OneTime => new CalcOneTime().CalculateDate(requestedDate),
            EnumPeriodicity.Recurrent => new CalcRecurrent().CalculateDate(requestedDate),
            _ => ResultPattern<SolvedDate>.Failure(Messages.ErrorUnsupportedPeriodicity)
        };
    }
}
