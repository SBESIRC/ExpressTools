using ThSitePlan.Configuration;
using PsApplication = Photoshop.Application;

namespace ThSitePlan.Photoshop
{
    public abstract class ThSitePlanPSGenerator
    {
        public abstract PsApplication PsAppInstance { get; set; }
        public abstract bool Generate(string path, ThSitePlanConfigItem configItem);
    }
}
