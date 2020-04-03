using System.Collections.Generic;
using ThSitePlan.Configuration;

namespace ThSitePlan.Photoshop
{
    public class ThSitePlanPSDefaultGenerator : ThSitePlanPSGenerator
    {
        private Dictionary<string, ThSitePlanPSWorker> Workers { get; set; }
        public ThSitePlanPSDefaultGenerator()
        {
            Workers = new Dictionary<string, ThSitePlanPSWorker>()
            {
                {"基本文字说明及图例", new ThSitePlanPSDefaultWorker()},
                {"场地标高", new ThSitePlanPSDefaultWorker()},
                {"尺寸标注", new ThSitePlanPSDefaultWorker()},
                 {"界线", new ThSitePlanPSDefaultWorker()},
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
    }
}
