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
        public ResidentialBuilding Building { get => building; set => building = value; }
        public ThAreaFrameFoundation Foundation { get => foundation; set => foundation = value; }
        public Database Database { get => database; set => database = value; }
        internal ThAreaFrameRoof Roof { get => roof; set => roof = value; }

        // 构造函数 (current database)
        public static ThAreaFrameEngine ResidentialEngine()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                return ResidentialEngineInternal(acadDatabase);
            }
        }

        // 构造函数 (side database)
        public static ThAreaFrameEngine ResidentialEngine(string fileName)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Open(fileName, DwgOpenMode.ReadOnly, true))
            {
                return ResidentialEngineInternal(acadDatabase);
            }
        }

        // 构造函数 (AcadDatabase wrapper)
        private static ThAreaFrameEngine ResidentialEngineInternal(AcadDatabase acadDatabase)
        {
            var names = new List<string>();
            acadDatabase.Layers.ForEachDbObject(l => names.Add(l.Name));
            var roofNames = names.Where(n => n.StartsWith(@"单体楼顶间"));
            var foundationNames = names.Where(n => n.StartsWith(@"单体基底"));
            var building = ResidentialBuilding.CreateWithLayers(names.ToArray());
            var foundation = ThAreaFrameFoundation.Foundation(foundationNames.FirstOrDefault());
            var roof = roofNames.Any() ? ThAreaFrameRoof.Roof(roofNames.FirstOrDefault()) : null;
            ThAreaFrameEngine engine = new ThAreaFrameEngine()
            {
                roof = roof,
                building = building,
                foundation = foundation,
                database = acadDatabase.Database
            };
            return engine;
        }

        // Dispose()函数
        public void Dispose()
        {
            database.Dispose();
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

        // 地上层数
        public int AboveGroundStoreyNumber()
        {
            int floor = 1;
            while (building.RoomsOnStorey(floor).Count != 0)
            {
                floor++;
            }
            return (floor - 1);
        }

        // 地下层数
        public int UnderGroundStoreyNumber()
        {
            int floor = 1;
            while (building.RoomsOnStorey(-floor).Count != 0)
            {
                floor++;
            }
            return (floor - 1);

        }

        // 楼层总面积
        public double AreaOfFloor(int floor, bool far = false)
        {
            return AreaOfDwelling(floor, far) + AreaOfBalconies(floor, far) + AreaOfBaywindows(floor, far) + AreaOfMiscellaneous(floor, far);
        }

        // 标准楼层面积
        public List<double> AreaOfStandardStoreys()
        {
            var areas = new List<double>();
            foreach (List<ResidentialStorey> storeys in building.StandardStoreys())
            {
                double area = 0.0;
                storeys.ForEach(s => area += AreaOfFloor(s.number));
                areas.Add(area);
            }
            return areas;
        }

        // 报建面积
        public double AreaOfApplication()
        {
            double area = 0.0;
            for (int storey = 1; storey <= AboveGroundStoreyNumber(); storey++)
            {
                area += AreaOfFloor(storey, false);
            }
            return area + AreaOfRoof(false);
        }

        // 地下面积
        public double AreaOfUnderGround()
        {
            double area = 0.0;
            for (int storey = 1; storey <= UnderGroundStoreyNumber(); storey++)
            {
                area += AreaOfFloor(-storey, false);
            }
            return area;
        }

        // 计容面积
        public double AreaOfCapacityBuilding()
        {
            double area = 0.0;
            for (int storey = 1; storey <= AboveGroundStoreyNumber(); storey++)
            {
                area += AreaOfFloor(storey, true);
            }
            return area + AreaOfRoof(true);
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

        // 套内面积总和
        private double AreaOfDwelling(int floor, bool far/*Floor Area Ratio*/ = false)
        {
            double area = 0.0;
            foreach (var room in building.RoomsOnStorey(floor))
            {
                double ratio = far ? double.Parse(room.dwelling.floorAreaRatio) : double.Parse(room.dwelling.areaRatio);
                area += (ThAreaFrameDbUtils.SumOfArea(Database, room.dwelling.layer) * ratio);
            }
            return area;
        }

        // 阳台面积总和
        private double AreaOfBalconies(int floor, bool far/*Floor Area Ratio*/ = false)
        {
            double area = 0.0;
            foreach (var room in building.RoomsOnStorey(floor))
            {
                foreach (var balcony in room.balconies)
                {
                    double ratio = far ? double.Parse(balcony.floorAreaRatio) : double.Parse(balcony.areaRatio);
                    area += (ThAreaFrameDbUtils.SumOfArea(Database, balcony.layer) * ratio  
                        / ThAreaFrameDbUtils.CountOfDwelling(Database, balcony.dwellingID)
                        * ThAreaFrameDbUtils.CountOfAreaFrames(Database, room.dwelling.layer));
                }
            }
            return area;
        }

        // 飘窗面积总和
        private double AreaOfBaywindows(int floor, bool far/*Floor Area Ratio*/ = false)
        {
            double area = 0.0;
            foreach (var room in building.RoomsOnStorey(floor))
            {
                foreach (var baywindow in room.baywindows)
                {
                    double ratio = far ? double.Parse(baywindow.floorAreaRatio) : double.Parse(baywindow.areaRatio);
                    area += (ThAreaFrameDbUtils.SumOfArea(Database, baywindow.layer) * ratio
                        / ThAreaFrameDbUtils.CountOfDwelling(Database, baywindow.dwellingID)
                        * ThAreaFrameDbUtils.CountOfAreaFrames(Database, room.dwelling.layer));
                }
            }
            return area;
        }

        // 其他面积总和
        private double AreaOfMiscellaneous(int floor, bool far/*Floor Area Ratio*/ = false)
        {
            double area = 0.0;
            foreach (var room in building.RoomsOnStorey(floor))
            {
                foreach (var item in room.miscellaneous)
                {
                    double ratio = far ? double.Parse(item.floorAreaRatio) : double.Parse(item.areaRatio);
                    area += (ThAreaFrameDbUtils.SumOfArea(Database, item.layer) * ratio
                        / ThAreaFrameDbUtils.CountOfDwelling(Database, item.dwellingID)
                        * ThAreaFrameDbUtils.CountOfAreaFrames(Database, room.dwelling.layer));
                }
            }
            return area;
        }
    }
}
