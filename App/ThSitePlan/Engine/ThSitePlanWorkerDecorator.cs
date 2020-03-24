using System;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;

namespace ThSitePlan.Engine
{
    public class ThSitePlanWorkerDecorator : ThSitePlanWorker
    {
        protected ThSitePlanWorker Worker { get; set; }
        public ThSitePlanWorkerDecorator(ThSitePlanWorker worker)
        {
            Worker = worker;
        }

        public override ObjectIdCollection Filter(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            return Worker.Filter(database, configItem, options);
        }
    }
}
