using System;
using System.Collections.Generic;
using ThSitePlan.Configuration;
using PsApplication = Photoshop.Application;

namespace ThSitePlan.Photoshop
{
    public class ThSitePlanPSDefaultGenerator : ThSitePlanPSGenerator
    {
        public override PsApplication PsAppInstance { get; set; }
        private Dictionary<string, ThSitePlanPSWorker> Workers { get; set; }
        public ThSitePlanPSDefaultGenerator()
        {
            Workers = new Dictionary<string, ThSitePlanPSWorker>()
            {
                {"基本文字说明及图例", new ThSitePlanPSDefaultWorker()},
                //{"建筑物-高层建筑-建筑色块", new ThSitePlanPSDefaultWorker()},
                //{"建筑物-高层建筑-建筑线稿", new ThSitePlanPSDefaultWorker()},
                //{"建筑物-高层建筑-建筑信息", new ThSitePlanPSDefaultWorker()},
                //{"建筑物-场地外建筑-原始场地线稿", new ThSitePlanPSDefaultWorker()},
            };
        }

        public override bool Generate(string path, ThSitePlanConfigItem configItem)
        {
            var key = (string)configItem.Properties["Name"];
            if (Workers.ContainsKey(key))
            {
                Workers[key].PsAppInstance = PsAppInstance;
                Workers[key].DoProcess(path, configItem);
            }
            return true;
        }
    }
}
