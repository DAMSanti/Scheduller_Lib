using Scheduler_Lib.Classes;

namespace Scheduler_Lib.Services {
    public class Service {
        public SolvedDate CalcDate(RequestedDate requestedDate) {
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

        private SolvedDate CalcOneTime(RequestedDate requestedDate) {
            if (requestedDate.ChangeDate != null) {
                return new SolvedDate {
                    NewDate = requestedDate.ChangeDate.Value,
                    Description = $"Occurs once: Schedule will be used on {requestedDate.ChangeDate.Value.Date} at {requestedDate.ChangeDate.Value.TimeOfDay} starting on {requestedDate.StartDate} "
                };
            }

            if (requestedDate.Offset != null) {
                var nuevaFecha = requestedDate.Date.Add(requestedDate.Offset.Value);
                if (nuevaFecha > requestedDate.EndDate && nuevaFecha < requestedDate.StartDate) {
                    return new SolvedDate {
                        NewDate = requestedDate.Date,
                        Description = $"ERROR: The given date is after the end date."
                    };
                }
                return new SolvedDate {
                    NewDate = nuevaFecha,
                    Description = $"Occurs Once: Schedule will be used on {nuevaFecha} starting on {requestedDate.StartDate} "
                };
            }

            throw new Exception("New date time or offset required in Once mode.");
        }


        private SolvedDate CalcRecurrent(RequestedDate requestedDate)
        {
            if (requestedDate.Offset == null)
            {
                throw new Exception("Necesitas un Offset positivo para el calculo recurrente");
            }

            TimeSpan span = requestedDate.Offset.Value;

            if (span.Days < 0)
            {
                throw new Exception("El offset no puede ser negativo.");
            }

            if (requestedDate.Date < requestedDate.StartDate && requestedDate.Date > requestedDate.EndDate)
            {
                throw new Exception("Las fechas tienen que estar entre la fecha inicial y la fecha final.");
            }

            var daysSpan = requestedDate.EndDate - requestedDate.Date;
            var spans = daysSpan / requestedDate.Offset;

            var nextDate = requestedDate.Date.Add(requestedDate.Offset.Value);
            return new SolvedDate
            {
                NewDate = nextDate,
                Description = $"Occurs every {requestedDate.Offset.Value.Days} days. Schedule will be used on {requestedDate.Date.Date} at {requestedDate.Date.TimeOfDay} starting on {requestedDate.StartDate}"
            };
        }
    }
}