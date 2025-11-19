namespace Scheduler_Lib.Core.Services.Localization;

internal static class LocalizationResources {
    internal static string GetDayName(DayOfWeek day, string language) => language switch {
        "es_ES" => day switch {
            DayOfWeek.Monday => "Lunes",
            DayOfWeek.Tuesday => "Martes",
            DayOfWeek.Wednesday => "Miércoles",
            DayOfWeek.Thursday => "Jueves",
            DayOfWeek.Friday => "Viernes",
            DayOfWeek.Saturday => "Sábado",
            DayOfWeek.Sunday => "Domingo",
            _ => day.ToString()
        },
        "en_US" => day switch {
            DayOfWeek.Monday => "Monday",
            DayOfWeek.Tuesday => "Tuesday",
            DayOfWeek.Wednesday => "Wednesday",
            DayOfWeek.Thursday => "Thursday",
            DayOfWeek.Friday => "Friday",
            DayOfWeek.Saturday => "Saturday",
            DayOfWeek.Sunday => "Sunday",
            _ => day.ToString()
        },
        "en_GB" => day switch {
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
        ("weekly.every.week", "es_ES") => "cada semana",
        ("weekly.every.weeks", "es_ES") => "cada {0} semana(s)",
        ("weekly.at", "es_ES") => "a las {0}",
        ("weekly.occurs.every", "es_ES") => "ocurre cada {0}",
        ("weekly.on.days", "es_ES") => "en",

        ("weekly.every.week", "en_US") => "every week",
        ("weekly.every.weeks", "en_US") => "every {0} week(s)",
        ("weekly.at", "en_US") => "at {0}",
        ("weekly.occurs.every", "en_US") => "occurs every {0}",
        ("weekly.on.days", "en_US") => "on",

        ("weekly.every.week", "en_GB") => "every week",
        ("weekly.every.weeks", "en_GB") => "every {0} week(s)",
        ("weekly.at", "en_GB") => "at {0}",
        ("weekly.occurs.every", "en_GB") => "occurs every {0}",
        ("weekly.on.days", "en_GB") => "on",

        ("daily.every.day", "es_ES") => "cada día",
        ("daily.every.days", "es_ES") => "cada {0} día(s)",
        ("daily.at", "es_ES") => "a las {0}",
        ("daily.occurs.every", "es_ES") => "ocurre cada {0}",

        ("daily.every.day", "en_US") => "every day",
        ("daily.every.days", "en_US") => "every {0} day(s)",
        ("daily.at", "en_US") => "at {0}",
        ("daily.occurs.every", "en_US") => "occurs every {0}",

        ("daily.every.day", "en_GB") => "every day",
        ("daily.every.days", "en_GB") => "every {0} day(s)",
        ("daily.at", "en_GB") => "at {0}",
        ("daily.occurs.every", "en_GB") => "occurs every {0}",

        ("monthly.on.day", "es_ES") => "el día {0} de cada mes",
        ("monthly.the.day", "es_ES") => "el {0} {1} de cada mes",
        ("monthly.every.month", "es_ES") => "cada mes",

        ("monthly.on.day", "en_US") => "on day {0} of every month",
        ("monthly.the.day", "en_US") => "the {0} {1} of every month",
        ("monthly.every.month", "en_US") => "every month",

        ("monthly.on.day", "en_GB") => "on day {0} of every month",
        ("monthly.the.day", "en_GB") => "the {0} {1} of every month",
        ("monthly.every.month", "en_GB") => "every month",

        ("monthly.every.months", "es_ES") => "cada {0} mes(es)",

        ("monthly.every.months", "en_US") => "every {0} month(s)",

        ("monthly.every.months", "en_GB") => "every {0} month(s)",

        ("monthlytype.day", "es_ES") => "día",
        ("monthlytype.weekday", "es_ES") => "día laborable",
        ("monthlytype.weekendday", "es_ES") => "día de fin de semana",

        ("monthlytype.day", "en_US") => "day",
        ("monthlytype.weekday", "en_US") => "weekday",
        ("monthlytype.weekendday", "en_US") => "weekend day",

        ("monthlytype.day", "en_GB") => "day",
        ("monthlytype.weekday", "en_GB") => "weekday",
        ("monthlytype.weekendday", "en_GB") => "weekend day",

        ("frequency.first", "es_ES") => "primer",
        ("frequency.second", "es_ES") => "segundo",
        ("frequency.third", "es_ES") => "tercer",
        ("frequency.fourth", "es_ES") => "cuarto",
        ("frequency.last", "es_ES") => "último",

        ("frequency.first", "en_US") => "first",
        ("frequency.second", "en_US") => "second",
        ("frequency.third", "en_US") => "third",
        ("frequency.fourth", "en_US") => "fourth",
        ("frequency.last", "en_US") => "last",

        ("frequency.first", "en_GB") => "first",
        ("frequency.second", "en_GB") => "second",
        ("frequency.third", "en_GB") => "third",
        ("frequency.fourth", "en_GB") => "fourth",
        ("frequency.last", "en_GB") => "last",

        ("time.second", "es_ES") => "segundo",
        ("time.seconds", "es_ES") => "segundos",
        ("time.minute", "es_ES") => "minuto",
        ("time.minutes", "es_ES") => "minutos",
        ("time.hour", "es_ES") => "hora",
        ("time.hours", "es_ES") => "horas",
        ("time.day", "es_ES") => "día",
        ("time.days", "es_ES") => "días",

        ("time.second", "en_US") => "second",
        ("time.seconds", "en_US") => "seconds",
        ("time.minute", "en_US") => "minute",
        ("time.minutes", "en_US") => "minutes",
        ("time.hour", "en_US") => "hour",
        ("time.hours", "en_US") => "hours",
        ("time.day", "en_US") => "day",
        ("time.days", "en_US") => "days",

        ("time.second", "en_GB") => "second",
        ("time.seconds", "en_GB") => "seconds",
        ("time.minute", "en_GB") => "minute",
        ("time.minutes", "en_GB") => "minutes",
        ("time.hour", "en_GB") => "hour",
        ("time.hours", "en_GB") => "hours",
        ("time.day", "en_GB") => "day",
        ("time.days", "en_GB") => "days",

        ("next.execution", "es_ES") => "Próxima ejecución",
        ("from", "es_ES") => "desde",
        ("to", "es_ES") => "hasta",
        ("and", "es_ES") => "y",

        ("next.execution", "en_US") => "Next execution",
        ("from", "en_US") => "from",
        ("to", "en_US") => "to",
        ("and", "en_US") => "and",

        ("next.execution", "en_GB") => "Next execution",
        ("from", "en_GB") => "from",
        ("to", "en_GB") => "to",
        ("and", "en_GB") => "and",

        _ => key
    };
}