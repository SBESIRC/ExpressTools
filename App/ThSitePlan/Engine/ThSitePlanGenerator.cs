using System;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using ThSitePlan.Configuration;

namespace ThSitePlan
{
    public abstract class ThSitePlanGenerator
    {
        public abstract ObjectId OriginFrame { get; set; }
        public abstract Tuple<ObjectId, Vector3d> Frame { get; set; }
        public abstract bool Generate(Database database, ThSitePlanConfigItem configItem);
    }
}
