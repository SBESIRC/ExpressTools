using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThColumnInfo
{
    /// <summary>
    /// 目前支持带弧的Polyline
    /// </summary>
    public class CurveAnyInsidePoint
    {
        public bool isFinded = false;
        private Point3d insidePt  = Point3d.Origin;
        private Curve curve;
        public Point3d InsidePt => insidePt;
        public CurveAnyInsidePoint(Curve curve)
        {
            this.curve = curve;
        }
        public void FindInSidePt()
        {
            if(this.curve==null || this.curve.IsDisposed)
            {
                return;
            }
            if (this.curve is Circle circle)
            {
                FindCircleCenPt(circle);
            }
            else if(this.curve is Polyline polyline)
            {
                FindPolylineInSidePt(polyline);
            }
            else if (this.curve is Polyline2d polyline2d)
            {
                FindPolyline2dInSidePt(polyline2d);
            }
        }
        private void FindCircleCenPt(Circle circle)
        {
            this.insidePt = circle.Center;
            this.isFinded = true;
        }
        private void FindPolylineInSidePt(Polyline polyline)
        {
            if(!polyline.Closed)
            {
                return;
            }
            JudgePtInCurve(polyline);
        }
        private void FindPolyline2dInSidePt(Polyline2d polyline2d)
        {
            if (!polyline2d.Closed)
            {
                return;
            }
            JudgePtInCurve(polyline2d);
        }
        private void JudgePtInCurve(Curve curve)
        {
            List<Point3d> pts = ThColumnInfoUtils.GetPolylinePts(curve);
            Point3dCollection ptCol = new Point3dCollection();
            pts.ForEach(i => ptCol.Add(i));
            double minL = double.MaxValue;
            for(int i=0;i< pts.Count-1;i++)
            {
                double dis = pts[i].DistanceTo(pts[i + 1]);
                if (dis > 0 && dis < minL)
                {
                    minL = dis;
                }
            }
            minL *= 0.2;
            int m = 0;
            int n = 0;
            foreach (Point3d pt in pts)
            {
                for(int i=1;i<=4;i++)
                {
                    switch (i)
                    {
                        case 1:
                            m = -1;
                            n = -1;
                            break;
                        case 2:
                            m = 1;
                            n = -1;
                            break;
                        case 3:
                            m = 1;
                            n = 1;
                            break;
                        case 4:
                            m = -1;
                            n = 1;
                            break;
                    }
                    Point3d tempPt = pt + new Vector3d(m * minL,n* minL,0.0);
                    this.isFinded = ThColumnInfoUtils.IsPointInPolyline(ptCol, tempPt);
                    if(this.isFinded)
                    {
                        this.insidePt = tempPt;
                        break;
                    }
                }
                if (this.isFinded)
                {
                    break;
                }
            }
        }
    }
}
