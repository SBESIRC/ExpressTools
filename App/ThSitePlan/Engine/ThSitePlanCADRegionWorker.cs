using AcHelper;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;

namespace ThSitePlan.Engine
{
    public class ThSitePlanRegionWorker : ThSitePlanCADWorker
    {
        public ThSitePlanRegionWorker(string[] dxfNames,
            PolygonSelectionMode mode = PolygonSelectionMode.Window) : base(dxfNames, mode)
        {
            //
        }

        public override bool DoProcess(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            using (var objs = Filter(database, configItem, options))
            {
                if (objs.Count == 0)
                {
                    return false;
                }

                Active.Editor.CreateHatchWithRegions(objs);
            }

            return true;
        }
    }
}
