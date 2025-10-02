using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scheduller_Lib.Classes;

namespace Scheduller_Lib.Services {
    public class Service {
        public SolvedDate CalcDate(RequestedDate requestedDate) {
            if (requestedDate == null) {
                throw new Exception("Error: La solicitud no puede ser nula");
            }

            if (!requestedDate.Enabled) {
                return new SolvedDate {
                    NewDate = requestedDate.Date,
                    Description = "Desactivado: No se ha realizado ninguna modificación"
                };
            }

            switch (requestedDate.Periodicity) {
                case Periodicity.OneTime:
                    return CalcOneTime(requestedDate);
                case Periodicity.Recurrent:
                    return CalcRecurrent(requestedDate);
                default:
                    throw new Exception("No se reconoce este tipo de Periodicidad.");
            }
        }

        private SolvedDate CalcOneTime(RequestedDate requestedDate) {
            if (requestedDate.ChangeDate != null) {
                return new SolvedDate {
                    NewDate = requestedDate.ChangeDate.Value,
                    Description = $"Cambio Único: Se ha cambiado la fecha a {requestedDate.ChangeDate}"
                };
            }

            if (requestedDate.Offset != null) {
                var nuevaFecha = requestedDate.Date.Add(requestedDate.Offset.Value);
                if (nuevaFecha > requestedDate.EndDate) {
                    return new SolvedDate {
                        NewDate = requestedDate.Date,
                        Description = $"ERROR: La fecha introducida es posterior a la fecha final."
                    };
                }
                return new SolvedDate {
                    NewDate = nuevaFecha,
                    Description = $"Cambio Único: Se ha cambiado la fecha a {nuevaFecha}"
                };
            }

            throw new Exception("El calculo unico requiere de fecha de reemplazo o un offset de dias.");
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