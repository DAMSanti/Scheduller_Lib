using System;
using Scheduler_Lib.Classes;
using Scheduler_Lib.Interaz;

namespace Scheduler_Lib.Services {
    public class Service {
        public static SolvedDate CalcDate(RequestedDate requestedDate) {
            if (requestedDate == null) {
                throw new Exception("Error: The request shouldn't be null.");
            }

            if (!requestedDate.Enabled) {
                return new SolvedDate {
                    NewDate = requestedDate.Date,
                    Description = "Disabled: No changes performed."
                };
            }

            //var calcDate = ScheduleCalculator.GetCalculator(requestedDate.Periodicity);
            //return calcDate.CalcDate(requestedDate);
        }
    }
}