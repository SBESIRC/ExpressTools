using ThSitePlan.Configuration;

namespace ThSitePlan.Photoshop
{
    public class ThSitePlanPSDefaultGenerator : ThSitePlanPSGenerator
    {
        public ThSitePlanPSService PSService { get; set; }
        public ThSitePlanPSDefaultGenerator(ThSitePlanPSService psService)
        {
            PSService = psService;
        }

        public override bool Generate(string path, ThSitePlanConfigItem configItem)
        {
            var worker = new ThSitePlanPSDefaultWorker(PSService);
            return worker.DoProcess(path, configItem);
        }

        public override bool Update(string path, ThSitePlanConfigItem configItem)
        {
            var worker = new ThSitePlanPSDefaultWorker(PSService);
            return worker.DoUpdate(path, configItem);
        }

        public override bool Clean(ThSitePlanConfigItem configItem)
        {
            var worker = new ThSitePlanPSDefaultWorker(PSService);
            return worker.CleanInPS(configItem);
        }
    }
}
