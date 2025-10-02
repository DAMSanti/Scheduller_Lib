using Scheduler_Lib.Classes;

namespace Scheduler_Lib.Services
{
    public class ServiceTest
    {
        [Fact]
        public void CalcDate_NullArgument()
        {
            RequestedDate testRequested = null;
            Assert.Throws<Exception>(() => Service.CalcDate(testRequested));
        }

        /*[Fact]
        public void CalcDate_Enabled() {
            RequestedDate testRequested = new RequestedDate();
            testRequested.Enabled = false;
            SolvedDate solvedDate = new SolvedDate
            {
                NewDate = requestedDate.Date,
                Description = "Desactivado: No se ha realizado ninguna modificación"
            };
            Assert.Equal(Service.CalcDate(testRequested), solvedDate);
        }*/

    }
}
