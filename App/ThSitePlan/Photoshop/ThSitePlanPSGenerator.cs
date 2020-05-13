using ThSitePlan.Configuration;

namespace ThSitePlan.Photoshop
{
    public abstract class ThSitePlanPSGenerator
    {
        public abstract bool Update(string path, ThSitePlanConfigItem configItem);
        public abstract bool Generate(string path, ThSitePlanConfigItem configItem);
    }
}
