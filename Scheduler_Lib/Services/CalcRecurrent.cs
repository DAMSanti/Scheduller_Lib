using Scheduler_Lib.Classes;
using Scheduler_Lib.Interface;

namespace Scheduler_Lib.Services {
    public class CalcRecurrent : ISchedule {
        public SolvedDate CalcDate(RequestedDate requestedDate) {
            Validations.Validations.ValidateRecurrent(requestedDate);

            var nextDate = requestedDate.Date.Add(requestedDate.Offset.Value);

            return new SolvedDate {
                NewDate = nextDate,
                Description = $"Occurs every {requestedDate.Offset.Value.Days} days. Schedule will be used on {requestedDate.Date:dd/MM/yyyy}" +
                              $" at {requestedDate.Date:HH:mm} starting on {requestedDate.StartDate:dd/MM/yyyy}"
            };
        }
    }
}