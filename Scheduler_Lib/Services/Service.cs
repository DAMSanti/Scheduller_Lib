using Scheduler_Lib.Classes;

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

            switch (requestedDate.Periodicity) {
                case Periodicity.OneTime:
                    return CalcOneTime(requestedDate);
                case Periodicity.Recurrent:
                    return CalcRecurrent(requestedDate);
                default:
                    throw new Exception("Periodicity not recognized.");
            }
        }

        private static SolvedDate CalcOneTime(RequestedDate requestedDate) {
            if (requestedDate.ChangeDate != null) {
                return new SolvedDate {
                    NewDate = requestedDate.ChangeDate.Value,
                    Description = $"Occurs once: Schedule will be used on {requestedDate.ChangeDate.Value.Date} " +
                                  $"at {requestedDate.ChangeDate.Value.TimeOfDay} starting on {requestedDate.StartDate} "
                };
            }

            if (requestedDate.Offset != null) {
                var newDate = requestedDate.Date.Add(requestedDate.Offset.Value);
                if (newDate > requestedDate.EndDate && newDate < requestedDate.StartDate) {
                    return new SolvedDate {
                        NewDate = requestedDate.Date,
                        Description = $"ERROR: The given date is after the end date."
                    };
                }
                return new SolvedDate {
                    NewDate = newDate,
                    Description = $"Occurs Once: Schedule will be used on {newDate} starting on {requestedDate.StartDate} "
                };
            }

            throw new Exception("New date time or offset required in Once mode.");
        }


        private static SolvedDate CalcRecurrent(RequestedDate requestedDate) {
            if (requestedDate.Offset == null) {
                throw new Exception("Positive Offset required.");
            }

            TimeSpan span = requestedDate.Offset.Value;

            if (span.Days < 0) {
                throw new Exception("Offset must be positive.");
            }

            if (requestedDate.Date < requestedDate.StartDate 
                && requestedDate.Date > requestedDate.EndDate 
                && (requestedDate.Date + requestedDate.Offset) < requestedDate.EndDate) {
                throw new Exception("The date should be between start and end date.");
            }

            var nextDate = requestedDate.Date.Add(requestedDate.Offset.Value);

            return new SolvedDate {
                NewDate = nextDate,
                Description = $"Occurs every {requestedDate.Offset.Value.Days} days. Schedule will be used on {requestedDate.Date.Date}" +
                              $" at {requestedDate.Date.TimeOfDay} starting on {requestedDate.StartDate}"
            };
        }
    }
}