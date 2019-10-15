using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ThAreaFrame.Test
{
    [TestFixture]
    class ThAreaFrameStoreyManagerTest
    {
        [Test]
        public void ResidentialBuildingStoreys()
        {
            string[] names =
            {
                "单体基底_1_1#_住宅_10_1_____是__V2.2",
                "单体楼顶间_住宅_1.0_1.0_V2.2",
                "屋顶构件_屋顶绿地_0.5_V2.2",
                "住宅构件_飘窗_0.5_0.0_HT1_c1_V2.2",
                "住宅构件_飘窗_0.5_0.0_HT1_c2_V2.2",
                "住宅构件_飘窗_0.5_0.0_HT1_c3^9_V2.2",
                "住宅构件_飘窗_0.5_0.0_HT1_c10_V2.2",
                "住宅构件_其他构件_0.5_0.0_HT1_c1_V2.2",
                "住宅构件_其他构件_0.5_0.5_HT1_c1_V2.2",
                "住宅构件_套内_1.0_0.0_A_HT1_c-1__V2.2",
                "住宅构件_套内_1.0_1.0_A_HT1_c1__V2.2",
                "住宅构件_套内_1.0_1.0_A_HT1_c2__V2.2",
                "住宅构件_套内_1.0_1.0_A_HT1_c3^9__V2.2",
                "住宅构件_套内_1.0_1.0_A_HT1_c10__V2.2",
                "住宅构件_阳台_0.5_0.5_HT1_c1__V2.2",
                "住宅构件_阳台_0.5_0.5_HT1_c2__V2.2",
                "住宅构件_阳台_0.5_0.5_HT1_c3^9__V2.2",
                "住宅构件_阳台_0.5_0.5_HT1_c10__V2.2"
            };

            var rBuilding = ResidentialBuilding.CreateWithLayers(names);
            var aBuilding = AOccupancyBuilding.CreateWithLayers(names);
            var storeyManager = new ThAreaFrameStoreyManager(rBuilding, aBuilding);
            Assert.AreEqual(storeyManager.OrdinaryStoreyCollection, new List<int>
            {
                1, 2, 10
            });
            Assert.AreEqual(storeyManager.StandardStoreyCollections, new List<List<int>>
            {
                new List<int> { 3, 4, 5, 6, 7, 8, 9 }
            });
            Assert.AreEqual(storeyManager.UnderGroundStoreyCollection, new List<int> { -1 });
        }

        [Test]
        public void AOccupancyBuildingStoreys()
        {
            string[] names =
            {
                "单体基底_3_3#_公建_6_1_____是__V2.2",
                "单体楼顶间_公建_1.0_1.0_V2.2",
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
                "附属公建_主体_商业__1.0_1.0__c6__V2.2",
                "屋顶构件_屋顶绿地_0.5_V2.2"
            };

            var rBuilding = ResidentialBuilding.CreateWithLayers(names);
            var aBuilding = AOccupancyBuilding.CreateWithLayers(names);
            var storeyManager = new ThAreaFrameStoreyManager(rBuilding, aBuilding);
            Assert.AreEqual(storeyManager.OrdinaryStoreyCollection, new List<int>
            {
                1, 6
            });
            Assert.AreEqual(storeyManager.StandardStoreyCollections, new List<List<int>>
            {
                new List<int> { 2, 3, 4, 5 }
            });
            Assert.AreEqual(storeyManager.UnderGroundStoreyCollection, new List<int> { -1 });
        }

        [Test]
        public void ComplexBuildingStoreys()
        {
            string[] names =
            {
                "单体基底_5_5#_混合_6_1_____是__V2.2",
                "单体楼顶间_公建_1.0_1.0_V2.2",
                "单体楼顶间_住宅_1.0_1.0_V2.2",
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
                "附属公建_主体_商业__1.0_1.0__c6__V2.2",
                "屋顶构件_屋顶绿地_0.5_V2.2",
                "住宅构件_飘窗_0.5_0.0_HT1_c1_V2.2",
                "住宅构件_飘窗_0.5_0.0_HT1_c2^5_V2.2",
                "住宅构件_飘窗_0.5_0.0_HT1_c6_V2.2",
                "住宅构件_其他构件_0.5_0.0_HT1_c1_V2.2",
                "住宅构件_其他构件_0.5_0.5_HT1_c1_V2.2",
                "住宅构件_套内_1.0_0.0_A_HT1_c-1__V2.2",
                "住宅构件_套内_1.0_1.0_A_HT1_c1__V2.2",
                "住宅构件_套内_1.0_1.0_A_HT1_c2^5__V2.2",
                "住宅构件_套内_1.0_1.0_A_HT1_c6__V2.2",
                "住宅构件_阳台_0.5_0.5_HT1_c1__V2.2",
                "住宅构件_阳台_0.5_0.5_HT1_c2^5__V2.2",
                "住宅构件_阳台_0.5_0.5_HT1_c6__V2.2"

            };

            var rBuilding = ResidentialBuilding.CreateWithLayers(names);
            var aBuilding = AOccupancyBuilding.CreateWithLayers(names);
            var storeyManager = new ThAreaFrameStoreyManager(rBuilding, aBuilding);
            Assert.AreEqual(storeyManager.OrdinaryStoreyCollection, new List<int>
            {
                1, 6
            });
            Assert.AreEqual(storeyManager.StandardStoreyCollections, new List<List<int>>
            {
                new List<int> { 2, 3, 4, 5 }
            });
            Assert.AreEqual(storeyManager.UnderGroundStoreyCollection, new List<int> { -1 });
        }
    }
}
