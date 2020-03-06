using System;
using System.Linq;
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
    /// <summary>
    /// 解构图集生成器
    /// </summary>
    public class ThSitePlanContentGenerator : ThSitePlanGenerator
    {
        public override ObjectId OriginFrame { get; set; }
        public override Tuple<ObjectId, Vector3d> Frame { get; set; }
        public override bool Generate(Database database, ThSitePlanConfigItem configItem)
        {
            var layers = configItem.Properties["CADLayer"] as Dictionary<string, string>;
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var filterlist = OpFilter.Bulid(o => 
                    o.Dxf((int)DxfCode.LayerName) == string.Join(",", layers.Values));
                PromptSelectionResult psr = Active.Editor.SelectByPolyline(
                    OriginFrame, 
                    PolygonSelectionMode.Crossing, 
                    filterlist);
                if (psr.Status == PromptStatus.OK)
                {
                    var objs = new ObjectIdCollection(psr.Value.GetObjectIds());
                    acadDatabase.Database.CopyWithMove(objs, Matrix3d.Displacement(Frame.Item2));
                    return true;
                }
                return false;
            }
        }
    }
}
