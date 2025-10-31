using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Core.Model.Enum;
using Scheduler_Lib.Core.Services.Calculation;
using Scheduler_Lib.Core.Services.Calculation.Helpers;
using Scheduler_Lib.Core.Services.Description;
using Scheduler_Lib.Infrastructure.Validations;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Core.Services;

/// <summary>
/// Facade principal del sistema de scheduling.
/// Punto de entrada único para calcular fechas programadas.
/// </summary>
public class SchedulerService {
    public static ResultPattern<SchedulerOutput> CalculateDate(SchedulerInput schedulerInput) {
        // 1. Validación inicial
        var validation = Validations.ValidateCalculateDate(schedulerInput);
        if (!validation.IsSuccess)
            return ResultPattern<SchedulerOutput>.Failure(validation.Error!);

        // 2. Validación específica según periodicidad
        var specificValidation = schedulerInput.Periodicity switch {
            EnumConfiguration.Once => ValidationOnce.ValidateOnce(schedulerInput),
            EnumConfiguration.Recurrent => ValidationRecurrent.ValidateRecurrent(schedulerInput),
            _ => ResultPattern<bool>.Failure(Messages.ErrorUnsupportedPeriodicity)
        };

        if (!specificValidation.IsSuccess)
            return ResultPattern<SchedulerOutput>.Failure(specificValidation.Error!);

        // 3. Obtener timezone
        var tz = TimeZoneInfo.FindSystemTimeZoneById(Config.TimeZoneId);

        // 4. Calcular siguiente fecha
        DateTimeOffset nextDate;
        
        if (schedulerInput.Periodicity == EnumConfiguration.Once) {
            // Para eventos únicos, usar TargetDate directamente
            var dateTimeHelper = new DateTimeHelper();
            var targetDate = schedulerInput.TargetDate!.Value;
            nextDate = dateTimeHelper.CreateDateTimeOffset(targetDate.DateTime, tz);
        } else {
            // Para eventos recurrentes, usar DateCalculator
            var dateCalculator = new DateCalculator();
            nextDate = dateCalculator.GetNextExecutionDate(schedulerInput, tz);
        }

        // 5. Generar descripción
        var descriptionService = new DescriptionService();
        var description = descriptionService.BuildDescription(schedulerInput, tz, nextDate);

        // 6. Construir resultado
        var output = new SchedulerOutput {
            NextDate = nextDate,
            Description = description
        };

        return ResultPattern<SchedulerOutput>.Success(output);
    }
}