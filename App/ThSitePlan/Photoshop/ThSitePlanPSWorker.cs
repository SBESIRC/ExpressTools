using ThSitePlan.Configuration;

namespace ThSitePlan.Photoshop
{
    public abstract class ThSitePlanPSWorker
    {
        public abstract bool DoProcess(string path, ThSitePlanConfigItem configItem);
        public abstract bool DoUpdate(string path, ThSitePlanConfigItem configItem);
    }
}
