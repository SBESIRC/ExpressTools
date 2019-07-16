using System;
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
        private ThAreaFrameRoof roof;
        private ResidentialBuilding building;
        private ThAreaFrameFoundation foundation;
        private AOccupancyBuilding aOccupancyBuilding;
        private Dictionary<string, IThAreaFrameCalculator> calculators;
        public ResidentialBuilding Building { get => building; set => building = value; }
        public ThAreaFrameFoundation Foundation { get => foundation; set => foundation = value; }
        public Database Database { get => database; set => database = value; }
        public ThAreaFrameRoof Roof { get => roof; set => roof = value; }
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

        // 构造函数 (AcadDatabase wrapper)
        private static ThAreaFrameEngine EngineInternal(AcadDatabase acadDatabase)
        {
            var names = new List<string>();
            acadDatabase.Layers.ForEachDbObject(l => names.Add(l.Name));
            var roofNames = names.Where(n => n.StartsWith(@"单体楼顶间"));
            var foundationNames = names.Where(n => n.StartsWith(@"单体基底"));
            var residentialNames = names.Where(n => n.StartsWith(@"住宅构件"));
            var aOccupancyNames = names.Where(n => n.StartsWith(@"附属公建"));
            if (!residentialNames.Any() && !aOccupancyNames.Any())
            {
                return null;
            }
            var building = ResidentialBuilding.CreateWithLayers(residentialNames.ToArray());
            var foundation = ThAreaFrameFoundation.Foundation(foundationNames.FirstOrDefault());
            var aOccupancyBuilding = AOccupancyBuilding.CreateWithLayers(aOccupancyNames.ToArray());
            var roof = roofNames.Any() ? ThAreaFrameRoof.Roof(roofNames.FirstOrDefault()) : null;
            ThAreaFrameEngine engine = new ThAreaFrameEngine()
            {
                roof = roof,
                building = building,
                foundation = foundation,
                database = acadDatabase.Database,
                aOccupancyBuilding = aOccupancyBuilding,
                calculators = new Dictionary<string, IThAreaFrameCalculator>()
            };
            if (building.Validate())
            {
                engine.Calculators.Add("住宅构件", ThAreaFrameResidentCalculator.Calculator(building, acadDatabase.Database));
            }
            if (aOccupancyBuilding.Validate())
            {
                engine.Calculators.Add("附属公建", ThAreaFrameAOccupancyCalculator.Calculator(aOccupancyBuilding, acadDatabase.Database));
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

        // 面积计算器
        public IThAreaFrameCalculator Calculator
        {
            get
            { 
                if (Building.Validate() && AOccupancyBuilding.Validate())
                {
                    //TODO：公建和住宅在同一图纸中
                    return null;
                }
                else if (Building.Validate())
                {
                    return this.Calculators["住宅构件"];
                }
                else if (AOccupancyBuilding.Validate())
                {
                    return this.Calculators["附属公建"];
                }

                return null;
            }
        }

        // 楼栋基底面积
        public double AreaOfFoundation()
        {
            return (foundation.useInArea == @"是") ? ThAreaFrameDbUtils.SumOfArea(Database, foundation.layer) : 0.0;
        }

        // 出屋面楼梯间及屋顶机房
        public double AreaOfRoof(bool far/*Floor Area Ratio*/ = false)
        {
            if (roof != null)
            {
                double ratio = far ? double.Parse(roof.floorAreaRatio) : double.Parse(roof.areaRatio);
                return ThAreaFrameDbUtils.SumOfArea(Database, roof.layer) * ratio;
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
                int storeyNumber = 0;
                if (Building.Validate())
                {
                    storeyNumber += Calculators["住宅构件"].AboveGroundStoreyNumber();
                }
                if (AOccupancyBuilding.Validate())
                {
                    storeyNumber += Calculators["附属公建"].AboveGroundStoreyNumber();
                }
                return storeyNumber;
            }
        }

        // 计容面积
        public double AreaOfCapacityBuilding(double roofArea)
        {
            double area = 0.0;
            if (Building.Validate())
            {
                area += Calculators["住宅构件"].AreaOfCapacityBuilding(roofArea);
            }
            if (AOccupancyBuilding.Validate())
            {
                area += Calculators["附属公建"].AreaOfCapacityBuilding(roofArea);
            }
            return area;
        }

        // 地下面积
        public double AreaOfUnderGround()
        {
            double area = 0.0;
            if (Building.Validate())
            {
                area += Calculators["住宅构件"].AreaOfUnderGround();
            }
            if (AOccupancyBuilding.Validate())
            {
                area += Calculators["附属公建"].AreaOfUnderGround();
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

        // 地上建筑面积
        public double AreaOfAboveGround(double roofArea)
        {
            double area = 0.0;
            if (Building.Validate())
            {
                area += Calculators["住宅构件"].AreaOfAboveGround(roofArea);
            }
            if (AOccupancyBuilding.Validate())
            {
                area += Calculators["附属公建"].AreaOfAboveGround(roofArea);
            }
            return area;
        }
    }
}
