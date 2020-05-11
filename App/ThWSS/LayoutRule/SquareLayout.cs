using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Linq2Acad;

namespace ThWSS.LayoutRule
{
    public class SquareLayout : LayoutInterface
    {
        public double sideLength = 4400;
        public double maxLength = 2200;
        public double minLength = 500;

        public List<List<Point3d>> Layout(Polyline polyline)
        {
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
            var layoutP = LayoutPoints(longLine.StartPoint, tDir, vDir, shortLine.Length, longLine.Length);

            return layoutP;
        }

        /// <summary>
        /// 按正方形保护排布点
        /// </summary>
        /// <param name="sP"></param>
        /// <param name="transverseDir"></param>
        /// <param name="verticalDir"></param>
        /// <param name="width"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private List<List<Point3d>> LayoutPoints(Point3d sP, Vector3d transverseDir, Vector3d verticalDir, double width, double length)
        {
            //横向排布条件
            double tRemainder = width % sideLength / 2;
            double tNum = Math.Floor(width / sideLength);
            if (tRemainder < minLength)
            {
                tRemainder = minLength;
            }
            else if (tRemainder > maxLength)
            {
                tRemainder = maxLength;
                tNum += 1;
            }
            double moveLength = (width - tRemainder * 2) / tNum;

            //竖向排布条件
            double vRemainder = length % sideLength / 2;
            double vNum = Math.Floor(length / sideLength);
            if (vRemainder < minLength)
            {
                vRemainder = minLength;
            }
            else if (vRemainder > maxLength)
            {
                vRemainder = maxLength;
                vNum += 1;
            }
            double vMoveLength = (length - vRemainder * 2) / vNum;

            //排布点
            List<List<Point3d>> allP = new List<List<Point3d>>();
            sP = sP + tRemainder * transverseDir + vRemainder * verticalDir;
            for (int i = 0; i <= vNum; i++)
            {
                List<Point3d> p = new List<Point3d>();
                Point3d tsp = sP;
                for (int j = 0; j <=tNum; j++)
                {
                    p.Add(tsp);
                    tsp = tsp + moveLength * transverseDir;
                }
                allP.Add(p);
                sP = sP + vMoveLength * verticalDir;
            }

            return allP;
        }
    }
}
