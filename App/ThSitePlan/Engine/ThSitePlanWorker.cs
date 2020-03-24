using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;
using Linq2Acad;

namespace ThSitePlan.Engine
{
    public class ThSitePlanOptions
    {
        public Dictionary<string, object> Options { get; set; }
    }

    public abstract class ThSitePlanWorker
    {
        public virtual bool DoProcess(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                using (var objs = Filter(database, configItem, options))
                {
                    Vector3d offset = (Vector3d)options.Options["Offset"];
                    acadDatabase.Database.CopyWithMove(objs, Matrix3d.Displacement(offset));
                }
                return true;
            }
        }

        public abstract ObjectIdCollection Filter(Database database, ThSitePlanConfigItem configItem, ThSitePlanOptions options);
    }
}