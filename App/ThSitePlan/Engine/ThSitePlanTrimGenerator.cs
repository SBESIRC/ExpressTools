using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;

namespace ThSitePlan.Engine
{
    public class ThSitePlanTrimGenerator : ThSitePlanGenerator
    {
        public override ObjectId OriginFrame { get; set; }
        public override Tuple<ObjectId, Vector3d> Frame { get; set; }
        private Dictionary<string, ThSitePlanWorker> Workers { get; set; }

        public ThSitePlanTrimGenerator()
        {
            Workers = new Dictionary<string, ThSitePlanWorker>()
            {
                {"原始场地叠加线稿", new ThSitePlanTrimWorker()},
                {"道路-外部车行道路-道路线稿", new ThSitePlanTrimWorker()},
                {"道路-外部车行道路-道路色块", new ThSitePlanTrimWorker()},
                {"道路-外部景观道路-道路线稿", new ThSitePlanTrimWorker()},
            };
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

            var key = (string)configItem.Properties["Name"];
            if (Workers.ContainsKey(key))
            {
                Workers[key].DoProcess(database, configItem, options);
            }
            return true;
        }

    }
}
