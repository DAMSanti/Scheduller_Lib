using Scheduler_Lib.Classes;
using Scheduler_Lib.Enum;

namespace Scheduler_Lib.Services
{
    public class CalcRecurrentTest
    {
        [Fact]
        public void OffSet_Recurrent_Valid()
        {
            var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var requestedDate = new RequestedDate
            {
                Date = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero),
                Enabled = true,
                StartDate = start,
                EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
                Offset = TimeSpan.FromDays(1),
                Periodicity = Periodicity.OneTime,
            };

            var preResult = new CalcRecurrent();
            var result = preResult.CalcDate(requestedDate);

            var expectedDate = requestedDate.Date.Add(requestedDate.Offset.Value);
            Assert.Equal(expectedDate, result.NewDate);
            var expectedDesc =
                $"Occurs every {requestedDate.Offset.Value.Days} days. Schedule will be used on {requestedDate.Date:dd/MM/yyyy}" +
                $" at {requestedDate.Date:HH:mm} starting on {start:dd/MM/yyyy}";
            Assert.Equal(expectedDesc, result.Description);
        }

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
            Assert.Equal("Positive Offset required.", result.Message);
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
            Assert.Equal("Positive Offset required.", result.Message);
        }

        [Theory]
        [InlineData(2024,12,31)]
        [InlineData(2026,1,1)]
        public void OutsideRange_Recurrent_Invalid(int y, int m, int d)
        {
            var requestedDate = new RequestedDate
            {
                Date = new DateTimeOffset(y, m, d, 0, 0, 0, TimeSpan.Zero),
                Enabled = true,
                StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
                Offset = TimeSpan.FromDays(1),
                Periodicity = Periodicity.Recurrent
            };

            var preResult = new CalcRecurrent();
            var result = Assert.Throws<Exception>(() => preResult.CalcDate(requestedDate));
            Assert.Equal("The date should be between start and end date.", result.Message);
        }
    }
}
 