using Scheduler_Lib.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler_Lib.Validations {
    public static class Validations {
        public static void ValidateRecurrent(RequestedDate requestedDate) {
            if (requestedDate.Offset == null || requestedDate.Offset.Value.Days <= 0) {
                throw new Exception("Positive Offset required.");
            }

            if (requestedDate.Date < requestedDate.StartDate || requestedDate.Date > requestedDate.EndDate) {
                throw new Exception("The date should be between start and end date.");
            }
        }
        public static void ValidateCalc(RequestedDate requestedDate)
        {
            if (requestedDate == null)
            {
                throw new Exception("Error: The request shouldn't be null.");
            }
        }
    }
}
