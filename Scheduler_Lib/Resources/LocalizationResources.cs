namespace Scheduler_Lib.Core.Services.Localization;

internal static class LocalizationResources {
    private static readonly Dictionary<string, Dictionary<string, string>> Descriptions = new() {
        { "weekly.every.week", new() {
            { "es_ES", "cada semana" },
            { "en_US", "every week" },
            { "en_GB", "every week" }
        }},
        { "weekly.every.weeks", new() {
            { "es_ES", "cada {0} semana(s)" },
            { "en_US", "every {0} week(s)" },
            { "en_GB", "every {0} week(s)" }
        }},
        { "weekly.at", new() {
            { "es_ES", "a las {0}" },
            { "en_US", "at {0}" },
            { "en_GB", "at {0}" }
        }},
        { "weekly.occurs.every", new() {
            { "es_ES", "ocurre cada {0}" },
            { "en_US", "occurs every {0}" },
            { "en_GB", "occurs every {0}" }
        }},
        { "weekly.on.days", new() {
            { "es_ES", "en" },
            { "en_US", "on" },
            { "en_GB", "on" }
        }},
        { "daily.every.day", new() {
            { "es_ES", "cada día" },
            { "en_US", "every day" },
            { "en_GB", "every day" }
        }},
        { "daily.every.days", new() {
            { "es_ES", "cada {0} día(s)" },
            { "en_US", "every {0} day(s)" },
            { "en_GB", "every {0} day(s)" }
        }},
        { "daily.at", new() {
            { "es_ES", "a las {0}" },
            { "en_US", "at {0}" },
            { "en_GB", "at {0}" }
        }},
        { "daily.occurs.every", new() {
            { "es_ES", "ocurre cada {0}" },
            { "en_US", "occurs every {0}" },
            { "en_GB", "occurs every {0}" }
        }},
        { "monthly.on.day", new() {
            { "es_ES", "el día {0} de cada mes" },
            { "en_US", "on day {0} of every month" },
            { "en_GB", "on day {0} of every month" }
        }},
        { "monthly.the.day", new() {
            { "es_ES", "el {0} {1} de cada mes" },
            { "en_US", "the {0} {1} of every month" },
            { "en_GB", "the {0} {1} of every month" }
        }},
        { "monthly.every.month", new() {
            { "es_ES", "cada mes" },
            { "en_US", "every month" },
            { "en_GB", "every month" }
        }},
        { "monthly.every.months", new() {
            { "es_ES", "cada {0} mes(es)" },
            { "en_US", "every {0} month(s)" },
            { "en_GB", "every {0} month(s)" }
        }},
        { "monthlytype.day", new() {
            { "es_ES", "día" },
            { "en_US", "day" },
            { "en_GB", "day" }
        }},
        { "monthlytype.weekday", new() {
            { "es_ES", "día laborable" },
            { "en_US", "weekday" },
            { "en_GB", "weekday" }
        }},
        { "monthlytype.weekendday", new() {
            { "es_ES", "día de fin de semana" },
            { "en_US", "weekend day" },
            { "en_GB", "weekend day" }
        }},
        { "frequency.first", new() {
            { "es_ES", "primer" },
            { "en_US", "first" },
            { "en_GB", "first" }
        }},
        { "frequency.second", new() {
            { "es_ES", "segundo" },
            { "en_US", "second" },
            { "en_GB", "second" }
        }},
        { "frequency.third", new() {
            { "es_ES", "tercer" },
            { "en_US", "third" },
            { "en_GB", "third" }
        }},
        { "frequency.fourth", new() {
            { "es_ES", "cuarto" },
            { "en_US", "fourth" },
            { "en_GB", "fourth" }
        }},
        { "frequency.last", new() {
            { "es_ES", "último" },
            { "en_US", "last" },
            { "en_GB", "last" }
        }},
        { "time.second", new() {
            { "es_ES", "segundo" },
            { "en_US", "second" },
            { "en_GB", "second" }
        }},
        { "time.seconds", new() {
            { "es_ES", "segundos" },
            { "en_US", "seconds" },
            { "en_GB", "seconds" }
        }},
        { "time.minute", new() {
            { "es_ES", "minuto" },
            { "en_US", "minute" },
            { "en_GB", "minute" }
        }},
        { "time.minutes", new() {
            { "es_ES", "minutos" },
            { "en_US", "minutes" },
            { "en_GB", "minutes" }
        }},
        { "time.hour", new() {
            { "es_ES", "hora" },
            { "en_US", "hour" },
            { "en_GB", "hour" }
        }},
        { "time.hours", new() {
            { "es_ES", "horas" },
            { "en_US", "hours" },
            { "en_GB", "hours" }
        }},
        { "time.day", new() {
            { "es_ES", "día" },
            { "en_US", "day" },
            { "en_GB", "day" }
        }},
        { "time.days", new() {
            { "es_ES", "días" },
            { "en_US", "days" },
            { "en_GB", "days" }
        }},
        { "next.execution", new() {
            { "es_ES", "Próxima ejecución" },
            { "en_US", "Next execution" },
            { "en_GB", "Next execution" }
        }},
        { "from", new() {
            { "es_ES", "desde" },
            { "en_US", "from" },
            { "en_GB", "from" }
        }},
        { "to", new() {
            { "es_ES", "hasta" },
            { "en_US", "to" },
            { "en_GB", "to" }
        }},
        { "and", new() {
            { "es_ES", "y" },
            { "en_US", "and" },
            { "en_GB", "and" }
        }},
        { "occurs.once", new() {
            { "es_ES", "Ocurre una vez: El programador se usará el {0} a las {1} comenzando el {2}" },
            { "en_US", "Occurs once: Schedule will be used on {0} at {1} starting on {2}" },
            { "en_GB", "Occurs once: Schedule will be used on {0} at {1} starting on {2}" }
        }},
    };

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

    internal static string GetDescription(string key, string language) {
        if (string.IsNullOrWhiteSpace(language)) language = "es_ES";
        if (Descriptions.TryGetValue(key, out var perLang) && perLang.TryGetValue(language, out var value))
            return value;
        return key;
    }
}