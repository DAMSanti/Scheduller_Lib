using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scheduler_Lib.Classes;

namespace Scheduler_Lib.Services
{
    public class CalcOneTimeTest
    {
        [Fact]
        public void ChangeDate_OneTime()
        {
            var start = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var change = new DateTimeOffset(2025, 10, 5, 0, 0, 0, TimeSpan.Zero);
            var requestedDate = new RequestedDate
            {
                Date = new DateTimeOffset(2025, 10, 3, 0,0,0, TimeSpan.Zero),
                Enabled = true,
                StartDate = start,
                EndDate = new DateTimeOffset(2025, 12, 31, 0,0,0, TimeSpan.Zero),
                ChangeDate = change,
                Periodicity = Periodicity.OneTime,
            };

            var preResult = new CalcOneTime();
            var result = preResult.CalcDate(requestedDate);

            Assert.Equal(change, result.NewDate);
            var expectedResult = $"Occurs Once: Schedule will be used on {change:dd/MM/yyyy HH:mm} starting on {start:dd/MM/yyyy HH:mm} ";
            Assert.Equal(expectedResult, result.Description);
        }
    }
}
