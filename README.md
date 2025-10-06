# Scheduler_Lib

## Descripción General

**Scheduler_Lib** es una biblioteca desarrollada en .NET 8 que permite calcular fechas programadas de manera flexible, soportando tanto eventos únicos como recurrentes. Su diseño sigue principios SOLID y buenas prácticas de arquitectura, facilitando la extensión, el mantenimiento y la reutilización del código.

---

## Funcionalidad Principal

- **Cálculo de Fechas Únicas (OneTime):**  
  Permite programar un evento para una fecha específica o con un desplazamiento determinado.

- **Cálculo de Fechas Recurrentes (Recurrent):**  
  Permite programar eventos que se repiten cada cierto número de días, generando una lista de futuras fechas de ejecución.

- **Validaciones:**  
  Incluye validaciones para asegurar la coherencia de los datos de entrada (por ejemplo, que el desplazamiento sea positivo y que las fechas estén en rango).

- **Centralización de Mensajes:**  
  Todos los textos de error y mensajes relevantes están centralizados en una única clase, facilitando su modificación y traducción.

---

## Estructura de Carpetas

- `Core/Model`: Modelos de datos principales (`RequestedDate`, `SolvedDate`, etc.).
- `Core/Services`: Lógica de negocio para el cálculo de fechas (`CalcOneTime`, `CalcRecurrent`, etc.).
- `Core/Factory`: Fábrica para obtener la estrategia de cálculo adecuada según la periodicidad.
- `Core/Interface`: Interfaces que definen los contratos de los servicios.
- `Infrastructure/Validations`: Validaciones de entrada y reglas de negocio.
- `Resources`: Centralización de mensajes y textos de error.
- `Scheduler_Test`: Pruebas unitarias para garantizar la calidad y robustez de la biblioteca.

---


## Pruebas

El proyecto incluye pruebas unitarias con xUnit para validar el correcto funcionamiento de las validaciones y los cálculos de fechas.

---

## Requisitos

- .NET 8.0 o superior

---

© Santiago Manuel Tamayo Arozamena 2025
