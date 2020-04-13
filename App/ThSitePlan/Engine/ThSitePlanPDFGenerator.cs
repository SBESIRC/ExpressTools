using System;
using System.IO;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;
using AcHelper;
using Linq2Acad;

namespace ThSitePlan.Engine
{
    /// <summary>
    /// PDF生成器
    /// </summary>
    public class ThSitePlanPDFGenerator : ThSitePlanGenerator
    {
        public override ObjectId OriginFrame { get; set; }
        public override Tuple<ObjectId, Vector3d> Frame { get; set; }
        public override bool Generate(Database database, ThSitePlanConfigItem configItem)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var frame = acadDatabase.Element<Polyline>(Frame.Item1);
                var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
                    (string)configItem.Properties["Name"]);
                Active.Editor.ZoomObject(frame.ObjectId);
                var extents2d = Active.Editor.ToPlotWindow(frame);
                ThSitePlanPlotUtils.DoPlot(extents2d, file);
                return true;
            }
        }
    }
}
