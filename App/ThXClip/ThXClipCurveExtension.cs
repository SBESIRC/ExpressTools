using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThXClip
{
    public static class ThXClipCurveExtension
    {
        /// <summary>
        /// 修剪Arc
        /// </summary>
        /// <param name="ellipse"></param>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static DBObjectCollection XClip(this Arc arc, Point2dCollection pts, bool keepInternal = false)
        {
            DBObjectCollection dBObjects = new DBObjectCollection();
            if (arc == null)
            {
                return dBObjects;
            }
            Polyline polyline = ThXClipCadOperation.CreatePolyline(pts);
            Point3dCollection intersectPts = new Point3dCollection();
            arc.IntersectWith(polyline, Intersect.OnBothOperands, intersectPts, IntPtr.Zero, IntPtr.Zero);
            polyline.Dispose(); 
            if(intersectPts!=null && intersectPts.Count>0)
            {
                int index = intersectPts.IndexOf(arc.StartPoint);
                if (index >= 0)
                {
                    intersectPts.RemoveAt(index);
                }
                index = intersectPts.IndexOf(arc.EndPoint);
                if (index >= 0)
                {
                    intersectPts.RemoveAt(index);
                }
            }
            if (intersectPts == null || intersectPts.Count == 0) //无交点，
            {
                if (ThXClipCadOperation.IsPointInPolyline(pts, arc.Center))  //圆心在pts范围内
                {
                    if (keepInternal)
                    {
                        dBObjects.Add(arc);
                    }
                    else
                    {
                        arc.Dispose();
                    }
                }
                else
                {
                    if (!keepInternal)
                    {
                        dBObjects.Add(arc);
                    }
                    else
                    {
                        arc.Dispose();
                    }
                }
                return dBObjects;
            }
            else if (intersectPts.Count == 1)
            {
                if (intersectPts[0].IsEqualTo(arc.StartPoint,ThCADCommon.Global_Tolerance)||
                    intersectPts[0].IsEqualTo(arc.EndPoint, ThCADCommon.Global_Tolerance))
                {
                    Point3d arcTopPt = ThXClipCadOperation.GetArcTopPt(arc.Center, arc.StartPoint, arc.EndPoint);
                    if (ThXClipCadOperation.IsPointInPolyline(pts, arcTopPt))  //弧顶在pts范围内
                    {
                        if (keepInternal)
                        {
                            dBObjects.Add(arc);
                        }
                        else
                        {
                            arc.Dispose();
                        }
                    }
                    else
                    {
                        if (!keepInternal)
                        {
                            dBObjects.Add(arc);
                        }
                        else
                        {
                            arc.Dispose();
                        }
                    }
                    intersectPts.Dispose();
                    return dBObjects;
                }
            }
            List<Point3d> intersectPtList = new List<Point3d>();
            foreach (Point3d pt in intersectPts)
            {
                if (!(pt.IsEqualTo(arc.StartPoint,ThCADCommon.Global_Tolerance) ||
                    pt.IsEqualTo(arc.StartPoint, ThCADCommon.Global_Tolerance)))
                {
                    intersectPtList.Add(pt);
                }
            }
            List<Point3d> sortPts = new List<Point3d>();
            sortPts.Insert(0, arc.StartPoint);
            intersectPtList.ForEach(i => sortPts.Add(i));
            sortPts.Add(arc.EndPoint);
            sortPts = ThXClipCadOperation.SortArcPts(sortPts, arc.Center);
            bool isClockWise = ThXClipCadOperation.JudgeTwoVectorIsAnticlockwise(arc.Center.GetVectorTo(arc.StartPoint),
                arc.Center.GetVectorTo(arc.EndPoint)); //逆时针
            if(!isClockWise)
            {
                sortPts.Reverse();
            }
            intersectPts.Dispose(); //释放交点
            Point3dCollection sortIntersecPts = new Point3dCollection();
            sortPts.ForEach(i=> sortIntersecPts.Add(i));
            List<Curve> splitCurves = new List<Curve>(); 
            if(sortIntersecPts.Count>0)
            {
                DBObjectCollection dBObjs = arc.GetSplitCurves(sortIntersecPts);
                if (dBObjs.Count == 0)
                {
                    splitCurves.Add(arc);
                }
                else
                {
                    foreach (DBObject dbObj in dBObjs)
                    {
                        splitCurves.Add(dbObj as Curve);
                    }
                }
            }
            else
            {
                splitCurves.Add(arc);
            }
            //List<Curve> splitCurves = SplitCurves(arc, intersectPts);
            if(splitCurves.Count>1) //说明arc被分割成多段
            {
                arc.Dispose();
            }
            for(int i=0;i< splitCurves.Count; i++)
            {
                Arc currentArc = splitCurves[i] as Arc;
                Point3d arcTopPt = ThXClipCadOperation.GetArcTopPt(currentArc.Center, currentArc.StartPoint, currentArc.EndPoint);
                if (ThXClipCadOperation.IsPointInPolyline(pts, arcTopPt))
                {
                    if (keepInternal)
                    {
                        dBObjects.Add(splitCurves[i]);
                    }
                    else
                    {
                        currentArc.Dispose();
                    }
                }
                else
                {
                    if (!keepInternal)
                    {
                        dBObjects.Add(splitCurves[i]);
                    }
                    else
                    {
                        currentArc.Dispose();
                    }
                }
            }
            return dBObjects;
        }
        /// <summary>
        /// 修剪Spline
        /// </summary>
        /// <param name="ellipse"></param>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static DBObjectCollection XClip(this Spline spline, Point2dCollection pts, bool keepInternal = false)
        {
            DBObjectCollection dBObjects = new DBObjectCollection();
            if (spline == null)
            {
                return dBObjects;
            }
            Polyline polyline = ThXClipCadOperation.CreatePolyline(pts);
            Point3dCollection intersectPts = new Point3dCollection();
            polyline.IntersectWith(spline, Intersect.OnBothOperands, intersectPts, IntPtr.Zero, IntPtr.Zero);
            polyline.Dispose();
            if (intersectPts!=null && intersectPts.Count > 0)
            {
                int index = intersectPts.IndexOf(spline.StartPoint);
                if (index >= 0)
                {
                    intersectPts.RemoveAt(index);
                }
                if (intersectPts.Count > 0)
                {
                    index = intersectPts.IndexOf(spline.EndPoint);
                    if (index >= 0)
                    {
                        intersectPts.RemoveAt(index);
                    }
                }
            }
            if (intersectPts == null || intersectPts.Count == 0)
            {
                if (ThXClipCadOperation.IsPointInPolyline(pts, spline.StartPoint))  //圆心在pts范围内
                {
                    if (keepInternal)
                    {
                        dBObjects.Add(spline);
                    }
                    else
                    {
                        spline.Dispose();
                    }
                }
                else
                {
                    if (!keepInternal)
                    {
                        dBObjects.Add(spline);
                    }
                    else
                    {
                        spline.Dispose();
                    }
                }
                return dBObjects;
            }
            else if (intersectPts.Count == 1)
            {
                Point3d checkPt = Point3d.Origin;
                bool isPortPt = false;
                if (intersectPts[0].IsEqualTo(spline.StartPoint,ThCADCommon.Global_Tolerance))
                {
                    checkPt = spline.EndPoint;
                    isPortPt = true;
                }
                else if (intersectPts[0].IsEqualTo(spline.EndPoint, ThCADCommon.Global_Tolerance))
                {
                    checkPt = spline.StartPoint;
                    isPortPt = true;
                }
                if (isPortPt)
                {
                    if (ThXClipCadOperation.IsPointInPolyline(pts, checkPt)) //判断Spline是否在Polyline里面
                    {
                        if (keepInternal)
                        {
                            dBObjects.Add(spline);
                        }
                        else
                        {
                            spline.Dispose();
                        }
                    }
                    else
                    {
                        if (!keepInternal)
                        {
                            dBObjects.Add(spline);
                        }
                        else
                        {
                            spline.Dispose();
                        }
                    }
                    intersectPts.Dispose();
                    return dBObjects;
                }
            }
            List<Curve> splitCurves = SplitCurves(spline, intersectPts);
            if(splitCurves.Count>1)
            {
                spline.Dispose();
            }
            for (int i=0; i< splitCurves.Count;i++)
            {
                Point3d checkPt = Point3d.Origin;
                Spline currentSpline = splitCurves[i] as Spline;
                if (currentSpline.NumControlPoints > 1)
                {
                    checkPt = ThXClipCadOperation.GetExtentPt(currentSpline.StartPoint, currentSpline.GetControlPointAt(1), 1.0); //起点到第2个控制点，延长1mm,找到一个检查点
                }
                int numberFitPoints = currentSpline.NumFitPoints;
                for (int j = 0; j < numberFitPoints; j++)
                {
                    Point3d fitPoint = currentSpline.GetFitPointAt(j);
                    if (!fitPoint.IsEqualTo(currentSpline.StartPoint, ThCADCommon.Global_Tolerance) &&
                        !fitPoint.IsEqualTo(currentSpline.EndPoint, ThCADCommon.Global_Tolerance))
                    {
                        checkPt = fitPoint;
                        break;
                    }
                }
                if (ThXClipCadOperation.IsPointInPolyline(pts, checkPt))
                {
                    if (keepInternal)
                    {
                        dBObjects.Add(currentSpline);
                    }
                    else
                    {
                        currentSpline.Dispose();
                    }
                }
                else
                {
                    if (!keepInternal)
                    {
                        dBObjects.Add(currentSpline);
                    }
                    else
                    {
                        currentSpline.Dispose();
                    }
                }
            }
            return dBObjects;
        }
        /// <summary>
        /// 修剪圆
        /// </summary>
        /// <param name="ellipse"></param>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static DBObjectCollection XClip(this Circle circle, Point2dCollection pts, bool keepInternal = false)
        {
            DBObjectCollection dBObjects = new DBObjectCollection();
            if (circle == null)
            {
                return dBObjects;
            }
            Polyline polyline = ThXClipCadOperation.CreatePolyline(pts);
            Point3dCollection intersectPts = new Point3dCollection();
            circle.IntersectWith(polyline, Intersect.OnBothOperands, intersectPts, IntPtr.Zero, IntPtr.Zero);
            polyline.Dispose();
            if (intersectPts == null || intersectPts.Count <= 1) //无交点，或一个交点
            {
                if (ThXClipCadOperation.IsPointInPolyline(pts, circle.Center))  //圆心在pts范围内
                {
                    if (keepInternal)
                    {
                        dBObjects.Add(circle);
                    }
                    else
                    {
                        circle.Dispose();
                    }
                }
                else
                {
                    if (!keepInternal)
                    {
                        dBObjects.Add(circle);
                    }
                    else
                    {
                        circle.Dispose();
                    }
                }
                if(intersectPts!=null)
                {
                    intersectPts.Dispose();
                }
                return dBObjects;
            }
            Dictionary<Point3d, double> intersPtAngDic = new Dictionary<Point3d, double>();
            foreach (Point3d pt in intersectPts)
            {
                double angle = ThXClipCadOperation.AngleFromXAxis(circle.Center, pt);
                if (!intersPtAngDic.ContainsKey(pt))
                {
                    intersPtAngDic.Add(pt, angle);
                }
            }
            intersectPts.Dispose();
            List<Curve> splitCurves = new List<Curve>();
            List<Point3d> sortPts = intersPtAngDic.OrderBy(i => i.Value).Select(i => i.Key).ToList();
            if(sortPts.Count>1)
            {
                Point3dCollection sortIntersecPts = new Point3dCollection();
                sortPts.ForEach(i=> sortIntersecPts.Add(i));
                DBObjectCollection splitDbObjs= circle.GetSplitCurves(sortIntersecPts);
                foreach(DBObject dbObj in splitDbObjs)
                {
                    splitCurves.Add(dbObj as Curve);
                }
            }
            else
            {
                splitCurves.Add(circle);
            }
            if(splitCurves.Count>1)
            {
                circle.Dispose();
            }
            if(splitCurves==null || splitCurves.Count==0) //没有分割
            {
                if (ThXClipCadOperation.IsPointInPolyline(pts, circle.Center))  //圆心在pts范围内
                {
                    if (keepInternal)
                    {
                        dBObjects.Add(circle);
                    }
                    else
                    {
                        circle.Dispose();
                    }
                }
                else
                {
                    if (!keepInternal)
                    {
                        dBObjects.Add(circle);
                    }
                    else
                    {
                        circle.Dispose();
                    }
                }
                return dBObjects;
            }
            for (int i = 0; i < splitCurves.Count; i++)
            {
                Point3d checkPt = Point3d.Origin;
                if(splitCurves[i] is Arc)
                {
                    Arc currentArc = splitCurves[i] as Arc;
                    checkPt = ThXClipCadOperation.GetArcTopPt(currentArc.Center, currentArc.StartPoint, currentArc.EndPoint);
                }
                else if(splitCurves[i] is Circle)
                {
                    Circle currentCircle= splitCurves[i] as Circle;
                    checkPt = currentCircle.Center;
                }
                else
                {
                    splitCurves[i].Dispose();
                    continue;
                }
                if (ThXClipCadOperation.IsPointInPolyline(pts, checkPt))
                {
                    if (keepInternal)
                    {
                        dBObjects.Add(splitCurves[i]);
                    }
                    else
                    {
                        splitCurves[i].Dispose();
                    }
                }
                else
                {
                    if (!keepInternal) //保留外部
                    {
                        dBObjects.Add(splitCurves[i]);
                    }
                    else
                    {
                        splitCurves[i].Dispose();
                    }
                }
            }
            return dBObjects;
        }
        /// <summary>
        /// 修剪椭圆
        /// </summary>
        /// <param name="ellipse"></param>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static DBObjectCollection XClip(this Ellipse ellipse, Point2dCollection pts, bool keepInternal = false)
        {
            DBObjectCollection dBObjects = new DBObjectCollection();
            if(ellipse==null)
            {
                return dBObjects;
            }
            double ellipseJiaJiao = ellipse.EndAngle - ellipse.StartAngle;
            bool isCompletedEllipse = false;
            if (ellipseJiaJiao == Math.PI * 2) //完整的椭圆
            {
                isCompletedEllipse = true;
            }
            Polyline polyline = ThXClipCadOperation.CreatePolyline(pts);
            Point3dCollection intersectPts = new Point3dCollection();
            ellipse.IntersectWith(polyline, Intersect.OnBothOperands, intersectPts, IntPtr.Zero, IntPtr.Zero);
            polyline.Dispose();
            bool isGoOn = true;
            if(intersectPts == null || intersectPts.Count == 0)
            {
                isGoOn = false;
            }
            else if(intersectPts.Count == 1 && isCompletedEllipse)
            {
                isGoOn = false;
            }
            else if(intersectPts.Count == 1 && isCompletedEllipse==false)
            {
                if (intersectPts[0].IsEqualTo(ellipse.StartPoint, ThCADCommon.Global_Tolerance) ||
                   intersectPts[0].IsEqualTo(ellipse.EndPoint, ThCADCommon.Global_Tolerance))
                {
                    isGoOn = false;
                }
            }
            if (isGoOn==false)
            {
                if (ThXClipCadOperation.IsPointInPolyline(pts, ellipse.Center))  //圆心在pts范围内
                {
                    if (keepInternal) //保留内部
                    {
                        dBObjects.Add(ellipse);
                    }
                    else
                    {
                        ellipse.Dispose();
                    }
                }
                else
                {
                    if (!keepInternal) //保留外部
                    {
                        dBObjects.Add(ellipse);
                    }
                    else
                    {
                        ellipse.Dispose();
                    }
                }
                return dBObjects;
            }
            Dictionary<double, Point3d> ptAngDic = new Dictionary<double, Point3d>();
            List<Point3d> sortPts = new List<Point3d>();
            if(!isCompletedEllipse)
            {
                ptAngDic.Add(ellipse.StartAngle, ellipse.StartPoint);
                ptAngDic.Add(ellipse.EndAngle, ellipse.EndPoint);
            }
            foreach (Point3d pt in intersectPts)
            {
                double para = ellipse.GetParameterAtPoint(pt);
                double ang = ellipse.GetAngleAtParameter(para);
                if(!ptAngDic.ContainsKey(ang))
                {
                    ptAngDic.Add(ang, pt);
                }
            }
            sortPts= ptAngDic.OrderBy(i => i.Key).Select(i=>i.Value).ToList();
            if (!isCompletedEllipse)
            {
                if (sortPts.IndexOf(ellipse.StartPoint) != 0)
                {
                    sortPts.Reverse();
                }
            }
            intersectPts.Dispose();
            Point3dCollection sortIntersecPts = new Point3dCollection();
            sortPts.ForEach(i => sortIntersecPts.Add(i));
            List<Curve> splitCurves = new List<Curve>();
            if(isCompletedEllipse)
            {
                if(sortIntersecPts.Count<=1)
                {
                    splitCurves.Add(ellipse);
                }
                else
                {
                    DBObjectCollection dbObjs = ellipse.GetSplitCurves(sortIntersecPts);
                    foreach (DBObject dbObj in dbObjs)
                    {
                        splitCurves.Add(dbObj as Ellipse);
                    }
                }
            }
            else
            {
                splitCurves= SplitCurves(ellipse, intersectPts);
                if(splitCurves==null  || splitCurves.Count==1)
                {
                    splitCurves.Add(ellipse);
                }
            }
            if(splitCurves.Count>1)
            {
                ellipse.Dispose();
            }
            for (int i = 0; i < splitCurves.Count; i++)
            {
                Point3d checkPt = Point3d.Origin;
                Ellipse currentEllipse = splitCurves[i] as Ellipse;
                if(currentEllipse==null)
                {
                    continue;
                }
                double midAng = (currentEllipse.StartAngle + currentEllipse.EndAngle) / 2.0;
                double para = currentEllipse.GetParameterAtAngle(midAng);
                Point3d ellipseTopPt = currentEllipse.GetPointAtParameter(para);
                if (ThXClipCadOperation.IsPointInPolyline(pts, ellipseTopPt))
                {
                    if (keepInternal)
                    {
                        dBObjects.Add(currentEllipse);
                    }
                    else
                    {
                        currentEllipse.Dispose();
                    }
                }
                else
                {
                    if (!keepInternal)
                    {
                        dBObjects.Add(currentEllipse);
                    }
                    else
                    {
                        currentEllipse.Dispose();
                    }
                }
            }
            return dBObjects;
        }
        /// <summary>
        /// 修剪线
        /// </summary>
        /// <param name="ellipse"></param>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static DBObjectCollection XClip(this Line line, Point2dCollection pts, bool keepInternal = false)
        {
            DBObjectCollection dBObjects = new DBObjectCollection();
            if (line == null)
            {
                return dBObjects;
            }
            Polyline polyline = ThXClipCadOperation.CreatePolyline(pts);
            Point3dCollection intersectPts = new Point3dCollection();
            polyline.IntersectWith(line, Intersect.OnBothOperands, intersectPts, IntPtr.Zero, IntPtr.Zero);
            polyline.Dispose();
            if (intersectPts==null || intersectPts.Count == 0) //无交点
            {
                if (ThXClipCadOperation.IsPointInPolyline(pts, ThXClipCadOperation.GetMidPt(line.StartPoint, line.EndPoint))) //线在曲线内
                {
                    if (keepInternal)
                    {
                        dBObjects.Add(line);
                    }
                    else
                    {
                        line.Dispose();
                    }
                }
                else
                {
                    if (!keepInternal)
                    {
                        dBObjects.Add(line);
                    }
                    else
                    {
                        line.Dispose();
                    }
                }
                if(intersectPts!=null)
                {
                    intersectPts.Dispose();
                }
                return dBObjects;
            }
            List<Point3d> sortPts = new List<Point3d>();
            foreach(Point3d interPt in intersectPts)
            {
                if(interPt.IsEqualTo(line.StartPoint,ThCADCommon.Global_Tolerance) ||
                   interPt.IsEqualTo(line.EndPoint, ThCADCommon.Global_Tolerance))
                {
                    continue;
                }
                else
                {
                    bool isSuccess = true;
                    bool isInLine = true;
                    Point3d newPt = ThXClipCadOperation.TransPtToLine(line.StartPoint, line.EndPoint, interPt,out isSuccess,out isInLine);
                    if(isInLine && isSuccess)
                    {
                        sortPts.Add(newPt);
                    }
                }
            }
            List<Curve> splitCurves = new List<Curve>();
            if (sortPts.Count>0)
            {
                sortPts = sortPts.OrderBy(i => i.DistanceTo(line.StartPoint)).ToList();
                Point3dCollection sortIntersectPts = new Point3dCollection();
                sortPts.ForEach(i => sortIntersectPts.Add(i));
                DBObjectCollection dbObjs = line.GetSplitCurves(sortIntersectPts);
                foreach (DBObject dbObj in dbObjs)
                {
                    splitCurves.Add(dbObj as Curve);
                }
            }
            else
            {
                splitCurves.Add(line);
            }
            if (splitCurves != null && splitCurves.Count > 1)
            {
                line.Dispose();
            }
            for (int i = 0; i < splitCurves.Count; i++)
            {
                Line currentLine = splitCurves[i] as Line;
                Point3d midPt = ThXClipCadOperation.GetMidPt(currentLine.StartPoint, currentLine.EndPoint);
                if (ThXClipCadOperation.IsPointInPolyline(pts, midPt))
                {
                    if (keepInternal) //保留里面
                    {
                        dBObjects.Add(currentLine);
                    }
                     else
                    {
                        currentLine.Dispose();
                    }
                }
                else
                {
                    if (!keepInternal)
                    {
                        dBObjects.Add(currentLine);
                    }
                    else
                    {
                        currentLine.Dispose();
                    }
                }
            }
            return dBObjects;
        }
        /// <summary>
        /// 修剪Polyline
        /// </summary>
        /// <param name="ellipse"></param>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static DBObjectCollection XClip(this Polyline polyline, Point2dCollection pts, bool keepInternal = false)
        {
            DBObjectCollection dBObjects = new DBObjectCollection();
            if(polyline==null)
            {
                return dBObjects;
            }
            Polyline clipBoundaryPline = ThXClipCadOperation.CreatePolyline(pts);
            Point3dCollection intersectPts = new Point3dCollection();
            polyline.IntersectWith(clipBoundaryPline, Intersect.OnBothOperands, intersectPts, IntPtr.Zero, IntPtr.Zero);
            clipBoundaryPline.Dispose();
            bool isGoOn = true;
            if (intersectPts == null || intersectPts.Count == 0)
            {
                isGoOn = false;
            }
            if(isGoOn==false)
            {
                Polyline wpPolyline = ThXClipCadOperation.CreatePolyline(pts);
                for(int i=0;i<polyline.NumberOfVertices;i++)
                {
                    Point3d firstPt = polyline.GetPoint3dAt(i);
                    if (!ThXClipCadOperation.IsPointOnPolyline(wpPolyline, firstPt)) //检查点不在裁剪边界上
                    {
                        if (ThXClipCadOperation.IsPointInPolyline(pts, firstPt)) //线在曲线内
                        {
                            if (keepInternal)
                            {
                                dBObjects.Add(polyline);
                            }
                            else
                            {
                                polyline.Dispose();
                            }
                        }
                        else
                        {
                            if (!keepInternal)
                            {
                                dBObjects.Add(polyline);
                            }
                            else
                            {
                                polyline.Dispose();
                            }
                        }
                        break;
                    }
                }
                if (intersectPts != null)
                {
                    intersectPts.Dispose();
                }
                wpPolyline.Dispose();
                return dBObjects;
            }
            //获取交点在Polyline上哪些分段上
            Dictionary<Point3d, int> ptSegmentIndexDic = new Dictionary<Point3d, int>();
            foreach (Point3d point in intersectPts)
            {
                int index = ThXClipCadOperation.GetPointOnPolylineSegment(polyline, point);
                if (index >= 0)
                {
                    if (!ptSegmentIndexDic.ContainsKey(point))
                    {
                        ptSegmentIndexDic.Add(point, index);
                    }
                }
            }
            List<int> setmentIndexList= ptSegmentIndexDic.OrderBy(i => i.Value).Select(i => i.Value).ToList();
            setmentIndexList = setmentIndexList.Distinct().ToList();
            Dictionary<int, List<Point3d>> breakPtDic = new Dictionary<int, List<Point3d>>();
            foreach(int segmentIndex in setmentIndexList)
            {
                List<Point3d> segmentPtList = ptSegmentIndexDic.Where(i => i.Value == segmentIndex).Select(i => i.Key).ToList();
                breakPtDic.Add(segmentIndex, segmentPtList);
            }
            List<Point3d> sortPts = new List<Point3d>();
            foreach(var dicItem in breakPtDic)
            {
                List<Point3d> segmentPts = dicItem.Value;
                if (segmentPts.Count<=1)
                {
                    sortPts.AddRange(segmentPts);
                    continue;
                }
                SegmentType st = polyline.GetSegmentType(dicItem.Key);
                if (st== SegmentType.Line)
                {
                    LineSegment3d lineSegment3D = polyline.GetLineSegmentAt(dicItem.Key);
                    segmentPts=segmentPts.OrderBy(i => i.DistanceTo(lineSegment3D.StartPoint)).ToList();
                    sortPts.AddRange(segmentPts);
                }
                else if(st == SegmentType.Arc)
                {
                    CircularArc3d circularArc3D = polyline.GetArcSegmentAt(dicItem.Key);
                    bool isClockWise=ThXClipCadOperation.JudgeTwoVectorIsAnticlockwise(circularArc3D.Center.GetVectorTo(circularArc3D.StartPoint),
                        circularArc3D.Center.GetVectorTo(circularArc3D.EndPoint));
                    Dictionary<double, Point3d> ptAngDic = new Dictionary<double, Point3d>();
                    foreach(Point3d pt in segmentPts)
                    {
                        double angle = ThXClipCadOperation.AngleFromXAxis(circularArc3D.Center, pt);
                        if(!ptAngDic.ContainsKey(angle))
                        {
                            ptAngDic.Add(angle, pt);
                        }                        
                    }
                    List<Point3d> arcSortPts= ptAngDic.OrderBy(i => i).Select(i => i.Value).ToList();
                    if(!isClockWise)
                    {
                        arcSortPts.Reverse();
                    }
                    sortPts.AddRange(arcSortPts);
                }
                else
                {
                    continue;
                }
            }
            List<Curve> splitCurves = new List<Curve>();
            Point3dCollection sortIntersecPts = new Point3dCollection();
            sortPts.ForEach(i => sortIntersecPts.Add(i));
            if(sortIntersecPts.Count>0)
            {
                DBObjectCollection dbObjs = polyline.GetSplitCurves(sortIntersecPts);
                if (dbObjs.Count == 0)
                {
                    splitCurves.Add(polyline);
                }
                foreach (DBObject dbObj in dbObjs)
                {
                    splitCurves.Add(dbObj as Curve);
                }
            }
            else
            {
                splitCurves.Add(polyline);
            }
            if(splitCurves.Count>1)
            {
                polyline.Dispose();
            }
            for (int i = 0; i < splitCurves.Count; i++)
            {
                Polyline currentPolyline = splitCurves[i] as Polyline;
                if(currentPolyline==null)
                {
                    continue;
                }
                bool doMark = true;
                int j = 0;
                Point3d checkPt = Point3d.Origin;
                while (doMark)
                {
                    SegmentType st = currentPolyline.GetSegmentType(j);
                    if (st == SegmentType.Line)
                    {
                        LineSegment3d lineSegment = currentPolyline.GetLineSegmentAt(j);
                        if (!lineSegment.StartPoint.IsEqualTo(lineSegment.EndPoint, ThCADCommon.Global_Tolerance))
                        {
                            checkPt = ThXClipCadOperation.GetMidPt(lineSegment.StartPoint, lineSegment.EndPoint);
                            break;
                        }
                    }
                    else if (st == SegmentType.Arc)
                    {
                        CircularArc3d circularArc3d = currentPolyline.GetArcSegmentAt(j);
                        if (!circularArc3d.StartPoint.IsEqualTo(circularArc3d.EndPoint, ThCADCommon.Global_Tolerance))
                        {
                            checkPt = ThXClipCadOperation.GetArcTopPt(circularArc3d.Center, circularArc3d.StartPoint, circularArc3d.EndPoint);
                            break;
                        }
                    }
                    j++;
                }
                if (ThXClipCadOperation.IsPointInPolyline(pts, checkPt))
                {
                    if (keepInternal)
                    {
                        dBObjects.Add(currentPolyline);
                    }
                    else
                    {
                        currentPolyline.Dispose(); //丢弃
                        continue;
                    }
                }
                else
                {
                    if (!keepInternal)
                    {
                        dBObjects.Add(currentPolyline);
                    }
                    else
                    {
                        currentPolyline.Dispose();
                        continue;
                    }
                }
            }
            return dBObjects;
        }
        /// <summary>
        /// 修剪Polyline2d
        /// </summary>
        /// <param name="ellipse"></param>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static DBObjectCollection XClip(this Polyline2d polyline2d, Point2dCollection pts, bool keepInternal = false)
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
        public static DBObjectCollection XClip(this Polyline3d polyline3d, Point2dCollection pts, bool keepInternal = false)
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
        public static DBObjectCollection XClip(this Xline xline, Point2dCollection pts, bool keepInternal = false)
        {
            DBObjectCollection dBObjects = new DBObjectCollection();
            Polyline polyline = ThXClipCadOperation.CreatePolyline(pts);
            Point3dCollection intersectPts = new Point3dCollection();
            polyline.IntersectWith(xline, Intersect.OnBothOperands, intersectPts, IntPtr.Zero, IntPtr.Zero);
            if (intersectPts == null || intersectPts.Count <= 1) //现在Polyline外面
            {
                if (!keepInternal)
                {
                    Entity ent = xline.Clone() as Entity;
                    dBObjects.Add(ent);
                }
                return dBObjects;
            }
            List<Point3d> points = new List<Point3d>();
            points.Add(xline.BasePoint);
            foreach (Point3d pt in intersectPts)
            {
                if (!(pt.DistanceTo(xline.BasePoint) <= 1.0 || pt.DistanceTo(xline.SecondPoint) <= 1.0))
                {
                    points.Add(pt);
                }
            }
            Point3d referencePt = xline.BasePoint + xline.UnitDir.GetNormal().MultiplyBy(double.MaxValue / 2.0);
            points = points.OrderBy(i => i.DistanceTo(referencePt)).ToList();
            List<List<Point3d>> ptPair = new List<List<Point3d>>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                Point3d pt = ThXClipCadOperation.GetMidPt(points[i], points[i + 1]);
                if (ThXClipCadOperation.IsPointInPolyline(pts, pt))
                {
                    if (keepInternal)
                    {
                        ptPair.Add(new List<Point3d> { points[i], points[i + 1] });
                    }
                }
                else
                {
                    if (!keepInternal)
                    {
                        ptPair.Add(new List<Point3d> { points[i], points[i + 1] });
                    }
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
                Line newLine = new Line(startPt, endPt);
                dBObjects.Add(newLine);
            }
            if (!keepInternal)
            {
                Ray newRay1 = new Ray();
                newRay1.BasePoint = points[0];
                newRay1.UnitDir = xline.UnitDir.Negate();

                Ray newRay2 = new Ray();
                newRay2.BasePoint = points[points.Count - 1];
                newRay2.UnitDir = xline.UnitDir;
                dBObjects.Add(newRay1);
                dBObjects.Add(newRay2);
            }
            return dBObjects;
        }
        /// <summary>
        /// 修剪Xline
        /// </summary>
        /// <param name="ellipse"></param>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static DBObjectCollection XClip(this Ray ray, Point2dCollection pts, bool keepInternal = false)
        {
            DBObjectCollection dBObjects = new DBObjectCollection();
            Polyline polyline = ThXClipCadOperation.CreatePolyline(pts);
            Point3dCollection intersectPts = new Point3dCollection();
            polyline.IntersectWith(ray, Intersect.OnBothOperands, intersectPts, IntPtr.Zero, IntPtr.Zero);

            //线在曲线外
            if (intersectPts == null || intersectPts.Count == 0 ||
                (intersectPts.Count==1 && intersectPts[0].DistanceTo(ray.StartPoint)<1.0))
            {
                if (!keepInternal)
                {
                    Entity ent = ray.Clone() as Entity;
                    dBObjects.Add(ent);
                    return dBObjects;
                }
            }
            List<Point3d> points = new List<Point3d>();
            points.Add(ray.BasePoint);
            foreach (Point3d pt in intersectPts)
            {
                if (pt.DistanceTo(ray.BasePoint) > 1.0)
                {
                    points.Add(pt);
                }
            }
            points = points.OrderBy(i => i.DistanceTo(ray.BasePoint)).ToList();
            List<List<Point3d>> ptPair = new List<List<Point3d>>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                Point3d pt = ThXClipCadOperation.GetMidPt(points[i], points[i + 1]);
                if (ThXClipCadOperation.IsPointInPolyline(pts, pt))
                {
                    if (keepInternal)
                    {
                        ptPair.Add(new List<Point3d> { points[i], points[i + 1] });
                    }
                }
                else
                {
                    if (!keepInternal)
                    {
                        ptPair.Add(new List<Point3d> { points[i], points[i + 1] });
                    }
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
                Line newLine = new Line(startPt, endPt);
                newLine.ColorIndex = ray.ColorIndex;
                newLine.Layer = ray.Layer;
                newLine.LineWeight = ray.LineWeight;
                dBObjects.Add(newLine);
            }
            if (!keepInternal)
            {
                Ray newRay = new Ray();
                newRay.BasePoint = points[points.Count - 1];
                newRay.UnitDir = ray.UnitDir;
                dBObjects.Add(newRay);
            }
            return dBObjects;
        }
        /// <summary>
        ///按传入的点分割曲线
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="intersectPts"></param>
        /// <returns></returns>
        private static List<Curve> SplitCurves(Curve curve, Point3dCollection intersectPts)
        {
            if (curve == null || intersectPts == null || intersectPts.Count == 0)
            {
                return new List<Curve>();
            }            
            DBObjectCollection splitCurves = new DBObjectCollection();
            DBObjectCollection needSplitCurves = new DBObjectCollection();
            needSplitCurves.Add(curve);
            foreach (Point3d pt in intersectPts)
            {
                Point3dCollection splitPts = new Point3dCollection();
                splitPts.Add(pt);
                //以下判断分割的点与要被分割的曲线是否相交
                Line horLine = new Line(pt + Vector3d.XAxis.MultiplyBy(-0.5), pt + Vector3d.XAxis.MultiplyBy(0.5));
                Line verLine = new Line(pt + Vector3d.YAxis.MultiplyBy(-0.5), pt + Vector3d.YAxis.MultiplyBy(0.5));
                for (int i = 0; i < needSplitCurves.Count; i++)
                {
                    Curve currentCurve = needSplitCurves[i] as Curve;
                    if (currentCurve == null)
                    {
                        continue;
                    }
                    Point3dCollection tempCollection1 = new Point3dCollection();
                    Point3dCollection tempCollection2 = new Point3dCollection();
                    horLine.IntersectWith(currentCurve, Intersect.OnBothOperands, tempCollection1, IntPtr.Zero, IntPtr.Zero);
                    verLine.IntersectWith(currentCurve, Intersect.OnBothOperands, tempCollection2, IntPtr.Zero, IntPtr.Zero);
                    if (!((tempCollection1 != null && tempCollection1.Count > 0) ||
                        (tempCollection2 != null && tempCollection2.Count > 0)
                        ))//没有交点,不往下分割
                    {
                        splitCurves.Add(currentCurve);
                        continue;
                    }
                    if(tempCollection1!=null)
                    {
                        tempCollection1.Dispose();
                    }
                    if (tempCollection2 != null)
                    {
                        tempCollection2.Dispose();
                    }
                    DBObjectCollection dbObjs = currentCurve.GetSplitCurves(splitPts);
                    if (dbObjs != null && dbObjs.Count > 1) //被分割的曲线已被成功分割
                    {
                        currentCurve.Dispose();
                    }
                    foreach (DBObject splitDbObj in dbObjs)
                    {
                        splitCurves.Add(splitDbObj);
                    }
                }
                horLine.Dispose();
                horLine.Dispose();
                splitPts.Dispose();
                needSplitCurves = new DBObjectCollection();
                needSplitCurves = splitCurves;
                splitCurves = new DBObjectCollection();
            }
            List<Curve> curves = new List<Curve>();
            foreach (DBObject dbObj in needSplitCurves)
            {
                Curve currentCurve = dbObj as Curve;
                if (currentCurve == null)
                {
                    continue;
                }
                curves.Add(currentCurve);
            }
            List<Curve> uniqueCurves = new List<Curve>();
            while (curves.Count > 0)
            {
                Curve currentCurve = curves[0];
                uniqueCurves.Add(currentCurve);
                curves.RemoveAt(0);
                List<Curve> repeatedOBjs =
                    curves.Where(i => (i.StartPoint.IsEqualTo(currentCurve.StartPoint, ThCADCommon.Global_Tolerance) &&
                 i.EndPoint.IsEqualTo(currentCurve.EndPoint, ThCADCommon.Global_Tolerance)) ||
                 (i.StartPoint.IsEqualTo(currentCurve.EndPoint, ThCADCommon.Global_Tolerance) &&
                 i.EndPoint.IsEqualTo(currentCurve.StartPoint, ThCADCommon.Global_Tolerance))).Select(i => i).ToList();

                curves = curves.Where(i => (!(i.StartPoint.IsEqualTo(currentCurve.StartPoint, ThCADCommon.Global_Tolerance) &&
                 i.EndPoint.IsEqualTo(currentCurve.EndPoint, ThCADCommon.Global_Tolerance)) ||
                 (i.StartPoint.IsEqualTo(currentCurve.EndPoint, ThCADCommon.Global_Tolerance) &&
                 i.EndPoint.IsEqualTo(currentCurve.StartPoint, ThCADCommon.Global_Tolerance)))).Select(i => i).ToList(); //把break后，具有相同位置的过滤掉
                if (repeatedOBjs != null && repeatedOBjs.Count > 0) //把重复的Spline删除
                {
                    for (int i = 0; i < repeatedOBjs.Count; i++)
                    {
                        repeatedOBjs[i].Dispose();
                    }
                }
            }
            return uniqueCurves;
        }
    }
}
