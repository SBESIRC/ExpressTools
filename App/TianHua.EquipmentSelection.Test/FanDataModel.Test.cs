using Xunit;
using TianHua.FanSelection;

namespace TianHua.EquipmentSelection.Test
{
    public class FanDataModelTest
    {
        [Fact]
        public void FanNumberTest()
        {
            var model = new FanDataModel()
            {
                InstallSpace = "4",
                InstallFloor = "F2",
                VentNum = "2",
                Scenario = "平时送风"
            };
            Assert.Equal("EF-4-F2-2", model.GetFanNum());
        }
    }
}
