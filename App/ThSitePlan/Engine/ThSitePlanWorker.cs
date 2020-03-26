using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;

namespace ThSitePlan.Engine
{
    public class ThSitePlanOptions
    {
        public Dictionary<string, object> Options { get; set; }
    }

    public abstract class ThSitePlanWorker
    {
        public abstract bool DoProcess(Database database, 
            ThSitePlanConfigItem configItem, 
            ThSitePlanOptions options);

        public abstract ObjectIdCollection Filter(Database database, 
            ThSitePlanConfigItem configItem, 
            ThSitePlanOptions options);
    }
}