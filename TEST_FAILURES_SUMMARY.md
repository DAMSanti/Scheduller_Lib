# Resumen de Fallos de Tests tras Refactorización

## Estadísticas
- **Total de Tests**: 266
- **Tests Correctos**: 236 (88.7%)
- **Tests Fallando**: 30 (11.3%)

## Categorías de Problemas

### 1. Problemas de Reflexión (Tests Internos)
**Archivos afectados:**
- `DailyRecurrenceIntegrationTests.cs`

**Tests fallando:**
- `DailyRecurrence_Internal_FillWeeklySlots_Reflection_Coverage`
- `DailyRecurrence_Internal_GetCandidateLocalForWeekAndDay_Reflection_ReturnsNullOnOverflow`
- `DailyRecurrence_Internal_GetCandidateLocalForWeekAndDay_Reflection_ReturnsCandidateWhenSameDay`
- `DailyRecurrence_Internal_FillWeeklySlots_Reflection_GeneratesSlotsWithinRange`

**Causa:** 
Estos tests usan reflexión para acceder a clases internas que ya no existen o han cambiado de namespace:
- `Scheduler_Lib.Core.Services.Calculators.Daily.DailyRecurrenceCalculator` ya no existe
- La funcionalidad se movió a `DailyRecurrenceStrategy` y otros componentes

**Solución:**
1. **Opción A (Recomendada)**: Eliminar estos tests internos ya que prueban implementación interna
2. **Opción B**: Actualizar para usar las nuevas estrategias con reflexión
3. **Opción C**: Hacer públicas ciertas clases para testing (no recomendado)

---

### 2. Problemas de Validación
**Archivos afectados:**
- `SchedulerIntegrationTest.cs` (CalculateDateIntegrationTests)
- `ValidateOnceDirectTests.cs` (archivo antiguo a eliminar)

**Tests fallando:**
- `ValidateCalculateDate_ShouldFail_WhenSchedulerInputIsNull`
- `ValidateOnce_ShouldSuccess_WhenOnceDaily`
- `ValidateOnce_ShouldSuccess_WhenEndDateNullAndTargetDateValid`
- `ValidateOnce_ShouldSuccess_WhenTargetDateEqualsStartDateWithoutEndDate`
- `ValidateOnce_ShouldFail_WithAllThreeErrors`
- `ValidateOnce_ShouldFail_WithMultipleErrors_OnceWeeklyAndStartGreaterThanEnd`
- `SchedulerCalculation_ShouldSuccess_WhenOnceConfigurationIsValid`

**Causa:**
Los tests esperan que el validador retorne ciertos errores específicos pero la nueva arquitectura puede estar cambiando el orden o formato de los mensajes de error.

**Solución:**
Revisar cada test y actualizar las expectativas según el comportamiento real de la nueva arquitectura de validación.

---

### 3. Problemas de MonthlyRecurrence
**Archivos afectados:**
- `MonthlyRecurrenceIntegrationTests.cs`

**Tests fallando:**
- `MonthlyRecurrence_ShouldSuccess_WhenCurrentDateAfterAllValidDates`
  - **Error**: Expected `2025-02-28T23:00:00` but got `2025-03-01T00:00:00`
  - **Causa**: Problema con zonas horarias o cálculo de fin de mes

- `MonthlyDay_ShouldSuccess_WhenDailyTimeWindowGeneratesMultipleSlotsPerDay`
  - **Error**: Expected 15 slots but got 14
  - **Causa**: Cálculo de slots diarios incorrecto

- `MonthlyThe_ShouldSuccess_WhenDailyTimeWindowWithLastWeekday`
  - **Error**: Expected 30 slots but got 29
  - **Causa**: Cálculo de slots diarios incorrecto

- `MonthlyRecurrence_ShouldSuccess_WhenNoValidDatesInRange`
  - **Error**: Expected `2025-02-15T10:00:00` but got `2025-02-15T00:00:00`
  - **Causa**: No se está aplicando correctamente el `OccursOnceAt`

- `MonthlyThe_ShouldSuccess_WhenDailyTimeWindowWithFirstMondayMultipleSlots`
- `MonthlyThe_ShouldSuccess_WhenDailyTimeWindowWithSecondWeekendDay`

**Solución:**
Revisar la lógica de `MonthlyRecurrenceStrategy` y `DailySlotGenerator` para asegurar que:
1. Los cálculos de fin de mes consideran zonas horarias correctamente
2. Los slots diarios se generan correctamente
3. El tiempo de `OccursOnceAt` se aplica cuando corresponde

---

### 4. Problemas de Localización
**Archivos afectados:**
- `LocalizationTest.cs`

