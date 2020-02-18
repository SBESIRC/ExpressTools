using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using AcHelper;
using Linq2Acad;
using GeometryExtensions;
using ThEssential.Align;

namespace ThEssential.Distribute
{
    public static class ThDistributeObjectIdCollectionExtension
    {
        public static void Distribute(this ObjectIdCollection collection, DistributeMode mode)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                double width = 0,height = 0;
                var extents = new Extents3d();
                var extentsDict = new Dictionary<ObjectId, Extents3d>();
                foreach (ObjectId objId in collection)
                {
                    var obj = acadDatabase.Element<Entity>(objId, false);
                    var objExtents = obj.GeometricExtentsEx();
                    width += objExtents.Width();
                    height += objExtents.Height();
                    extents.AddExtents(objExtents);
                    extentsDict.Add(objId, objExtents);
                }

                if (mode == DistributeMode.XGap)
                {
                    double gap = extents.Width() - width;
                    double distribute = gap / (extentsDict.Count - 1);
                    var extentsList = extentsDict.ToList();
                    extentsList.Sort((o1, o2) => o1.Value.CenterPoint().X.CompareTo(o2.Value.CenterPoint().X));
                    double distance = extentsList.ElementAt(0).Value.Width();
                    for (int i = 1; i < extentsList.Count; i++)
                    {
                        // Extents3d原X方向的坐标点
                        double x = extentsList.ElementAt(i).Value.MinPoint.X;
                        // 调整分布后X方向的坐标点
                        double x1 = extentsList.ElementAt(0).Value.MinPoint.X + distance + distribute;
                        // 调整分布的偏移量
                        var displacement = new Vector3d(x1 - x, 0, 0);
                        var wcsDisplacement = displacement.TransformBy(Active.Editor.UCS2WCS());
                        // 根据偏移量移动实体
                        var obj = acadDatabase.Element<Entity>(extentsList.ElementAt(i).Key, true);
                        obj.TransformBy(Matrix3d.Displacement(wcsDisplacement));
                        // 累计已经处理的Extents3d的总宽度
                        distance += extentsList.ElementAt(i).Value.Width() + distribute;
                    }
                }
                else if (mode == DistributeMode.YGap)
                {
                    double gap = extents.Height() - height;
                    double distribute = gap / (extentsDict.Count - 1);
                    var extentsList = extentsDict.ToList();
                    extentsList.Sort((o1, o2) => o1.Value.CenterPoint().Y.CompareTo(o2.Value.CenterPoint().Y));
                    double distance = extentsList.ElementAt(0).Value.Height();
                    for (int i = 1; i < extentsList.Count; i++)
                    {
                        // Extents3d原Y方向的坐标点
                        double y = extentsList.ElementAt(i).Value.MinPoint.Y;
                        // 调整分布后Y方向的坐标点
                        double y1 = extentsList.ElementAt(0).Value.MinPoint.Y + distance + distribute;
                        // 调整分布的偏移量
                        var displacement = new Vector3d(0, y1 - y, 0);
                        var wcsDisplacement = displacement.TransformBy(Active.Editor.UCS2WCS());
                        // 根据偏移量移动实体
                        var obj = acadDatabase.Element<Entity>(extentsList.ElementAt(i).Key, true);
                        obj.TransformBy(Matrix3d.Displacement(wcsDisplacement));
                        // 累计已经处理的Extents3d的总高度
                        distance += extentsList.ElementAt(i).Value.Height() + distribute;
                    }
                }
            }
        }
    }
}
