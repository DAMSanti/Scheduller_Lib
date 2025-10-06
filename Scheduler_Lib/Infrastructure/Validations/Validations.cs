using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Infrastructure.Validations;
public static class Validations {
    public static void ValidateRecurrent(RequestedDate requestedDate) {
        if (requestedDate.Offset == null || requestedDate.Offset.Value <= 0) {
            throw new NegativeOffsetException(Messages.ErrorPositiveOffsetRequired);
        }

        if (requestedDate.Date < requestedDate.StartDate || requestedDate.Date > requestedDate.EndDate) {
            throw new DateOutOfRangeException(Messages.ErrorDateOutOfRange);
        }
    }

    public static void ValidateCalc(RequestedDate requestedDate) {
        if (requestedDate == null) {
            throw new NullRequestException(Messages.ErrorRequestNull);
        }
    }
}