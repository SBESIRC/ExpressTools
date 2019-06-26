using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ThAreaFrame.Test
{
    [TestFixture]
    class ThAreaFrameCommonTest
    {
        [Test]
        public void CreateObject()
        {
            ThAreaFrameCommon common = ThAreaFrameCommon.Common(@"公摊_1_走廊_c1_A_HT1_v2.1");
            Assert.AreEqual(common.type, @"公摊");
            Assert.AreEqual(common.areaFactor, @"1");
            Assert.AreEqual(common.name, @"走廊");
        }
    }
}