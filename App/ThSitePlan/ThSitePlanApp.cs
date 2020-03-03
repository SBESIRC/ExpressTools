using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Linq2Acad;

namespace ThSitePlan
{
    public class ThSitePlanApp : IExtensionApplication
    {
        public void Initialize()
        {
            //
        }

        public void Terminate()
        {
            //
        }

        [CommandMethod("TIANHUACAD", "THSITEPLAN", CommandFlags.Modal)]
        public void ThSitePlan()
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var objs = acadDatabase.Database.SelectAll("P-Land-TREE");
                var displacement = Matrix3d.Displacement(new Vector3d(3000, 0, 0));
                acadDatabase.Database.CopyWithMove(objs, displacement);
            }
        }
    }
}
