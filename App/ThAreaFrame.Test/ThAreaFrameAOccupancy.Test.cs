using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ThAreaFrame.Test
{
    [TestFixture]
    class ThAreaFrameAOccupancyTest
    {
        [Test]
        public void CreateObjects()
        {
            ThAreaFrameAOccupancy main = ThAreaFrameAOccupancy.Main("附属公建_主体_教育_幼儿园_1_1_0_c1^3_PUB1_V2.1");
            Assert.AreEqual(main.areaType, "主体");
            Assert.AreEqual(main.publicAreaID, "PUB1");

            ThAreaFrameAOccupancy balcony = ThAreaFrameAOccupancy.Balcony("附属公建_阳台_教育_幼儿园_1_1_c1^3_PUB1_V2.1");
            Assert.AreEqual(balcony.areaType, "阳台");
            Assert.AreEqual(balcony.publicAreaID, "PUB1");

            ThAreaFrameAOccupancy stilt = ThAreaFrameAOccupancy.Stilt("附属公建_架空_教育_幼儿园_1_1_1_c1_V2.1");
            Assert.AreEqual(stilt.areaType, "架空");
            Assert.AreEqual(stilt.storeys, "c1");

            ThAreaFrameAOccupancy barWindow = ThAreaFrameAOccupancy.Baywindow("附属公建_飘窗_教育_小学_0.5_0.5_c1^3_V2.1");
            Assert.AreEqual(barWindow.areaType, "飘窗");
            Assert.AreEqual(barWindow.storeys, "c1^3");

            ThAreaFrameAOccupancy rainshed = ThAreaFrameAOccupancy.Rainshed("附属公建_雨棚_教育_小学_0.5_0.5_c1^3_V2.1");
            Assert.AreEqual(rainshed.areaType, "雨棚");
            Assert.AreEqual(rainshed.storeys, "c1^3");

            ThAreaFrameAOccupancy miscellaneous = ThAreaFrameAOccupancy.Miscellaneous("附属公建_附属其他构件_通风井_室内停车库_小型汽车_1_0_c1_V2.1");
            Assert.AreEqual(miscellaneous.areaType, "附属其他构件");
            Assert.AreEqual(miscellaneous.storeys, "c1");
        }

        [Test]
        public void CreateBuilding()
        {
            string[] aOccupancies =
            {
                "附属公建_附属其他构件_商业__0.5_0.5_c1__V2.2",
                "附属公建_架空_商业__0.5_0.5__c1__V2.2",
                "附属公建_架空_商业__0.5_0.5__c2^5__V2.2",
                "附属公建_架空_商业__0.5_0.5__c6__V2.2",
                "附属公建_飘窗_商业__0.5_0.0_c1__V2.2",
                "附属公建_飘窗_商业__0.5_0.0_c2^5__V2.2",
                "附属公建_飘窗_商业__0.5_0.0_c6__V2.2",
                "附属公建_阳台_商业__0.5_0.5_c1__V2.2",
                "附属公建_阳台_商业__0.5_0.5_c2^5__V2.2",
                "附属公建_阳台_商业__0.5_0.5_c6__V2.2",
                "附属公建_雨棚_商业__0.5_0.0_c1__V2.2",
                "附属公建_主体_商业__1.0_0.0__c-1__V2.2",
                "附属公建_主体_商业__1.0_1.0__c1__V2.2",
                "附属公建_主体_商业__1.0_1.0__c2^5__V2.2",
                "附属公建_主体_商业__1.0_1.0__c6__V2.2"
            };

            AOccupancyBuilding building = AOccupancyBuilding.CreateWithLayers(aOccupancies);
            Assert.AreEqual(building.StandardStoreys().Count, 4);
            Assert.AreEqual(building.OrdinaryStoreys().Count, 2);
        }
    }
}
