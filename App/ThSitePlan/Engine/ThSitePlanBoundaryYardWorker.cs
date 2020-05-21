using AcHelper;
using Linq2Acad;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;
using NFox.Cad.Collections;

namespace ThSitePlan.Engine
{
    public class ThSitePlanBoundaryYardWorker : ThSitePlanBoundaryWorker
    {
        public override bool DoProcess(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                // 根据院子线稿，获取院子面域
                using (var objs = FilterCurve(database, configItem, options))
                {
                    if (objs.Count == 0)
                    {
                        return false;
                    }

                    foreach (ObjectId boundary in acadDatabase.Database.CreateBoundaries(objs))
                    {
                        boundary.CreateHatchWithPolygon();
                    }
                }

                // 删除院子线稿
                using (var objs = Filter(database, configItem, options))
                {
                    if (objs.Count == 0)
                    {
                        return false;
                    }

                    Active.Editor.EraseCmd(objs);
                }

                return true;
            }
        }

        public override ObjectIdCollection Filter(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            ObjectId frame = (ObjectId)options.Options["Frame"];
            var filter = OpFilter.Bulid(o => o.Dxf((int)DxfCode.LayerName) == ThSitePlanCommon.LAYER_BUILD_OUT);
            PromptSelectionResult psr = Active.Editor.SelectByPolyline(
                frame,
                PolygonSelectionMode.Crossing,
                filter);
            if (psr.Status == PromptStatus.OK)
            {
                return new ObjectIdCollection(psr.Value.GetObjectIds());
            }
            else
            {
                return new ObjectIdCollection();
            }
        }

        private ObjectIdCollection FilterCurve(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            ObjectId frame = (ObjectId)options.Options["Frame"];
            var filter = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == string.Join(",", new string[]
            {
                RXClass.GetClass(typeof(Arc)).DxfName,
                RXClass.GetClass(typeof(Line)).DxfName,
                RXClass.GetClass(typeof(Polyline)).DxfName,
            }));
            PromptSelectionResult psr = Active.Editor.SelectByPolyline(
                frame,
                PolygonSelectionMode.Window,
                filter);
            if (psr.Status == PromptStatus.OK)
            {
                return new ObjectIdCollection(psr.Value.GetObjectIds());
            }
            else
            {
                return new ObjectIdCollection();
            }
        }
    }
}
