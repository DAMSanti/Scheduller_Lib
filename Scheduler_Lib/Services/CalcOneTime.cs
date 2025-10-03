using Scheduler_Lib.Classes;
using Scheduler_Lib.Interfaz;

namespace Scheduler_Lib.Services
{
    public class CalcOneTime : ISchedule {
        public SolvedDate CalcDate(RequestedDate requestedDate) {
            if (requestedDate.ChangeDate != null) {
                return new SolvedDate
                {
                    NewDate = requestedDate.ChangeDate.Value,
                    Description = $"Occurs once: Schedule will be used on {requestedDate.ChangeDate.Value:dd/MM/yyyy} at {requestedDate.ChangeDate.Value:HH:mm} starting on {requestedDate.StartDate:dd/MM/yyyy}"
                };
            }

            if (requestedDate.Offset != null) {
                var newDate = requestedDate.Date.Add(requestedDate.Offset.Value);
                if (newDate > requestedDate.EndDate || newDate < requestedDate.StartDate) {
                    return new SolvedDate
                    {
                        NewDate = requestedDate.Date,
                        Description = "ERROR: The given date is after the end date."
                    };
                }

                return new SolvedDate
                {
                    NewDate = newDate,
                    Description = $"Occurs Once: Schedule will be used on {newDate:dd/MM/yyyy HH:mm} starting on {requestedDate.StartDate:dd/MM/yyyy HH:mm}"
                };
            }

            throw new Exception("New date time or offset required in Once mode.");
        }
    }
}