using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using GeoAPI.Geometries;
using Linq2Acad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThCADCore.NTS;

namespace ThWSS.Utlis
{
    public class RegionDivisionByBeamUtils
    {
        readonly Tolerance tolerance = new Tolerance(0.001, 0.001);

        public List<Polyline> DivisionRegion(Polyline room, List<Polyline> beamPolys)
        {
            DBObjectCollection pLines = new DBObjectCollection();
            for (int i = 0; i < room.NumberOfVertices; i++)
            {
                var current = room.GetPoint3dAt(i);
                var next = room.GetPoint3dAt((i + 1) % room.NumberOfVertices);
                pLines.Add(new Line(current, next));
            }
            
            foreach (var bPoly in beamPolys)
            {
                for (int i = 0; i < bPoly.NumberOfVertices; i++)
                {
                    var current = bPoly.GetPoint3dAt(i);
                    var next = bPoly.GetPoint3dAt((i + 1) % bPoly.NumberOfVertices);
                    pLines.Add(new Line(current, next));
                }
            }
            
            return GetDivisionines(room, beamPolys);
        }

        /// <summary>
        /// 计算区域分割
        /// </summary>
        /// <param name="allLines"></param>
        /// <param name="allPoints"></param>
        /// <returns></returns>
        private List<Polyline> GetDivisionines(DBObjectCollection allLines, List<Polyline> beamPolys)
        {
            var polygons = allLines.Polygonize();
            var geometrys = beamPolys.Select(x => (x.ToNTSLineString() as ILinearRing).ToPolygon());
            foreach (var geoms in geometrys)
            {
                polygons = polygons.Where(x =>
                {
                    try
                    {
                        var intRes = x.Intersection(geoms);
                        if (intRes.Area > 10)
                        {
                            return false;
                        }
                        return true;
                    }
                    catch
                    {
                        using (AcadDatabase acadDatabase = AcadDatabase.Active())
                        {
                            var pline = (x as IPolygon).ToDbPolylines()[0];
                            pline.ColorIndex = 2;
                            
                            acadDatabase.ModelSpace.Add(pline);

                            pline = (geoms as IPolygon).ToDbPolylines()[0];
                            pline.ColorIndex = 3;

                            acadDatabase.ModelSpace.Add(pline);
                            return false;
                        }
                    }
                }).ToList();
            }
            
            return polygons.SelectMany(x=>(x as IPolygon).ToDbPolylines()).ToList();
        }

        /// <summary>
        /// 计算区域分割
        /// </summary>
        /// <param name="polyline"></param>
        /// <param name="beamPolys"></param>
        /// <returns></returns>
        private List<Polyline> GetDivisionines(Polyline polyline, List<Polyline> beamPolys)
        {
            DBObjectCollection dBObjects = new DBObjectCollection();
            foreach (var bpl in beamPolys)
            {
                dBObjects.Add(bpl);
            }
            return polyline.Difference(dBObjects).Cast<Polyline>().ToList();
        }
    }
}
