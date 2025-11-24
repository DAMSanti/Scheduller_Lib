# Scheduler_Lib

## Descripci�n General

**Scheduler_Lib** es una biblioteca desarrollada en .NET 8 que permite calcular fechas programadas de manera flexible, soportando tanto eventos �nicos como recurrentes. Su dise�o sigue principios SOLID y buenas pr�cticas de arquitectura, facilitando la extensi�n, el mantenimiento y la reutilizaci�n del c�digo.

---

## Funcionalidad Principal

- **C�lculo de Fechas �nicas (OneTime):**  
  Permite programar un evento para una fecha espec�fica o con un desplazamiento determinado. El c�lculo tiene en cuenta la zona horaria y los posibles cambios por horario de verano (Daylight Saving Time).

- **C�lculo de Fechas Recurrentes (Recurrent):**  
  Permite programar eventos que se repiten cada cierto n�mero de d�as, generando una lista de futuras fechas de ejecuci�n. El c�lculo de cada fecha recurrente tambi�n considera la zona horaria y los cambios de offset por horario de verano.

- **Validaciones:**  
  Incluye validaciones para asegurar la coherencia de los datos de entrada (por ejemplo, que el desplazamiento sea positivo, que las fechas est�n en rango y que los datos requeridos no sean nulos).

- **Centralizaci�n de Mensajes:**  
  Todos los textos de error y mensajes relevantes est�n centralizados en una �nica clase, facilitando su modificaci�n y traducci�n.

---

## Estructura de Carpetas

- `Core/Model`: Modelos de datos principales (`RequestedDate`, `SolvedDate`, etc.).
- `Core/Services`: L�gica de negocio para el c�lculo de fechas (`CalcOneTime`, `CalcRecurrent`, etc.).
- `Core/Factory`: F�brica para obtener la estrategia de c�lculo adecuada seg�n la periodicidad.
- `Infrastructure/Validations`: Validaciones de entrada y reglas de negocio.
- `Resources`: Centralizaci�n de mensajes y textos de error.
- `Scheduler_Test`: Pruebas unitarias para garantizar la calidad y robustez de la biblioteca.

---


## Pruebas

El proyecto incluye pruebas unitarias con xUnit para validar el correcto funcionamiento de las validaciones y los c�lculos de fechas.

---

## Requisitos

- .NET 8.0 o superior

---

[![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/DAMSanti/Scheduller_Lib)

� Santiago Manuel Tamayo Arozamena 2025
