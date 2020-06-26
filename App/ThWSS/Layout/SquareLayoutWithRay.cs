using System;
using DotNetARX;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using ThWSS.Model;
using ThWSS.Utlis;
using ThWSS.Bussiness;


namespace ThWSS.Layout
{
    class SquareLayoutWithRay : SquareLayout
    {
        public SquareLayoutWithRay(SprayLayoutModel layoutModel) : base(layoutModel)
        {

        }
        public List<List<SprayLayoutData>> Layout(Polyline room, Polyline polyline)
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
            var layoutP = LayoutPoints(roomLines, longLine.StartPoint, vDir, tDir, longLine.Length);
            //layoutP.AddRange(AdjustPoints(layoutP.SelectMany(x => x).ToList(), roomLines, longLine.StartPoint, tDir, vDir, shortLine.Length));

            // 计算保护半径
            // 暂时只支持矩形保护半径
            var curve = new Polyline()
            {
                Closed = true,
            };
            double distance = Math.Sqrt(2) * (sideLength / 2.0);
            var vertices = new Point3dCollection()
            {
                Point3d.Origin + distance * (vDir + tDir).GetNormal(),
                Point3d.Origin + distance * (vDir - tDir).GetNormal(),
                Point3d.Origin - distance * (vDir + tDir).GetNormal(),
                Point3d.Origin - distance * (vDir - tDir).GetNormal()
            };
            curve.CreatePolyline(vertices);

            var sprays = new List<List<SprayLayoutData>>();
            foreach (var points in layoutP)
            {
                var sprayList = new List<SprayLayoutData>();
                foreach (var point in points)
                {
                    var offset = Matrix3d.Displacement(point.GetAsVector());
                    sprayList.Add(SprayLayoutData.Create(point, curve.GetTransformedCopy(offset) as Curve));
                }
                sprays.Add(sprayList);
            }
            return sprays;
        }
    }
}
