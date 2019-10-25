using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Linq2Acad;
using Autodesk.AutoCAD.DatabaseServices;


namespace ThAreaFrame
{
    public class ThAreaFrameDbDataSource : IThAreaFrameDataSource, IDisposable
    {
        private readonly Database database;
        public ThAreaFrameDbDataSource(Database db)
        {
            database = db;
        }

        public void Dispose()
        {
            if (!database.IsDisposed)
            {
                database.Dispose();
            }
        }

        public int CountOfAreaFrames(string layer)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                return acadDatabase.ModelSpace
                                .OfType<Polyline>()
                                .Where(e => e.Layer == layer)
                                .Count();
            }
        }

        public int CountOfDwelling(string dwellingID)
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

        public double SumOfArea(string layer)
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
    }
}
