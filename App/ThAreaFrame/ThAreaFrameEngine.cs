using System;
using System.Collections.Generic;
using System.Linq;

namespace ThAreaFrame
{
    public class ThAreaFrameEngine : IDisposable, IComparable<ThAreaFrameEngine>
    {
        private IThAreaFrameDataSource dataSource;
        private List<ThAreaFrameRoof> roofs;
        private ResidentialBuilding building;
        private ThAreaFrameFoundation foundation;
        private AOccupancyBuilding aOccupancyBuilding;
        private ThAreaFrameRoofGreenSpace roofGreenSpace;
        private ThAreaFrameStoreyManager storeyManager;
        private Dictionary<string, IThAreaFrameCalculator> calculators;
        private List<ThAreaFrameRoof> Roofs { get => roofs; set => roofs = value; }
        private ResidentialBuilding Building { get => building; set => building = value; }
        private ThAreaFrameFoundation Foundation { get => foundation; set => foundation = value; }
        private ThAreaFrameRoofGreenSpace RoofGreenSpace { get => roofGreenSpace; set => roofGreenSpace = value; }
        private AOccupancyBuilding AOccupancyBuilding { get => aOccupancyBuilding; set => aOccupancyBuilding = value; }
        private Dictionary<string, IThAreaFrameCalculator> Calculators { get => calculators; set => calculators = value; }

        // 构造函数
        public static ThAreaFrameEngine Engine(IThAreaFrameDataSource ds)
        {
            var names = ds.Layers();
            var roofNames = names.Where(n => n.StartsWith(@"单体楼顶间"));
            var foundationNames = names.Where(n => n.StartsWith(@"单体基底"));
            var residentialNames = names.Where(n => n.StartsWith(@"住宅构件"));
            var aOccupancyNames = names.Where(n => n.StartsWith(@"附属公建"));
            var roofGreenSpaceNames = names.Where(n => n.StartsWith(@"屋顶构件_屋顶绿地"));
            if (!foundationNames.Any())
            {
                return null;
            }
            var building = ResidentialBuilding.CreateWithLayers(residentialNames.ToArray());
            var foundation = ThAreaFrameFoundation.Foundation(foundationNames.FirstOrDefault());
            var aOccupancyBuilding = AOccupancyBuilding.CreateWithLayers(aOccupancyNames.ToArray());
            var roofGreenSpace = roofGreenSpaceNames.Any() ? ThAreaFrameRoofGreenSpace.RoofOfGreenSpace(roofGreenSpaceNames.FirstOrDefault()) : null;
            ThAreaFrameEngine engine = new ThAreaFrameEngine()
            {
                dataSource = ds,
                building = building,
                foundation = foundation,
                roofGreenSpace = roofGreenSpace,
                aOccupancyBuilding = aOccupancyBuilding,
                roofs = new List<ThAreaFrameRoof>(),
                calculators = new Dictionary<string, IThAreaFrameCalculator>(),
                storeyManager = new ThAreaFrameStoreyManager(building, aOccupancyBuilding)
            };
            foreach (var name in roofNames)
            {
                engine.roofs.Add(ThAreaFrameRoof.Roof(name));
                // 兼容V2.2版本旧实现
                //  V2.2版本旧实现中“单体楼顶间”并未包含“住宅”或者“公建”信息，默认为“公建”。
                //  对于只有“公建”的图纸，需要把“单体楼顶间”调整为“公建”。
                if (!building.Validate() && aOccupancyBuilding.Validate())
                {
                    foreach (var roof in engine.roofs)
                    {
                        roof.category = "公建";
                    }
                }
            }
            if (building.Validate())
            {
                var roof = engine.roofs.Where(o => o.category == "住宅").FirstOrDefault();
                engine.Calculators.Add("住宅构件", ThAreaFrameResidentCalculator.Calculator(building, roof, engine.dataSource));
            }
            if (aOccupancyBuilding.Validate())
            {
                var roof = engine.roofs.Where(o => o.category == "公建").FirstOrDefault();
                engine.Calculators.Add("附属公建", ThAreaFrameAOccupancyCalculator.Calculator(aOccupancyBuilding, roof, engine.dataSource));
            }
            return engine;
        }

        // Dispose()函数
        public void Dispose()
        {
            //
        }

        // 比较
        public int CompareTo(ThAreaFrameEngine other)
        {
            return this.foundation.CompareTo(other.foundation);
        }

        // 楼号
        public string Name
        {
            get
            {
                return foundation.name;
            }
        }

        // 楼栋基底面积
        public double AreaOfFoundation()
        {
            return (foundation.useInArea == @"是") ? dataSource.SumOfArea(foundation.layer) : 0.0;
        }

