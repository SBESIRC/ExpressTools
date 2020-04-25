using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;
using AcHelper;
using NFox.Cad.Collections;

namespace ThSitePlan.Engine
{
    public class ThSitePlanCADWorker : ThSitePlanWorker
    {
        private string[] DxfNames { get; set; }
        public ThSitePlanCADWorker(string[] dxfNames)
        {
            DxfNames = dxfNames;
        }

        public override bool DoProcess(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            using (var objs = Filter(database, configItem, options))
            {
                if (objs.Count == 0)
                {
                    return false;
                }

                Active.Editor.CreateRegions(objs);
            }

            using (var regions = FilterRegion(database, configItem, options))
            {
                if (regions.Count == 0)
                {
                    return false;
                }

                Active.Editor.UnionRegions(regions);
            }

            using (var regions = FilterRegion(database, configItem, options))
            {
                if (regions.Count == 0)
                {
                    return false;
                }

                Active.Editor.ExplodeCmd(regions);
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

        public override ObjectIdCollection Filter(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            ObjectId frame = (ObjectId)options.Options["Frame"];
            var filter = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == string.Join(",", DxfNames));
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

        private ObjectIdCollection FilterRegion(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            ObjectId frame = (ObjectId)options.Options["Frame"];
            var filter = OpFilter.Bulid(o => o.Dxf((int)DxfCode.Start) == string.Join(",", new string[]
            {
                RXClass.GetClass(typeof(Region)).DxfName,
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
