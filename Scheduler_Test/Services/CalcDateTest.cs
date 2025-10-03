using Scheduler_Lib.Classes;
using Scheduler_Lib.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler_Lib.Services
{
    public class CalcDateTest
    {
        [Fact]
        public void NullRequest()
        {
            RequestedDate requestedDate = null;
            var result = Assert.Throws<Exception>(() => Service.CalcDate(requestedDate));
            Assert.Equal("Error: The request shouldn't be null.", result.Message);
        }

        [Fact]
        public void DisabledRequest()
        {
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
            Assert.Equal("Positive Offset required.", result.Message);
        }
    }
}
