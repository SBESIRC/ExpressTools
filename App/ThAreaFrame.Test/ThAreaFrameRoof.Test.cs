using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ThAreaFrame.Test
{
    [TestFixture]
    class ThAreaFrameRoofTest
    {
        [Test]
        public void CreateObject()
        {
            // 旧图层名，默认“住宅”
            ThAreaFrameRoof legacy_roof = ThAreaFrameRoof.Roof("单体楼顶间_1.0_1.0_V2.1");
            Assert.AreEqual(legacy_roof.category, "住宅");

            // 新图层名，未包含“所属类型”，默认“住宅”
            ThAreaFrameRoof default_roof = ThAreaFrameRoof.Roof("单体楼顶间_1.0_1.0_V2.2");
            Assert.AreEqual(default_roof.category, "住宅");

            // 新图层名，“所属类型”值为“住宅”
            ThAreaFrameRoof resident_roof = ThAreaFrameRoof.Roof("单体楼顶间_住宅_1.0_1.0_V2.2");
            Assert.AreEqual(resident_roof.category, "住宅");

            // 新图层名，“所属类型”值为“公建”
            ThAreaFrameRoof aocupancy_roof = ThAreaFrameRoof.Roof("单体楼顶间_公建_1.0_1.0_V2.2");
            Assert.AreEqual(aocupancy_roof.category, "公建");
        }
    }
}
