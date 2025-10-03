using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scheduler_Lib.Classes;

namespace Scheduler_Lib.Services
{
    public class CalcDateTest
    {
        public void NullRequest()
        {
            RequestedDate requestedDate = null;
            var result = Assert.Throws<Exception>(() => Service.CalcDate(requestedDate));
            Assert.Equal("Error: The request shouldn't be null.", result.Message);
        }
    }
}
