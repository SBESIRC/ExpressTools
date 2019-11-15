using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using TianHua.AutoCAD.Utility.ExtensionTools;
using DotNetARX;
using Linq2Acad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AcHelper;

namespace Triangle
{
    class Utils
    {
        public enum NodeType
        {
            Unknown = 0,
            RegularPoint = 1,
            StartPoint = 2,
            MergePoint = 3,
            SplitPoint = 4,
            EndPoint = 5,
        }

        public class PathNode
        {
            public Point3d BeforePoint;
            public Point3d CurPoint;
            public Point3d NextPoint;

            NodeType type = NodeType.Unknown;

            public PathNode(Point3d beforePt, Point3d curPt, Point3d nextPt)
            {
                BeforePoint = beforePt;
                CurPoint = curPt;
                NextPoint = nextPt;
            }
        }

        class NodeComparer : IComparer<PathNode>
        {
            public int Compare(PathNode cur, PathNode next)
            {
                if (cur.CurPoint.Y > next.CurPoint.Y)
                    return 1;
                else if(cur.CurPoint.Y < next.CurPoint.Y)
                {
                    return -1;
                }

                if (cur.CurPoint.X < next.CurPoint.X)
                    return 1;

                return 0;
            }
        }

        public static List<Curve> GetAllCurves()
        {
            List<Curve> curves = null;
            using (var db = AcadDatabase.Active())
            {
                // curve 读取
                curves = db.ModelSpace.OfType<Curve>().ToList<Curve>();
            }

            return curves;
        }

        public static List<PathNode> ConvertPolyline2PathNodes(Polyline poly)
        {
            if (poly == null || poly.Vertices().Count < 3)
                return null;

            if (poly.Closed == false)
                return null;
            var pathNodes = new List<PathNode>();

            var ptCol = poly.Vertices();
            for (int i = 0; i < ptCol.Count; i++)
            {
                var beforePt = ptCol[((i - 1 + ptCol.Count) % ptCol.Count)];
                var curPoint = ptCol[i];
                var nextPoint = ptCol[((i + 1) % ptCol.Count)];
                pathNodes.Add(new PathNode(beforePt, curPoint, nextPoint));
            }

            pathNodes.Sort(new NodeComparer());
            return pathNodes;
        }

        public static List<PathNode> DefinePathNodes(List<PathNode> pathNodes)
        {
            if (pathNodes == null || pathNodes.Count < 3)
                return null;

            var resNodes = new List<PathNode>();

            return resNodes;
        }
    }
}
