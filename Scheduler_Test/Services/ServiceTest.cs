using System.ComponentModel;
using Scheduler_Lib.Classes;
using Scheduler_Lib.Enum;

namespace Scheduler_Lib.Services
{
    public class ServiceTest
    {
      /*  [Fact]
        public void CalcDate_NullArgument()
        {
            RequestedDate testRequested = null;
            Assert.Throws<Exception>(() => Service.CalcDate(testRequested));
        }

        [Fact]
        public void CalcDate_Disabled()
        {
            var requestedDate = new RequestedDate();
            requestedDate.Enabled = false;

            var expectedResult = new SolvedDate();
            expectedResult.NewDate = requestedDate.Date;
            expectedResult.Description = "Disabled: No changes performed.";

            var result = Service.CalcDate(requestedDate);
            //Assert.Equal(result, expectedResult);
            Assert.Equal(expectedResult.NewDate, result.NewDate);
            Assert.Equal(expectedResult.Description, result.Description);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(Periodicity.OneTime)]
        [InlineData(Periodicity.Recurrent)]
        public void CalcDate_SwitchTest(Periodicity? periodicity) {
            if (periodicity == null) {
                var requestedDateNull = new RequestedDate {
                    Enabled = true,
                    StartDate = DateTimeOffset.Now,
                    EndDate = DateTimeOffset.Now.AddDays(5),
                    ChangeDate = DateTimeOffset.Now.AddDays(3),
                };
                Assert.Throws<Exception>(() => Service.CalcDate(requestedDateNull));
            } else {
               var requestedDate1 = new RequestedDate {
                    Enabled = true,
                    Date = new DateTimeOffset(2025, 10, 2, 0, 0, 0, TimeSpan.FromHours(2)),
                    StartDate = DateTimeOffset.Now,
                    EndDate = DateTimeOffset.Now.AddDays(5),
                    ChangeDate = DateTimeOffset.Now.AddDays(3),
                    Periodicity = periodicity.Value
               };

                var expectedDate1 = new SolvedDate {
                    NewDate = requestedDate1.ChangeDate.Value,
                    Description = $"Occurs once: Schedule will be used on 05/10/2025 " +
                                  $"at {requestedDate1.ChangeDate.Value.TimeOfDay} starting on {requestedDate1.StartDate} "
                };

                Assert.Equal(expectedDate1.NewDate, Service.CalcDate(requestedDate1).NewDate);
                Assert.Equal(expectedDate1.Description, Service.CalcDate(requestedDate1).Description);



        


                /*var requestedDate2 = new RequestedDate {
                    Enabled = true,
                    StartDate = DateTimeOffset.Now,
                    EndDate = DateTimeOffset.Now.AddDays(5),
                    ChangeDate = DateTimeOffset.Now.AddDays(3),
                    Periodicity = periodicity.Value,
                    Offset = TimeSpan.FromDays(1)
                };

                var newDate = requestedDate2.Date.Add(requestedDate2.Offset.Value);

                var expectedDate2 = new SolvedDate {
                    NewDate = newDate,
                    Description = $"Occurs Once: Schedule will be used on {newDate} starting on {requestedDate2.StartDate} "
                };

                var result1 = Service.CalcDate(requestedDate1);
                var result2 = Service.CalcDate(requestedDate2);
                */
                //Assert.Equal(expectedDate1, result1);
                //Assert.Equal(expectedDate2, result2);

         //   }
       // }

    }
}
