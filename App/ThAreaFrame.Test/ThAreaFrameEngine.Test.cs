using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSubstitute;
using NUnit.Framework;

namespace ThAreaFrame.Test
{
    [TestFixture]
    public class ThAreaFrameEngineTest
    {
        [Test]
        public void CreateEngine()
        {
            var ds = Substitute.For<IThAreaFrameDataSource>();
            ds.Layers().Returns(new List<string>()
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
            });
            var engine = ThAreaFrameEngine.Engine(ds);
        }
    }
}