using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;
using Linq2Acad;
using AcHelper;
using NFox.Cad.Collections;

namespace ThSitePlan.Engine
{
    public class ThSitePlanDefaultWorker : ThSitePlanWorker
    {
        public override bool DoProcess(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            Vector3d offset = (Vector3d)options.Options["Offset"];
            ObjectId originFrame = (ObjectId)options.Options["OriginFrame"];
            var layers = configItem.Properties["CADLayer"] as Dictionary<string, string>;
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var filterlist = OpFilter.Bulid(o =>
                    o.Dxf((int)DxfCode.LayerName) == string.Join(",", layers.Values));
                PromptSelectionResult psr = Active.Editor.SelectByPolyline(
                    originFrame,
                    PolygonSelectionMode.Crossing,
                    filterlist);
                if (psr.Status == PromptStatus.OK)
                {
                    var objs = new ObjectIdCollection(psr.Value.GetObjectIds());
                    acadDatabase.Database.CopyWithMove(objs, Matrix3d.Displacement(offset));
                    return true;
                }
                return false;
            }
        }
    }
}
