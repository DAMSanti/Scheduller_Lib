namespace Scheduler_Lib.Core.Services.Localization;

internal static class LocalizationResources {
    internal static string GetDayName(DayOfWeek day, string language) => language switch {
        "es-ES" => day switch {
            DayOfWeek.Monday => "Lunes",
            DayOfWeek.Tuesday => "Martes",
            DayOfWeek.Wednesday => "Miércoles",
            DayOfWeek.Thursday => "Jueves",
            DayOfWeek.Friday => "Viernes",
            DayOfWeek.Saturday => "Sábado",
            DayOfWeek.Sunday => "Domingo",
            _ => day.ToString()
        },
        "en-US" => day switch {
            DayOfWeek.Monday => "Monday",
            DayOfWeek.Tuesday => "Tuesday",
            DayOfWeek.Wednesday => "Wednesday",
            DayOfWeek.Thursday => "Thursday",
            DayOfWeek.Friday => "Friday",
            DayOfWeek.Saturday => "Saturday",
            DayOfWeek.Sunday => "Sunday",
            _ => day.ToString()
        },
        "en-GB" => day switch {
            DayOfWeek.Monday => "Monday",
            DayOfWeek.Tuesday => "Tuesday",
            DayOfWeek.Wednesday => "Wednesday",
            DayOfWeek.Thursday => "Thursday",
            DayOfWeek.Friday => "Friday",
            DayOfWeek.Saturday => "Saturday",
            DayOfWeek.Sunday => "Sunday",
            _ => day.ToString()
        },
        _ => day.ToString()
    };

    internal static string GetDescription(string key, string language) => (key, language) switch {
        ("weekly.every.week", "es-ES") => "cada semana",
        ("weekly.every.weeks", "es-ES") => "cada {0} semana(s)",
        ("weekly.at", "es-ES") => "a las {0}",
        ("weekly.occurs.every", "es-ES") => "ocurre cada {0}",
        ("weekly.on.days", "es-ES") => "en",

        ("weekly.every.week", "en-US") => "every week",
        ("weekly.every.weeks", "en-US") => "every {0} week(s)",
        ("weekly.at", "en-US") => "at {0}",
        ("weekly.occurs.every", "en-US") => "occurs every {0}",
        ("weekly.on.days", "en-US") => "on",

        ("weekly.every.week", "en-GB") => "every week",
        ("weekly.every.weeks", "en-GB") => "every {0} week(s)",
        ("weekly.at", "en-GB") => "at {0}",
        ("weekly.occurs.every", "en-GB") => "occurs every {0}",
        ("weekly.on.days", "en-GB") => "on",

        ("daily.every.day", "es-ES") => "cada día",
        ("daily.every.days", "es-ES") => "cada {0} día(s)",
        ("daily.at", "es-ES") => "a las {0}",
        ("daily.occurs.every", "es-ES") => "ocurre cada {0}",

        ("daily.every.day", "en-US") => "every day",
        ("daily.every.days", "en-US") => "every {0} day(s)",
        ("daily.at", "en-US") => "at {0}",
        ("daily.occurs.every", "en-US") => "occurs every {0}",

        ("daily.every.day", "en-GB") => "every day",
        ("daily.every.days", "en-GB") => "every {0} day(s)",
        ("daily.at", "en-GB") => "at {0}",
        ("daily.occurs.every", "en-GB") => "occurs every {0}",

        ("monthly.on.day", "es-ES") => "el día {0} de cada mes",
        ("monthly.the.day", "es-ES") => "el {0} {1} de cada mes",
        ("monthly.every.month", "es-ES") => "cada mes",

        ("monthly.on.day", "en-US") => "on day {0} of every month",
        ("monthly.the.day", "en-US") => "the {0} {1} of every month",
        ("monthly.every.month", "en-US") => "every month",

        ("monthly.on.day", "en-GB") => "on day {0} of every month",
        ("monthly.the.day", "en-GB") => "the {0} {1} of every month",
        ("monthly.every.month", "en-GB") => "every month",

        ("monthly.every.months", "es-ES") => "cada {0} mes(es)",

        ("monthly.every.months", "en-US") => "every {0} month(s)",

        ("monthly.every.months", "en-GB") => "every {0} month(s)",

        ("monthlytype.day", "es-ES") => "día",
        ("monthlytype.weekday", "es-ES") => "día laborable",
        ("monthlytype.weekendday", "es-ES") => "día de fin de semana",

        ("monthlytype.day", "en-US") => "day",
        ("monthlytype.weekday", "en-US") => "weekday",
        ("monthlytype.weekendday", "en-US") => "weekend day",

        ("monthlytype.day", "en-GB") => "day",
        ("monthlytype.weekday", "en-GB") => "weekday",
        ("monthlytype.weekendday", "en-GB") => "weekend day",

        ("frequency.first", "es-ES") => "primer",
        ("frequency.second", "es-ES") => "segundo",
        ("frequency.third", "es-ES") => "tercer",
        ("frequency.fourth", "es-ES") => "cuarto",
        ("frequency.last", "es-ES") => "último",

        ("frequency.first", "en-US") => "first",
        ("frequency.second", "en-US") => "second",
        ("frequency.third", "en-US") => "third",
        ("frequency.fourth", "en-US") => "fourth",
        ("frequency.last", "en-US") => "last",

        ("frequency.first", "en-GB") => "first",
        ("frequency.second", "en-GB") => "second",
        ("frequency.third", "en-GB") => "third",
        ("frequency.fourth", "en-GB") => "fourth",
        ("frequency.last", "en-GB") => "last",

        ("time.second", "es-ES") => "segundo",
        ("time.seconds", "es-ES") => "segundos",
        ("time.minute", "es-ES") => "minuto",
        ("time.minutes", "es-ES") => "minutos",
        ("time.hour", "es-ES") => "hora",
        ("time.hours", "es-ES") => "horas",
        ("time.day", "es-ES") => "día",
        ("time.days", "es-ES") => "días",

        ("time.second", "en-US") => "second",
        ("time.seconds", "en-US") => "seconds",
        ("time.minute", "en-US") => "minute",
        ("time.minutes", "en-US") => "minutes",
        ("time.hour", "en-US") => "hour",
        ("time.hours", "en-US") => "hours",
        ("time.day", "en-US") => "day",
        ("time.days", "en-US") => "days",

        ("time.second", "en-GB") => "second",
        ("time.seconds", "en-GB") => "seconds",
        ("time.minute", "en-GB") => "minute",
        ("time.minutes", "en-GB") => "minutes",
        ("time.hour", "en-GB") => "hour",
        ("time.hours", "en-GB") => "hours",
        ("time.day", "en-GB") => "day",
        ("time.days", "en-GB") => "days",

        ("next.execution", "es-ES") => "Próxima ejecución",
        ("from", "es-ES") => "desde",
        ("to", "es-ES") => "hasta",
        ("and", "es-ES") => "y",

        ("next.execution", "en-US") => "Next execution",
        ("from", "en-US") => "from",
        ("to", "en-US") => "to",
        ("and", "en-US") => "and",

        ("next.execution", "en-GB") => "Next execution",
        ("from", "en-GB") => "from",
        ("to", "en-GB") => "to",
        ("and", "en-GB") => "and",

        _ => key
    };
}