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


        [Test]
        public void AOccupancyArea()
        {
            var ds = Substitute.For<IThAreaFrameDataSource>();
            ds.Layers().Returns(new List<string>()
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
                "屋顶构件_屋顶绿地_0.5_V2.2",
            });

            // 一层
            ds.SumOfArea("附属公建_主体_商业__1.0_1.0__c1__V2.2").Returns(200);
            ds.SumOfArea("附属公建_阳台_商业__0.5_0.5_c1__V2.2").Returns(20);
            ds.SumOfArea("附属公建_飘窗_商业__0.5_0.0_c1__V2.2").Returns(20);
            ds.SumOfArea("附属公建_架空_商业__0.5_0.5__c1__V2.2").Returns(50);
            ds.SumOfArea("附属公建_雨棚_商业__0.5_0.0_c1__V2.2").Returns(20);
            ds.SumOfArea("附属公建_附属其他构件_商业__0.5_0.5_c1__V2.2").Returns(40);

            // 二层~五层（标准层）
            ds.SumOfArea("附属公建_主体_商业__1.0_1.0__c2^5__V2.2").Returns(200);
            ds.SumOfArea("附属公建_阳台_商业__0.5_0.5_c2^5__V2.2").Returns(20);
            ds.SumOfArea("附属公建_飘窗_商业__0.5_0.0_c2^5__V2.2").Returns(20);
            ds.SumOfArea("附属公建_架空_商业__0.5_0.5__c2^5__V2.2").Returns(50);

            // 六层
            ds.SumOfArea("附属公建_主体_商业__1.0_1.0__c6__V2.2").Returns(200);
            ds.SumOfArea("附属公建_阳台_商业__0.5_0.5_c6__V2.2").Returns(20);
            ds.SumOfArea("附属公建_飘窗_商业__0.5_0.0_c6__V2.2").Returns(20);
            ds.SumOfArea("附属公建_架空_商业__0.5_0.5__c6__V2.2").Returns(50);

            // 地下一层
            ds.SumOfArea("附属公建_主体_商业__1.0_0.0__c-1__V2.2").Returns(200);

            // 单体楼顶间
            ds.SumOfArea("单体楼顶间_公建_1.0_1.0_V2.2").Returns(20);

            // 屋顶绿地
            ds.SumOfArea("屋顶构件_屋顶绿地_0.5_V2.2").Returns(20);

            // 单体基底
            ds.SumOfArea("单体基底_3_3#_公建_6_1_____是__V2.2").Returns(200);

            // 引擎
            var engine = ThAreaFrameEngine.Engine(ds);

            // 一楼面积
            Assert.AreEqual(engine.AreaOfFloor(1, true), 255);
            Assert.AreEqual(engine.AreaOfFloor(1, false), 275);

            // 标准面积
            Assert.AreEqual(engine.AreaOfFloor(2, true), 235);
            Assert.AreEqual(engine.AreaOfFloor(2, false), 245);

            // 六楼面积
            Assert.AreEqual(engine.AreaOfFloor(6, true), 235);
            Assert.AreEqual(engine.AreaOfFloor(6, false), 245);

            // 地下一层面积
            Assert.AreEqual(engine.AreaOfFloor(-1, true), 0);
            Assert.AreEqual(engine.AreaOfFloor(-1, false), 200);


            // 建筑面积
            Assert.AreEqual(engine.AreaOfAboveGround() +
                engine.AreaOfUnderGround() +
                engine.AreaOfRoof() -
                engine.AreaOfStilt(),
                1570
                );

            // 计容面积
            Assert.AreEqual(engine.AreaOfCapacityBuilding(true) + engine.AreaOfRoof(true), 1450);

            // 绿地面积
            Assert.AreEqual(engine.AreaOfRoofGreenSpace(), 10);

            // 楼梯间
            Assert.AreEqual(engine.AreaOfRoof(true), 20);
            Assert.AreEqual(engine.AreaOfRoof(false), 20);

            // 架空面积
            Assert.AreEqual(engine.AreaOfStilt(), 150);
        }

        [Test]
        public void CompositeArea()
        {
            var ds = Substitute.For<IThAreaFrameDataSource>();
            ds.Layers().Returns(new List<string>()
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
                "住宅构件_阳台_0.5_0.5_HT1_c6__V2.2",
            });

            // 一层
            ds.SumOfArea("住宅构件_套内_1.0_1.0_A_HT1_c1__V2.2").Returns(100);
            ds.SumOfArea("附属公建_主体_商业__1.0_1.0__c1__V2.2").Returns(100);
            ds.SumOfArea("住宅构件_阳台_0.5_0.5_HT1_c1__V2.2").Returns(10);
            ds.SumOfArea("住宅构件_飘窗_0.5_0.0_HT1_c1_V2.2").Returns(10);
            ds.SumOfArea("住宅构件_其他构件_0.5_0.0_HT1_c1_V2.2").Returns(10);
            ds.SumOfArea("住宅构件_其他构件_0.5_0.5_HT1_c1_V2.2").Returns(20);
            ds.SumOfArea("附属公建_阳台_商业__0.5_0.5_c1__V2.2").Returns(10);
            ds.SumOfArea("附属公建_飘窗_商业__0.5_0.0_c1__V2.2").Returns(10);
            ds.SumOfArea("附属公建_雨棚_商业__0.5_0.0_c1__V2.2").Returns(10);
            ds.SumOfArea("附属公建_附属其他构件_商业__0.5_0.5_c1__V2.2").Returns(20);
            ds.SumOfArea("附属公建_架空_商业__0.5_0.5__c1__V2.2").Returns(25);

            // 2~5层（标准楼层）
            ds.SumOfArea("住宅构件_套内_1.0_1.0_A_HT1_c2^5__V2.2").Returns(100);
            ds.SumOfArea("住宅构件_阳台_0.5_0.5_HT1_c2^5__V2.2").Returns(10);
            ds.SumOfArea("住宅构件_飘窗_0.5_0.0_HT1_c2^5_V2.2").Returns(10);
            ds.SumOfArea("附属公建_主体_商业__1.0_1.0__c2^5__V2.2").Returns(100);
            ds.SumOfArea("附属公建_阳台_商业__0.5_0.5_c2^5__V2.2").Returns(10);
            ds.SumOfArea("附属公建_飘窗_商业__0.5_0.0_c2^5__V2.2").Returns(10);
            ds.SumOfArea("附属公建_架空_商业__0.5_0.5__c2^5__V2.2").Returns(25);

            // 六层
            ds.SumOfArea("住宅构件_套内_1.0_1.0_A_HT1_c6__V2.2").Returns(100);
            ds.SumOfArea("住宅构件_阳台_0.5_0.5_HT1_c6__V2.2").Returns(10);
            ds.SumOfArea("住宅构件_飘窗_0.5_0.0_HT1_c6_V2.2").Returns(10);
            ds.SumOfArea("附属公建_飘窗_商业__0.5_0.0_c6__V2.2").Returns(10);
            ds.SumOfArea("附属公建_主体_商业__1.0_1.0__c6__V2.2").Returns(100);
            ds.SumOfArea("附属公建_阳台_商业__0.5_0.5_c6__V2.2").Returns(10);
            ds.SumOfArea("附属公建_飘窗_商业__0.5_0.0_c6__V2.2").Returns(10);
            ds.SumOfArea("附属公建_架空_商业__0.5_0.5__c6__V2.2").Returns(25);

            // 地下一层
            ds.SumOfArea("住宅构件_套内_1.0_0.0_A_HT1_c-1__V2.2").Returns(100);
            ds.SumOfArea("附属公建_主体_商业__1.0_0.0__c-1__V2.2").Returns(100);

            // 单体楼顶间
            ds.SumOfArea("单体楼顶间_公建_1.0_1.0_V2.2").Returns(10);
            ds.SumOfArea("单体楼顶间_住宅_1.0_1.0_V2.2").Returns(10);

            // 屋顶绿地
            ds.SumOfArea("屋顶构件_屋顶绿地_0.5_V2.2").Returns(20);

            // 单体基底
            ds.SumOfArea("单体基底_3_3#_公建_6_1_____是__V2.2").Returns(200);

            // 引擎
            var engine = ThAreaFrameEngine.Engine(ds);

            // 一楼面积
            Assert.AreEqual(engine.AreaOfFloor(1, true), 242.5);
            Assert.AreEqual(engine.AreaOfFloor(1, false), 262.5);

            // 标准面积
            Assert.AreEqual(engine.AreaOfFloor(2, true), 222.5);
            Assert.AreEqual(engine.AreaOfFloor(2, false), 232.5);

            // 六楼面积
            Assert.AreEqual(engine.AreaOfFloor(6, true), 222.5);
            Assert.AreEqual(engine.AreaOfFloor(6, false), 232.5);

            // 地下一层面积
            Assert.AreEqual(engine.AreaOfFloor(-1, true), 0);
            Assert.AreEqual(engine.AreaOfFloor(-1, false), 200);


            // 建筑面积
            Assert.AreEqual(engine.AreaOfAboveGround() +
                engine.AreaOfUnderGround() +
                engine.AreaOfRoof() -
                engine.AreaOfStilt(),
                1570
                );

            // 计容面积
            Assert.AreEqual(engine.AreaOfCapacityBuilding(true) + engine.AreaOfRoof(true), 1375);

            // 绿地面积
            Assert.AreEqual(engine.AreaOfRoofGreenSpace(), 10);

            // 楼梯间
            Assert.AreEqual(engine.AreaOfRoof(true), 20);
            Assert.AreEqual(engine.AreaOfRoof(false), 20);

            // 架空面积
            Assert.AreEqual(engine.AreaOfStilt(), 75);
        }

        [Test]
        public void MasterArea()
        {
            var ds = Substitute.For<IThAreaFrameDataSource>();
            ds.Layers().Returns(new List<string>()
            {
                "车场车位_室外车场_露天车场_小型汽车_2__V2.2",
                "用地_公共绿地__V2.2",
                "用地_规划净用地_500__V2.2"
            });

            // "规划净用地面积"
            ds.SumOfArea("用地_公共绿地__V2.2").Returns(1000);
            ds.SumOfArea("用地_规划净用地_500__V2.2").Returns(5000);
            ds.CountOfAreaFrames("车场车位_室外车场_露天车场_小型汽车_2__V2.2").Returns(10);


            // 引擎
            var engine = ThAreaFrameMasterEngine.Engine(ds);

            // "规划净用地面积"
            Assert.AreEqual(engine.AreaOfPlanning(), 5000);

            // "地上停车位（个）"
            Assert.AreEqual(engine.CountOfAboveGroundParkingLot(), 20);

            // 公共绿地
            Assert.AreEqual(engine.AreaOfGreenSpace(), 1000);

            // "居住户数"
            Assert.AreEqual(engine.CountOfHousehold(), 500);

            // "居住人数"
            Assert.AreEqual(engine.CountOfHouseholdPopulation(), (int)(500 * 3.2));
        }
    }
}
