using System;
using Scheduler_Lib.Classes;
using Scheduler_Lib.Interfaz;

namespace Scheduler_Lib.Services {
    public class CalcRecurrent : ISchedule {
        public SolvedDate CalcDate(RequestedDate requestedDate) {
            if (requestedDate.Offset == null || requestedDate.Offset.Value.Days <= 0) {
                throw new Exception("Positive Offset required.");
            }

            if (requestedDate.Date < requestedDate.StartDate || requestedDate.Date > requestedDate.EndDate) {
                throw new Exception("The date should be between start and end date.");
            }

            var nextDate = requestedDate.Date.Add(requestedDate.Offset.Value);

            return new SolvedDate {
                NewDate = nextDate,
                Description = $"Occurs every {requestedDate.Offset.Value.Days} days. Schedule will be used on {requestedDate.Date:dd/MM/yyyy}" +
                              $" at {requestedDate.Date:HH:mm} starting on {requestedDate.StartDate:dd/MM/yyyy}"
            };
        }
    }
}