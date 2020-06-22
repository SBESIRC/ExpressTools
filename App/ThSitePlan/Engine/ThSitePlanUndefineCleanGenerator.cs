using System;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFox.Cad.Collections;
using Autodesk.AutoCAD.EditorInput;
using AcHelper;
using DotNetARX;

namespace ThSitePlan.Engine
{
    public class ThSitePlanUndefineCleanGenerator : ThSitePlanGenerator
    {
        public override ObjectId OriginFrame { get; set; }
        public override Tuple<ObjectId, Vector3d> Frame { get ; set ; }

        public override bool Generate(Database database, ThSitePlanConfigItem configItem)
        {
            using (var objs = Filter(database,configItem))
            {
                foreach (ObjectId oid in objs)
                {
                    if (oid.IsErased == true)
                    {
                        continue;
                    }
                    oid.Erase();
                }
            }
            return true;
        }

        public ObjectIdCollection Filter(Database database, ThSitePlanConfigItem configItem)
        {
            var layers = configItem.Properties["CADLayer"] as List<string>;
            var filterlist = OpFilter.Bulid(o =>o.Dxf((int)DxfCode.LayerName) == string.Join(",", layers.ToArray()));
            PromptSelectionResult psr = Active.Editor.SelectByPolyline(
                OriginFrame,
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
