using System;
using AcHelper;
using Linq2Acad;
using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;
using ThSitePlan.Log;

namespace ThSitePlan.Engine
{
    public class ThSitePlanPlantGenerator : ThSitePlanGenerator
    {
        public override ObjectId OriginFrame { get; set; }
        public override Tuple<ObjectId, Vector3d> Frame { get; set; }
        public override ILogger Logger { get; set; }

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
            if (scriptId == "5")
            {
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
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
