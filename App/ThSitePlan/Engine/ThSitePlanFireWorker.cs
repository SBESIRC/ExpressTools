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
using ThSitePlan;

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
        /// 过滤出图框内的所有消防场地
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
                    DBObjectCollection plines = new DBObjectCollection();
                    foreach(ObjectId obj in psr.Value.GetObjectIds())
                    {
                        plines.Add(acadDatabase.Element<Polyline>(obj));
                    }

                    ObjectIdCollection objs = new ObjectIdCollection();
                    foreach(Entity obj in plines.GetPolyLineBounding(Tolerance.Global))
                    {
                        objs.Add(acadDatabase.ModelSpace.Add(obj));
                    }
                    return objs;
                }
                else
                {
                    return new ObjectIdCollection();
                }
            }
        }
    }
}