**Tests fallando:**
- `Once_Configuration_ShouldGenerateLocalizedDescription(language: "en_GB")`
- `Once_Configuration_ShouldGenerateLocalizedDescription(language: "en_US")`

**Causa:**
Los tests de localización para configuración "Once" pueden estar fallando debido a cambios en cómo se construyen las descripciones.

**Solución:**
Revisar `SchedulerDescriptionBuilder` para asegurar que la localización funciona correctamente para todas las configuraciones "Once".

---

### 5. Problemas de DescriptionBuilder
**Archivos afectados:**
- `DescriptionBuilderIntegrationTests.cs`

**Tests fallando:**
- `DescriptionBuilder_Once_ReturnsOccursOnceDescription`

**Causa:**
El formato o contenido de la descripción generada ha cambiado con la refactorización.

**Solución:**
Actualizar las expectativas del test para que coincidan con el nuevo formato de descripción.

---

### 6. Problemas de RecurrenceCalculator
**Archivos afectados:**
- `RecurrenceCalculator_GetFutureDatesTests.cs`

**Tests fallando:**
- `GetFutureDates_ReturnsEmpty_WhenRecurrencyIsUnsupported`

**Causa:**
El comportamiento de `RecurrenceCalculator.GetFutureDates()` ha cambiado cuando se encuentra una recurrencia no soportada.

**Solución:**
Verificar si el método ahora lanza una excepción o retorna un error en lugar de una lista vacía.

---

### 7. Problemas de Utilidades
**Archivos afectados:**
- `UtilityTests.cs`

**Tests fallando:**
- `BaseDateTimeCalculator_ShouldReturnStartDate_WhenNoTargetAndCurrentIsDefault`

**Causa:**
Intenta usar reflexión para acceder a `BaseDateTimeCalculator` que puede haber cambiado.

**Solución:**
- Si es un test de implementación interna, eliminarlo
- Si es funcionalidad importante, crear un test de integración sin reflexión

---

### 8. Tests con problemas de conteo de slots
**Tests fallando:**
- `DailyRecurrence_ShouldSuccess_WhenOccursEveryWith15MinuteIntervals`
  - **Error esperado**: Probablemente un cálculo incorrecto de slots

**Solución:**
Revisar la lógica de generación de slots en `DailySlotGenerator`.

---

## Plan de Acción Recomendado

### Fase 1: Limpieza (Prioridad Alta)
1. **Eliminar archivo antiguo**: `ValidateOnceDirectTests.cs`
2. **Eliminar tests de reflexión internos**: Los 4 tests en `DailyRecurrenceIntegrationTests.cs`
3. **Eliminar test de reflexión**: `BaseDateTimeCalculator_ShouldReturnStartDate_WhenNoTargetAndCurrentIsDefault`

### Fase 2: Correcciones de MonthlyRecurrence (Prioridad Alta)
1. Revisar `MonthlyRecurrenceStrategy` para problemas de zona horaria
2. Revisar `DailySlotGenerator` para asegurar conteo correcto de slots
3. Asegurar que `OccursOnceAt` se aplica correctamente en recurrencias mensuales

### Fase 3: Correcciones de Validación (Prioridad Media)
1. Revisar y actualizar tests en `SchedulerIntegrationTest.cs`
2. Asegurar que los mensajes de error coinciden con las expectativas

### Fase 4: Correcciones de Localización y Descripción (Prioridad Media)
1. Actualizar tests de localización
2. Actualizar tests de `DescriptionBuilder`

### Fase 5: Correcciones Menores (Prioridad Baja)
1. Revisar test de `RecurrenceCalculator`
2. Revisar test de `DailyRecurrence` con intervalos de 15 minutos

---

## Archivos para Eliminar o Comentar Completamente
1. `ValidateOnceDirectTests.cs` - Ya existe `ValidateOnceDirectTests_New.cs`

## Archivos que Requieren Cambios Mayores
1. `DailyRecurrenceIntegrationTests.cs` - Eliminar 4 tests de reflexión
2. `MonthlyRecurrenceIntegrationTests.cs` - Corregir 6 tests
3. `SchedulerIntegrationTest.cs` - Revisar 7 tests de validación
4. `LocalizationTest.cs` - Corregir 2 tests
5. `DescriptionBuilderIntegrationTests.cs` - Corregir 1 test
6. `RecurrenceCalculator_GetFutureDatesTests.cs` - Corregir 1 test
7. `UtilityTests.cs` - Eliminar 1 test de reflexión

---

## Próximos Pasos Inmediatos

1. ¿Quieres que elimine los tests de reflexión internos?
2. ¿Quieres que empiece a corregir los problemas de MonthlyRecurrence?
3. ¿Prefieres que actualice primero los tests de validación?

Por favor, indícame por dónde quieres que empecemos.
