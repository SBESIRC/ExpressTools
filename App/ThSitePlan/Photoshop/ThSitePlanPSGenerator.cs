using ThSitePlan.Configuration;

namespace ThSitePlan.Photoshop
{
    public abstract class ThSitePlanPSGenerator
    {
        public abstract bool Generate(string path, ThSitePlanConfigItem configItem);
    }
}
