using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace ThXClip
{
    public static class ThCurveXclipExtension
    {
        /// <summary>
        /// 修剪圆
        /// </summary>
        /// <param name="ellipse"></param>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static DBObjectCollection XClip(this Circle circle, Point2dCollection pts)
        {
            DBObjectCollection dBObjects = new DBObjectCollection();
            Polyline polyline = CadOperation.CreatePolyline(pts);
            Point3dCollection intersectPts = new Point3dCollection();
            circle.IntersectWith(polyline, Intersect.OnBothOperands,intersectPts,IntPtr.Zero,IntPtr.Zero);
            if(intersectPts==null || intersectPts.Count==0) //无交点，
            {
                if(CadOperation.IsPointInPolyline(pts,circle.Center))  //圆心在pts范围内
                {
                    return dBObjects;
                }
                else
                {
                    DBObject dbobj = circle.Clone() as DBObject;
                    dBObjects.Add(dbobj);
                    return dBObjects;
                }
            }
            else if(intersectPts.Count == 1)
            {
                DBObject dbobj = circle.Clone() as DBObject;
                dBObjects.Add(dbobj);
                return dBObjects;
            }
            Dictionary<double, Point3d> intersPtAngDic = new Dictionary<double, Point3d>();
            foreach(Point3d  pt in intersectPts)
            {
                Vector3d vec = pt.GetVectorTo(circle.Center);
                double angle=Vector3d.XAxis.GetAngleTo(vec);
                if(!intersPtAngDic.ContainsKey(angle))
                {
                    intersPtAngDic.Add(angle, pt);
                }     
             }
            List<Point3d> sortPts=intersPtAngDic.OrderBy(i => i.Key).Select(i=>i.Value).ToList();
            List<List<Point3d>> ptPair = new List<List<Point3d>>();
            for (int i = 0; i < sortPts.Count - 1; i++)
            {
                if (sortPts[i].DistanceTo(sortPts[i + 1])<=1.0)
                {
                    continue;
                }
                Point3d midPt = CadOperation.GetMidPt(sortPts[i], sortPts[i + 1]);
                Point3d arcTopPt = CadOperation.GetExtentPt(circle.Center, midPt, circle.Radius);
                if (CadOperation.IsPointInPolyline(pts, arcTopPt))
                {
                    continue;
                }
                else
                {
                    ptPair.Add(new List<Point3d> { sortPts[i], sortPts[i + 1] });
                }
            }
            for (int i = 0; i < ptPair.Count; i++)
            {
                Point3d startPt = ptPair[i][0];
                Point3d endPt = ptPair[i][1];
                int num = i;
                for (int j = i + 1; j < ptPair.Count; j++)
                {
                    if (ptPair[j][0].DistanceTo(endPt) <= 1.0)
                    {
                        endPt = ptPair[j][1];
                        num = j;
                    }
                    else
                    {
                        break;
                    }
                }
                i = num;
                Vector3d startVec = startPt.GetVectorTo(circle.Center);
                double startAngle = Vector3d.XAxis.GetAngleTo(startVec);
                Vector3d endVec = endPt.GetVectorTo(circle.Center);
                double endAngle = Vector3d.XAxis.GetAngleTo(endVec);
                Arc arc = new Arc(circle.Center, circle.Normal, circle.Radius, startAngle, endAngle);
                arc.ColorIndex = circle.ColorIndex;
                arc.Layer = circle.Layer;
                arc.LineWeight = circle.LineWeight;
                dBObjects.Add(arc);
            }
            Point3d newArcSp = sortPts[sortPts.Count - 1];
            Point3d newArcEp = sortPts[0];
            Point3d newArcTopPt = CadOperation.GetExtentPt(circle.Center, CadOperation.GetMidPt(newArcSp, newArcEp), circle.Center.DistanceTo(newArcSp));
            if (!CadOperation.IsPointInPolyline(pts, newArcTopPt)) //假设弧形顶点不在WipeOut里面，则保留此弧段
            {
                Arc arc = CadOperation.CreateArc(circle.Center, newArcSp, newArcEp);
                dBObjects.Add(arc);
            }
            return dBObjects;
        }
        /// <summary>
        /// 修剪椭圆
        /// </summary>
        /// <param name="ellipse"></param>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static DBObjectCollection XClip(this Ellipse ellipse, Point2dCollection pts)
        {
            DBObjectCollection dBObjects = new DBObjectCollection();
            Polyline polyline = CadOperation.CreatePolyline(pts);
            Point3dCollection intersectPts = new Point3dCollection();
            ellipse.IntersectWith(polyline, Intersect.OnBothOperands, intersectPts, IntPtr.Zero, IntPtr.Zero);
            if (intersectPts == null || intersectPts.Count == 0)
            {
                if (CadOperation.IsPointInPolyline(pts, ellipse.Center))  //圆心在pts范围内
                {
                    return dBObjects;
                }
                else
                {
                    DBObject dbobj = ellipse.Clone() as DBObject;
                    dBObjects.Add(dbobj);
                    return dBObjects;
                }
            }
            else if (intersectPts.Count == 1)
            {
                if (intersectPts[0].DistanceTo(ellipse.StartPoint) <= 1.0 ||
                    intersectPts[0].DistanceTo(ellipse.EndPoint) <= 1.0)
                {
                    dBObjects.Add(ellipse); //不做任何操作
                    return dBObjects; //不做任何操作
                }
            }
            List<double> angs = new List<double>();
            angs.Add(ellipse.StartAngle);
            foreach (Point3d pt in intersectPts)
            {
                double para = ellipse.GetParameterAtPoint(pt);
                double ang = ellipse.GetAngleAtParameter(para);
                if(angs.IndexOf(ang)<0)
                {
                    angs.Add(ang);
                }
            }
            angs.Add(ellipse.EndAngle);
            angs = angs.OrderBy(i=>i).ToList();
            List<Point3d> sortPts = new List<Point3d>();
            for(int i=0;i< angs.Count;i++)
            {
                double para = ellipse.GetParameterAtAngle(angs[0]);
                Point3d pt = ellipse.GetPointAtParameter(para);
                sortPts.Add(pt);
            }
            List<List<Point3d>> ptPair = new List<List<Point3d>>();
            for(int i = 0; i < angs.Count-1;i++)
            {
                double midAng = (angs[i] + angs[i + 1]) / 2.0;
                double para = ellipse.GetParameterAtAngle(midAng);
                Point3d ellipseTopPt = ellipse.GetPointAtParameter(para);
                if (CadOperation.IsPointInPolyline(pts, ellipseTopPt))
                {
                    continue;
                }
                else
                {
                    double startPara = ellipse.GetParameterAtAngle(angs[i]);
                    double endPara = ellipse.GetParameterAtAngle(angs[i+1]);
                    Point3d startPt = ellipse.GetPointAtParameter(startPara);
                    Point3d endPt = ellipse.GetPointAtParameter(endPara);
                    ptPair.Add(new List<Point3d> { startPt, endPt });
                }
            }
            for (int i = 0; i < ptPair.Count; i++)
            {
                Point3d startPt = ptPair[i][0];
                Point3d endPt = ptPair[i][1];
                int num = i;
                for (int j = i + 1; j < ptPair.Count; j++)
                {
                    if (ptPair[j][0].DistanceTo(endPt) <= 1.0)
                    {
                        endPt = ptPair[j][1];
                        num = j;
                    }
                    else
                    {
                        break;
                    }
                }
                i = num;
                Vector3d startVec = ellipse.Center.GetVectorTo(startPt);
                Vector3d endVec= ellipse.Center.GetVectorTo(endPt);
                double startPara = ellipse.GetParameterAtPoint(startPt);
                double endPara = ellipse.GetParameterAtPoint(endPt);
                double startAngle = ellipse.GetAngleAtParameter(startPara);
                double endAngle = ellipse.GetAngleAtParameter(endPara);
                bool isAnticlockWise = CadOperation.JudgeTwoVectorIsAnticlockwise(startVec, endVec);
                Ellipse newEllipse = new Ellipse();
                if (isAnticlockWise)
                {
                    newEllipse = new Ellipse(ellipse.Center, ellipse.Normal,
                    ellipse.MajorAxis, ellipse.RadiusRatio, startAngle, endAngle);
                }
                else
                {
                    newEllipse = new Ellipse(ellipse.Center, ellipse.Normal,
                    ellipse.MajorAxis, ellipse.RadiusRatio, endAngle, startAngle);
                }
                newEllipse.ColorIndex = ellipse.ColorIndex;
                newEllipse.Layer = ellipse.Layer;
                newEllipse.LineWeight = ellipse.LineWeight;
                dBObjects.Add(newEllipse);
            }
            return dBObjects;
        }
        /// <summary>
        /// 修剪线
        /// </summary>
        /// <param name="ellipse"></param>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static DBObjectCollection XClip(this Line line, Point2dCollection pts)
        {
            DBObjectCollection dBObjects = new DBObjectCollection();
            Polyline polyline = CadOperation.CreatePolyline(pts);
            Point3dCollection intersectPts = new Point3dCollection();
            polyline.IntersectWith(line, Intersect.OnBothOperands, intersectPts, IntPtr.Zero, IntPtr.Zero);
            if (intersectPts == null || intersectPts.Count == 0)
            {
                Point3d midPt = CadOperation.GetMidPt(line.StartPoint, line.EndPoint);
                if (CadOperation.IsPointInPolyline(pts, midPt)) //线在曲线内
                {
                    return dBObjects;
                }
                else
                {
                    Line newLine = line.Clone() as Line;
                    dBObjects.Add(newLine);
                    return dBObjects;
                }
            }
            List<Point3d> points = new List<Point3d>();
            points.Add(line.StartPoint);
            foreach (Point3d pt in intersectPts)
            {
                points.Add(pt);
            }
            points.Add(line.EndPoint);
            points=points.OrderBy(i => i.DistanceTo(line.StartPoint)).ToList();
            List<List<Point3d>> ptPair = new List<List<Point3d>>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                if (points[i].DistanceTo(points[i + 1]) <= 1.0)
                {
                    continue;
                }
                Point3d pt = CadOperation.GetMidPt(points[i], points[i + 1]);
                if (CadOperation.IsPointInPolyline(pts, pt))
                {
                    continue;
                }
                else
                {
                    ptPair.Add(new List<Point3d>{ points[i], points[i + 1] });
                }
            }
            for(int i=0;i< ptPair.Count;i++)
            {
                Point3d startPt = ptPair[i][0];
                Point3d endPt = ptPair[i][1];
                int num = i;
                for (int j=i+1;j< ptPair.Count;j++)
                {
                    if(ptPair[j][0].DistanceTo(endPt)<=1.0)
                    {
                        endPt = ptPair[j][1];
                        num = j;
                    }
                    else
                    {
                        break;
                    }
                }
                i = num;
                Line newLine = new Line(startPt, endPt);
                newLine.ColorIndex = line.ColorIndex;
                newLine.Layer = line.Layer;
                newLine.LineWeight = line.LineWeight;
                dBObjects.Add(newLine);
            }
            return dBObjects;
        }
        /// <summary>
        /// 修剪Polyline
        /// </summary>
        /// <param name="ellipse"></param>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static DBObjectCollection XClip(this Polyline polyline, Point2dCollection pts)
        {
            DBObjectCollection dBObjects = new DBObjectCollection();
            Polyline clipBoundaryPline = CadOperation.CreatePolyline(pts);
            Point3dCollection intersectPts = new Point3dCollection();
            polyline.IntersectWith(clipBoundaryPline, Intersect.OnBothOperands, intersectPts, IntPtr.Zero, IntPtr.Zero);
            if (intersectPts == null || intersectPts.Count == 0)
            {
                Point3d firstPt = polyline.GetPoint3dAt(0);
                if (CadOperation.IsPointInPolyline(pts, firstPt)) //线在曲线内
                {
                    return dBObjects;
                }
                else
                {
                    Polyline newPolyLine = polyline.Clone() as Polyline;
                    dBObjects.Add(newPolyLine);
                    return dBObjects;
                }
            }
            //获取交点在Polyline上哪些分段上
            Dictionary<Point3d, int> ptSegmentIndexDic = new Dictionary<Point3d, int>();
            foreach (Point3d point in intersectPts)
            {
                int index = CadOperation.GetPointOnPolylineSegment(polyline, point);
                if (index < 0)
                {
                    continue;
                }
                if (!ptSegmentIndexDic.ContainsKey(point))
                {
                    ptSegmentIndexDic.Add(point, index);
                }
            }
            //拆分多段线
            List<int> segmentIndexes = new List<int>();
            for (int i = 0; i < polyline.NumberOfVertices; i++)
            {
                List<Point3d> splitPts = ptSegmentIndexDic.Where(j => j.Value == i).Select(j => j.Key).ToList();
                if (splitPts == null || splitPts.Count==0)
                {
                    segmentIndexes.Add(i);
                }
                else
                {
                   SegmentType st= polyline.GetSegmentType(i);
                   if(st== SegmentType.Line)
                    {
                       LineSegment3d line= polyline.GetLineSegmentAt(i);
                        bool headHasCurve = true;
                        bool endHasCurve = true;
                       List<List<Point3d>> splitSegments= GetLineSplits(line, pts, splitPts, out headHasCurve, out endHasCurve);
                        if(splitSegments.Count==1 && headHasCurve && endHasCurve)
                        {
                            segmentIndexes.Add(i);
                        }
                    }
                }
            }
            return dBObjects;
        }
        private static List<List<Point3d>> GetLineSplits(LineSegment3d line, Point2dCollection pts,List<Point3d> splitPts,
            out bool headHasCurve,out bool endHasCurve)
        {
            List<List<Point3d>> returnLinePts = new List<List<Point3d>>();
            List<Point3d> points = new List<Point3d>();
            headHasCurve = true;
            endHasCurve = true;
            points.Add(line.StartPoint);
            splitPts = splitPts.Where(i => i.DistanceTo(line.StartPoint) > 0 && i.DistanceTo(line.EndPoint) > 0).ToList();
            points.AddRange(splitPts);
            points.Add(line.EndPoint);
            points = points.OrderBy(j => j.DistanceTo(line.StartPoint)).ToList();

            List<List<Point3d>> ptPair = new List<List<Point3d>>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                Point3d pt = CadOperation.GetMidPt(points[i], points[i + 1]);
                if (CadOperation.IsPointInPolyline(pts, pt))
                {
                    if(i == 0)
                    {
                        headHasCurve = false;
                    }
                    else if (i == points.Count - 2)
                    {
                        endHasCurve = false;
                    }
                    continue;
                }
                else
                {
                    ptPair.Add(new List<Point3d> { points[i], points[i + 1] });
                }
            }
            for (int i = 0; i < ptPair.Count; i++)
            {
                Point3d startPt = ptPair[i][0];
                Point3d endPt = ptPair[i][1];
                int num = i;
                for (int j = i + 1; j < ptPair.Count; j++)
                {
                    if (ptPair[j][0].DistanceTo(endPt) <= 1.0)
                    {
                        endPt = ptPair[j][1];
                        num = j;
                    }
                    else
                    {
                        break;
                    }
                }
                i = num;
                returnLinePts.Add(new List<Point3d> { startPt, endPt });
            }
            return returnLinePts;
        }
        /// <summary>
        /// 修剪Arc
        /// </summary>
        /// <param name="ellipse"></param>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static DBObjectCollection XClip(this Arc arc, Point2dCollection pts)
        {
            DBObjectCollection dBObjects = new DBObjectCollection();
            Polyline polyline = CadOperation.CreatePolyline(pts);
            Point3dCollection intersectPts = new Point3dCollection();
            arc.IntersectWith(polyline, Intersect.OnBothOperands, intersectPts, IntPtr.Zero, IntPtr.Zero);
            if (intersectPts == null || intersectPts.Count == 0) //无交点，
            {
                if (CadOperation.IsPointInPolyline(pts, arc.Center))  //圆心在pts范围内
                {
                    return dBObjects;
                }
                else
                {
                    DBObject dbobj = arc.Clone() as DBObject;
                    dBObjects.Add(dbobj);
                    return dBObjects;
                }
            }
            else if (intersectPts.Count == 1)
            {
                if(intersectPts[0].DistanceTo(arc.StartPoint)<=1.0 ||
                    intersectPts[0].DistanceTo(arc.EndPoint) <= 1.0)
                {
                    DBObject dbobj = arc.Clone() as DBObject;
                    dBObjects.Add(dbobj);
                    return dBObjects;
                }
            }
            List<Point3d> sortPts = new List<Point3d>();
            sortPts.Add(arc.StartPoint);
            foreach(Point3d pt in intersectPts)
            {
                sortPts.Add(pt);
            }
            sortPts.Add(arc.EndPoint);
            Dictionary<double, Point3d> intersPtAngDic = new Dictionary<double, Point3d>();
            foreach (Point3d pt in sortPts)
            {
                double angle = CadOperation.AngleFromXAxis(arc.Center, pt);
                if (!intersPtAngDic.ContainsKey(angle))
                {
                    intersPtAngDic.Add(angle, pt);
                }
            }
            sortPts = intersPtAngDic.OrderBy(i => i.Key).Select(i => i.Value).ToList();
            List<List<Point3d>> ptPair = new List<List<Point3d>>();
            for (int i = 0; i < sortPts.Count - 1; i++)
            {
                if (sortPts[i].DistanceTo(sortPts[i + 1]) <= 1.0)
                {
                    continue;
                }
                Point3d midPt = CadOperation.GetMidPt(sortPts[i], sortPts[i + 1]);
                Point3d arcTopPt = CadOperation.GetExtentPt(arc.Center, midPt, arc.Radius);
                if (CadOperation.IsPointInPolyline(pts, arcTopPt))
                {
                    continue;
                }
                else
                {
                    ptPair.Add(new List<Point3d> { sortPts[i], sortPts[i + 1] });
                }
            }
            for (int i = 0; i < ptPair.Count; i++)
            {
                Point3d startPt = ptPair[i][0];
                Point3d endPt = ptPair[i][1];
                int num = i;
                for (int j = i + 1; j < ptPair.Count; j++)
                {
                    if (ptPair[j][0].DistanceTo(endPt) <= 1.0)
                    {
                        endPt = ptPair[j][1];
                        num = j;
                    }
                    else
                    {
                        break;
                    }
                }
                i = num;
                double startAngle = CadOperation.AngleFromXAxis(arc.Center,startPt);
                double endAngle = CadOperation.AngleFromXAxis(arc.Center, endPt);
                Arc newArc = new Arc(arc.Center, arc.Normal, arc.Radius, startAngle, endAngle);
                arc.ColorIndex = arc.ColorIndex;
                arc.Layer = arc.Layer;
                arc.LineWeight = arc.LineWeight;
                dBObjects.Add(arc);
            }            
            return dBObjects;
        }
        /// <summary>
        /// 修剪Polyline2d
        /// </summary>
        /// <param name="ellipse"></param>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static DBObjectCollection XClip(this Polyline2d polyline2d, Point2dCollection pts)
        {
            DBObjectCollection dBObjects = new DBObjectCollection();
            return dBObjects;
        }
        /// <summary>
        /// 修剪Polyline3d
        /// </summary>
        /// <param name="ellipse"></param>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static DBObjectCollection XClip(this Polyline3d polyline3d, Point2dCollection pts)
        {
            DBObjectCollection dBObjects = new DBObjectCollection();
            return dBObjects;
        }
        /// <summary>
        /// 修剪Spline
        /// </summary>
        /// <param name="ellipse"></param>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static DBObjectCollection XClip(this Spline spline, Point2dCollection pts)
        {
            DBObjectCollection dBObjects = new DBObjectCollection();
            return dBObjects;
        }
        /// <summary>
        /// 修剪Xline
        /// </summary>
        /// <param name="ellipse"></param>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static DBObjectCollection XClip(this Xline xline, Point2dCollection pts)
        {
            DBObjectCollection dBObjects = new DBObjectCollection();
            return dBObjects;
        }
        /// <summary>
        /// 修剪Xline
        /// </summary>
        /// <param name="ellipse"></param>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static DBObjectCollection XClip(this Ray ray, Point2dCollection pts)
        {
            DBObjectCollection dBObjects = new DBObjectCollection();
            return dBObjects;
        }
    }
}
