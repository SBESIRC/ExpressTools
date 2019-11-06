using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;

namespace ThColumnInfo
{
    /// <summary>
    /// 关联柱
    /// </summary>
    public class ThRelateColumn
    {
        private List<ColumnInf> localColumnInfs = new List<ColumnInf>();
        private List<ColumnRelateInf> columnRelateInfs = new List<ColumnRelateInf>();
        public List<ColumnRelateInf> ColumnRelateInfs
        {
            get
            {
                return this.columnRelateInfs;
            }
        }
        public ThRelateColumn(List<ColumnInf> localColumnInfs, List<ColumnRelateInf> columnRelateInfs)
        {
            this.localColumnInfs = localColumnInfs;
            this.columnRelateInfs = columnRelateInfs;
        }
        public void Relate()
        {
            if(this.localColumnInfs.Count==0 || this.columnRelateInfs.Count==0)
            {
                return;
            }
            for (int i = 0; i < this.columnRelateInfs.Count; i++)
            {
                if (this.columnRelateInfs[i].InModelPts == null || this.columnRelateInfs[i].InModelPts.Count == 0)
                {
                    continue;
                }
                List<ColumnInf> columnInfs = this.localColumnInfs.Where(j => 
                CheckTwoPolylineIntersect(this.columnRelateInfs[i].InModelPts,j.Points)).Select(j => j).ToList();//获取重叠柱子
                this.columnRelateInfs[i].ModelColumnInfs = columnInfs;
            }
        }
        private Polyline CreateZeroPolyline(List<Point3d> pts)
        {
            Polyline polyline = new Polyline();
            if(pts==null || pts.Count==0)
            {
                return polyline;
            }
            for (int i = 0; i < pts.Count; i++)
            {
                polyline.AddVertexAt(i, new Autodesk.AutoCAD.Geometry.Point2d(pts[i].X, pts[i].Y), 0, 0, 0);
            }
            polyline.Closed = true;
            return polyline;
        }
        /// <summary>
        /// 检查两个Polyline是否相交
        /// </summary>
        /// <param name="pts1"></param>
        /// <param name="pts2"></param>
        /// <returns></returns>
        private bool CheckTwoPolylineIntersect(List<Point3d> pts1, List<Point3d> pts2)
        {
            bool isOverLap = false;
            if(pts1==null || pts2==null || pts1.Count==0 || pts2.Count==0)
            {
                return isOverLap;
            }
            Polyline polyline1 = CreateZeroPolyline(pts1);
            Polyline polyline2 = CreateZeroPolyline(pts2);
            Point3dCollection intersectPts = new Point3dCollection();
            polyline1.IntersectWith(polyline2, Intersect.OnBothOperands, intersectPts, IntPtr.Zero, IntPtr.Zero);
            if(intersectPts==null || intersectPts.Count==0)
            {
                Point2dCollection pts1Col = new Point2dCollection();
                Point2dCollection pts2Col = new Point2dCollection();
                pts1.ForEach(i => pts1Col.Add(new Point2d(i.X, i.Y)));
                pts2.ForEach(i => pts2Col.Add(new Point2d(i.X, i.Y)));
                if (ThColumnInfoUtils.IsPointInPolyline(pts2Col, new Point2d(pts1[0].X,pts1[0].Y)) ||
                    ThColumnInfoUtils.IsPointInPolyline(pts1Col, new Point2d(pts2[0].X, pts2[0].Y)))
                {
                    isOverLap = true;
                }
            }
            else
            {
                isOverLap = true;
            }
            return isOverLap;
        }
    }
}
