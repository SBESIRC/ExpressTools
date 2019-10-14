using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace ThXClip
{
    public static class ThCurveXClipExtension
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
            Polyline polyline = CadOperation.CreatePolyline(pts);
            Point3dCollection intersectPts = new Point3dCollection();
            arc.IntersectWith(polyline, Intersect.OnBothOperands, intersectPts, IntPtr.Zero, IntPtr.Zero);
            if (intersectPts == null || intersectPts.Count == 0) //无交点，
            {
                if (CadOperation.IsPointInPolyline(pts, arc.Center))  //圆心在pts范围内
                {
                    if (keepInternal)
                    {
                        Entity ent = arc.Clone() as Entity;
                        dBObjects.Add(ent);
                    }
                }
                else
                {
                    if (!keepInternal)
                    {
                        Entity ent = arc.Clone() as Entity;
                        dBObjects.Add(ent);
                    }
                }
                return dBObjects;
            }
            else if (intersectPts.Count == 1)
            {
                if (intersectPts[0].DistanceTo(arc.StartPoint) <= 1.0 ||
                    intersectPts[0].DistanceTo(arc.EndPoint) <= 1.0)
                {
                    Point3d arcTopPt = CadOperation.GetArcTopPt(arc.Center, arc.StartPoint, arc.EndPoint);
                    if (CadOperation.IsPointInPolyline(pts, arcTopPt))  //弧顶在pts范围内
                    {
                        if (keepInternal)
                        {
                            Entity ent = arc.Clone() as Entity;
                            dBObjects.Add(ent);
                        }
                    }
                    else
                    {
                        if (!keepInternal)
                        {
                            Entity ent = arc.Clone() as Entity;
                            dBObjects.Add(ent);
                        }
                    }
                    return dBObjects;
                }
            }
            List<Point3d> intersectPtList = new List<Point3d>();
            foreach (Point3d pt in intersectPts)
            {
                if (pt.DistanceTo(arc.StartPoint) > 1.0 && pt.DistanceTo(arc.EndPoint) > 1.0)
                {
                    intersectPtList.Add(pt);
                }
            }
            List<Point3d> sortPts = new List<Point3d>();
            sortPts.Insert(0, arc.StartPoint);
            intersectPtList.ForEach(i => sortPts.Add(i));
            sortPts.Add(arc.EndPoint);
            sortPts = CadOperation.SortArcPts(sortPts, arc.Center);
            List<List<Point3d>> ptPair = new List<List<Point3d>>();
            for (int i = 0; i < sortPts.Count - 1; i++)
            {
                if (sortPts[i].DistanceTo(sortPts[i + 1]) <= 1.0)
                {
                    continue;
                }
                Point3d arcTopPt = CadOperation.GetArcTopPt(arc.Center, sortPts[i], sortPts[i + 1]);
                if (CadOperation.IsPointInPolyline(pts, arcTopPt))
                {
                    if (keepInternal)
                    {
                        ptPair.Add(new List<Point3d> { sortPts[i], sortPts[i + 1] });
                    }
                }
                else
                {
                    if (!keepInternal)
                    {
                        ptPair.Add(new List<Point3d> { sortPts[i], sortPts[i + 1] });
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
                double startAngle = CadOperation.AngleFromXAxis(arc.Center, startPt);
                double endAngle = CadOperation.AngleFromXAxis(arc.Center, endPt);
                Arc newArc = new Arc(arc.Center, arc.Radius, startAngle, endAngle);
                dBObjects.Add(newArc);
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
            Polyline polyline = CadOperation.CreatePolyline(pts);
            Point3dCollection intersectPts = new Point3dCollection();
            polyline.IntersectWith(spline, Intersect.OnBothOperands, intersectPts, IntPtr.Zero, IntPtr.Zero);
            if (intersectPts == null || intersectPts.Count == 0)
            {
                if (CadOperation.IsPointInPolyline(pts, spline.StartPoint))  //圆心在pts范围内
                {
                    if (keepInternal)
                    {
                        Entity ent = spline.Clone() as Entity;
                        dBObjects.Add(ent);
                    }
                }
                else
                {
                    if (!keepInternal)
                    {
                        Entity ent = spline.Clone() as Entity;
                        dBObjects.Add(ent);
                    }
                }
                return dBObjects;
            }
            else
            {
                //暂时不支持
                Entity ent = spline.Clone() as Entity;
                dBObjects.Add(ent);
                return dBObjects;
            }
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
            Polyline polyline = CadOperation.CreatePolyline(pts);
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
                Point3d pt = CadOperation.GetMidPt(points[i], points[i + 1]);
                if (CadOperation.IsPointInPolyline(pts, pt))
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
            Polyline polyline = CadOperation.CreatePolyline(pts);
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
                Point3d pt = CadOperation.GetMidPt(points[i], points[i + 1]);
                if (CadOperation.IsPointInPolyline(pts, pt))
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
        /// 修剪圆
        /// </summary>
        /// <param name="ellipse"></param>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static DBObjectCollection XClip(this Circle circle, Point2dCollection pts, bool keepInternal = false)
        {
            DBObjectCollection dBObjects = new DBObjectCollection();
            Polyline polyline = CadOperation.CreatePolyline(pts);
            Point3dCollection intersectPts = new Point3dCollection();
            circle.IntersectWith(polyline, Intersect.OnBothOperands, intersectPts, IntPtr.Zero, IntPtr.Zero);
            if (intersectPts == null || intersectPts.Count == 0) //无交点，
            {
                if (CadOperation.IsPointInPolyline(pts, circle.Center))  //圆心在pts范围内
                {
                    if (keepInternal)
                    {
                        Entity ent = circle.Clone() as Entity;
                        dBObjects.Add(ent);
                    }
                }
                else
                {
                    if (!keepInternal)
                    {
                        Entity ent = circle.Clone() as Entity;
                        dBObjects.Add(ent);
                    }                   
                }
                return dBObjects;
            }            
            Dictionary<Point3d,double> intersPtAngDic = new Dictionary<Point3d, double>();
            foreach (Point3d pt in intersectPts)
            {
                double angle = CadOperation.AngleFromXAxis(circle.Center, pt);               
                if (!intersPtAngDic.ContainsKey(pt))
                {
                    intersPtAngDic.Add(pt, angle);
                }
            }
            List<Point3d> sortPts = intersPtAngDic.OrderBy(i=>i.Value).Select(i=>i.Key).ToList();
            List<List<Point3d>> ptPair = new List<List<Point3d>>();
            for (int i = 0; i < sortPts.Count - 1; i++)
            {
                if (sortPts[i].DistanceTo(sortPts[i + 1]) <= 1.0)
                {
                    continue;
                }
                Point3d arcTopPt = CadOperation.GetArcTopPt(circle.Center, sortPts[i], sortPts[i + 1]);
                if (CadOperation.IsPointInPolyline(pts, arcTopPt))
                {
                    if (keepInternal)
                    {
                        ptPair.Add(new List<Point3d> { sortPts[i], sortPts[i + 1] });
                    }
                }
                else
                {
                    if (!keepInternal) //保留外部
                    {
                        ptPair.Add(new List<Point3d> { sortPts[i], sortPts[i + 1] });
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
                double startAngle = CadOperation.AngleFromXAxis(circle.Center,startPt);
                double endAngle = CadOperation.AngleFromXAxis(circle.Center, endPt);    
                Arc arc = new Arc(circle.Center,circle.Radius, startAngle, endAngle); //逆时针
                dBObjects.Add(arc);
            }
            Point3d newArcTopPt = CadOperation.GetArcTopPt(circle.Center, sortPts[sortPts.Count - 1], sortPts[0]);
            if (!CadOperation.IsPointInPolyline(pts, newArcTopPt)) //假设弧形顶点不在WipeOut里面，则保留此弧段
            {
                if(!keepInternal)
                {
                    Arc arc = CadOperation.CreateArc(circle.Center, sortPts[sortPts.Count - 1], sortPts[0]);
                    dBObjects.Add(arc);
                }
            }
            else //弧在闭合线内
            {
                if (keepInternal)
                {
                    Arc arc = CadOperation.CreateArc(circle.Center, sortPts[sortPts.Count - 1], sortPts[0]);
                    dBObjects.Add(arc);
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
            Polyline polyline = CadOperation.CreatePolyline(pts);
            Point3dCollection intersectPts = new Point3dCollection();
            ellipse.IntersectWith(polyline, Intersect.OnBothOperands, intersectPts, IntPtr.Zero, IntPtr.Zero);
            if (intersectPts == null || intersectPts.Count == 0)
            {
                if (CadOperation.IsPointInPolyline(pts, ellipse.Center))  //圆心在pts范围内
                {
                    if (keepInternal) //保留内部
                    {
                        Entity ent = ellipse.Clone() as Entity;
                        dBObjects.Add(ent);
                    }
                }
                else
                {
                    if (!keepInternal) //保留外部
                    {
                        Entity ent = ellipse.Clone() as Entity;
                        dBObjects.Add(ent);
                    }
                }
            }
            else if (intersectPts.Count == 1)
            {
                if(intersectPts[0].DistanceTo(ellipse.StartPoint) <= 1.0 ||
                   intersectPts[0].DistanceTo(ellipse.EndPoint) <= 1.0)
                {
                    Point3d checkPt = Point3d.Origin;
                    if (ellipse.StartPoint.DistanceTo(ellipse.EndPoint) <= 1.0)
                    {
                        checkPt = ellipse.Center;
                    }
                    else if (intersectPts[0].DistanceTo(ellipse.StartPoint) <= 1.0)
                    {
                        checkPt = ellipse.EndPoint;
                    }
                    else
                    {
                        checkPt = ellipse.StartPoint;
                    }
                    if (CadOperation.IsPointInPolyline(pts, checkPt)) //弧在闭合圈内
                    {
                        if (keepInternal)
                        {
                            Entity ent = ellipse.Clone() as Entity;
                            dBObjects.Add(ent);                           
                        }
                    }
                    else
                    {
                        if (!keepInternal)
                        {
                            Entity ent = ellipse.Clone() as Entity;
                            dBObjects.Add(ent);                            
                        }
                    }
                    return dBObjects; //不做任何操作
                }
            }
            List<double> angs = new List<double>();            
            double ellipseJiaJiao=ellipse.EndAngle - ellipse.StartAngle;
            bool isCompletedEllipse = false;
            if(ellipseJiaJiao==Math.PI*2) //完整的椭圆
            {
                isCompletedEllipse = true;
            }
            if(!isCompletedEllipse)
            {
                angs.Add(ellipse.StartAngle);
                angs.Add(ellipse.EndAngle);
            }
            foreach (Point3d pt in intersectPts)
            {
                double para = ellipse.GetParameterAtPoint(pt);
                double ang = ellipse.GetAngleAtParameter(para);
                if (angs.IndexOf(ang) < 0)
                {
                    angs.Add(ang);
                }
            }
            angs = angs.OrderBy(i => i).ToList();
            if(!isCompletedEllipse)
            {
                if(angs.IndexOf(ellipse.StartAngle)!=0)
                {
                   angs.Reverse();
                }
            }
            List<List<Point3d>> ptPair = new List<List<Point3d>>();
            bool ynDraw = false;
            for (int i = 0; i < angs.Count - 1; i++)
            {
                double midAng = (angs[i] + angs[i + 1]) / 2.0;
                double para = ellipse.GetParameterAtAngle(midAng);
                Point3d ellipseTopPt = ellipse.GetPointAtParameter(para);
                ynDraw = false;
                if (CadOperation.IsPointInPolyline(pts, ellipseTopPt))
                {
                    if (keepInternal)
                        ynDraw = true;
                }
                else
                {
                    if (!keepInternal)
                        ynDraw = true;
                }
                if(ynDraw)
                {
                    double startPara = ellipse.GetParameterAtAngle(angs[i]);
                    double endPara = ellipse.GetParameterAtAngle(angs[i + 1]);
                    Point3d startPt = ellipse.GetPointAtParameter(startPara);
                    Point3d endPt = ellipse.GetPointAtParameter(endPara);
                    ptPair.Add(new List<Point3d> { startPt, endPt });
                }
            }
            if(isCompletedEllipse)
            {
                ynDraw = false;
                double midAng = (angs[angs.Count-1] + angs[0]) / 2.0;
                double para = ellipse.GetParameterAtAngle(midAng);
                Point3d ellipseTopPt = ellipse.GetPointAtParameter(para);
                if (CadOperation.IsPointInPolyline(pts, ellipseTopPt))
                {
                    if (keepInternal)
                        ynDraw = true;
                }
                else
                {
                    if (!keepInternal)
                        ynDraw = true;
                }
                if(ynDraw)
                {
                    double startPara = ellipse.GetParameterAtAngle(angs[angs.Count - 1]);
                    double endPara = ellipse.GetParameterAtAngle(angs[0]);
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
                Vector3d endVec = ellipse.Center.GetVectorTo(endPt);
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
        public static DBObjectCollection XClip(this Line line, Point2dCollection pts, bool keepInternal = false)
        {           
            DBObjectCollection dBObjects = new DBObjectCollection();
            Polyline polyline = CadOperation.CreatePolyline(pts);
            Point3dCollection intersectPts = new Point3dCollection();
            polyline.IntersectWith(line, Intersect.OnBothOperands, intersectPts, IntPtr.Zero, IntPtr.Zero);
            List<Point3d> points = new List<Point3d>();
            if(intersectPts!=null)
            {
                foreach (Point3d pt in intersectPts)
                {
                    if (pt.DistanceTo(line.StartPoint) > 1.0 && pt.DistanceTo(line.EndPoint) > 1.0)
                    {
                        points.Add(pt);
                    }
                }
            }
            if (points.Count == 0)
            {
                if (CadOperation.IsPointInPolyline(pts, CadOperation.GetMidPt(line.StartPoint ,line.EndPoint))) //线在曲线内
                {
                    if (keepInternal)
                    {
                        Line newLine = line.Clone() as Line;
                        dBObjects.Add(newLine);
                    }
                }
                else
                {
                    if (!keepInternal)
                    {
                        Entity ent = line.Clone() as Entity;
                        dBObjects.Add(ent);
                    }             
                }
                return dBObjects;
            }
            points.Insert(0,line.StartPoint);
            points.Add(line.EndPoint);
            points = points.OrderBy(i => i.DistanceTo(line.StartPoint)).ToList();
            List<List<Point3d>> ptPair = new List<List<Point3d>>();
            bool ynDraw = false;
            for (int i = 0; i < points.Count - 1; i++)
            {
                if (points[i].DistanceTo(points[i + 1]) <= 1.0)
                {
                    continue;
                }
                ynDraw = false;
                Point3d pt = CadOperation.GetMidPt(points[i], points[i + 1]);
                if (CadOperation.IsPointInPolyline(pts, pt))
                {
                    if (keepInternal) //保留里面
                        ynDraw = true;
                }
                else
                {
                    if (!keepInternal)
                        ynDraw = true;
                }
                if(ynDraw)
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
                Line newLine = new Line(startPt, endPt);                       
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
        public static DBObjectCollection XClip(this Polyline polyline, Point2dCollection pts, bool keepInternal = false)
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
                    if (keepInternal)
                    {
                        Polyline newPolyLine = polyline.Clone() as Polyline;
                        dBObjects.Add(newPolyLine);
                    }
                }
                else
                {
                    if (!keepInternal)
                    {
                        Polyline newPolyLine = polyline.Clone() as Polyline;
                        dBObjects.Add(newPolyLine);
                    }
                }
                return dBObjects;
            }
            //获取交点在Polyline上哪些分段上
            Dictionary<Point3d, int> ptSegmentIndexDic = new Dictionary<Point3d, int>();
            foreach (Point3d point in intersectPts)
            {
                int index = CadOperation.GetPointOnPolylineSegment(polyline, point);
                if (index >= 0)
                {
                    if (!ptSegmentIndexDic.ContainsKey(point))
                    {
                        ptSegmentIndexDic.Add(point, index);
                    }
                }
            }
            //拆分多段线
            List<SegmentSplitInf> segmentSplitInfs = new List<SegmentSplitInf>();
            for (int i = 0; i < polyline.NumberOfVertices; i++)
            {
                List<Point3d> splitPts = ptSegmentIndexDic.Where(j => j.Value == i).Select(j => j.Key).ToList();
                if (splitPts == null || splitPts.Count == 0)
                {
                    continue;
                }
                SegmentType st = polyline.GetSegmentType(i);
                bool headHasCurve = true;
                bool endHasCurve = true;
                List<List<Point3d>> splitPoints = new List<List<Point3d>>();
                if (st == SegmentType.Line)
                {
                    LineSegment3d line = polyline.GetLineSegmentAt(i);
                    splitPoints = GetLineSplits(line, pts, splitPts, out headHasCurve, out endHasCurve, keepInternal);
                }
                else if (st == SegmentType.Arc)
                {
                    CircularArc3d arc = polyline.GetArcSegmentAt(i);
                    splitPoints = GetArcSplits(arc, pts, splitPts, out headHasCurve, out endHasCurve, keepInternal);
                }
                for (int j = 0; j < splitPoints.Count; j++)
                {
                    SegmentSplitInf segmentSplitInf = new SegmentSplitInf();
                    segmentSplitInf.Index = i;
                    segmentSplitInf.ST = st;
                    if (headHasCurve && j == 0)
                    {
                        segmentSplitInf.IsHead = true;
                    }
                    if (endHasCurve && j == splitPoints.Count - 1)
                    {
                        segmentSplitInf.IsTail = true;
                    }
                    segmentSplitInf.StartPoint = splitPoints[j][0];
                    segmentSplitInf.EndPoint = splitPoints[j][1];
                    segmentSplitInfs.Add(segmentSplitInf);
                }
            }
            List<AddPolylineInf> addPolylineInfs = new List<AddPolylineInf>();
            for (int i = 0; i < polyline.NumberOfVertices; i++)
            {
                List<SegmentSplitInf> findRes1 = segmentSplitInfs.Where(k => k.Index == i).Select(k => k).ToList();
                if (findRes1 != null && findRes1.Count > 0)  //判断Polyline的分段是否有分割点，如果有过滤掉
                {
                    continue;
                }
                AddPolylineInf addPolylineInf = new AddPolylineInf();
                addPolylineInf.SegmentStartIndex = i;
                addPolylineInf.SegmentEndIndex = i;
                int j = i;
                for (; j < polyline.NumberOfVertices; j++)
                {
                    List<SegmentSplitInf> findRes2 = segmentSplitInfs.Where(k => k.Index == j).Select(k => k).ToList();
                    if (findRes2 != null && findRes2.Count > 0) //Polyline的分段是否有分割点，如果有,则把和之前、后续分段追加到连续的线段上
                    {
                        addPolylineInf.TailSplitIndex = j;

                        SegmentSplitInf headTempSsi = null;
                        int headSplitIndex = -1;
                        if (addPolylineInf.SegmentStartIndex == 0)
                        {
                            headSplitIndex = polyline.NumberOfVertices - 1;
                        }
                        else
                        {
                            headSplitIndex = addPolylineInf.SegmentStartIndex - 1;
                        }
                        headTempSsi = segmentSplitInfs.Where(k => k.Index == headSplitIndex && k.IsTail).Select(k => k).FirstOrDefault();
                        if (headTempSsi != null) //表示首段前有分段
                        {
                            headTempSsi.IsUsed = true;
                            addPolylineInf.HeadSplitIndex = headSplitIndex;
                            addPolylineInf.HeadSplitPt = headTempSsi.StartPoint;
                            if (headTempSsi.ST == SegmentType.Line)
                            {
                                addPolylineInf.HeadIsLine = true;
                            }
                            else
                            {
                                addPolylineInf.HeadIsLine = false;
                            }
                        }
                        SegmentSplitInf tailTempSsi = null;
                        int tailSplitIndex = -1;
                        if (addPolylineInf.SegmentEndIndex == polyline.NumberOfVertices - 1)
                        {
                            tailSplitIndex = 0;
                        }
                        else
                        {
                            tailSplitIndex = addPolylineInf.SegmentEndIndex + 1;
                        }
                        tailTempSsi = segmentSplitInfs.Where(k => k.Index == tailSplitIndex && k.IsHead).Select(k => k).FirstOrDefault();
                        if (tailTempSsi != null) //表示首段前有分段
                        {
                            tailTempSsi.IsUsed = true;
                            addPolylineInf.TailSplitIndex = tailSplitIndex;
                            addPolylineInf.TailSplitPt = tailTempSsi.EndPoint;
                            if (tailTempSsi.ST == SegmentType.Line)
                            {
                                addPolylineInf.TailIsLine = true;
                            }
                            else
                            {
                                addPolylineInf.TailIsLine = false;
                            }
                        }
                        i = j;
                        break;
                    }
                    else
                    {
                        addPolylineInf.SegmentEndIndex = j;
                    }
                }
                addPolylineInfs.Add(addPolylineInf);
            }
            List<SegmentSplitInf> restSegments = segmentSplitInfs.Where(k => k.IsUsed == false).Select(k => k).ToList();
            if (addPolylineInfs.Count == 0 && restSegments.Count == 0) //无分段
            {
                Polyline newPolyline = polyline.Clone() as Polyline;
                dBObjects.Add(newPolyline);
            }
            else
            {
                for (int i = 0; i < addPolylineInfs.Count; i++)
                {
                    Polyline newPolyline = AddSplitSegmengtToPolyline(polyline, addPolylineInfs[i]);
                    if (newPolyline != null && newPolyline.Length > 0.0)
                    {
                        dBObjects.Add(newPolyline);
                    }
                }
                if (restSegments.Count > 0)
                {
                    List<Polyline> polylines =CadOperation.RepairPolylineRestCurve(restSegments, polyline);
                    polylines.ForEach(i => dBObjects.Add(i));
                }
            }
            return dBObjects;
        }
        /// <summary>
        /// 原始线
        /// </summary>
        /// <param name="polyline"></param>
        /// <param name="segmentIndexes"></param>
        /// <param name="headsplitPt"></param>
        /// <param name="tailsplitPt"></param>
        /// <param name="headSplitIndex"></param>
        /// <param name="tailSplitIndex"></param>
        /// <param name="headIsLine"></param>
        /// <param name="tailIsLine"></param>
        /// <returns></returns>
        private static Polyline AddSplitSegmengtToPolyline(Polyline polyline, AddPolylineInf addPolylineInf)
        {
            Polyline newPolyline = new Polyline();
            if (polyline == null || addPolylineInf == null)
            {
                return null;
            }
            int j = 0;
            if (addPolylineInf.HeadIsLine != null) //头部需要添加物体
            {
                double headSW = polyline.GetStartWidthAt(addPolylineInf.HeadSplitIndex); //起始线起点宽度
                double headEW = polyline.GetStartWidthAt(addPolylineInf.HeadSplitIndex + 1); //起始线终点宽度
                Point2d splitPt2d = new Point2d(addPolylineInf.HeadSplitPt.X, addPolylineInf.HeadSplitPt.Y);
                if (addPolylineInf.HeadIsLine == true)
                {
                    newPolyline.AddVertexAt(j++, splitPt2d, 0.0, headSW, headEW);
                }
                else
                {
                    CircularArc2d circularArc2D = polyline.GetArcSegment2dAt(addPolylineInf.HeadSplitIndex);
                    Point2d cenPt = circularArc2D.Center;
                    Point2d arcEp = circularArc2D.EndPoint;
                    double arcBulge = CadOperation.GetBulge(cenPt, splitPt2d, arcEp);
                    newPolyline.AddVertexAt(j++, splitPt2d, arcBulge, headSW, headEW);
                }
            }
            for (int i = addPolylineInf.SegmentStartIndex; i <= addPolylineInf.SegmentEndIndex; i++)
            {
                double bulge = polyline.GetBulgeAt(i);
                double startWidth = polyline.GetStartWidthAt(i);
                double endWidth = polyline.GetStartWidthAt(i + 1);
                Point2d pt = polyline.GetPoint2dAt(i);
                newPolyline.AddVertexAt(j++, pt, bulge, startWidth, endWidth);
            }
            if (addPolylineInf.TailIsLine != null) //尾部需要添加物体
            {
                double tailSW = polyline.GetStartWidthAt(addPolylineInf.TailSplitIndex); //起始线起点宽度
                double tailEW = polyline.GetStartWidthAt(addPolylineInf.TailSplitIndex + 1); //起始线终点宽度
                Point2d splitPt2d = new Point2d(addPolylineInf.TailSplitPt.X, addPolylineInf.TailSplitPt.Y);
                if (addPolylineInf.TailIsLine == true)
                {
                    Point2d startPt = polyline.GetPoint2dAt(addPolylineInf.TailSplitIndex);
                    newPolyline.AddVertexAt(j++, startPt, 0.0, tailSW, tailEW);
                    newPolyline.AddVertexAt(j++, splitPt2d, 0.0, 0.0, 0.0);
                }
                else
                {
                    CircularArc2d circularArc2D = polyline.GetArcSegment2dAt(addPolylineInf.TailSplitIndex);
                    Point2d cenPt = circularArc2D.Center;
                    Point2d arcSp = circularArc2D.StartPoint;
                    double arcBulge = CadOperation.GetBulge(cenPt, arcSp, splitPt2d);
                    newPolyline.AddVertexAt(j++, arcSp, arcBulge, tailSW, tailEW);
                    newPolyline.AddVertexAt(j++, splitPt2d, 0.0, 0.0, 0.0);
                }
            }
            return newPolyline;
        }
        private static List<List<Point3d>> GetLineSplits(LineSegment3d line, Point2dCollection pts, List<Point3d> splitPts,
            out bool headHasCurve, out bool endHasCurve, bool keepInternal = false)
        {
            List<List<Point3d>> returnLinePts = new List<List<Point3d>>();
            splitPts = splitPts.Where(i => i.DistanceTo(line.StartPoint) > 1.0 && i.DistanceTo(line.EndPoint) > 1.0).Select(i => i).ToList();
            List<Point3d> points = new List<Point3d>();
            headHasCurve = false;
            endHasCurve = false;
            points.Add(line.StartPoint);
            if (splitPts != null && splitPts.Count > 0)
            {
                points.AddRange(splitPts);
            }
            points.Add(line.EndPoint);
            points = points.OrderBy(j => j.DistanceTo(line.StartPoint)).ToList();

            List<List<Point3d>> ptPair = new List<List<Point3d>>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                Point3d pt = CadOperation.GetMidPt(points[i], points[i + 1]);
                if (CadOperation.IsPointInPolyline(pts, pt)) //线在多段线内
                {
                    if (!keepInternal)
                        continue;
                }
                else //线在外面 
                {
                    if (keepInternal) //保留里面
                        continue;
                }
                if (i == 0)
                {
                    if (points[i + 1].DistanceTo(line.StartPoint) >= 1.0 &&
                        points[i + 1].DistanceTo(line.EndPoint) >= 1.0)
                    {
                        headHasCurve = true;
                    }
                }
                else if (i == points.Count - 2)
                {
                    if (points[i].DistanceTo(line.StartPoint) >= 2.0 &&
                        points[i].DistanceTo(line.EndPoint) >= 2.0)
                    {
                        endHasCurve = true;
                    }
                }
                ptPair.Add(new List<Point3d> { points[i], points[i + 1] });
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
        private static List<List<Point3d>> GetArcSplits(CircularArc3d arc, Point2dCollection pts, List<Point3d> splitPts,
            out bool headHasCurve, out bool endHasCurve, bool keepInternal = false)
        {
            List<List<Point3d>> returnLinePts = new List<List<Point3d>>();
            headHasCurve = false;
            endHasCurve = false;
            splitPts = splitPts.Where(i => i.DistanceTo(arc.StartPoint) > 1.0 && i.DistanceTo(arc.EndPoint) > 1.0).Select(i => i).ToList();
            Dictionary<Point3d, Point3d> ptDic = new Dictionary<Point3d, Point3d>();
            foreach (Point3d pt in splitPts)
            {
                Point3d orthoPt = CadOperation.GetOrthoPtOnLine(arc.StartPoint, arc.EndPoint, pt);
                List<PtAndLinePos> ptPoses = CadOperation.JudgePtAndLineRelation(arc.StartPoint, arc.EndPoint, orthoPt);
                if (ptPoses.Count == 1 && ptPoses.IndexOf(PtAndLinePos.In) >= 0 && !ptDic.ContainsKey(pt)) //判断交点在线内
                {
                    ptDic.Add(pt, orthoPt);
                }
            }
            List<Point3d> points = ptDic.OrderBy(i => i.Value.DistanceTo(arc.StartPoint)).Select(i => i.Key).ToList();
            points.Insert(0, arc.StartPoint);
            points.Add(arc.EndPoint);
            List<List<Point3d>> ptPair = new List<List<Point3d>>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                Point3d pt = CadOperation.GetMidPt(points[i], points[i + 1]);
                if (CadOperation.IsPointInPolyline(pts, pt)) //如果分段在WipeOut内
                {
                    if (!keepInternal)
                        continue;
                }
                else //如果分段在WipeOut外
                {
                    if (keepInternal)
                        continue;
                }
                if (i == 0)
                {
                    if (points[i + 1].DistanceTo(arc.StartPoint) >= 1.0 &&
                        points[i + 1].DistanceTo(arc.EndPoint) >= 1.0)
                    {
                        headHasCurve = true;
                    }
                }
                else if (i == points.Count - 2)
                {
                    if (points[i].DistanceTo(arc.StartPoint) >= 1.0 &&
                        points[i].DistanceTo(arc.EndPoint) >= 1.0)
                    {
                        endHasCurve = true;
                    }
                }
                ptPair.Add(new List<Point3d> { points[i], points[i + 1] });
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
    }
    public class SegmentSplitInf
    {
        /// <summary>
        /// 在Polyline上的哪一段
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 分割的起点
        /// </summary>
        public Point3d StartPoint { get; set; }
        /// <summary>
        /// 分割的终点
        /// </summary>
        public Point3d EndPoint { get; set; }
        /// <summary>
        /// 此段是头部吗
        /// </summary>
        public bool IsHead { get; set; } = false;
        /// <summary>
        /// 此段是尾部吗
        /// </summary>
        public bool IsTail { get; set; } = false;
        /// <summary>
        /// 是否被使用
        /// </summary>
        public bool IsUsed { get; set; } = false;
        /// <summary>
        /// 线段类型
        /// </summary>
        public SegmentType ST { get; set; }
    }
    class AddPolylineInf
    {
        //假设 一个闭合有四个顶点的多段线，在编号3段和2段有分割点
        //SegmentStartIndex=0,SegmentEndIndex=1,要把三段分割的一部分（如果有）和2段分割的一部分（如果有）附加上{0,1}

        /// <summary>
        /// Polyline连续的分段起始索引
        /// </summary>
        public int SegmentStartIndex { get; set; }
        /// <summary>
        /// Polyline连续的分段起始索引
        /// </summary>
        public int SegmentEndIndex { get; set; }
        /// <summary>
        /// Polyline连续段的第一段是否有分割点
        /// </summary>
        public Point3d HeadSplitPt { get; set; }
        /// <summary>
        /// Polyline连续段的最后一段是否有分割点
        /// </summary>
        public Point3d TailSplitPt { get; set; }
        /// <summary>
        /// 连续分段头部有分割点的索引
        /// </summary>
        public int HeadSplitIndex { get; set; }
        /// <summary>
        /// 连续分段尾部有分割点的索引
        /// </summary>
        public int TailSplitIndex { get; set; }
        /// <summary>
        /// 头部是线，还是圆弧，如果为空，表示头部无分割点
        /// </summary>
        public bool? HeadIsLine { get; set; }
        /// <summary>
        /// 尾部是线，还是圆弧，如果为空，表示尾部无分割点
        /// </summary>
        public bool? TailIsLine { get; set; }
    }
}
