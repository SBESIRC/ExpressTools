using System;
using Linq2Acad;
using DotNetARX;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThWSS.Bussiness.SprayLayout
{
    public static class SprayLayoutDbExtension
    {
        public static List<SprayLayoutData> SprayFromDatabase(this Database database)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                throw new NotImplementedException();
            }
        }

        public static void AddSprayXData(this ObjectId obj, SprayLayoutData spray)
        {
            TypedValueList valulist = new TypedValueList();
            if (spray.Radii is Polyline polyline)
            {
                foreach (Point3d point in polyline.Vertices())
                {
                    valulist.Add((int)DxfCode.ExtendedDataWorldXCoordinate, point);
                }
            }
            obj.AddXData(ThWSSCommon.RegAppName_ThWSS_Spray, valulist);
        }

        public static void RemoveSprayXData(this ObjectId obj)
        {
            obj.RemoveXData(ThWSSCommon.RegAppName_ThWSS_Spray);
        }
    }
}