namespace Scheduler_Lib.Core.Model;

    public class UnsupportedPeriodicityException : Exception {
        public UnsupportedPeriodicityException() {

        }
        public UnsupportedPeriodicityException(string mensaje) : base(mensaje) {
        }
    }

    public class OnceModeException : Exception {
        public OnceModeException() {

        }
        public OnceModeException(string mensaje) : base(mensaje) {
        }
    }

    public class NegativeOffsetException : Exception {
        public NegativeOffsetException() {

        }
        public NegativeOffsetException(string mensaje) : base(mensaje) {
        }
    }

    public class DateOutOfRangeException : Exception {
        public DateOutOfRangeException() {

        }
        public DateOutOfRangeException(string mensaje) : base(mensaje) {
        }
    }

    public class NullRequestException : Exception
    {
        public NullRequestException()
        {

        }
        public NullRequestException(string mensaje) : base(mensaje)
        {
        }
    }


