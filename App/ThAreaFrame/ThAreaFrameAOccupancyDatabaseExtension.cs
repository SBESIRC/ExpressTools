using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrame
{
    public static class ThAreaFrameAOccupancyDatabaseExtension
    {
        public static double AreaOfEntities(this Database database, 
            IEnumerable<ThAreaFrameAOccupancy> aOccupancies, bool far = false)
        {
            double area = 0.0;
            foreach (var aOccupancy in aOccupancies)
            {
                double ratio = far ? double.Parse(aOccupancy.floorAreaRatio) : double.Parse(aOccupancy.areaRatio);
                area += (ThAreaFrameDbUtils.SumOfArea(database, aOccupancy.layer) * ratio);
            }
            return area;
        }
    }
}
