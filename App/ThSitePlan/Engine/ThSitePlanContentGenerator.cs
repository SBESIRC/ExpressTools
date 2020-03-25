using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;

namespace ThSitePlan.Engine
{
    /// <summary>
    /// 解构图集生成器
    /// </summary>
    public class ThSitePlanContentGenerator : ThSitePlanGenerator
    {
        public override ObjectId OriginFrame { get; set; }
        public override Tuple<ObjectId, Vector3d> Frame { get; set; }
        private Dictionary<string, ThSitePlanWorker> Workers { get; set; }
        public ThSitePlanContentGenerator()
        {
            Workers = new Dictionary<string, ThSitePlanWorker>()
            {
                {"原始场地线稿", new ThSitePlanDefaultWorker()},
                {"建筑信息", new ThSitePlanDefaultWorker()},
                {"建筑线稿", new ThSitePlanDefaultWorker()},
                {"建筑色块", new ThSitePlanDefaultWorker()},
                {"树木线稿", new ThSitePlanDefaultWorker()},
                {"树木色块", new ThSitePlanDefaultWorker()},
                {"场地线稿", new ThSitePlanDefaultWorker()},
                {"场地色块", new ThSitePlanDefaultWorker()},
                {"道路线稿", new ThSitePlanDefaultWorker()},
                {"道路色块", new ThSitePlanDefaultWorker()},
            };
        }
        public override bool Generate(Database database, ThSitePlanConfigItem configItem)
        {
            var options = new ThSitePlanOptions()
            {
                Options = new Dictionary<string, object>()
                {
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
