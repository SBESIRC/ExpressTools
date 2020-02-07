using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using Linq2Acad;
using DotNetARX;

namespace ThEssential.Equipment
{
    public static class ThEquipmentDbExtension
    {
        public static Point3d Position(this Database database, ObjectId objId)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var obj = acadDatabase.Element<BlockReference>(objId);
                return obj.Position;
            }
        }

        public static string Model(this Database database, ObjectId objId)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var obj = acadDatabase.Element<DBText>(objId);
                return obj.TextString;
            }
        }

        public static ObjectIdCollection Model(this Database database, ThAnchorPoint point)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                var objIds = new ObjectIdCollection();
                var hatches = acadDatabase.ModelSpace
                    .OfType<Hatch>()
                    .Where(o => o.ContainsPoint(point.Position));
                foreach(var hatch in hatches)
                {
                    acadDatabase.ModelSpace
                        .OfType<DBText>()
                        .Where(o => hatch.ContainsPoint(o.Position))
                        .ForEachDbObject(o => objIds.Add(o.ObjectId));
                }
                return objIds;
            }
        }

        public static ThEquipmentCoordinateSystem EquipmentCoordinateSystem(this Database database)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(database))
            {
                // 提取图纸中设备坐标系统的控制点
                var anchorPoints = new List<ThAnchorPoint>();
                var blockReferences = acadDatabase.ModelSpace
                    .OfType<BlockReference>()
                    .Where(o => o.Name == "控制点");
                foreach (var item in blockReferences)
                {
                    var objId = item.ObjectId;
                    var attributes = objId.GetAttributesInBlockReference();
                    anchorPoints.Add(new ThAnchorPoint()
                    {
                        Position = acadDatabase.Database.Position(objId),
                        Flow = Convert.ToDouble(attributes.Where(o => o.Key == "Q").First().Value),
                        Lift = Convert.ToDouble(attributes.Where(o => o.Key == "H").First().Value)
                    });
                }

                // 计算设备坐标系统
                var coordinateSystem = new ThEquipmentCoordinateSystem();

                // X方向
                anchorPoints.Sort((o1, o2) => o1.Flow.CompareTo(o2.Flow));
                coordinateSystem.Xaxis = anchorPoints.Last();

                // Y方向
                anchorPoints.Sort((o1, o2) => o1.Lift.CompareTo(o2.Lift));
                coordinateSystem.Yaxis = anchorPoints.Last();

                // 原点
                anchorPoints.Remove(coordinateSystem.Xaxis);
                anchorPoints.Remove(coordinateSystem.Yaxis);
                coordinateSystem.Origin = anchorPoints.Last();

                // 返回设备坐标系统
                return coordinateSystem;
            }
        }
    }
}
