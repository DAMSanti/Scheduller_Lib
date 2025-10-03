using Scheduler_Lib.Enum;
using Scheduler_Lib.Interfaz;

namespace Scheduler_Lib.Factory
{
    public class ScheduleCalculatorTest {
        [Fact]
        public void GetScheduleCalculator_Once() {
            var calc = ScheduleCalculator.GetScheduleCalculator(Periodicity.OneTime);
            Assert.IsAssignableFrom<ISchedule>(calc);
            Assert.Equal("Scheduler_Lib.Services.CalcOneTime", calc.GetType().FullName);
        }

        [Fact]
        public void GetScheduleCalculator_Recurrent() {
            var calc = ScheduleCalculator.GetScheduleCalculator(Periodicity.Recurrent);
            Assert.IsAssignableFrom<ISchedule>(calc);
            Assert.Equal("Scheduler_Lib.Services.CalcRecurrent", calc.GetType().FullName);
        }

        [Fact]
        public void GetScheduleCalculator_Unsupported() {
            var calc = (Periodicity)5;
            var result = Assert.Throws<Exception>(() => ScheduleCalculator.GetScheduleCalculator(calc));
            Assert.Equal("Unsupported periodicity", result.Message);
        }
    }
}
