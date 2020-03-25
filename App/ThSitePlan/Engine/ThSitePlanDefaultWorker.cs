using System.Collections.Generic;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;
using AcHelper;
using NFox.Cad.Collections;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThSitePlan.Engine
{
    public class ThSitePlanDefaultWorker : ThSitePlanWorker
    {
        public override ObjectIdCollection Filter(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            ObjectId originFrame = (ObjectId)options.Options["OriginFrame"];
            var layers = configItem.Properties["CADLayer"] as Dictionary<string, string>;
            var filterlist = OpFilter.Bulid(o => o.Dxf((int)DxfCode.LayerName) == string.Join(",", layers.Values));
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