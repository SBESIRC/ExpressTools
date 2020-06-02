using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;
using Linq2Acad;
using AcHelper;

namespace ThSitePlan.Engine
{
    public class ThSitePlanBoundaryGenerator : ThSitePlanGenerator
    {
        public override ObjectId OriginFrame { get; set; }
        public override Tuple<ObjectId, Vector3d> Frame { get; set; }
        private Dictionary<string, ThSitePlanWorker> Workers { get; set; }
        public ThSitePlanBoundaryGenerator()
        {
            //
        }

        public override bool Generate(Database database, ThSitePlanConfigItem configItem)
        {
            var options = new ThSitePlanOptions()
            {
                Options = new Dictionary<string, object>()
                {
                    {"Frame", Frame.Item1},
                    {"Offset",  Frame.Item2},
                    {"OriginFrame", OriginFrame},
                }
            };

            var scriptId = configItem.Properties["CADScriptID"].ToString();
            if (scriptId == "1")
            {
                //如果CAD脚本为1，即为区域填充图框，直接从原始复制图框将相应的图形元素移动到当前图框中
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    //获取当前色块填充图框
                    ThSitePlanDbEngine.Instance.Initialize(Active.Database);
                    var currenthatchframe = ThSitePlanDbEngine.Instance.FrameByName(configItem.Properties["Name"].ToString());

                    var newoptions = new ThSitePlanOptions()
                    {
                        Options = new Dictionary<string, object>() {
                            { "Frame", currenthatchframe },
                            { "Offset", null },
                            { "OriginFrame", null },
                         }
                    };
                    var itemworker = new ThSitePlanBoundaryBuildingWorker();
                    itemworker.DoProcess(database, configItem, newoptions);
                }
            }
            else if (scriptId == "2")
            {
                //获取当前色块填充图框
                ThSitePlanDbEngine.Instance.Initialize(Active.Database);
                var currenthatchframe = ThSitePlanDbEngine.Instance.FrameByName(configItem.Properties["Name"].ToString());

                var newoptions = new ThSitePlanOptions()
                {
                    Options = new Dictionary<string, object>() {
                            { "Frame", currenthatchframe },
                            { "Offset", null },
                            { "OriginFrame", null },
                         }
                };
                var itemworker = new ThSitePlanBoundaryPathWorker();
                itemworker.DoProcess(database, configItem, newoptions);
            }
            return true;
        }
    }
}
