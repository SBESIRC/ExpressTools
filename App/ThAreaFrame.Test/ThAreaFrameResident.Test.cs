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
        public void BuildingWithSingleStandardStoreys()
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
        }
    }
}