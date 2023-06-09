﻿using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThAreaFrame
{
    class ThAreaFrameResidentCalculator : IThAreaFrameCalculator
    {
        private readonly IThAreaFrameDataSource dataSource;
        private readonly ThAreaFrameRoof roof;
        private readonly ResidentialBuilding building;
        public ResidentialBuilding Building => building;
        public ThAreaFrameRoof Roof => roof;

        public ThAreaFrameResidentCalculator(
            ResidentialBuilding building,
            ThAreaFrameRoof roof,
            IThAreaFrameDataSource dataSource)
        {
            this.roof = roof;
            this.building = building;
            this.dataSource = dataSource;
        }

        public static IThAreaFrameCalculator Calculator(
            ResidentialBuilding building,
            ThAreaFrameRoof roof,
            IThAreaFrameDataSource dataSource)
        {
            return new ThAreaFrameResidentCalculator(building, roof, dataSource);
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
                return dataSource.SumOfArea(roof.layer) * ratio;
            }
            return 0.0;
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
                area += dataSource.SumOfArea(room.dwelling.layer) * ratio;
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
                        area += dataSource.SumOfArea(balcony.layer) * ratio
                            / dataSource.CountOfDwelling(balcony.dwellingID)
                            * dataSource.CountOfAreaFrames(room.dwelling.layer);
                    }
                    else if (balcony.version == ThCADCommon.RegAppName_AreaFrame_Version)
                    {
                        double ratio = far ? double.Parse(balcony.floorAreaRatio) : double.Parse(balcony.areaRatio);
                        area += dataSource.SumOfArea(balcony.layer) * ratio;
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
                        area += dataSource.SumOfArea(baywindow.layer) * ratio
                            / dataSource.CountOfDwelling(baywindow.dwellingID)
                            * dataSource.CountOfAreaFrames(room.dwelling.layer);
                    }
                    else if (baywindow.version == ThCADCommon.RegAppName_AreaFrame_Version)
                    {
                        double ratio = far ? double.Parse(baywindow.floorAreaRatio) : double.Parse(baywindow.areaRatio);
                        area += dataSource.SumOfArea(baywindow.layer) * ratio;
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
                        area += dataSource.SumOfArea(item.layer) * ratio
                            / dataSource.CountOfDwelling(item.dwellingID)
                            * dataSource.CountOfAreaFrames(room.dwelling.layer);
                    }
                    else if (item.version == ThCADCommon.RegAppName_AreaFrame_Version)
                    {
                        double ratio = far ? double.Parse(item.floorAreaRatio) : double.Parse(item.areaRatio);
                        area += dataSource.SumOfArea(item.layer) * ratio;
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
            return Building.OrdinaryStoreys().Select(o => o.number);
        }

        public IEnumerable<int> StandardStoreyCollection()
        {
            return Building.StandardStoreys().Select(o => o.number);
        }

        public IEnumerable<int> UnderGroundStoreyCollection()
        {
            return Building.UnderGroundStoreys().Select(o => o.number);
        }
    }
}
