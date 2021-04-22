using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;
using ThSitePlan.Log;

namespace ThSitePlan.Engine
{
    public class ThSitePlanFrameNameGenerator:ThSitePlanGenerator
    {
        public override ObjectId OriginFrame { get; set; }
        public override Tuple<ObjectId, Vector3d> Frame { get; set; }
        public override ILogger Logger { get; set; }

        public ThSitePlanFrameNameGenerator()
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

            var worker = new ThSitePlanAddNameTextWorker();
            worker.DoProcess(database, configItem, options);
            return true;
        }
    }
}
