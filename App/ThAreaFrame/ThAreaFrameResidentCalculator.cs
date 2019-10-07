using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThAreaFrame
{
    class ThAreaFrameResidentCalculator : IThAreaFrameCalculator
    {
        private readonly Database database;
        private readonly ThAreaFrameRoof roof;
        private readonly ResidentialBuilding building;
        public ResidentialBuilding Building => building;
        public ThAreaFrameRoof Roof => roof;
        public Database Database => database;

        public ThAreaFrameResidentCalculator(
            ResidentialBuilding building,
            ThAreaFrameRoof roof,
            Database database)
        {
            this.database = database;
            this.building = building;
            this.roof = roof;
        }

        public static IThAreaFrameCalculator Calculator(
            ResidentialBuilding building,
            ThAreaFrameRoof roof,
            Database database)
        {
            return new ThAreaFrameResidentCalculator(building, roof, database);
        }

        public double AreaOfCapacityBuilding(bool far/*Floor Area Ratio*/ = false)
        {
            double area = 0.0;
            for (int storey = 1; storey <= AboveGroundStoreyNumber(); storey++)
            {
                area += AreaOfFloor(storey, far);
            }
            return area;
        }

        public double AreaOfFloor(int floor, bool far = false)
        {
            double area = 0.0;
            area += AreaOfDwelling(floor, far);
            area += AreaOfBalconies(floor, far);
            area += AreaOfBaywindows(floor, far);
            area += AreaOfMiscellaneous(floor, far);
            return area;
        }

        public double AreaOfRoof(bool far = false)
        {
            if (roof != null)
            {
                double ratio = far ? double.Parse(roof.floorAreaRatio) : double.Parse(roof.areaRatio);
                return ThAreaFrameDbUtils.SumOfArea(Database, roof.layer) * ratio;
            }
            return 0.0;
        }

        public List<double> AreaOfStandardStoreys(bool far = false)
        {
            var areas = new List<double>();
            foreach (List<ResidentialStorey> storeys in building.StandardStoreys())
            {
                double area = 0.0;
                storeys.ForEach(s => area += AreaOfFloor(s.number, far));
                areas.Add(area / storeys.Count);
            }
            return areas;
        }

        public double AreaOfUnderGround(bool far = false)
        {
            double area = 0.0;
            for (int storey = 1; storey <= UnderGroundStoreyNumber(); storey++)
            {
                area += AreaOfFloor(-storey, far);
            }
            return area;
        }

        public double AreaOfAboveGround(bool far = false)
        {
            double area = 0.0;
            for (int storey = 1; storey <= AboveGroundStoreyNumber(); storey++)
            {
                area += AreaOfFloor(storey, far);
            }
            return area;
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
                    if (balcony.version == ThCADCommon.RegAppName_AreaFrame_Version_Legacy)
                    {
                        double ratio = far ? double.Parse(balcony.floorAreaRatio) : double.Parse(balcony.areaRatio);
                        area += (ThAreaFrameDbUtils.SumOfArea(Database, balcony.layer) * ratio
                            / ThAreaFrameDbUtils.CountOfDwelling(Database, balcony.dwellingID)
                            * ThAreaFrameDbUtils.CountOfAreaFrames(Database, room.dwelling.layer));
                    }
                    else if (balcony.version == ThCADCommon.RegAppName_AreaFrame_Version)
                    {
                        double ratio = far ? double.Parse(balcony.floorAreaRatio) : double.Parse(balcony.areaRatio);
                        area += ThAreaFrameDbUtils.SumOfArea(Database, balcony.layer) * ratio;
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
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
                    if (baywindow.version == ThCADCommon.RegAppName_AreaFrame_Version_Legacy)
                    {
                        double ratio = far ? double.Parse(baywindow.floorAreaRatio) : double.Parse(baywindow.areaRatio);
                        area += (ThAreaFrameDbUtils.SumOfArea(Database, baywindow.layer) * ratio
                            / ThAreaFrameDbUtils.CountOfDwelling(Database, baywindow.dwellingID)
                            * ThAreaFrameDbUtils.CountOfAreaFrames(Database, room.dwelling.layer));
                    }
                    else if (baywindow.version == ThCADCommon.RegAppName_AreaFrame_Version)
                    {
                        double ratio = far ? double.Parse(baywindow.floorAreaRatio) : double.Parse(baywindow.areaRatio);
                        area += ThAreaFrameDbUtils.SumOfArea(Database, baywindow.layer) * ratio;
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
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
                    if (item.version == ThCADCommon.RegAppName_AreaFrame_Version_Legacy)
                    {
                        double ratio = far ? double.Parse(item.floorAreaRatio) : double.Parse(item.areaRatio);
                        area += (ThAreaFrameDbUtils.SumOfArea(Database, item.layer) * ratio
                            / ThAreaFrameDbUtils.CountOfDwelling(Database, item.dwellingID)
                            * ThAreaFrameDbUtils.CountOfAreaFrames(Database, room.dwelling.layer));
                    }
                    else if (item.version == ThCADCommon.RegAppName_AreaFrame_Version)
                    {
                        double ratio = far ? double.Parse(item.floorAreaRatio) : double.Parse(item.areaRatio);
                        area += ThAreaFrameDbUtils.SumOfArea(Database, item.layer) * ratio;
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }
            }
            return area;
        }

        public int AboveGroundStoreyNumber()
        {
            int floor = 1;
            while (building.RoomsOnStorey(floor).Count != 0)
            {
                floor++;
            }
            return (floor - 1);
        }

        public int UnderGroundStoreyNumber()
        {
            int floor = 1;
            while (building.RoomsOnStorey(-floor).Count != 0)
            {
                floor++;
            }
            return (floor - 1);
        }

        public double AreaOfStilt()
        {
            return 0.0;
        }

        public IEnumerable<int> OrdinaryStoreyCollection()
        {
            return building.OrdinaryStoreys().Select(o => o.number);
        }

        public IEnumerable<int> UnderGroundStoreyCollection()
        {
            return building.UnderGroundStoreys().Select(o => o.number);
        }
    }
}
