using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Services;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Factory;
public static class ScheduleCalculator {
    public static ResultPattern<SolvedDate> GetScheduleCalculator(RequestedDate requestedDate) {
        switch (requestedDate.Periodicity) {
            case EnumPeriodicity.OneTime:
                return new CalcOneTime().CalculateDate(requestedDate);
            case EnumPeriodicity.Recurrent:
                return new CalcRecurrent().CalculateDate(requestedDate);
            default:
                return ResultPattern<SolvedDate>.Failure(Messages.ErrorUnsupportedPeriodicity);
        }
    }
}
