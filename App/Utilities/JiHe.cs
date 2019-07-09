using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Interop.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TianHua.AutoCAD.Utility.ExtensionTools
{
    public static class JiHe
    {
        /// <summary>
        /// 获取图形的中心位置(忽略3D位置，2D点)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ent"></param>
        /// <returns></returns>
        public static Point2d GetCenter<T>(this T ent) where T : Entity
        {
            return new Point2d((ent.GeometricExtents.MinPoint.X + ent.GeometricExtents.MaxPoint.X) / 2, (ent.GeometricExtents.MinPoint.Y + ent.GeometricExtents.MaxPoint.Y) / 2);
        }

        /// <summary>
        /// 获取图形的中心位置(3D点)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ent"></param>
        /// <returns></returns>
        public static Point3d Get3DCenter<T>(this T ent) where T : Entity
        {
            return new Point3d((ent.GeometricExtents.MinPoint.X + ent.GeometricExtents.MaxPoint.X) / 2, (ent.GeometricExtents.MinPoint.Y + ent.GeometricExtents.MaxPoint.Y) / 2, (ent.GeometricExtents.MinPoint.Z + ent.GeometricExtents.MaxPoint.Z) / 2);
        }

        public static Point3d toPoint3d(this Point2d point2d)
        {
            return new Point3d(point2d.X, point2d.Y, 0);
        }

        public static Point2d toPoint2d(this Point3d point3d)
        {
            return new Point2d(point3d.X, point3d.Y);
        }

        /// <summary>
        /// 获取任意实体的所有端点和夹点
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ent"></param>
        /// <returns></returns>
        public static Point3dCollection GetAllGripPoints<T>(this T ent) where T : Entity
        {
            Point3dCollection pts = new Point3dCollection();
            IntegerCollection inters = new IntegerCollection();

            ent.GetGripPoints(pts, inters, inters);
            return pts;
        }
    }
}
