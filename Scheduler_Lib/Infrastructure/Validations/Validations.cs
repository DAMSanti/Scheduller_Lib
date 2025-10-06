using Scheduler_Lib.Core.Model;

namespace Scheduler_Lib.Infrastructure.Validations;
public static class Validations {
    public static void ValidateRecurrent(RequestedDate requestedDate) {
        if (requestedDate.Offset == null || requestedDate.Offset.Value <= 0) {
            throw new Exception(Messages.ErrorPositiveOffsetRequired);
        }

        if (requestedDate.Date < requestedDate.StartDate || requestedDate.Date > requestedDate.EndDate) {
            throw new Exception(Messages.ErrorDateOutOfRange);
        }
    }

    public static void ValidateCalc(RequestedDate requestedDate) {
        if (requestedDate == null) {
            throw new Exception(Messages.ErrorRequestNull);
        }
    }
}