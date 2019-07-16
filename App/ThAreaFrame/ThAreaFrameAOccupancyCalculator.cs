using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrame
{
    class ThAreaFrameAOccupancyCalculator : IThAreaFrameCalculator
    {
        private readonly Database database;
        private readonly AOccupancyBuilding building;
        public Database Database => database;
        public AOccupancyBuilding Building => building;

        public ThAreaFrameAOccupancyCalculator(AOccupancyBuilding building, Database database)
        {
            this.building = building;
            this.database = database;
        }

        public static IThAreaFrameCalculator Calculator(AOccupancyBuilding building, Database database)
        {
            return new ThAreaFrameAOccupancyCalculator(building, database);
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
            string[] entities = { "主体", "阳台", "飘窗", "雨棚", "附属其他构件" };
            foreach (string entity in entities)
            {
                var aOccupancies = Building.aOccupancies.Where(o => o.areaType == entity && o.IsOnStorey(floor));
                area += Database.AreaOfEntities(aOccupancies, far);
            }
            return area;
        }

        public List<double> AreaOfStandardStoreys(bool far = false)
        {
            var areas = new List<double>();
            foreach (List<AOccupancyStorey> storeys in Building.StandardStoreys())
            {
                double area = 0.0;
                storeys.ForEach(s => area += AreaOfFloor(s.number, far));
                areas.Add(area);
            }
            return areas;
        }

        public double AreaOfUnderGround()
        {
            double area = 0.0;
            for (int storey = 1; storey <= UnderGroundStoreyNumber(); storey++)
            {
                area += AreaOfFloor(-storey, false);
            }
            return area;
        }

        public double AreaOfAboveGround(double roofArea)
        {
            double area = 0.0;
            for (int storey = 1; storey <= AboveGroundStoreyNumber(); storey++)
            {
                area += AreaOfFloor(storey, false);
            }
            return area + roofArea;
        }

        public double AreaOfCapacityBuilding(double roofArea)
        {
            double area = 0.0;
            for (int storey = 1; storey <= AboveGroundStoreyNumber(); storey++)
            {
                area += AreaOfFloor(storey, true);
            }
            return area + roofArea;
        }

        public double AreaOfStilt()
        {
            var aOccupancies = Building.aOccupancies.Where(o => o.areaType == "架空");
            return Database.AreaOfEntities(aOccupancies);
        }
    }
}
