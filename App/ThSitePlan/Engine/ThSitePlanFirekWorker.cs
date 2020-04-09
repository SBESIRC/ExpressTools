using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;
using Linq2Acad;
using AcHelper;
using NFox.Cad.Collections;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.ApplicationServices;

namespace ThSitePlan.Engine
{
    public class ThSitePlanFireWorker : ThSitePlanWorker
    {
        public override bool DoProcess(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                using (var objs = Filter(database, configItem, options))
                {
                    var loops = acadDatabase.Database.CreateRegionLoops(objs);
                    foreach (ObjectId loop in loops)
                    {
                        loop.CreateHatchWithPolygon();
                    }

                    return true;
                }
            }
        }

        /// <summary>
        /// 过滤出图框内的所有停车场
        /// </summary>
        /// <param name="database"></param>
        /// <param name="configItem"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override ObjectIdCollection Filter(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                ObjectId frame = (ObjectId)options.Options["Frame"];
                var filter = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == string.Join(",", new string[]
                {
                RXClass.GetClass(typeof(Polyline)).DxfName
                }));
                PromptSelectionResult psr = Active.Editor.SelectByPolyline(
                    frame,
                    PolygonSelectionMode.Window,
                    filter);
                if (psr.Status == PromptStatus.OK)
                {
                    ObjectIdCollection SelectObjs = new ObjectIdCollection(psr.Value.GetObjectIds());
                    ObjectIdCollection SelectObjs_Closed = new ObjectIdCollection();

                    List<Point2d> PolVet = new List<Point2d>();

                    foreach (ObjectId obj in SelectObjs)
                    {
                        var pline = acadDatabase.Element<Polyline>(obj);

                        for (int i = 0; i < pline.NumberOfVertices; i++)
                        {
                            PolVet.Add(pline.GetPoint2dAt(i));
                        }

                        Polyline newpoly = CreatePolyline(PolVet, acadDatabase.Database);
                        if (newpoly.NumberOfVertices >= 4)
                        {
                            SelectObjs_Closed.Add(acadDatabase.ModelSpace.Add(newpoly));
                        }
                    }

                    return SelectObjs_Closed;
                }
                else
                {
                    return new ObjectIdCollection();
                }
            }
        }

        private Polyline CreatePolyline(List<Point2d> points,Database ctdb)
        {
            var poly = new Polyline(points.Count());
            List<Point2d> DisPots = points.Distinct().ToList();

            for (int i = 0; i < DisPots.Count; i++)
            {
                poly.AddVertexAt(i, points[i], 0, 0, 0);
            }

            poly.SetDatabaseDefaults(ctdb);
            poly.Closed = true;

            return poly;
        }
    }
}