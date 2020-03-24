using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;

namespace ThSitePlan.Engine
{
    public class ThSitePlanCurveWorker : ThSitePlanWorkerDecorator
    {
        public ThSitePlanCurveWorker(ThSitePlanWorker worker):base(worker)
        {
        }

        public override ObjectIdCollection Filter(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            var filterObjs = new ObjectIdCollection();
            foreach(ObjectId obj in Worker.Filter(database, configItem, options))
            {
                if (obj.ObjectClass == RXClass.GetClass(typeof(Line)))
                {
                    filterObjs.Add(obj);
                }

                if (obj.ObjectClass == RXClass.GetClass(typeof(Polyline)))
                {
                    filterObjs.Add(obj);
                }

                if (obj.ObjectClass == RXClass.GetClass(typeof(Circle)))
                {
                    filterObjs.Add(obj);
                }

                if (obj.ObjectClass == RXClass.GetClass(typeof(Arc)))
                {
                    filterObjs.Add(obj);
                }

            }
            return filterObjs;
        }
    }
}
