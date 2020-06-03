using System;
using AcHelper;
using Linq2Acad;
using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;

namespace ThSitePlan.Engine
{
    public class ThSitePlanPlantGenerator : ThSitePlanGenerator
    {
        public override ObjectId OriginFrame { get; set; }
        public override Tuple<ObjectId, Vector3d> Frame { get; set; }
        private Dictionary<string, ThSitePlanWorker> Workers { get; set; }

        public override bool Generate(Database database, ThSitePlanConfigItem configItem)
        {
            var options = new ThSitePlanOptions()
            {
                Options = new Dictionary<string, object>()
                {
                    { "Frame", Frame.Item1 },
                    { "Offset", Frame.Item2 },
                    { "OriginFrame", OriginFrame },
                }
            };

            var scriptId = configItem.Properties["CADScriptID"].ToString();
            if (scriptId == "4")
            {
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
                    var itemworker = new ThSitePlanPlantWorker();
                    itemworker.DoProcess(database, configItem, newoptions);
                }
            }

            return true;
        }

    }
}
