using System;
using System.IO;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;

namespace ThSitePlan.Engine
{
    public class ThSitePlaneImageGenerator : ThSitePlanGenerator
    {
        public override ObjectId OriginFrame { get; set; }
        public override Tuple<ObjectId, Vector3d> Frame { get; set; }
        public override bool Generate(Database database, ThSitePlanConfigItem configItem)
        {
            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), (string)configItem.Properties["Name"]);
            ThSitePlanPlotUtils.RenderPdfToPng(Path.ChangeExtension(file, ".pdf"), Path.ChangeExtension(file, ".png"));
            return true;
        }
    }
}
