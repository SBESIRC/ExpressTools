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
                {"基本文字说明及图例", new ThSitePlanMoveWorker()},
                {"场地标高", new ThSitePlanMoveWorker()},
                {"尺寸标注", new ThSitePlanMoveWorker()},
                {"界线", new ThSitePlanMoveWorker()},
                //{"原始场地线稿", new ThSitePlanMoveWorker()},
                //{"建筑信息", new ThSitePlanMoveWorker()},
                //{"建筑线稿", new ThSitePlanMoveWorker()},
                //{"建筑色块", new ThSitePlanCopyWorker()},
                //{"树木线稿", new ThSitePlanMoveWorker()},
                //{"树木色块", new ThSitePlanCopyWorker()},
                //{"场地线稿", new ThSitePlanMoveWorker()},
                //{"场地色块", new ThSitePlanMoveWorker()},
                //{"道路线稿", new ThSitePlanMoveWorker()},
                //{"道路色块", new ThSitePlanMoveWorker()},
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
