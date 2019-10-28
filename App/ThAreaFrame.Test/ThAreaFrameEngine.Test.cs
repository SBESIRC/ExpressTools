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
        public void ResidentialArea()
        {
            var ds = Substitute.For<IThAreaFrameDataSource>();
            ds.Layers().Returns(new List<string>()
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
                "住宅构件_阳台_0.5_0.5_HT1_c10__V2.2",
            });

            // 一层
            ds.SumOfArea("住宅构件_套内_1.0_1.0_A_HT1_c1__V2.2").Returns(200);
            ds.SumOfArea("住宅构件_阳台_0.5_0.5_HT1_c1__V2.2").Returns(20);
            ds.SumOfArea("住宅构件_飘窗_0.5_0.0_HT1_c1_V2.2").Returns(20);
            ds.SumOfArea("住宅构件_其他构件_0.5_0.0_HT1_c1_V2.2").Returns(20);
            ds.SumOfArea("住宅构件_其他构件_0.5_0.5_HT1_c1_V2.2").Returns(40);

            // 二层
            ds.SumOfArea("住宅构件_套内_1.0_1.0_A_HT1_c2__V2.2").Returns(200);
            ds.SumOfArea("住宅构件_阳台_0.5_0.5_HT1_c2__V2.2").Returns(20);
            ds.SumOfArea("住宅构件_飘窗_0.5_0.0_HT1_c2_V2.2").Returns(20);

            // 标准楼层
            ds.SumOfArea("住宅构件_套内_1.0_1.0_A_HT1_c3^9__V2.2").Returns(200);
            ds.SumOfArea("住宅构件_阳台_0.5_0.5_HT1_c3^9__V2.2").Returns(20);
            ds.SumOfArea("住宅构件_飘窗_0.5_0.0_HT1_c3^9_V2.2").Returns(20);

            // 十楼
            ds.SumOfArea("住宅构件_套内_1.0_1.0_A_HT1_c10__V2.2").Returns(200);
            ds.SumOfArea("住宅构件_阳台_0.5_0.5_HT1_c10__V2.2").Returns(20);
            ds.SumOfArea("住宅构件_飘窗_0.5_0.0_HT1_c10_V2.2").Returns(20);

            // 地下一楼
            ds.SumOfArea("住宅构件_套内_1.0_0.0_A_HT1_c-1__V2.2").Returns(200);

            // 单体楼顶间
            ds.SumOfArea("单体楼顶间_住宅_1.0_1.0_V2.2").Returns(20);

            // 屋顶绿地
            ds.SumOfArea("屋顶构件_屋顶绿地_0.5_V2.2").Returns(20);

            // 单体基底
            ds.SumOfArea("单体基底_1_1#_住宅_10_1_____是__V2.2").Returns(200);

            // 引擎
            var engine = ThAreaFrameEngine.Engine(ds);

            // 一楼面积
            Assert.AreEqual(engine.AreaOfFloor(1, true), 230);
            Assert.AreEqual(engine.AreaOfFloor(1, false), 250);

            // 二楼面积
            Assert.AreEqual(engine.AreaOfFloor(2, true), 210);
            Assert.AreEqual(engine.AreaOfFloor(2, false), 220);

            // 标准面积
            Assert.AreEqual(engine.AreaOfFloor(3, true), 210);
            Assert.AreEqual(engine.AreaOfFloor(3, false), 220);

            // 十楼面积
            Assert.AreEqual(engine.AreaOfFloor(10, true), 210);
            Assert.AreEqual(engine.AreaOfFloor(10, false), 220);

            // 地下一层面积
            Assert.AreEqual(engine.AreaOfFloor(-1, true), 0);
            Assert.AreEqual(engine.AreaOfFloor(-1, false), 200);


            // 建筑面积
            Assert.AreEqual(engine.AreaOfAboveGround() + 
                engine.AreaOfUnderGround() +
                engine.AreaOfRoof() -
                engine.AreaOfStilt(),
                2450
                );

            // 计容面积
            Assert.AreEqual(engine.AreaOfCapacityBuilding(true) + engine.AreaOfRoof(true), 2140);

            // 绿地面积
            Assert.AreEqual(engine.AreaOfRoofGreenSpace(), 10);

            // 楼梯间
            Assert.AreEqual(engine.AreaOfRoof(true), 20);
            Assert.AreEqual(engine.AreaOfRoof(false), 20);
        }
    }
}
