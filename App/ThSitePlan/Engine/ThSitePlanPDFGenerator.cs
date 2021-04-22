using System;
using System.IO;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;
using AcHelper;
using Linq2Acad;
using ThSitePlan.Log;

namespace ThSitePlan.Engine
{
    /// <summary>
    /// PDF生成器
    /// </summary>
    public class ThSitePlanPDFGenerator : ThSitePlanGenerator
    {
        public override ObjectId OriginFrame { get; set; }
        public override Tuple<ObjectId, Vector3d> Frame { get; set; }
        public override ILogger Logger { get; set; }

        public ThSitePlanPDFGenerator(ILogger logger = null)
        {
            Logger = logger;
        }

        public override bool Generate(Database database, ThSitePlanConfigItem configItem)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var name = (string)configItem.Properties["Name"];
                string file = Path.Combine(ThSitePlanSettingsService.Instance.OutputPath, name);
                var frame = acadDatabase.Element<Polyline>(Frame.Item1);
                Active.Editor.ZoomObject(frame.ObjectId);
                var extents2d = Active.Editor.ToPlotWindow(frame);
                try
                {
                    ThSitePlanPlotUtils.DoPlot(extents2d, file, IsLandscapFrame(frame));
                    if(null != Logger)
                    {
                        var msg = "成功打印PDF文件：" + file;
                        Logger.LogInfo(msg);
                    }

                    return true;
                }
                catch (Exception e)
                {
                    var msg = "打印PDF文件出现问题：" + name;
                    Logger.LogError(msg);
                    throw e;
                }
            }
        }

        public bool IsLandscapFrame(Polyline frame)
        {
            Extents3d frameextent = frame.GeometricExtents;
            return frameextent.Width() > frameextent.Height();
        }
    }
}
