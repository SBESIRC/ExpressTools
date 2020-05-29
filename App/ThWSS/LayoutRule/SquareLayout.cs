using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Linq2Acad;
using ThWss.View;
using ThWSS.Model;

namespace ThWSS.LayoutRule
{
    public class SquareLayout : LayoutInterface
    {
        double sideLength = 4400;
        double sideMinLength = 0;
        double maxLength = 2200;
        double minLength = 500;
        public SquareLayout(SparyLayoutModel layoutModel)
        {

        }

        public List<List<Point3d>> Layout(Polyline room, Polyline polyline)
        {
            //房间线
            List<Line> roomLines = new List<Line>();
            for (int i = 0; i < room.NumberOfVertices; i++)
            {
                var current = room.GetPoint2dAt(i);
                var next = room.GetPoint2dAt((i + 1) % room.NumberOfVertices);
                roomLines.Add(new Line(new Point3d(current.X, current.Y, 0), new Point3d(next.X, next.Y, 0)));
            }

            //房间obb框线
            List<Line> lineLst = new List<Line>();
            for (int i = 0; i < polyline.NumberOfVertices; i++)
            {
                var current = polyline.GetPoint2dAt(i);
                var next = polyline.GetPoint2dAt((i + 1) % polyline.NumberOfVertices);
                lineLst.Add(new Line(new Point3d(current.X, current.Y, 0), new Point3d(next.X, next.Y, 0)));
            }

            Line longLine = lineLst.OrderByDescending(x => x.Length).First();
            Line shortLine = lineLst.Where(x => !x.Delta.GetNormal().IsParallelTo(longLine.Delta.GetNormal(), Tolerance.Global)).First();
            double sDis = longLine.StartPoint.DistanceTo(shortLine.StartPoint);
            double eDis = longLine.StartPoint.DistanceTo(shortLine.EndPoint);

            Vector3d vDir = longLine.Delta.GetNormal();  //纵向方向
            Vector3d tDir = sDis < eDis ? shortLine.Delta.GetNormal() : -shortLine.Delta.GetNormal();   //横向方向
            var layoutP = LayoutPoints(roomLines, longLine.StartPoint, tDir, vDir, shortLine.Length);
            layoutP.AddRange(AdjustPoints(layoutP.SelectMany(x => x).ToList(), roomLines, longLine.StartPoint, tDir, vDir, longLine.Length));

            return layoutP;
        }

        /// <summary>
        /// 按正方形保护排布点
        /// </summary>
        /// <param name="roomLines"></param>
        /// <param name="pt"></param>
        /// <param name="transverseDir"></param>
        /// <param name="verticalDir"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        private List<List<Point3d>> LayoutPoints(List<Line> roomLines, Point3d pt, Vector3d transverseDir, Vector3d verticalDir, double width)
        {
            //横向排布条件
            CalLayoutWay(width, out double tRemainder, out double tNum, out double tMoveLength);

            List<List<Point3d>> allP = new List<List<Point3d>>();
            pt = pt + tRemainder * transverseDir;
            for (int i = 0; i <= tNum; i++)
            {
                List<Point3d> points = GetRayIntersectPoints(roomLines, pt, verticalDir);
                points = points.OrderBy(x => x.DistanceTo(pt)).ToList();
                while (points.Count > 0)
                {
                    Point3d sp = points.First();
                    points.Remove(sp);
                    if (points.Count <= 0)
                    {
                        break;
                    }
                    Point3d ep = points.First();
                    points.Remove(ep);
                    CalLayoutWay(sp.DistanceTo(ep), out double vRemainder, out double vNum, out double vMoveLength);

                    List<Point3d> p = new List<Point3d>();
                    Point3d tsp = sp + vRemainder * verticalDir;
                    for (int j = 0; j <= vNum; j++)
                    {
                        p.Add(tsp);
                        tsp = tsp + vMoveLength * verticalDir;
                    }
                    allP.Add(p);
                }
                pt = pt + tMoveLength * transverseDir;
            }
            return allP;
        }

        /// <summary>
        /// 换方向再次排布,补充点
        /// </summary>
        /// <param name="vPoints"></param>
        /// <param name="roomLines"></param>
        /// <param name="pt"></param>
        /// <param name="transverseDir"></param>
        /// <param name="verticalDir"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private List<List<Point3d>> AdjustPoints(List<Point3d> vPoints, List<Line> roomLines, Point3d pt, Vector3d transverseDir, Vector3d verticalDir, double length)
        {
            //竖向排布条件
            CalLayoutWay(length, out double vRemainder, out double vNum, out double vMoveLength);
            double dis = Math.Sqrt(maxLength * maxLength * 2);
            List<List<Point3d>> allP = new List<List<Point3d>>();
            pt = pt + vRemainder * verticalDir;
            for (int i = 0; i <= vNum; i++)
            {
                List<Point3d> points = GetRayIntersectPoints(roomLines, pt, transverseDir);
                points = points.OrderBy(x => x.DistanceTo(pt)).ToList();
                while (points.Count > 0)
                {
                    Point3d sp = points.First();
                    points.Remove(sp);
                    if (points.Count <= 0)
                    {
                        break;
                    }
                    Point3d ep = points.First();
                    points.Remove(ep);
                    CalLayoutWay(sp.DistanceTo(ep), out double tRemainder, out double tNum, out double tMoveLength);

                    List<Point3d> p = new List<Point3d>();
                    Point3d tsp = sp + tRemainder * transverseDir;
                    for (int j = 0; j <= tNum; j++)
                    {
                        if (vPoints.Where(x => x.DistanceTo(tsp) <= dis).Count() <= 0)
                        {
                            p.Add(tsp);
                        }
                        tsp = tsp + tMoveLength * transverseDir;
                    }
                    allP.Add(p);
                }
                pt = pt + vMoveLength * verticalDir;
            }

            return allP;
        }

        /// <summary>
        /// 获取射线交点
        /// </summary>
        /// <param name="roomLines"></param>
        /// <param name="sp"></param>
        /// <param name="transverseDir"></param>
        /// <returns></returns>
        private List<Point3d> GetRayIntersectPoints(List<Line> roomLines, Point3d sp, Vector3d dir)
        {
            Ray ray = new Ray();
            ray.BasePoint = sp;
            ray.UnitDir = dir;

            List<Point3d> allPoints = new List<Point3d>();
            var intersectLines = roomLines.Where(x => !x.Delta.GetNormal().IsParallelTo(dir, new Tolerance(0.0001, 0.0001))).ToList();
            foreach (var line in intersectLines)
            {
                Point3dCollection points = new Point3dCollection();
                line.IntersectWith(ray, Intersect.OnBothOperands, points, IntPtr.Zero, IntPtr.Zero);
                if (points.Count > 0)
                {
                    allPoints.Add(points[0]);
                }
            }

            return allPoints;
        }

        /// <summary>
        /// 计算排布规则(边界距离,步长等)
        /// </summary>
        /// <param name="length"></param>
        /// <param name="remainder"></param>
        /// <param name="num"></param>
        /// <param name="moveLength"></param>
        private void CalLayoutWay(double length, out double remainder, out double num, out double moveLength)
        {
            remainder = length % sideLength / 2;
            num = Math.Floor(length / sideLength);
            if (remainder < minLength)
            {
                remainder = minLength;
            }
            else if (remainder > maxLength)
            {
                remainder = maxLength;
                num += 1;
            }
            moveLength = (length - remainder * 2) / num;
        }
    }
}
