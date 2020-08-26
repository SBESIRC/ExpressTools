using NUnit.Framework;
using TianHua.FanSelection;

namespace TianHua.EquipmentSelection.Test
{
    [TestFixture]
    public class FanDataModelTest
    {
        [Test]
        public void FanNumberTest()
        {
            var model = new FanDataModel()
            {
                PID = "0",
                InstallSpace = "4",
                InstallFloor = "F2",
                VentNum = "2",
                Scenario = "平时送风"
            };
            Assert.AreEqual("SF-4-F2-2", model.FanNum);
        }
    }
}
