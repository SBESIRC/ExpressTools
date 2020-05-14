using AcHelper;
using Linq2Acad;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;
using NFox.Cad.Collections;

namespace ThSitePlan.Engine
{
    public class ThSitePlanYardHatchWorker : ThSitePlanCADWorker
    {
        public ThSitePlanYardHatchWorker(string[] dxfNames, PolygonSelectionMode mode = PolygonSelectionMode.Window) : base(dxfNames, mode)
        {
            //
        }

        public override bool DoProcess(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                using (var objs = FilterPolyline(database, configItem, options))
                {
                    if (objs.Count == 0)
                    {
                        return false;
                    }

                    Active.Editor.ExplodeCmd(objs);
                }

                using (var objs = Filterline(database, configItem, options))
                {
                    if (objs.Count == 0)
                    {
                        return false;
                    }

                    Active.Editor.ThOverKillCmd(objs);
                }

                using (var objs = Filter(database, configItem, options))
                {
                    if (objs.Count == 0)
                    {
                        return false;
                    }

                    Active.Editor.BoundaryCmdEx(objs);
                }

                using (var regions = FilterRegion(database, configItem, options))
                {
                    if (regions.Count == 0)
                    {
                        return false;
                    }

                    Active.Editor.CreateHatchWithRegions(regions);
                }

                return true;
            }
        }

        private ObjectIdCollection Filterline(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            ObjectId frame = (ObjectId)options.Options["Frame"];
            var filter = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == string.Join(",", RXClass.GetClass(typeof(Line)).DxfName));
            PromptSelectionResult psr = Active.Editor.SelectByPolyline(
                frame,
                SelectionMode,
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

        private ObjectIdCollection FilterPolyline(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            ObjectId frame = (ObjectId)options.Options["Frame"];
            var filter = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == string.Join(",", RXClass.GetClass(typeof(Polyline)).DxfName));
            PromptSelectionResult psr = Active.Editor.SelectByPolyline(
                frame,
                SelectionMode,
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
