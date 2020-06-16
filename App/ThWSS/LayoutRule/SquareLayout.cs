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
using ThWSS.Utlis;

namespace ThWSS.LayoutRule
{
    public class SquareLayout : LayoutInterface
    {
        double sideLength = 3600;
        double sideMinLength = 0;
        double maxLength = 1800;
        double minLength = 100;
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
            var layoutP = LayoutPoints(roomLines, longLine.StartPoint, vDir, tDir, longLine.Length, shortLine.Length);
            //layoutP.AddRange(AdjustPoints(layoutP.SelectMany(x => x).ToList(), roomLines, longLine.StartPoint, tDir, vDir, shortLine.Length));

            return layoutP;
        }

        /// <summary>
        /// 按正方形保护排布点
        /// </summary>
        /// <param name="roomLines"></param>
        /// <param name="pt"></param>
        /// <param name="transverseDir"></param>
        /// <param name="verticalDir"></param>
        /// <param name="length"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        private List<List<Point3d>> LayoutPoints(List<Line> roomLines, Point3d pt, Vector3d transverseDir, Vector3d verticalDir, double length, double width)
        {
            //竖向排布条件
            CalLayoutWay(length, out double tRemainder, out double tNum, out double tMoveLength);
            //横向排布条件
            CalLayoutWay(width, out double vRemainder, out double vNum, out double vMoveLength);

            List<List<Point3d>> allP = new List<List<Point3d>>();
            pt = pt + tRemainder * transverseDir + vRemainder * verticalDir;
            for (int i = 0; i <= tNum; i++)
            {
                List<Point3d> p = new List<Point3d>();
                Point3d tsp = pt;
                for (int j = 0; j <= vNum; j++)
                {
                    p.Add(tsp);
                    tsp = tsp + vMoveLength * verticalDir;
                }
                allP.Add(p);
                pt = pt + tMoveLength * transverseDir;
            }
            return allP;
        }

        #region 暂不用
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
                List<Point3d> points = GeUtils.GetRayIntersectPoints(roomLines, pt, verticalDir);
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
                List<Point3d> points = GeUtils.GetRayIntersectPoints(roomLines, pt, transverseDir);
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
        #endregion
        
        /// <summary>
        /// 计算排布规则(边界距离,步长等)
        /// </summary>
        /// <param name="length"></param>
        /// <param name="remainder"></param>
        /// <param name="num"></param>
        /// <param name="moveLength"></param>
        private void CalLayoutWay(double length, out double remainder, out double num, out double moveLength)
        {
            num = Math.Floor(length / sideLength);
            if (num >= 1)
            {
                while (true)
                {
                    //让边距尽量等于0.5倍间距
                    moveLength = length / (num + 1);
                    //间距是50的倍数
                    moveLength = Math.Floor(moveLength / 50) * 50;
                    remainder = (length - moveLength * num) / 2;
                    if (remainder > maxLength)
                    {
                        num += 1;
                    }
                    else 
                    {
                        break;
                    }
                }
            }
            else
            {
                remainder = length / 2;
                moveLength = 0;
            }
        }
    }
}
