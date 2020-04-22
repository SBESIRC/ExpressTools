using System.Collections.Generic;
using ThSitePlan.Configuration;

namespace ThSitePlan.Photoshop
{
    public class ThSitePlanPSDefaultGenerator : ThSitePlanPSGenerator
    {
        private Dictionary<string, ThSitePlanPSWorker> Workers { get; set; }
        public ThSitePlanPSDefaultGenerator(ThSitePlanPSService psService)
        {
            Workers = new Dictionary<string, ThSitePlanPSWorker>()
            {
                {"基本文字说明及图例", new ThSitePlanPSDefaultWorker(psService)},
                {"场地标高", new ThSitePlanPSDefaultWorker(psService)},
                {"尺寸标注", new ThSitePlanPSDefaultWorker(psService)},
                {"界线", new ThSitePlanPSDefaultWorker(psService)},
                {"原始场地叠加线稿", new ThSitePlanPSDefaultWorker(psService)},

                {"建筑物-场地内建筑-建筑信息", new ThSitePlanPSDefaultWorker(psService)},
                {"建筑物-场地内建筑-建筑线稿", new ThSitePlanPSDefaultWorker(psService)},
                {"建筑物-场地外建筑-建筑信息", new ThSitePlanPSDefaultWorker(psService)},
                {"建筑物-场地外建筑-建筑线稿", new ThSitePlanPSDefaultWorker(psService)},
                {"建筑物-场地外建筑-建筑色块", new ThSitePlanPSDefaultWorker(psService)},
                {"建筑物-场地内建筑-建筑色块", new ThSitePlanPSDefaultWorker(psService)},

                {"树木-景观树-树木线稿", new ThSitePlanPSDefaultWorker(psService)},
                {"树木-行道树-树木线稿", new ThSitePlanPSDefaultWorker(psService)},
                {"树木-景观树-树木色块", new ThSitePlanPSDefaultWorker(psService)},
                {"树木-行道树-树木色块", new ThSitePlanPSDefaultWorker(psService)},

                {"场地-消防登高场地-场地线稿", new ThSitePlanPSDefaultWorker(psService)},
                {"场地-停车场地-场地线稿", new ThSitePlanPSDefaultWorker(psService)},
                {"场地-活动场地-场地线稿", new ThSitePlanPSDefaultWorker(psService)},
                {"场地-其他场地-场地线稿", new ThSitePlanPSDefaultWorker(psService)},
                {"场地-消防登高场地-场地色块", new ThSitePlanPSDefaultWorker(psService)},
                {"场地-停车场地-场地色块", new ThSitePlanPSDefaultWorker(psService)},
                {"场地-活动场地-场地色块", new ThSitePlanPSDefaultWorker(psService)},
                {"场地-其他场地-场地色块", new ThSitePlanPSDefaultWorker(psService)},

                {"道路-内部车行道路-道路线稿", new ThSitePlanPSDefaultWorker(psService)},
                {"道路-内部人行道路-道路线稿", new ThSitePlanPSDefaultWorker(psService)},
                {"道路-外部车行道路-道路线稿", new ThSitePlanPSDefaultWorker(psService)},
                {"道路-外部景观道路-道路线稿", new ThSitePlanPSDefaultWorker(psService)},
                {"道路-内部车行道路-道路色块", new ThSitePlanPSDefaultWorker(psService)},
                {"道路-内部人行道路-道路色块", new ThSitePlanPSDefaultWorker(psService)},
                {"道路-外部车行道路-道路色块", new ThSitePlanPSDefaultWorker(psService)},
                {"道路-外部景观道路-道路色块", new ThSitePlanPSDefaultWorker(psService)},

                {"铺装-场地外铺地-铺装线稿", new ThSitePlanPSDefaultWorker(psService)},
                {"铺装-场地外铺地-铺装色块", new ThSitePlanPSDefaultWorker(psService)},

                {"景观绿地-水景-水景线稿", new ThSitePlanPSDefaultWorker(psService)},
                {"景观绿地-水景-水景色块", new ThSitePlanPSDefaultWorker(psService)},
                {"景观绿地-景观-景观线稿", new ThSitePlanPSDefaultWorker(psService)},
                {"景观绿地-景观-景观色块", new ThSitePlanPSDefaultWorker(psService)},

            };
        }

        public override bool Generate(string path, ThSitePlanConfigItem configItem)
        {
            var key = (string)configItem.Properties["Name"];
            if (Workers.ContainsKey(key))
            {
                Workers[key].DoProcess(path, configItem);
            }
            return true;
        }

        public override bool Update(string path, ThSitePlanConfigItem configItem)
        {
            var key = (string)configItem.Properties["Name"];
            if (Workers.ContainsKey(key))
            {
                Workers[key].DoUpdate(path, configItem);
            }
            return true;

        }
    }
}
