namespace Scheduler_Lib.Resources;

internal static class LocalizationResources {
    private static readonly string[] SupportedLanguages = ["es_ES", "en_US", "en_GB"];
    private static readonly Dictionary<string, string> LanguageIndex = new() {
        { "es_ES", "0" },
        { "en_US", "1" },
        { "en_GB", "2" }
    };

    private static readonly Dictionary<string, string[]> Descriptions = new() {
        { "weekly.every.week", new[] { "cada semana", "every week", "every week" } },
        { "weekly.every.weeks", new[] { "cada {0} semana(s)", "every {0} week(s)", "every {0} week(s)" } },
        { "weekly.at", new[] { "a las {0}", "at {0}", "at {0}" } },
        { "weekly.occurs.every", new[] { "ocurre cada {0}", "occurs every {0}", "occurs every {0}" } },
        { "weekly.on.days", new[] { "en", "on", "on" } },
        { "daily.every.day", new[] { "cada día", "every day", "every day" } },
        { "daily.every.days", new[] { "cada {0} día(s)", "every {0} day(s)", "every {0} day(s)" } },
        { "daily.at", new[] { "a las {0}", "at {0}", "at {0}" } },
        { "daily.occurs.every", new[] { "ocurre cada {0}", "occurs every {0}", "occurs every {0}" } },
        { "monthly.on.day", new[] { "el día {0} de cada mes", "on day {0} of every month", "on day {0} of every month" } },
        { "monthly.the.day", new[] { "el {0} {1} de cada mes", "the {0} {1} of every month", "the {0} {1} of every month" } },
        { "monthly.every.month", new[] { "cada mes", "every month", "every month" } },
        { "monthly.every.months", new[] { "cada {0} mes(es)", "every {0} month(s)", "every {0} month(s)" } },
        { "monthlytype.day", new[] { "día", "day", "day" } },
        { "monthlytype.weekday", new[] { "día laborable", "weekday", "weekday" } },
        { "monthlytype.weekendday", new[] { "día de fin de semana", "weekend day", "weekend day" } },
        { "frequency.first", new[] { "primer", "first", "first" } },
        { "frequency.second", new[] { "segundo", "second", "second" } },
        { "frequency.third", new[] { "tercer", "third", "third" } },
        { "frequency.fourth", new[] { "cuarto", "fourth", "fourth" } },
        { "frequency.last", new[] { "último", "last", "last" } },
        { "time.second", new[] { "segundo", "second", "second" } },
        { "time.seconds", new[] { "segundos", "seconds", "seconds" } },
        { "time.minute", new[] { "minuto", "minute", "minute" } },
        { "time.minutes", new[] { "minutos", "minutes", "minutes" } },
        { "time.hour", new[] { "hora", "hour", "hour" } },
        { "time.hours", new[] { "horas", "hours", "hours" } },
        { "time.day", new[] { "día", "day", "day" } },
        { "time.days", new[] { "días", "days", "days" } },
        { "next.execution", new[] { "Próxima ejecución", "Next execution", "Next execution" } },
        { "from", new[] { "desde", "from", "from" } },
        { "to", new[] { "hasta", "to", "to" } },
        { "and", new[] { "y", "and", "and" } },
        { "occurs.once", new[] { "Ocurre una vez: El programador se usará el {0} a las {1} comenzando el {2}", "Occurs once: Schedule will be used on {0} at {1} starting on {2}", "Occurs once: Schedule will be used on {0} at {1} starting on {2}" } },
    };

    private static readonly Dictionary<DayOfWeek, string[]> DayNames = new() {
        { DayOfWeek.Monday, new[] { "Lunes", "Monday", "Monday" } },
        { DayOfWeek.Tuesday, new[] { "Martes", "Tuesday", "Tuesday" } },
        { DayOfWeek.Wednesday, new[] { "Miércoles", "Wednesday", "Wednesday" } },
        { DayOfWeek.Thursday, new[] { "Jueves", "Thursday", "Thursday" } },
        { DayOfWeek.Friday, new[] { "Viernes", "Friday", "Friday" } },
        { DayOfWeek.Saturday, new[] { "Sábado", "Saturday", "Saturday" } },
        { DayOfWeek.Sunday, new[] { "Domingo", "Sunday", "Sunday" } },
    };
    internal static string GetDayName(DayOfWeek day, string language) {
        if (!LanguageIndex.TryGetValue(language, out var index)) index = "0";
        int idx = int.Parse(index);

        if (DayNames.TryGetValue(day, out var names))
            return names[idx];

        return day.ToString();
    }

    internal static string GetDescription(string key, string language) {
        if (string.IsNullOrWhiteSpace(language)) language = "es_ES";
        if (!LanguageIndex.TryGetValue(language, out var index)) index = "0";
        int idx = int.Parse(index);

        if (Descriptions.TryGetValue(key, out var values))
            return values[idx];

        return key;
    }
}