using ThSitePlan.Configuration;
using PsApplication = Photoshop.Application;

namespace ThSitePlan.Photoshop
{
    public abstract class ThSitePlanPSWorker
    {
        public abstract PsApplication PsAppInstance { get; }
        public abstract bool DoProcess(string path, ThSitePlanConfigItem configItem);
        public abstract bool DoUpdate(string path, ThSitePlanConfigItem configItem);
    }
}
