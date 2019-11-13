using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThXClip
{
    public static class ThXClipCurveExtension
    {
        public static double CloseToCurveDis = 1.0;
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
            Point3dCollection intersectPts = GetIntersPoint(arc,pts);           
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
                sortIntersecPts = ThXClipCadOperation.GetNoRepeatedPtCollection(sortIntersecPts);
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
            Point3dCollection intersectPts = GetIntersPoint(spline,pts);            
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
            intersectPts = ThXClipCadOperation.GetNoRepeatedPtCollection(intersectPts);
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
            Point3dCollection intersectPts = GetIntersPoint(circle, pts);
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
                sortIntersecPts = ThXClipCadOperation.GetNoRepeatedPtCollection(sortIntersecPts);
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
            if (Math.Abs(Math.Abs(ThXClipCadOperation.RadToAng(ellipseJiaJiao))-360)<=0.00001) //完整的椭圆
            {
                isCompletedEllipse = true;
            }
            Point3dCollection intersectPts = GetIntersPoint(ellipse,pts);            
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
                    sortIntersecPts = ThXClipCadOperation.GetNoRepeatedPtCollection(sortIntersecPts);
                    DBObjectCollection dbObjs = ellipse.GetSplitCurves(sortIntersecPts);
                    foreach (DBObject dbObj in dbObjs)
                    {
                        splitCurves.Add(dbObj as Ellipse);
                    }
                }
            }
            else
            {
                intersectPts= ThXClipCadOperation.GetNoRepeatedPtCollection(intersectPts);
                splitCurves = SplitCurves(ellipse, intersectPts);
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
            Point3dCollection intersectPts = GetIntersPoint(line, pts);
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
                sortIntersectPts = ThXClipCadOperation.GetNoRepeatedPtCollection(sortIntersectPts);
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
        /// 修剪XClip与WipeOut
        /// </summary>
        /// <param name="wipeout"></param>
        /// <param name="pts">XClip边界</param>
        /// <param name="keepInternal">是否保留内部</param>
        /// <returns></returns>
        public static DBObjectCollection XClip(this Wipeout wipeout, Point2dCollection pts, bool keepInternal = false)
        {
#if ACAD_ABOVE_2012
            DBObjectCollection dBObjects = new DBObjectCollection();
            var doc = ThXClipCadOperation.GetMdiActiveDocument();
            if (wipeout == null)
            {
                return dBObjects;
            }
            try
            {
                IntersectType intersectType = GetXClipWithWipeOut(pts, wipeout);
                if (intersectType == IntersectType.None || intersectType == IntersectType.Contain || intersectType == IntersectType.Included)
                {
                    if (keepInternal)
                    {
                        ThXClipCadOperation.EraseObjIds(wipeout.ObjectId);
                    }
                    else
                    {
                        return dBObjects;
                    }
                }
                else
                {
                    Polyline xclipBoundaryPolyline = ThXClipCadOperation.CreatePolyline(pts); //XClip 边界线
                    Point2dCollection wpBoundaryPts = ThXClipCadOperation.GetWipeOutBoundaryPts(wipeout.ObjectId);
                    Polyline wpBoudaryLine = ThXClipCadOperation.CreatePolyline(wpBoundaryPts, true); //WipeOut边界线
                    Region xClipRegion = ThXClipCadOperation.CreateRegion(xclipBoundaryPolyline);
                    Region wpRegion = ThXClipCadOperation.CreateRegion(wpBoudaryLine);
                    if (keepInternal)
                    {
                        wpRegion.BooleanOperation(BooleanOperationType.BoolIntersect, xClipRegion);
                    }
                    else
                    {
                        wpRegion.BooleanOperation(BooleanOperationType.BoolSubtract, xClipRegion);
                    }
                    //获取新的WipeOut
                    Brep brep = new Brep(wpRegion);
                    List<Curve> curves = new List<Curve>();
                    foreach (var edge in brep.Edges)
                    {
                        var geCurve3d = edge.Curve;
                        if (geCurve3d is ExternalCurve3d geExternalCurve3d)
                        {
                            curves.Add(Curve.CreateFromGeCurve(geExternalCurve3d.NativeCurve));
                        }
                        else
                        {
                            curves.Add(Curve.CreateFromGeCurve(geCurve3d));
                        }
                    }
                    List<List<Curve>> totalRegions = new List<List<Curve>>();
                    while (curves.Count > 0)
                    {
                        List<Curve> subCurves = new List<Curve>();
                        subCurves.Add(curves[0]);
                        curves.RemoveAt(0);
                        for (int i = 0; i < curves.Count; i++)
                        {
                            if ((curves[i].EndPoint.DistanceTo(subCurves[subCurves.Count - 1].StartPoint) <= CloseToCurveDis) ||
                                (curves[i].EndPoint.DistanceTo(subCurves[subCurves.Count - 1].EndPoint) <= CloseToCurveDis))
                            {
                                subCurves.Add(curves[i]);
                                curves.RemoveAt(i);
                                i = -1;
                            }
                        }
                        if (subCurves.Count >= 2)
                        {
                            totalRegions.Add(subCurves);
                        }
                    }
                    for (int i = 0; i < totalRegions.Count; i++)
                    {                        
                        Point2dCollection wpPts = new Point2dCollection();
                        List<List<Point3d>> curvePts= ThXClipCadOperation.GetLoopCurvePts(totalRegions[i]);
                        if(curvePts.Count>1)
                        {
                            curvePts.ForEach(j => wpPts.Add(new Point2d(j[0].X, j[0].Y)));
                            wpPts.Add(wpPts[0]);
                            Wipeout wp = new Wipeout();
                            wp.SetDatabaseDefaults(doc.Database);
                            wp.SetFrom(wpPts, new Vector3d(0.0, 0.0, 0.1));
                            dBObjects.Add(wp);
                        } 
                    }
                    if (dBObjects.Count > 0)
                    {
                        ThXClipCadOperation.EraseObjIds(wipeout.ObjectId);
                    }
                    xclipBoundaryPolyline.Dispose();
                    wpBoudaryLine.Dispose();
                    xClipRegion.Dispose();
                    wpRegion.Dispose();
                }
            }
            catch(System.Exception ex)
            {
                ThXClipUtils.WriteException(ex);
            }
            return dBObjects;
#else
            // Curve.CreateFromGeCurve() was introduced in AutoCAD 2013
            // https://adndevblog.typepad.com/autocad/2012/04/converting-geometry-objects-to-database-entity.html
            throw new NotImplementedException();
#endif
        }

        private static IntersectType GetXClipWithWipeOut(Point2dCollection xclipEdgePts, Wipeout wp)
        {
            IntersectType intersectType = IntersectType.None;
            Polyline xclipBoundaryPolyline = ThXClipCadOperation.CreatePolyline(xclipEdgePts); //XClip 边界线
            Point2dCollection wpBoundaryPts = ThXClipCadOperation.GetWipeOutBoundaryPts(wp.ObjectId);
            Polyline wpBoudaryLine = ThXClipCadOperation.CreatePolyline(wpBoundaryPts, true); //WipeOut边界线
            Point3dCollection intersectPts = GetIntersPoint(wpBoudaryLine, xclipEdgePts);
            if (intersectPts == null || intersectPts.Count <= 1) //判断wipeout是否在XClip里面
            {
                List<Point3d> checkPts = new List<Point3d>();
                for (int i = 0; i < wpBoudaryLine.NumberOfVertices; i++)
                {
                    Point3d currentPt = wpBoudaryLine.GetPoint3dAt(i);
                    if (ThXClipCadOperation.IsClosestToCurve(xclipBoundaryPolyline, currentPt, CloseToCurveDis))
                    {
                        continue;
                    }
                    checkPts.Add(currentPt);
                    break;
                }
                if (checkPts.Count == 0)
                {
                    for (int i = 0; i < wpBoudaryLine.NumberOfVertices; i++)
                    {
                        SegmentType st = wpBoudaryLine.GetSegmentType(i);
                        if (st == SegmentType.Line)
                        {
                            Point3d lineMidPt = ThXClipCadOperation.GetMidPt(wpBoudaryLine.GetLineSegmentAt(i).StartPoint,
                                wpBoudaryLine.GetLineSegmentAt(i).EndPoint);
                            if (ThXClipCadOperation.IsClosestToCurve(xclipBoundaryPolyline, lineMidPt, CloseToCurveDis))
                            {
                                continue;
                            }                            
                            checkPts.Add(lineMidPt);
                            break;
                        }
                        else if (st == SegmentType.Arc)
                        {
                            CircularArc3d circularArc = wpBoudaryLine.GetArcSegmentAt(i);
                            Point3d arcTopPt = ThXClipCadOperation.GetArcTopPt(circularArc.Center, circularArc.StartPoint, circularArc.EndPoint);
                            if (ThXClipCadOperation.IsClosestToCurve(xclipBoundaryPolyline, arcTopPt, CloseToCurveDis))
                            {
                                continue;
                            }
                            checkPts.Add(arcTopPt);
                            break;
                        }
                    }
                }
                if (ThXClipCadOperation.IsPointInPolyline(xclipEdgePts, checkPts[0])) //WipeOut在XClip内
                {
                    intersectType = IntersectType.Contain;
                }
                else
                {
                    checkPts = new List<Point3d>();
                    for (int i = 0; i < xclipEdgePts.Count; i++)
                    {
                        Point3d currentPt = new Point3d(xclipEdgePts[i].X, xclipEdgePts[i].Y,0.0);
                        if(ThXClipCadOperation.IsClosestToCurve(wpBoudaryLine, currentPt,CloseToCurveDis))
                        {
                            continue;
                        }
                        checkPts.Add(currentPt);
                        break;
                    }
                    if (checkPts.Count == 0)
                    {
                        for (int i = 0; i < wpBoudaryLine.NumberOfVertices; i++)
                        {
                            SegmentType st = wpBoudaryLine.GetSegmentType(i);
                            if (st == SegmentType.Line)
                            {
                                Point3d lineMidPt = ThXClipCadOperation.GetMidPt(wpBoudaryLine.GetLineSegmentAt(i).StartPoint,
                                    wpBoudaryLine.GetLineSegmentAt(i).EndPoint);
                                if (ThXClipCadOperation.IsClosestToCurve(wpBoudaryLine, lineMidPt, CloseToCurveDis))
                                {
                                    continue;
                                }
                                checkPts.Add(lineMidPt);
                                break;
                            }
                            else if (st == SegmentType.Arc)
                            {
                                CircularArc3d circularArc = wpBoudaryLine.GetArcSegmentAt(i);
                                Point3d arcTopPt = ThXClipCadOperation.GetArcTopPt(circularArc.Center, circularArc.StartPoint, circularArc.EndPoint);
                                if (ThXClipCadOperation.IsClosestToCurve(wpBoudaryLine, arcTopPt, CloseToCurveDis))
                                {
                                    continue;
                                }
                                checkPts.Add(arcTopPt);
                                break;
                            }
                        }
                    }
                    if (ThXClipCadOperation.IsPointInPolyline(wpBoundaryPts, checkPts[0])) //XClip在WipeOut内
                    {
                        intersectType = IntersectType.Included;
                    }
                }
            }
            else
            {
                Region xClipRegion = ThXClipCadOperation.CreateRegion(xclipBoundaryPolyline);                
                Region wpRegion = ThXClipCadOperation.CreateRegion(wpBoudaryLine);
                double xClipOriginArea = xClipRegion.Area;
                double wipeOutOriginArea = wpRegion.Area;
                xClipRegion.BooleanOperation(BooleanOperationType.BoolIntersect, wpRegion);
                double xClipNewArea = xClipRegion.Area;
                double wipeOutNewArea = wpRegion.Area;
                if(xClipOriginArea- xClipNewArea>0.0)
                {
                    intersectType = IntersectType.Intersect;
                }
                xClipRegion.Dispose();
                wpRegion.Dispose();
            }
            if(xclipBoundaryPolyline.ObjectId != ObjectId.Null)
            {
                ThXClipCadOperation.EraseObjIds(xclipBoundaryPolyline.ObjectId);
            }
            else
            {
                xclipBoundaryPolyline.Dispose();
            }
            if (wpBoudaryLine.ObjectId != ObjectId.Null)
            {
                ThXClipCadOperation.EraseObjIds(wpBoudaryLine.ObjectId);
            }
            else
            {
                wpBoudaryLine.Dispose();
            }
            return intersectType;
        }
        /// <summary>
        /// 修剪Polyline
        /// </summary>
        /// <param name="polyline">传入的Polyline对象</param>
        /// <param name="pts">XClip或WipeOut边界点</param>
        /// <param name="keepInternal">是否保留内部</param>
        /// <returns></returns>
        public static DBObjectCollection XClip(this Polyline polyline, Point2dCollection pts, bool keepInternal = false)
        {
            DBObjectCollection dBObjects = new DBObjectCollection();
            if(polyline==null)
            {
                return dBObjects;
            }
            Polyline wpPolyline = ThXClipCadOperation.CreatePolyline(pts); //XClip或WipeOut边界
            List<Point3d> polylinePts = new List<Point3d>();
            for (int i = 0; i < polyline.NumberOfVertices; i++)
            {
                polylinePts.Add(polyline.GetPoint3dAt(i));
            }            
            Point3dCollection intersectPts = GetIntersPoint(polyline,pts);
            bool closed = polyline.Closed;
            if (polylinePts[0].IsEqualTo(polylinePts[polyline.NumberOfVertices - 1], ThCADCommon.Global_Tolerance)) //如果多段线的起点和终点相同
            {
                if (!closed)
                {
                    closed = true;
                }
            }
            bool isGoOn = true;
            if (intersectPts == null || intersectPts.Count == 0)
            {
                isGoOn = false;
            }
            else if (intersectPts.Count==1)
            {
                if(closed)
                {
                    isGoOn = false;
                }
                else
                {
                    List<int> ptIndexes = ThXClipCadOperation.PointIndex(polylinePts, intersectPts[0]);
                    if(ptIndexes.Count > 0)
                    {
                        if (ptIndexes[0] == 0 || ptIndexes[0] == polyline.NumberOfVertices - 1)
                        {
                            isGoOn = false;
                        }
                    }
                }
            }
            else if (intersectPts.Count==2)
            {
                List<int> firstPtIndexes = ThXClipCadOperation.PointIndex(polylinePts, intersectPts[0]);
                List<int> secondPtIndexes = ThXClipCadOperation.PointIndex(polylinePts, intersectPts[1]);
                if(closed==false && firstPtIndexes.Count>0 && secondPtIndexes.Count>0)
                {
                    if(firstPtIndexes[0]==0 && secondPtIndexes[0] == polyline.NumberOfVertices-1)
                    {
                        isGoOn = false;
                    }
                    else if(secondPtIndexes[0] == 0 && firstPtIndexes[0] == polyline.NumberOfVertices - 1)
                    {
                        isGoOn = false;
                    }
                }
            }
            if(isGoOn==false)
            {               
                List<Point3d> checkPts = new List<Point3d>();
                //先用索引点检查
                for(int i = 0; i < polyline.NumberOfVertices;i++)
                {
                    Point3d currentPt = polyline.GetPoint3dAt(i);
                    if(ThXClipCadOperation.IsClosestToCurve(wpPolyline, currentPt, CloseToCurveDis))
                    {
                        continue;
                    }
                    checkPts.Add(currentPt);
                    break;
                }
                if(checkPts.Count==0)
                {
                    for (int i = 0; i < polyline.NumberOfVertices; i++)
                    {
                       SegmentType st= polyline.GetSegmentType(i);
                       if(st== SegmentType.Line)
                        {
                            Point3d lineMidPt = ThXClipCadOperation.GetMidPt(polyline.GetLineSegmentAt(i).StartPoint, polyline.GetLineSegmentAt(i).EndPoint);
                            if(ThXClipCadOperation.IsClosestToCurve(wpPolyline, lineMidPt, CloseToCurveDis))
                            {
                                continue;
                            }
                            checkPts.Add(lineMidPt);
                            break;
                        }
                       else if(st == SegmentType.Arc)
                        {
                            CircularArc3d circularArc = polyline.GetArcSegmentAt(i);
                            Point3d arcTopPt= ThXClipCadOperation.GetArcTopPt(circularArc.Center, circularArc.StartPoint, circularArc.EndPoint);
                            if (ThXClipCadOperation.IsClosestToCurve(wpPolyline, arcTopPt, CloseToCurveDis))
                            {
                                continue;
                            }
                            checkPts.Add(arcTopPt);
                            break;
                        }
                    }
                }
                if(checkPts.Count>0)
                {
                    if (ThXClipCadOperation.IsPointInPolyline(pts, checkPts[0])) //线在曲线内
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
                }
                else 
                {
                    dBObjects.Add(polyline); //保留原对象
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
                segmentPtList = ThXClipCadOperation.GetNoRepeatedPtList(segmentPtList);
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
                    List<Point3d> arcSortPts= ptAngDic.OrderBy(i => i.Key).Select(i => i.Value).ToList();
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
            sortIntersecPts = ThXClipCadOperation.GetNoRepeatedPtCollection(sortIntersecPts);
            if (sortIntersecPts.Count>0)
            {
                DBObjectCollection dbObjs = polyline.GetSplitCurves(sortIntersecPts);
                if (dbObjs.Count == 0)
                {
                    splitCurves.Add(polyline);
                }
                foreach (DBObject dbObj in dbObjs)
                {
                    Polyline splitPolyline = dbObj as Polyline;
                    bool isValid = false;
                    for(int i=0;i< splitPolyline.NumberOfVertices;i++)
                    {
                        if(splitPolyline.GetSegmentType(i) == SegmentType.Line)
                        {
                          LineSegment3d tempLine=  splitPolyline.GetLineSegmentAt(i);
                            if(tempLine.Length>0.0)
                            {
                                isValid = true;
                                break;
                            }
                        }
                        else if(splitPolyline.GetSegmentType(i) == SegmentType.Arc)
                        {
                           CircularArc3d tempArc= splitPolyline.GetArcSegmentAt(i);
                            if (tempArc.EndAngle!=tempArc.StartAngle)
                            {
                                isValid = true;
                                break;
                            }
                        }
                    }
                    if(isValid)
                    {
                        splitCurves.Add(dbObj as Curve);
                    }
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
                int j = 0;
                List<Point3d> checkPts = new List<Point3d>();
                while (j< currentPolyline.NumberOfVertices)
                {
                    SegmentType st = currentPolyline.GetSegmentType(j);
                    if (st == SegmentType.Line)
                    {
                        LineSegment3d lineSegment = currentPolyline.GetLineSegmentAt(j);
                        if (!ThXClipCadOperation.IsClosestToCurve(wpPolyline, lineSegment.StartPoint, CloseToCurveDis))
                        {
                            checkPts.Add(lineSegment.StartPoint);
                        }
                        else if (!ThXClipCadOperation.IsClosestToCurve(wpPolyline, lineSegment.EndPoint, CloseToCurveDis))
                        {
                            checkPts.Add(lineSegment.EndPoint);
                        }
                        else
                        {
                            Point3d midPt = ThXClipCadOperation.GetMidPt(lineSegment.StartPoint, lineSegment.EndPoint);
                            if (!ThXClipCadOperation.IsClosestToCurve(wpPolyline, midPt, CloseToCurveDis))
                            {
                                checkPts.Add(midPt);
                            }
                        }
                    }
                    else if (st == SegmentType.Arc)
                    {
                        CircularArc3d circularArc3d = currentPolyline.GetArcSegmentAt(j);
                        if (!ThXClipCadOperation.IsClosestToCurve(wpPolyline, circularArc3d.StartPoint, CloseToCurveDis))
                        {
                            checkPts.Add(circularArc3d.StartPoint);
                        }
                        else if (!ThXClipCadOperation.IsClosestToCurve(wpPolyline, circularArc3d.EndPoint, CloseToCurveDis))
                        {
                            checkPts.Add(circularArc3d.EndPoint);
                        }
                        else
                        {
                            Point3d arcTopPt = ThXClipCadOperation.GetArcTopPt(circularArc3d.Center, circularArc3d.StartPoint, circularArc3d.EndPoint);
                            if (!ThXClipCadOperation.IsClosestToCurve(wpPolyline, arcTopPt, CloseToCurveDis))
                            {
                                checkPts.Add(arcTopPt);
                            }
                        }
                    }
                    j++;
                    if(checkPts.Count>0)
                    {
                        break;
                    }
                }
                if(checkPts.Count>0)
                {
                    if (ThXClipCadOperation.IsPointInPolyline(pts, checkPts[0]))
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
                else
                {
                    dBObjects.Add(currentPolyline);
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
        /// <summary>
        /// 返回曲线和XClip边界实际相交点
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="pts"></param>
        /// <returns></returns>
        private static Point3dCollection GetIntersPoint(Curve curve, Point2dCollection pts)
        {
            Point3dCollection actualIntersPts = new Point3dCollection(); //实际交点   
            Point3dCollection intersectPts = new Point3dCollection();
            Plane plane = new Plane(Point3d.Origin, Vector3d.ZAxis);
            Polyline clipBoundaryPline = ThXClipCadOperation.CreatePolyline(pts);
            clipBoundaryPline.IntersectWith(curve, Intersect.OnBothOperands, plane, intersectPts, IntPtr.Zero, IntPtr.Zero);
            clipBoundaryPline.Dispose();
            plane.Dispose();
            if (intersectPts != null && intersectPts.Count > 0)
            {
                foreach (Point3d intersPt in intersectPts)
                {
                    Line verticalLine = new Line(intersPt, new Point3d(intersPt.X, intersPt.Y, intersPt.Z + 1000));
                    Point3dCollection currentIntersectPts = new Point3dCollection();
                    verticalLine.IntersectWith(curve, Intersect.ExtendThis, currentIntersectPts, IntPtr.Zero, IntPtr.Zero);
                    if (currentIntersectPts != null)
                    {
                        foreach (Point3d currentIntersect in currentIntersectPts)
                        {
                            actualIntersPts.Add(currentIntersect);
                        }
                    }
                    verticalLine.Dispose();
                    currentIntersectPts.Dispose();
                }
                intersectPts.Dispose();
            }
            if (actualIntersPts!=null && actualIntersPts.Count>0)
            {
                actualIntersPts = ThXClipCadOperation.GetNoRepeatedPtCollection(actualIntersPts);
            }
            for (int i = 0; i < actualIntersPts.Count; i++)
            {
                Point3d closePt = curve.GetClosestPointTo(actualIntersPts[i], true);
                actualIntersPts[i] = closePt;
            }
            return actualIntersPts;
        }
    }
    public enum IntersectType
    {
        /// <summary>
        /// 不相交
        /// </summary>
        None,
        /// <summary>
        /// 相交
        /// </summary>
        Intersect,
        /// <summary>
        /// 包含
        /// </summary>
        Contain,
        /// <summary>
        /// 被包含
        /// </summary>
        Included
    }
}
