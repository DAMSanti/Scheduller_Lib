using Xunit;
using Scheduler_Lib.Classes;

namespace Scheduler_Lib.UnitTests;

public class UnitTest1
{
    [Fact]
    public void CalcDate_NullArgument() {
        var service = new Service();
        RequestedDate testRequested = null;
        Assert.Throws<Exception>(() => service.CalcDate(testRequested))
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