﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;

namespace ThSitePlan.Engine
{
    class ThSitePlanFrameNameGenerator:ThSitePlanGenerator
    {
        public override ObjectId OriginFrame { get; set; }
        public override Tuple<ObjectId, Vector3d> Frame { get; set; }
        private Dictionary<string, ThSitePlanWorker> Workers { get; set; }

        public ThSitePlanFrameNameGenerator()
        {
            Workers = new Dictionary<string, ThSitePlanWorker>()
            {
                {"基本文字说明及图例", new ThSitePlanAddNameTextWorker()},
                {"场地标高", new ThSitePlanAddNameTextWorker()},
                {"尺寸标注", new ThSitePlanAddNameTextWorker()},
                {"界线", new ThSitePlanAddNameTextWorker()},
                {"原始场地叠加线稿", new ThSitePlanAddNameTextWorker()},

                {"建筑物-场地内建筑-建筑信息", new ThSitePlanAddNameTextWorker()},
                {"建筑物-场地内建筑-建筑线稿", new ThSitePlanAddNameTextWorker()},
                {"建筑物-场地外建筑-建筑信息", new ThSitePlanAddNameTextWorker()},
                {"建筑物-场地外建筑-建筑线稿", new ThSitePlanAddNameTextWorker()},
                {"建筑物-场地外建筑-建筑色块", new ThSitePlanAddNameTextWorker()},
                {"建筑物-场地内建筑-建筑色块", new ThSitePlanAddNameTextWorker()},

                {"全局阴影", new ThSitePlanAddNameTextWorker()},

                {"树木-景观树-树木线稿", new ThSitePlanAddNameTextWorker()},
                {"树木-行道树-树木线稿", new ThSitePlanAddNameTextWorker()},
                {"树木-景观树-树木色块", new ThSitePlanAddNameTextWorker()},
                {"树木-行道树-树木色块", new ThSitePlanAddNameTextWorker()},

                {"场地-消防登高场地-场地线稿", new ThSitePlanAddNameTextWorker()},
                {"场地-停车场地-场地线稿", new ThSitePlanAddNameTextWorker()},
                {"场地-活动场地-场地线稿", new ThSitePlanAddNameTextWorker()},
                {"场地-其他场地-场地线稿", new ThSitePlanAddNameTextWorker()},
                {"场地-消防登高场地-场地色块", new ThSitePlanAddNameTextWorker()},
                {"场地-停车场地-场地色块", new ThSitePlanAddNameTextWorker()},
                {"场地-活动场地-场地色块", new ThSitePlanAddNameTextWorker()},
                {"场地-其他场地-场地色块", new ThSitePlanAddNameTextWorker()},

                {"道路-内部车行道路-道路线稿", new ThSitePlanAddNameTextWorker()},
                {"道路-内部人行道路-道路线稿", new ThSitePlanAddNameTextWorker()},
                {"道路-外部车行道路-道路线稿", new ThSitePlanAddNameTextWorker()},
                {"道路-外部景观道路-道路线稿", new ThSitePlanAddNameTextWorker()},
                {"道路-内部车行道路-道路色块", new ThSitePlanAddNameTextWorker()},
                {"道路-内部人行道路-道路色块", new ThSitePlanAddNameTextWorker()},
                {"道路-外部车行道路-道路色块", new ThSitePlanAddNameTextWorker()},
                {"道路-外部景观道路-道路色块", new ThSitePlanAddNameTextWorker()},

                {"铺装-场地外铺地-铺装线稿", new ThSitePlanAddNameTextWorker()},
                {"铺装-场地外铺地-铺装色块", new ThSitePlanAddNameTextWorker()},

                {"景观绿地-水景-水景线稿", new ThSitePlanAddNameTextWorker()},
                {"景观绿地-水景-水景色块", new ThSitePlanAddNameTextWorker()},
                {"景观绿地-景观-景观线稿", new ThSitePlanAddNameTextWorker()},
                {"景观绿地-景观-景观色块", new ThSitePlanAddNameTextWorker()},
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