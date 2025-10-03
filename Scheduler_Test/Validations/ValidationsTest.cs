using Scheduler_Lib.Classes;
using Scheduler_Lib.Enum;
using Scheduler_Lib.Services;

namespace Scheduler_Lib.Validations
{
    public class ValidationsTest
    {
        [Fact]
        public void NullOffset_Recurrent_Invalid()
        {
            var requestedDate = new RequestedDate
            {
                Date = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero),
                Enabled = true,
                StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
                Offset = null,
                Periodicity = Periodicity.Recurrent
            };

            var preResult = new CalcRecurrent();
            var result = Assert.Throws<Exception>(() => preResult.CalcDate(requestedDate));
            Assert.Equal("ERROR: Positive Offset required.", result.Message);
        }

        [Fact]
        public void NegativeOffset_Recurrent_Invalid()
        {
            var requestedDate = new RequestedDate
            {
                Date = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero),
                Enabled = true,
                StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
                Offset = TimeSpan.FromDays(-1),
                Periodicity = Periodicity.Recurrent
            };

            var preResult = new CalcRecurrent();
            var result = Assert.Throws<Exception>(() => preResult.CalcDate(requestedDate));
            Assert.Equal("ERROR: Positive Offset required.", result.Message);
        }

        [Fact]
        public void Recurrent_NoOffset()
        {
            RequestedDate requestedDate = new RequestedDate
            {
                Date = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero),
                Enabled = true,
                StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
                Offset = null,
                Periodicity = Periodicity.Recurrent
            };

            var result = Assert.Throws<Exception>(() => Service.CalcDate(requestedDate));
            Assert.Equal("ERROR: Positive Offset required.", result.Message);
        }
    }
}
