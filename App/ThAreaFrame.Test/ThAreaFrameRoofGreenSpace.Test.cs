using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ThAreaFrame.Test
{
    [TestFixture]
    class ThAreaFrameRoofGreenSpaceTest
    {
        [Test]
        public void CreateObject()
        {
            ThAreaFrameRoofGreenSpace roofGreenSpace = ThAreaFrameRoofGreenSpace.RoofOfGreenSpace(@"屋顶构件_屋顶绿地_0.5_v2.1");
            Assert.AreEqual(roofGreenSpace.type, @"屋顶构件");
            Assert.AreEqual(roofGreenSpace.areaType, @"屋顶绿地");
            Assert.AreEqual(roofGreenSpace.coefficient, @"0.5");
        }
    }
}
