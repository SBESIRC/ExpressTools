﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Linq2Acad;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrame
{
    class ThAreaFrameEngine : IDisposable, IComparable<ThAreaFrameEngine>
    {
        private Database database;
        private List<ThAreaFrameRoof> roofs;
        private ResidentialBuilding building;
        private ThAreaFrameFoundation foundation;
        private AOccupancyBuilding aOccupancyBuilding;
        private ThAreaFrameRoofGreenSpace roofGreenSpace;
        private Dictionary<string, IThAreaFrameCalculator> calculators;
        public List<ThAreaFrameRoof> Roofs { get => roofs; set => roofs = value; }
        public ResidentialBuilding Building { get => building; set => building = value; }
        public ThAreaFrameFoundation Foundation { get => foundation; set => foundation = value; }
        public Database Database { get => database; set => database = value; }
        public ThAreaFrameRoofGreenSpace RoofGreenSpace { get => roofGreenSpace; set => roofGreenSpace = value; }
        public AOccupancyBuilding AOccupancyBuilding { get => aOccupancyBuilding; set => aOccupancyBuilding = value; }
        public Dictionary<string, IThAreaFrameCalculator> Calculators { get => calculators; set => calculators = value; }

        // 构造函数 (current database)
        public static ThAreaFrameEngine Engine()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                return EngineInternal(acadDatabase);
            }
        }

        // 构造函数 (side database)
        public static ThAreaFrameEngine Engine(string fileName)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Open(fileName, DwgOpenMode.ReadOnly, true))
            {
                return EngineInternal(acadDatabase);
            }
        }

        // 构造函数 (side database)
        public static ThAreaFrameEngine Engine(Database database)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                return EngineInternal(acadDatabase);
            }
        }

        // 构造函数 (AcadDatabase wrapper)
        private static ThAreaFrameEngine EngineInternal(AcadDatabase acadDatabase)
        {
            var names = new List<string>();
            acadDatabase.Layers.ForEachDbObject(l => names.Add(l.Name));
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
                building = building,
                foundation = foundation,
                roofGreenSpace = roofGreenSpace,
                database = acadDatabase.Database,
                aOccupancyBuilding = aOccupancyBuilding,
                roofs = new List<ThAreaFrameRoof>(),
                calculators = new Dictionary<string, IThAreaFrameCalculator>()
            };
            foreach (var name in roofNames)
            {
                engine.roofs.Add(ThAreaFrameRoof.Roof(name));
            }
            if (building.Validate())
            {
                var roof = engine.roofs.Where(o => o.category == "住宅").FirstOrDefault();
                engine.Calculators.Add("住宅构件", ThAreaFrameResidentCalculator.Calculator(building, roof, acadDatabase.Database));
            }
            if (aOccupancyBuilding.Validate())
            {
                var roof = engine.roofs.Where(o => o.category == "公建").FirstOrDefault();
                engine.Calculators.Add("附属公建", ThAreaFrameAOccupancyCalculator.Calculator(aOccupancyBuilding, roof, acadDatabase.Database));
            }
            return engine;
        }

        // Dispose()函数
        public void Dispose()
        {
            if (!database.IsDisposed)
            {
                database.Dispose();
            }
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
            return (foundation.useInArea == @"是") ? ThAreaFrameDbUtils.SumOfArea(Database, foundation.layer) : 0.0;
        }

        // 屋顶绿化
        public double AreaOfRoofGreenSpace()
        {
            if (roofGreenSpace != null)
            {
                return ThAreaFrameDbUtils.SumOfArea(Database, roofGreenSpace.layer) * double.Parse(roofGreenSpace.coefficient);
            }
            return 0.0;
        }

        // 标准楼层个数
        public int StandardStoreyCount
        {
            get
            {
                return (Building.StandardStoreys().Count + AOccupancyBuilding.StandardStoreys().Count);
            }
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
                IEnumerable<int> resident = null;
                IEnumerable<int> aOccupancy = null;
                if (Building.Validate())
                {
                    resident = Calculators["住宅构件"].OrdinaryStoreyCollection();
                }
                if (AOccupancyBuilding.Validate())
                {
                    aOccupancy = Calculators["附属公建"].OrdinaryStoreyCollection();
                }
                if ((resident != null) && (aOccupancy != null))
                {
                    return aOccupancy.Union(resident).ToList();
                }
                else if (resident != null)
                {
                    return resident.ToList();
                }
                else if (aOccupancy != null)
                {
                    return aOccupancy.ToList();
                }
                else
                {
                    return null;
                }
            }
        }

        public List<int> UnderGroundStoreyCollection
        {
            get
            {
                IEnumerable<int> resident = null;
                IEnumerable<int> aOccupancy = null;
                if (Building.Validate())
                {
                    resident = Calculators["住宅构件"].UnderGroundStoreyCollection();
                }
                if (AOccupancyBuilding.Validate())
                {
                    aOccupancy = Calculators["附属公建"].UnderGroundStoreyCollection();
                }
                if ((resident != null) && (aOccupancy != null))
                {
                    return aOccupancy.Union(resident).ToList();
                }
                else if (resident != null)
                {
                    return resident.ToList();
                }
                else if (aOccupancy != null)
                {
                    return aOccupancy.ToList();
                }
                else
                {
                    return null;
                }

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
    }
}
