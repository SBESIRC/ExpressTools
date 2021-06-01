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
    public class ThSitePlanShadowGenerator : ThSitePlanGenerator
    {
        public override ObjectId OriginFrame { get; set; }
        public override Tuple<ObjectId, Vector3d> Frame { get; set; }
        public override ILogger Logger { get; set; }

        public ThSitePlanShadowGenerator()
        {
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
            if (scriptId == "3")
            {
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    ThSitePlanDbEngine.Instance.Initialize(Active.Database);
                    var itemName = configItem.Properties["Name"] as string;
                    var itemFrame = ThSitePlanDbEngine.Instance.FrameByName(itemName);

                    var newoptions = new ThSitePlanOptions()
                    {
                        Options = new Dictionary<string, object>() {
                            { "Frame", itemFrame },
                            { "Offset", null },
                            { "OriginFrame", null },
                         }
                    };
                    var itemworker = new ThSitePlanShadowWorker();
                    itemworker.DoProcess(database, configItem, newoptions);
                }
            }
            else if (scriptId == "4" || scriptId == "5")
            {
                using (AcadDatabase acadDatabase = AcadDatabase.Active())
                {
                    ThSitePlanDbEngine.Instance.Initialize(Active.Database);
                    var itemName = configItem.Properties["Name"] as string;
                    var itemFrame = ThSitePlanDbEngine.Instance.FrameByName(itemName);

                    var newoptions = new ThSitePlanOptions()
                    {
                        Options = new Dictionary<string, object>() {
                            { "Frame", itemFrame },
                            { "Offset", null },
                            { "OriginFrame", null },
                         }
                    };
                    var itemworker = new ThSitePlanSimpleShadowWorker();
                    itemworker.DoProcess(database, configItem, newoptions);
                }
            }
            return true;
        }
    }
}
