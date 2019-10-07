using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ThAreaFrame.Test
{
    [TestFixture]
    public class ResidentialRoomTest
    {
        [Test]
        public void RoomInOneFloor()
        {
            ResidentialRoom room = new ResidentialRoom()
            {
                dwelling = ResidentialAreaUnit.Dwelling(@"住宅构件_套内_1_1_A_A1_c1_PUB1_V2.1")
            };
            var expected = new List<ResidentialStorey>() { new ResidentialStorey { number = 1, standard = false  } };
            Assert.IsTrue(room.Storeys().SequenceEqual(expected));
        }

        [Test]
        public void RoomInSerialFloors()
        {
            ResidentialRoom room = new ResidentialRoom()
            {
                dwelling = ResidentialAreaUnit.Dwelling(@"住宅构件_套内_1_1_A_A1_c1^10_PUB1_V2.1")
            };
            var expected = new List<ResidentialStorey>(){
                new ResidentialStorey { number = 1,     standard    = true  },
                new ResidentialStorey { number = 2,     standard    = true  },
                new ResidentialStorey { number = 3,     standard    = true  },
                new ResidentialStorey { number = 4,     standard    = true  },
                new ResidentialStorey { number = 5,     standard    = true  },
                new ResidentialStorey { number = 6,     standard    = true  },
                new ResidentialStorey { number = 7,     standard    = true  },
                new ResidentialStorey { number = 8,     standard    = true  },
                new ResidentialStorey { number = 9,     standard    = true  },
                new ResidentialStorey { number = 10,    standard    = true  }
            };
            Assert.IsTrue(room.Storeys().SequenceEqual(expected));
        }

        [Test]
        public void RoomInEvenFloors()
        {
            ResidentialRoom room = new ResidentialRoom()
            {
                dwelling = ResidentialAreaUnit.Dwelling(@"住宅构件_套内_1_1_A_A1_o1^10_PUB1_V2.1")
            };
            var expected = new List<ResidentialStorey>() {
                new ResidentialStorey { number = 2,     standard    = true  },
                new ResidentialStorey { number = 4,     standard    = true  },
                new ResidentialStorey { number = 6,     standard    = true  },
                new ResidentialStorey { number = 8,     standard    = true  },
                new ResidentialStorey { number = 10,    standard    = true  }
            };
            Assert.IsTrue(room.Storeys().SequenceEqual(expected));
        }

        [Test]
        public void RoomInOddFloors()
        {
            ResidentialRoom room = new ResidentialRoom()
            {
                dwelling = ResidentialAreaUnit.Dwelling(@"住宅构件_套内_1_1_A_A1_j1^10_PUB1_V2.1")
            };
            var expected = new List<ResidentialStorey>() {
                new ResidentialStorey { number = 1,     standard    = true  },
                new ResidentialStorey { number = 3,     standard    = true  },
                new ResidentialStorey { number = 5,     standard    = true  },
                new ResidentialStorey { number = 7,     standard    = true  },
                new ResidentialStorey { number = 9,     standard    = true  }
            };
            Assert.IsTrue(room.Storeys().SequenceEqual(expected));
        }

        [Test]
        public void BuildingWithSingleStandardStoreysV21()
        {
            string[] layers = {
                "住宅构件_套内_1.0_0.0_A_HT6_c-1__V2.1",
                "住宅构件_套内_1.0_1.0_A_HT1_c1__V2.1",
                "住宅构件_套内_1.0_1.0_A_HT3_c2__V2.1",
                "住宅构件_套内_1.0_1.0_A_HT4_c3^5__V2.1",
                "住宅构件_套内_1.0_1.0_A_HT5_c6__V2.1",
                "住宅构件_阳台_1.0_1.0_HT3__V2.1",
                "住宅构件_阳台_1.0_1.0_HT4__V2.1",
                "住宅构件_阳台_1.0_1.0_HT5__V2.1"
            };
            var building = ResidentialBuilding.CreateWithLayers(layers);
            Assert.AreEqual(building.StandardStoreys().Count, 1);
            Assert.AreEqual(building.OrdinaryStoreys().Count, 3);

            // 户型
            Assert.AreEqual(building.rooms.Where(o => o.dwelling.dwellingID == "HT1").Count(), 1);
            Assert.AreEqual(building.rooms.Where(o => o.dwelling.dwellingID == "HT3").Count(), 1);
            Assert.AreEqual(building.rooms.Where(o => o.dwelling.dwellingID == "HT4").Count(), 1);
            Assert.AreEqual(building.rooms.Where(o => o.dwelling.dwellingID == "HT5").Count(), 1);
            Assert.AreEqual(building.rooms.Where(o => o.dwelling.dwellingID == "HT6").Count(), 1);

            // 阳台
            var room = building.rooms.Where(o => o.dwelling.dwellingID == "HT3").First();
            Assert.AreEqual(room.balconies.Count(), 1);
        }

        [Test]
        public void BuildingWithSingleStandardStoreysV22()
        {
            string[] layers =
            {
                "住宅构件_飘窗_0.5_0.0_HT1_c1_V2.2",
                "住宅构件_飘窗_0.5_0.0_HT1_c10_V2.2",
                "住宅构件_飘窗_0.5_0.0_HT1_c2_V2.2",
                "住宅构件_飘窗_0.5_0.0_HT1_c3^9_V2.2",
                "住宅构件_其他构件_0.5_0.0_HT1_c1_V2.2",
                "住宅构件_其他构件_0.5_0.5_HT1_c1_V2.2",
                "住宅构件_套内_1.0_0.0_A_HT1_c-1__V2.2",
                "住宅构件_套内_1.0_1.0_A_HT1_c1__V2.2",
                "住宅构件_套内_1.0_1.0_A_HT1_c10__V2.2",
                "住宅构件_套内_1.0_1.0_A_HT1_c2__V2.2",
                "住宅构件_套内_1.0_1.0_A_HT1_c3^9__V2.2",
                "住宅构件_阳台_0.5_0.5_HT1_c1__V2.2",
                "住宅构件_阳台_0.5_0.5_HT1_c10__V2.2",
                "住宅构件_阳台_0.5_0.5_HT1_c2__V2.2",
                "住宅构件_阳台_0.5_0.5_HT1_c3^9__V2.2"
            };
            var building = ResidentialBuilding.CreateWithLayers(layers);
            Assert.AreEqual(building.StandardStoreys().Count, 1);
            Assert.AreEqual(building.OrdinaryStoreys().Count, 3);

            // 户型
            Assert.AreEqual(building.rooms.Where(o => o.dwelling.dwellingID == "HT1").Count(), 5);

            // 构件
            var room = building.rooms.Where(
                o => o.dwelling.storeys == "c1" &&
                o.dwelling.dwellingID == "HT1").First();
            Assert.AreEqual(room.balconies.Count(), 1);
            Assert.AreEqual(room.baywindows.Count(), 1);
            Assert.AreEqual(room.miscellaneous.Count(), 2);
        }
    }
}