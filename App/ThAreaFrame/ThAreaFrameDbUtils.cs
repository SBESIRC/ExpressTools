using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Linq2Acad;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThAreaFrame
{
    class ThAreaFrameDbUtils
    {
        // 获取特定图层中所有面积框线的面积总和
        public static double SumOfArea(Database database, string layer)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                double area = 0.0;
                acadDatabase.ModelSpace
                            .OfType<Polyline>()
                            .Where(e => e.Layer == layer)
                            .ForEachDbObject(e => area += e.Area);
                return area;
            }
        }

        // 获取特定图层中面积框线的个数
        public static int CountOfAreaFrames(Database database, string layer)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                return acadDatabase.ModelSpace
                                .OfType<Polyline>()
                                .Where(e => e.Layer == layer)
                                .Count();
            }
        }

        // 获取特定套内的套数
        public static int CountOfDwelling(Database database, string dwellingID)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                return acadDatabase.ModelSpace
                                    .OfType<Polyline>()
                                    .Where(e => e.Layer.Split('_').ElementAt(0) == @"住宅构件" &&
                                                e.Layer.Split('_').ElementAt(1) == @"套内" &&
                                                e.Layer.Split('_').ElementAt(5) == dwellingID)
                                    .Count();
            }
        }
    }
}
