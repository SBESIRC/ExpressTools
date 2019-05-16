using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace TianHua.AutoCAD.Parking
{
    public class PolylineRec : Polyline
    {
        public Point3d Center { get; set; }//中心位置

        public PolylineRec(Point2d pt1, Point2d pt2) : base()
        {
            CreateRectangle(ref pt1, ref pt2);
            this.Center = this.GetCenter().toPoint3d();
        }

        /// <summary>
        /// 创建封闭矩形
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
public PolylineRec(Point2d pt1, Point2d pt2, string layerName, int index) : base()
        {
            CreateRectangle(ref pt1, ref pt2);
            this.Center = this.GetCenter().toPoint3d();
            this.Layer = layerName;
            this.ColorIndex = index;
        }

        /// <summary>
        /// 按中心位置和矩形大小创建封闭矩形
        /// </summary>
        /// <param name="pointCenter"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="layerName"></param>
        /// <param name="index"></param>
        public PolylineRec(Point2d pointCenter, double x, double y, string layerName, int index)
        {
            var pt1 = new Point2d(pointCenter.X - x / 2, pointCenter.Y + y / 2);
            var pt2 = new Point2d(pointCenter.X + x / 2, pointCenter.Y - y / 2);

            CreateRectangle(ref pt1, ref pt2);
            this.Center = this.GetCenter().toPoint3d();
            this.Layer = layerName;
            this.ColorIndex = index;
        }

        /// <summary>
        /// 通过二维点集合创建多段线
        /// </summary>
        /// <param name="pline">多段线对象</param>
        /// <param name="pts">多段线的顶点</param>
        private void CreatePolyline(Point2dCollection pts)
        {
            for (int i = 0; i < pts.Count; i++)
            {
                //添加多段线的顶点
                base.AddVertexAt(i, pts[i], 0, 0, 0);
            }
        }

        /// <summary>
        /// 通过不固定的点创建多段线
        /// </summary>
        /// <param name="pline">多段线对象</param>
        /// <param name="pts">多段线的顶点</param>
        private void CreatePolyline(params Point2d[] pts)
        {
            CreatePolyline(new Point2dCollection(pts));
        }

        /// <summary>
        /// 创建矩形
        /// </summary>
        /// <param name="pline">多段线对象</param>
        /// <param name="pt1">矩形的角点</param>
        /// <param name="pt2">矩形的角点</param>
        private void CreateRectangle(ref Point2d pt1, ref Point2d pt2)
        {
            //设置矩形的4个顶点
            double minX = Math.Min(pt1.X, pt2.X);
            double maxX = Math.Max(pt1.X, pt2.X);
            double minY = Math.Min(pt1.Y, pt2.Y);
            double maxY = Math.Max(pt1.Y, pt2.Y);
            Point2dCollection pts = new Point2dCollection();
            pts.Add(new Point2d(minX, minY));
            pts.Add(new Point2d(minX, maxY));
            pts.Add(new Point2d(maxX, maxY));
            pts.Add(new Point2d(maxX, minY));
            this.CreatePolyline(pts);
            this.Closed = true;//闭合多段线以形成矩形
        }



    }
}