        // 屋顶绿化
        public double AreaOfRoofGreenSpace()
        {
            if (roofGreenSpace != null)
            {
                return dataSource.SumOfArea(roofGreenSpace.layer) * double.Parse(roofGreenSpace.coefficient);
            }
            return 0.0;
        }

        // 地上层数
        public int AboveGroundStoreyNumber
        {
            get
            {
                return int.Parse(Foundation.aboveGroundFloorNumber);
            }
        }

        // 计容面积
        public double AreaOfCapacityBuilding(bool far/*Floor Area Ratio*/ = false)
        {
            double area = 0.0;
            if (Building.Validate())
            {
                area += Calculators["住宅构件"].AreaOfCapacityBuilding(far);
            }
            if (AOccupancyBuilding.Validate())
            {
                area += Calculators["附属公建"].AreaOfCapacityBuilding(far);
            }
            return area;
        }

        // 出屋面楼梯间及屋顶机房计容面积
        public double AreaOfRoof(bool far/*Floor Area Ratio*/ = false)
        {
            return ResidentAreaOfRoof(far) + AOccupancyAreaOfRoof(far);
        }

        // 楼梯间面积（住宅）
        public double ResidentAreaOfRoof(bool far/*Floor Area Ratio*/ = false)
        {
            double area = 0.0;
            if (Building.Validate())
            {
                area += Calculators["住宅构件"].AreaOfRoof(far);
            }
            return area;
        }

        // 楼梯间面积（公建）
        public double AOccupancyAreaOfRoof(bool far/*Floor Area Ratio*/ = false)
        {
            double area = 0.0;
            if (AOccupancyBuilding.Validate())
            {
                area += Calculators["附属公建"].AreaOfRoof(far);
            }
            return area;
        }

        // 地下面积
        public double AreaOfUnderGround(bool far/*Floor Area Ratio*/ = false)
        {
            double area = 0.0;
            if (Building.Validate())
            {
                area += Calculators["住宅构件"].AreaOfUnderGround(far);
            }
            if (AOccupancyBuilding.Validate())
            {
                area += Calculators["附属公建"].AreaOfUnderGround(far);
            }
            return area;
        }

        // 地上面积
        public double AreaOfAboveGround(bool far/*Floor Area Ratio*/ = false)
        {
            return ResidentAreaOfAboveGround(far) + AOccupancyAreaOfAboveGround(far);
        }

        // 地上面积（住宅）
        public double ResidentAreaOfAboveGround(bool far/*Floor Area Ratio*/ = false)
        {
            double area = 0.0;
            if (Building.Validate())
            {
                area += Calculators["住宅构件"].AreaOfAboveGround(far);
            }
            return area;
        }

        // 地上面积（公建）
        public double AOccupancyAreaOfAboveGround(bool far/*Floor Area Ratio*/ = false)
        {
            double area = 0.0;
            if (AOccupancyBuilding.Validate())
            {
                area += Calculators["附属公建"].AreaOfAboveGround(far);
            }
            return area;
        }

        // 架空
        public double AreaOfStilt()
        {
            double area = 0.0;
            if (Building.Validate())
            {
                area += Calculators["住宅构件"].AreaOfStilt();
            }
            if (AOccupancyBuilding.Validate())
            {
                area += Calculators["附属公建"].AreaOfStilt();
            }
            return area;
        }

        public List<int> OrdinaryStoreyCollection
        {
            get
            {
                return storeyManager.OrdinaryStoreyCollection;
            }
        }

        public List<List<int>> StandardStoreyCollections
        {
            get
            {
                return storeyManager.StandardStoreyCollections;
            }
        }

        public List<int> UnderGroundStoreyCollection
        {
            get
            {
                return storeyManager.UnderGroundStoreyCollection;
            }
        }

        public double AreaOfFloor(int floor, bool far = false)
        {
            double area = 0.0;
            if (Building.Validate())
            {
                area += Calculators["住宅构件"].AreaOfFloor(floor, far);
            }
            if (AOccupancyBuilding.Validate())
            {
                area += Calculators["附属公建"].AreaOfFloor(floor, far);
            }
            return area;
        }

        public List<double> AreaOfStandardStoreys(bool far = false)
        {
            List<double> areas = new List<double>();
            foreach(var storeys in StandardStoreyCollections)
            {
                double area = 0.0;
                foreach(var storey in storeys)
                {
                    if (Building.Validate())
                    {
                        area += Calculators["住宅构件"].AreaOfFloor(storey, far);
                    }
                    if (AOccupancyBuilding.Validate())
                    {
                        area += Calculators["附属公建"].AreaOfFloor(storey, far);
                    }
                }
                areas.Add(area);
            }
            return areas;
        }
    }
}
