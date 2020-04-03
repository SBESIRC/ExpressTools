using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;
using AcHelper;
using Linq2Acad;
using NFox.Cad.Collections;

namespace ThSitePlan.Engine
{
    public class ThSitePlanMoveWorker : ThSitePlanWorker
    {
        public override bool DoProcess(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                using (var objs = Filter(database, configItem, options))
                {
                    Vector3d offset = (Vector3d)options.Options["Offset"];
                    acadDatabase.Database.Move(objs, Matrix3d.Displacement(offset));
                }
                return true;
            }
        }

        public override ObjectIdCollection Filter(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            ObjectId originFrame = (ObjectId)options.Options["OriginFrame"];
            var layers = configItem.Properties["CADLayer"] as List<string>;
            var filterlist = OpFilter.Bulid(o => o.Dxf((int)DxfCode.LayerName) == string.Join(",", layers.ToArray()));
            PromptSelectionResult psr = Active.Editor.SelectByPolyline(
                originFrame,
                PolygonSelectionMode.Crossing,
                filterlist);
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

    public class ThSitePlanCopyWorker : ThSitePlanWorker
    {
        public override bool DoProcess(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                using (var objs = Filter(database, configItem, options))
                {
                    Vector3d offset = (Vector3d)options.Options["Offset"];
                    acadDatabase.Database.CopyWithMove(objs, Matrix3d.Displacement(offset));
                }
                return true;
            }
        }

        public override ObjectIdCollection Filter(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            ObjectId originFrame = (ObjectId)options.Options["OriginFrame"];
            var layers = configItem.Properties["CADLayer"] as List<string>;
            var filterlist = OpFilter.Bulid(o => o.Dxf((int)DxfCode.LayerName) == string.Join(",", layers.ToArray()));
            PromptSelectionResult psr = Active.Editor.SelectByPolyline(
                originFrame,
                PolygonSelectionMode.Crossing,
                filterlist);
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