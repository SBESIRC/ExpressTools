﻿using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrame
{
    class ThAreaFrameAOccupancyCalculator : IThAreaFrameCalculator
    {
        private readonly ThAreaFrameRoof roof;
        private readonly AOccupancyBuilding building;
        private readonly IThAreaFrameDataSource dataSource;
        public ThAreaFrameRoof Roof => roof;
        public AOccupancyBuilding Building => building;

        public ThAreaFrameAOccupancyCalculator(
            AOccupancyBuilding building,
            ThAreaFrameRoof roof,
            IThAreaFrameDataSource dataSource)
        {
            this.roof = roof;
            this.building = building;
            this.dataSource = dataSource;
        }

        public static IThAreaFrameCalculator Calculator(
            AOccupancyBuilding building,
            ThAreaFrameRoof roof,
            IThAreaFrameDataSource dataSource)
        {
            return new ThAreaFrameAOccupancyCalculator(building, roof, dataSource);
        }

        public int AboveGroundStoreyNumber()
        {
            int floor = 1;
            while (building.aOccupancies.Where(o => o.areaType == "主体" && o.IsOnStorey(floor)).Any())
            {
                floor++;
            }
            return (floor - 1);
        }

        public int UnderGroundStoreyNumber()
        {
            int floor = 1;
            while (building.aOccupancies.Where(o => o.areaType == "主体" && o.IsOnStorey(-floor)).Any())
            {
                floor++;
            }
            return (floor - 1);
        }

        public double AreaOfFloor(int floor, bool far = false)
        {
            double area = 0.0;
            string[] components = { "主体", "阳台", "架空", "飘窗", "雨棚", "附属其他构件" };
            foreach (string component in components)
            {
                area += AreaOfComponent(component, floor, far);
            }
            return area;
        }

        private double AreaOfComponent(string component, int floor, bool far = false)
        {
            double area = 0.0;
            var aOccupancies = Building.aOccupancies.Where(o => o.areaType == component && o.IsOnStorey(floor));
            foreach (var aOccupancy in aOccupancies)
            {
                double ratio = far ? double.Parse(aOccupancy.floorAreaRatio) : double.Parse(aOccupancy.areaRatio);
                area += dataSource.SumOfArea(aOccupancy.layer) * ratio;
            }
            return area;
        }

        public double AreaOfStilt()
        {
            double area = 0.0;

            // 普通楼层
            foreach (var storey in OrdinaryStoreyCollection())
            {
                area += AreaOfComponent("架空", storey, false);
            }
            // 标准楼层
            foreach(var storey in StandardStoreyCollection())
            {
                area += AreaOfComponent("架空", storey, false);
            }

            return area;
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

        public double AreaOfCapacityBuilding(bool far/*Floor Area Ratio*/ = false)
        {
            double area = 0.0;
            for (int storey = 1; storey <= AboveGroundStoreyNumber(); storey++)
            {
                area += AreaOfFloor(storey, far);
            }
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
