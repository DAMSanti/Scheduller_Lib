using Scheduler_Lib.Classes;
using Scheduler_Lib.Enum;

namespace Scheduler_Lib.Services {
    public class CalcDateTest {
        [Fact]
        public void CalcDate_Valid()
        {
            var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var change = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
            var requestedDate = new RequestedDate
            {
                Date = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero),
                Enabled = true,
                StartDate = start,
                EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
                ChangeDate = change,
                Periodicity = Periodicity.OneTime,
            };

            var preResult = new CalcOneTime();
            var result = preResult.CalcDate(requestedDate);

            Assert.Equal(change, result.NewDate);
            var expectedResult = $"Occurs once: Schedule will be used on {change:dd/MM/yyyy} at {change:HH:mm} starting on {requestedDate.StartDate:dd/MM/yyyy}";
            Assert.Equal(expectedResult, result.Description);
        }

        [Fact]
        public void NullRequest() {
            RequestedDate? requestedDate = null;
            var result = Assert.Throws<Exception>(() => Service.CalcDate(requestedDate));
            Assert.Equal("Error: The request shouldn't be null.", result.Message);
        }

        [Fact]
        public void DisabledRequest() {
            var date = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
            RequestedDate requestedDate = new RequestedDate
            {
                Date = date,
                Enabled = false
            };

            var result = Service.CalcDate(requestedDate);
            Assert.Equal(date, result.NewDate);
            Assert.Equal("Disabled: No changes performed.", result.Description);
        }

        [Fact]
        public void Recurrent_NoOffset() {
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
            Assert.Equal("Positive Offset required.", result.Message);
        }
    }
}
