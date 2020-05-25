using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThWSS.Utlis
{
    public class RegionDivisionUtils
    {
        public void DivisionRegion(Polyline room)
        {
            List<Line> pLines = new List<Line>();
            List<Point3d> points = new List<Point3d>();
            for (int i = 0; i < room.NumberOfVertices; i++)
            {
                var current = room.GetPoint3dAt(i);
                var next = room.GetPoint3dAt((i + 1) % room.NumberOfVertices);
                int j = i - 1;
                if (j < 0)
                {
                    j = room.NumberOfVertices - 1;
                }
                var pre = room.GetPoint3dAt(j);
                pLines.Add(new Line(current, next));

                bool isConvex = GeUtils.IsConvexPoint(room, current, next, pre);
                if (!isConvex)   //凹点要分割
                {
                    points.Add(current);
                }
            }
        }

        private List<Line> BreakLines(List<Line> allLines, List<Point3d> allPoints)
        {
            foreach (var pt in allPoints)
            {
                var tempLines = allLines.Where(x => !(x.StartPoint.IsEqualTo(pt, Tolerance.Global) || x.EndPoint.IsEqualTo(pt, Tolerance.Global))).ToList();

            }
            return null;
        }
    }
}
