using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices.Filters;
using Autodesk.AutoCAD.Internal;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThXClip
{
    public class ThXClipCadOperation
    {
        public static Tolerance Global_Tolerance = new Tolerance(1e-1, 1e-1);
        public static List<List<Point3d>> GetLoopCurvePts(List<Curve> curves)
        {
            List<List<Point3d>> ptList = new List<List<Point3d>>();
            while (curves.Count>0)
            {
                ptList.Add(new List<Point3d> { curves[0].StartPoint, curves[0].EndPoint });
                curves.RemoveAt(0);
                for (int i=0;i< curves.Count;i++)
                {
                    bool preEpCloseNextSp = ptList[ptList.Count - 1][1].DistanceTo(curves[i].StartPoint) <= 1.0 ? true : false;
                    bool preEpCloseNextEp = ptList[ptList.Count - 1][1].DistanceTo(curves[i].EndPoint) <= 1.0 ? true : false;
                    if(preEpCloseNextSp || preEpCloseNextEp)
                    {
                        if (preEpCloseNextSp)
                        {
                            ptList.Add(new List<Point3d> { curves[i].StartPoint, curves[i].EndPoint });
                        }
                        else if (preEpCloseNextEp)
                        {
                            ptList.Add(new List<Point3d> { curves[i].EndPoint, curves[i].StartPoint });
                        }
                        curves.RemoveAt(i);
                        i = -1;
                    }
                }
                if(curves.Count>0 || ptList[ptList.Count-1][1].DistanceTo(ptList[0][0])>2.0) //
                {
                    ptList = new List<List<Point3d>>();
                    break;
                }
            }
            return ptList;
        }

        public static Region CreateRegion(Curve polyline)
        {
            try
            {
                using (DBObjectCollection curves = new DBObjectCollection())
                {
                    polyline.Explode(curves);
                    DBObjectCollection regions = Region.CreateFromCurves(curves);
                    return (Region)regions[0];
                }
            }
            catch
            {
                return null;
            }
        }

        public static bool IsClosestToCurve(Curve curve,Point3d pt,double range)
        {
            Point3d closePt= curve.GetClosestPointTo(pt,true);
            if(closePt==null)
            {
                return false;
            }
            if(closePt.DistanceTo(pt)<=range)
            {
                return true;
            }
            return false;
        }
        public static bool IsGreaterThanOrEqualTo(int major, int minor)
        {
            Version version = Application.Version;
            if (version.Major > major)
            {
                return true;
            }
            else if (version.Major == major)
            {
                return version.Minor >= minor;
            }
            else
            {
                return false;
            }
        }
        public static void Dispose(List<DBObject> dbObjs)
        {
            for(int i=0;i<dbObjs.Count;i++)
            {
                if (dbObjs[i]!=null)
                {
                    dbObjs[i].Dispose();
                }
            }
        }
        public static void Dispose(DBObject dbObj)
        {
            if (dbObj != null)
            {
                dbObj.Dispose();
            }
        }
        public static void UnlockedLayers(List<string> layerNameList)
        {
            if (layerNameList == null || layerNameList.Count == 0)
            {
                return;
            }
            Document doc = GetMdiActiveDocument();
            using (Transaction trans=doc.TransactionManager.StartTransaction())
            {
                LayerTable lt = trans.GetObject(doc.Database.LayerTableId,OpenMode.ForRead) as LayerTable;
                foreach(string layerName in layerNameList)
                {
                    if(string.IsNullOrEmpty(layerName))
                    {
                        continue;
                    }
                    if(lt.Has(layerName))
                    {
                        LayerTableRecord ltr = trans.GetObject(lt[layerName],OpenMode.ForRead) as LayerTableRecord;
                        if(ltr.IsLocked)
                        {
                            ltr.UpgradeOpen();
                            ltr.IsLocked = false;
                            ltr.DowngradeOpen();
                        }
                    }
                }
                trans.Commit();
            }
        }

        /// <summary>
        /// 返回被锁定的层
        /// </summary>
        /// <returns></returns>
        public static List<string> UnlockedAllLayers()
        {
            List<string> lockedLayerNames = new List<string>();
            Document doc = GetMdiActiveDocument();
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                LayerTable lt = trans.GetObject(doc.Database.LayerTableId, OpenMode.ForRead) as LayerTable;
                foreach (var id in lt)
                {
                    LayerTableRecord ltr = trans.GetObject(id, OpenMode.ForRead) as LayerTableRecord;
                    if (ltr.IsLocked)
                    {
                        ltr.UpgradeOpen();
                        ltr.IsLocked = false;
                        lockedLayerNames.Add(ltr.Name);
                        ltr.DowngradeOpen();
                    }
                }
                trans.Commit();
            }
            return lockedLayerNames;
        }
        public static void LockedLayers(List<string> layerNameList)
        {
            if(layerNameList==null || layerNameList.Count==0)
            {
                return;
            }
            Document doc = GetMdiActiveDocument();
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                LayerTable lt = trans.GetObject(doc.Database.LayerTableId, OpenMode.ForRead) as LayerTable;
                foreach (string layerName in layerNameList)
                {
                    if (string.IsNullOrEmpty(layerName))
                    {
                        continue;
                    }
                    if (lt.Has(layerName))
                    {
                        LayerTableRecord ltr = trans.GetObject(lt[layerName], OpenMode.ForRead) as LayerTableRecord;
                        if (!ltr.IsLocked)
                        {
                            ltr.UpgradeOpen();
                            ltr.IsLocked = true;
                            ltr.DowngradeOpen();
                        }
                    }
                }
                trans.Commit();
            }
        }
        /// <summary>
        /// 获取直线外的一点在直线上的垂足点
        /// </summary>
        /// <param name="lineSp"></param>
        /// <param name="lineEp"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static Point3d GetOrthoPtOnLine(Point3d lineSp,Point3d lineEp,Point3d pt)
        {
            Vector3d lineVec = lineSp.GetVectorTo(lineEp);
            Plane plane = new Plane(lineSp, lineVec);
            Matrix3d ucsMt = Matrix3d.WorldToPlane(plane);
            Matrix3d wcsMt = Matrix3d.PlaneToWorld(plane);
            Point3d newPt=pt.TransformBy(ucsMt);
            newPt = new Point3d(0, 0, newPt.Z);
            newPt=newPt.TransformBy(wcsMt);
            plane.Dispose();
            return newPt;
        }
        /// <summary>
        /// 判断点与线的关系
        /// </summary>
        /// <param name="lineSp"></param>
        /// <param name="lineEp"></param>
        /// <param name="checkPt"></param>
        /// <returns></returns>
        public static List<PtAndLinePos> JudgePtAndLineRelation(Point3d lineSp, Point3d lineEp, Point3d checkPt)
        {
            List<PtAndLinePos> posList = new List<PtAndLinePos>();
            Vector3d lineVec = lineSp.GetVectorTo(lineEp);
            Plane plane = new Plane(lineSp, lineVec);
            Matrix3d ucsMt = Matrix3d.WorldToPlane(plane);
            Matrix3d wcsMt = Matrix3d.PlaneToWorld(plane);
            Point3d newPt = checkPt.TransformBy(ucsMt);
            if(newPt.X>0 || newPt.Y>0)
            {
                posList.Add(PtAndLinePos.Outer);
            }
            else
            {
                if(newPt.DistanceTo(Point3d.Origin)==0.0)
                {
                    posList.Add(PtAndLinePos.StartPort);
                    posList.Add(PtAndLinePos.In);
                }
                else if (newPt.DistanceTo(Point3d.Origin) == lineVec.Length)
                {
                    posList.Add(PtAndLinePos.EndPort);
                    posList.Add(PtAndLinePos.In);
                }
                else if(newPt.DistanceTo(Point3d.Origin) > 0.0 && newPt.DistanceTo(Point3d.Origin) < lineVec.Length)
                {
                    posList.Add(PtAndLinePos.In);
                }
                else
                {
                    posList.Add(PtAndLinePos.Extend);
                }
            }
            plane.Dispose();
            return posList;
        }        
        public static void NewUCS(string ucsName,Point3d origin ,Vector3d xVec,Vector3d yVec)
        {
            // Get the current document and database, and start a transaction
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the UCS table for read
                UcsTable acUCSTbl;
                acUCSTbl = acTrans.GetObject(acCurDb.UcsTableId,
                                                OpenMode.ForRead) as UcsTable;

                UcsTableRecord acUCSTblRec;

                // Check to see if the "New_UCS" UCS table record exists
                if (acUCSTbl.Has(ucsName) == false)
                {
                    acUCSTblRec = new UcsTableRecord();
                    acUCSTblRec.Name = ucsName;

                    // Open the UCSTable for write
                    acUCSTbl.UpgradeOpen();

                    // Add the new UCS table record
                    acUCSTbl.Add(acUCSTblRec);
                    acTrans.AddNewlyCreatedDBObject(acUCSTblRec, true);

                    acUCSTblRec.Dispose();
                }
                else
                {
                    acUCSTblRec = acTrans.GetObject(acUCSTbl[ucsName],
                                                    OpenMode.ForWrite) as UcsTableRecord;
                }
                acUCSTblRec.Origin = origin;
                acUCSTblRec.XAxis = xVec;
                acUCSTblRec.YAxis = yVec;

                // Open the active viewport
                ViewportTableRecord acVportTblRec;
                acVportTblRec = acTrans.GetObject(acDoc.Editor.ActiveViewportId,
                                                    OpenMode.ForWrite) as ViewportTableRecord;

                // Display the UCS Icon at the origin of the current viewport
                acVportTblRec.IconAtOrigin = true;
                acVportTblRec.IconEnabled = true;

                // Set the UCS current
                acVportTblRec.SetUcs(acUCSTblRec.ObjectId);
                acDoc.Editor.UpdateTiledViewportsFromDatabase();

                // Display the name of the current UCS
                UcsTableRecord acUCSTblRecActive;
                acUCSTblRecActive = acTrans.GetObject(acVportTblRec.UcsName,
                                                        OpenMode.ForRead) as UcsTableRecord;
                // If a point was entered, then translate it to the current UCS
                // Translate the point from the current UCS to the WCS
                //Matrix3d newMatrix = new Matrix3d();
                //newMatrix = Matrix3d.AlignCoordinateSystem(Point3d.Origin,
                //                                            Vector3d.XAxis,
                //                                            Vector3d.YAxis,
                //                                            Vector3d.ZAxis,
                //                                            acVportTblRec.Ucs.Origin,
                //                                            acVportTblRec.Ucs.Xaxis,
                //                                            acVportTblRec.Ucs.Yaxis,
                //                                            acVportTblRec.Ucs.Zaxis);
                acTrans.Commit();
            }
        }
        public static Point3d GetPtOnEllipse(Ellipse ellipse,Point3d pt)
        {
            Point3d returnPt = Point3d.Origin;           
            Matrix3d mt = Matrix3d.WorldToPlane(ellipse.Normal);
            Point3d pt1=pt.TransformBy(mt); //转到Ocs
            Point3d pt2 = ellipse.Center + ellipse.MajorAxis;
            pt2 = pt2.TransformBy(mt);

            Point3d origin = ellipse.Center.TransformBy(mt);

            Vector3d vec1 = origin.GetVectorTo(pt1); //传入点与椭圆中心的向量
            Vector3d vec2 = origin.GetVectorTo(pt2); //椭圆主轴点与椭圆中心的向量
            double ang= vec2.GetAngleTo(vec1); //主轴与Vec1的向量
            //若结果为正，则向量b在a的逆时针方向 否则，b在a的顺时针方向 若结果为0，则a与b共线
            double res = vec2.X * vec1.Y - vec1.X * vec2.Y;  //vec2 = (x1,y1)    vec1 = (x2,y2) a×b = x1y2 - x2y1 
            if (res<0) //顺时针
            {
                ang = ang * -1.0;
            }
            double para = ellipse.GetParameterAtAngle(ang);
            returnPt = ellipse.GetPointAtParameter(para);
            return returnPt;
        }
        public static Point3d GetPtOnEllipseByCross(Ellipse ellipse, Point3d pt)
        {
            Point3d returnPt = Point3d.Origin;
            double radius = ellipse.MajorRadius + ellipse.MinorRadius;
            Point3d extendPt = GetExtentPt(ellipse.Center, pt, radius);
            Line line = new Line(ellipse.Center, extendPt);
            Point3dCollection pts = new Point3dCollection();
            ellipse.IntersectWith(line, Intersect.OnBothOperands, pts, IntPtr.Zero, IntPtr.Zero);
            if(pts!=null && pts.Count>0)
            {
                returnPt = pts[0];
            }
            else
            {
                returnPt = GetPtOnEllipse(ellipse, pt);
            }
            return returnPt;
        }
        public static Point3d GetExtentPt(Point3d startPt,Point3d endPt,double length)
        {
            Vector3d vec = endPt - startPt;
            return startPt+vec.GetNormal().MultiplyBy(length);
        }
        /// <summary>
        /// 判断平面两个向量，是否逆时针
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static bool JudgeTwoVectorIsAnticlockwise(Vector3d vec1 ,Vector3d vec2)
        {
            //a = (x1,y1)    b = (x2,y2) a×b = x1y2 - x2y1 
            Vector3d normal= vec1.CrossProduct(vec2);
            Plane plane = new Plane(Point3d.Origin, normal);
            Matrix3d mt = Matrix3d.WorldToPlane(plane);
            Vector3d newVec1= vec1.TransformBy(mt);
            Vector3d newVec2 = vec2.TransformBy(mt);
            double res = newVec1.X * newVec2.Y - newVec2.X * newVec1.Y;
            if (res > 0) //若结果为正,向量b在a的逆时针方向
            {
                return true;
            }
            else //b在a的顺时针方向
            {
                return false;
            }
        }
        /// <summary>
        /// 判断b向量与a向量是逆时针
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool JudgeTwoVectorIsAnticlockwise(Vector2d a, Vector2d b)
        {
            //a = (x1,y1) b = (x2,y2) a×b = x1y2 - x2y1 
            //若结果为0，则a与b共线
            double res = a.X * b.Y - b.X * a.Y;
            if (res > 0) //若结果为正,向量b在a的逆时针方向
            {
                return true;
            }
            else //b在a的顺时针方向
            {
                return false;
            }
        }
        public static Arc CreateArc(Point3d cenPt,Point3d arcSp,Point3d arcEp)
        {
            Arc arc = new Arc();
            arc.Center = cenPt;
            arc.Radius = cenPt.DistanceTo(arcSp);
            double angle1 = AngleFromXAxis(cenPt, arcSp);
            double angle2 = AngleFromXAxis(cenPt, arcEp);
            arc.StartAngle = angle1;
            arc.EndAngle = angle2;
            return arc;
        }
        public static double AngleFromXAxis(Point3d pt1, Point3d pt2)
        {
            Vector2d vector = new Vector2d(pt2.X-pt1.X,pt2.Y-pt1.Y);
            return vector.Angle;
        }
        public static List<Point3d> SortArcPts(List<Point3d> pts, Point3d centPt)
        {
            List<Point3d> sortPts = new List<Point3d>();
            Dictionary<Point3d, double> ptAngDic = new Dictionary<Point3d, double>();
            foreach (Point3d pt in pts)
            {
                double ang = ThXClipCadOperation.AngleFromXAxis(centPt, pt);
                if(!ptAngDic.ContainsKey(pt))
                {
                    ptAngDic.Add(pt, ang);
                }
            }
            sortPts = ptAngDic.OrderBy(i => i.Value).Select(i => i.Key).ToList();
            return sortPts;
        }
        /// <summary>
        /// 弧度转角度
        /// </summary>
        /// <param name="rad"></param>
        /// <returns></returns>
        public static double RadToAng(double rad)
        {
            return rad / Math.PI * 180.0;
        }
        /// <summary>
        /// 角度转弧度
        /// </summary>
        /// <param name="ang"></param>
        /// <returns></returns>
        public static double AngToRad(double ang)
        {
            return ang / 180.0 * Math.PI;
        }
        /// <summary>
        /// 获取两点的中点
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <returns></returns>
        public static Point3d GetMidPt(Point3d pt1, Point3d pt2)
        {
            return new Point3d((pt1.X + pt2.X) / 2.0, (pt1.Y + pt2.Y) / 2.0, (pt1.Z + pt2.Z) / 2.0);
        }
        // Select object inside a polyline
        //  https://forums.autodesk.com/t5/net/select-object-inside-a-polyline/td-p/6018866
        public static PromptSelectionResult SelectByPolyline(Editor ed,
            Polyline pline,
            PolygonSelectionMode mode)
        {
            Point3dCollection polygon = new Point3dCollection();
            for (int i = 0; i < pline.NumberOfVertices; i++)
            {
                polygon.Add(pline.GetPoint3dAt(i));
            }
            PromptSelectionResult result;
            ViewTableRecord view = ed.GetCurrentView();
            ed.ZoomObject(pline.ObjectId);
            if (mode == PolygonSelectionMode.Crossing)
                result = ed.SelectCrossingPolygon(polygon);
            else
                result = ed.SelectWindowPolygon(polygon);
            ed.SetCurrentView(view);
            return result;
        }
        public static PromptSelectionResult SelectByPolyline(Editor ed, Point3dCollection polygon,
           PolygonSelectionMode mode)
        {
            PromptSelectionResult result;
            if (mode == PolygonSelectionMode.Crossing)
                result = ed.SelectCrossingPolygon(polygon);
            else
                result = ed.SelectWindowPolygon(polygon);
            return result;
        }
        public static PromptSelectionResult SelectByPolyline(Editor ed, Point2dCollection polygon,
    PolygonSelectionMode mode)
        {
            PromptSelectionResult result;
            Point3dCollection pts = new Point3dCollection();
            foreach(Point2d pt in polygon)
            {
                pts.Add(new Point3d(pt.X,pt.Y, 0));
            }
            if (mode == PolygonSelectionMode.Crossing)
                result = ed.SelectCrossingPolygon(pts);
            else
                result = ed.SelectWindowPolygon(pts);
            return result;
        }
        public static List<Point3d> GetNoRepeatedPtList(List<Point3d> ptList)
        {
            List<Point3d> newPtList = new List<Point3d>();
            foreach(Point3d pt in ptList)
            {
                List<Point3d> resList=newPtList.Where(i => i.IsEqualTo(pt, ThXClipCadOperation.Global_Tolerance)).Select(i => i).ToList();
                if (resList==null || resList.Count==0)
                {
                    newPtList.Add(pt);
                }
            }
            return newPtList;
        }
        public static Point2dCollection GetNoRepeatedPtCollection(Point2dCollection pts)
        {
            Point2dCollection resPts = new Point2dCollection();
            foreach (Point2d pt in pts)
            {
                bool isExisted = false;
                for(int i=0;i< resPts.Count;i++)
                {
                    if(resPts[i].IsEqualTo(pt, ThXClipCadOperation.Global_Tolerance))
                    {
                        isExisted = true;
                        break;
                    }
                }
                if(!isExisted)
                {
                    resPts.Add(pt);
                }
            }
            return resPts;
        }
        public static List<int> PointIndex(Point3dCollection pts,Point3d pt)
        {
            List<int> indexList = new List<int>();
            for(int i=0;i< pts.Count;i++)
            {
                if(pts[i].IsEqualTo(pt, ThXClipCadOperation.Global_Tolerance))
                {
                    indexList.Add(i);
                }
            }
            return indexList;
        }
        public static List<int> PointIndex(List<Point3d> pts, Point3d pt)
        {
            List<int> indexList = new List<int>();
            for (int i = 0; i < pts.Count; i++)
            {
                if (pts[i].IsEqualTo(pt, ThXClipCadOperation.Global_Tolerance))
                {
                    indexList.Add(i);
                }
            }
            return indexList;
        }
        public static Point3dCollection GetNoRepeatedPtCollection(Point3dCollection pts)
        {
            Point3dCollection resPts = new Point3dCollection();
            foreach (Point3d pt in pts)
            {
                bool isExisted = false;
                for (int i = 0; i < resPts.Count; i++)
                {
                    if (resPts[i].IsEqualTo(pt, ThXClipCadOperation.Global_Tolerance))
                    {
                        isExisted = true;
                        break;
                    }
                }
                if (!isExisted)
                {
                    resPts.Add(pt);
                }
            }
            return resPts;
        }
        public static PromptSelectionResult SelectByPolyline(Editor ed,
            Polyline pline,
            PolygonSelectionMode mode, SelectionFilter filter)
        {
            Point3dCollection polygon = new Point3dCollection();
            for (int i = 0; i < pline.NumberOfVertices; i++)
            {
                polygon.Add(pline.GetPoint3dAt(i));
            }
            PromptSelectionResult result;
            ViewTableRecord view = ed.GetCurrentView();
            ed.ZoomObject(pline.ObjectId);
            if (mode == PolygonSelectionMode.Crossing)
                result = ed.SelectCrossingPolygon(polygon, filter);
            else
                result = ed.SelectWindowPolygon(polygon, filter);
            ed.SetCurrentView(view);
            return result;
        }
        public static PromptSelectionResult SelectByPolyline(Editor ed,
    Point3dCollection polygon,
    PolygonSelectionMode mode, SelectionFilter filter)
        {
            PromptSelectionResult result;
            if (mode == PolygonSelectionMode.Crossing)
                result = ed.SelectCrossingPolygon(polygon, filter);
            else
                result = ed.SelectWindowPolygon(polygon, filter);
            return result;
        }
        public static PromptSelectionResult SelectByPolyline(Editor ed,
   Point2dCollection polygon,
   PolygonSelectionMode mode, SelectionFilter filter)
        {
            PromptSelectionResult result;
            Point3dCollection pts = new Point3dCollection();
            foreach(Point2d pt in polygon)
            {
                pts.Add(new Point3d(pt.X,pt.Y,0.0));
            }
            if (mode == PolygonSelectionMode.Crossing)
                result = ed.SelectCrossingPolygon(pts, filter);
            else
                result = ed.SelectWindowPolygon(pts, filter);
            return result;
        }
        public static PromptSelectionResult SelectByRectangle(Editor ed,Point3d pt1, Point3d pt2, PolygonSelectionMode mode)
        {
            Point3dCollection polygon = new Point3dCollection();
            double minX = Math.Min(pt1.X, pt2.X);
            double minY = Math.Min(pt1.Y, pt2.Y);
            double minZ = Math.Min(pt1.Z, pt2.Z);

            double maxX = Math.Max(pt1.X, pt2.X);
            double maxY = Math.Max(pt1.Y, pt2.Y);
            double maxZ = Math.Max(pt1.Z, pt2.Z);
            polygon.Add(new Point3d(minX, minY, minZ));
            polygon.Add(new Point3d(maxX, minY, minZ));
            polygon.Add(new Point3d(maxX, maxY, minZ));
            polygon.Add(new Point3d(minX, maxY, minZ));
            PromptSelectionResult result;
            if (mode == PolygonSelectionMode.Crossing)
                result = ed.SelectCrossingPolygon(polygon);
            else
                result = ed.SelectWindowPolygon(polygon);
            return result;
        }
        public static void ZoomObject(Editor ed, Point3dCollection pts)
        {
            Point3dCollection polygon = new Point3dCollection();
            foreach (Point3d pt in pts)
            {
                if(polygon.IndexOf(pt)<0)
                {
                    polygon.Add(pt);
                }
            }
            if(polygon.Count<2)
            {
                return;
            }
            if(polygon.Count==2)
            {
                Point3d pt1 = polygon[0];
                Point3d pt2 = polygon[1];
                double minX = Math.Min(pt1.X, pt2.X);
                double minY = Math.Min(pt1.Y, pt2.Y);
                double minZ = Math.Min(pt1.Z, pt2.Z);

                double maxX = Math.Max(pt1.X, pt2.X);
                double maxY = Math.Max(pt1.Y, pt2.Y);
                double maxZ = Math.Max(pt1.Z, pt2.Z);

                polygon = new Point3dCollection();
                polygon.Add(new Point3d(minX, minY, minZ));
                polygon.Add(new Point3d(maxX, minY, minZ));
                polygon.Add(new Point3d(maxX, maxY, minZ));
                polygon.Add(new Point3d(minX, maxY, minZ));
            }           
            Polyline3d polyline = new Polyline3d(Poly3dType.SimplePoly, polygon, true);
            List<ObjectId> objectIds = ThXClipCadOperation.AddToBlockTable(polyline);
            ed.ZoomObject(objectIds[0]);
            EraseObjIds(objectIds[0]);
        }
        public static void ZoomObject(Editor ed, Point3d minPt,Point3d maxPt)
        {
            Point3dCollection pts = new Point3dCollection();
            pts.Add(minPt);
            pts.Add(maxPt);
            ZoomObject(ed, pts);
        }
        /// <summary>
        /// 让用户选择要处理的问题
        /// </summary>
        /// <returns></returns>
        public static List<ObjectId> GetSelectObjects()
        {
            List<ObjectId> objIds = new List<ObjectId>();
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            TypedValue[] tvs = new TypedValue[] { new TypedValue((int)DxfCode.Start,"Insert")};
            SelectionFilter sf = new SelectionFilter(tvs);            
            PromptSelectionResult psr= ed.GetSelection(sf);
            if(psr.Status==PromptStatus.OK)
            {
                objIds = psr.Value.GetObjectIds().ToList();
            }
            return objIds;
        }
        /// <summary>
        /// 把点转到直线上
        /// </summary>
        /// <param name="lineSp"></param>
        /// <param name="lineEp"></param>
        /// <param name="transPt"></param>
        /// <param name="isSucess"></param>
        /// <param name="isInLine"></param>
        /// <returns></returns>
        public static Point3d TransPtToLine(Point3d lineSp,Point3d lineEp,Point3d transPt,out bool isSucess,out bool isInLine)
        {
            Point3d resPt = transPt;
            isSucess = true;
            isInLine = true;
             if (lineSp.IsEqualTo(lineEp, ThCADCommon.Global_Tolerance))
            {
                isSucess = false;
                isInLine = false;
            }
             else
            {
                Vector3d vec = lineSp.GetVectorTo(lineEp);
                Plane plane = new Plane(lineSp, vec);
                Matrix3d wcsToUcs = Matrix3d.WorldToPlane(plane);
                Matrix3d ucsToWcs = Matrix3d.PlaneToWorld(plane);
                transPt = transPt.TransformBy(wcsToUcs);
                if(!(transPt.Z>0 && transPt.Z< vec.Length))
                {
                    isInLine = false;
                }                
                transPt = new Point3d(0,0, transPt.Z);
                resPt = transPt.TransformBy(ucsToWcs);
                plane.Dispose();
            }
            return resPt;
        }
        public static Document GetMdiActiveDocument()
        {
            return Application.DocumentManager.MdiActiveDocument;
        }        
        /// <summary>
        /// 创建WipeOut
        /// </summary>
        /// <param name="db"></param>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static ObjectId CreateWipeout(Database db,Point2dCollection pts)
        {
            ObjectId wipeOutId = ObjectId.Null;
            Transaction tr =
              db.TransactionManager.StartTransaction();
            using (tr)
            {
                BlockTable bt =
                  (BlockTable)tr.GetObject(
                    db.BlockTableId,
                    OpenMode.ForRead,
                    false
                  );
                BlockTableRecord btr =
                  (BlockTableRecord)tr.GetObject(
                    bt[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite,
                    false
                  );
                Wipeout wo = new Wipeout();
                wo.SetDatabaseDefaults(db);
                wo.SetFrom(pts, new Vector3d(0.0, 0.0, 0.1));

                wipeOutId=btr.AppendEntity(wo);
                tr.AddNewlyCreatedDBObject(wo, true);
                tr.Commit();
            }
            return wipeOutId;
        }

        // Move the wipeouts to the bottom of the specified
        // block definitions

        public static ObjectIdCollection MoveWipeoutsToBottom(
          Transaction tr, ObjectIdCollection ids
        )
        {
            // The IDs of any block references we find
            // to return to the call for updating

            var brIds = new ObjectIdCollection();

            // We only need to get this once
            var wc = RXClass.GetClass(typeof(Wipeout));
            // Take a copy of the IDs passed in, as we'll modify the
            // original list for the caller to use

            var btrIds = new ObjectId[ids.Count];
            ids.CopyTo(btrIds, 0);

            // Loop through the blocks passed in, opening each one

            foreach (var btrId in btrIds)
            {
                var btr =
                  (BlockTableRecord)tr.GetObject(
                    btrId, OpenMode.ForWrite
                  );

                // Collect the wipeouts in the block

                var wipeouts = new ObjectIdCollection();
                foreach (ObjectId id in btr)
                {
                    var ent = (Entity)tr.GetObject(id, OpenMode.ForRead);
                    if (ent.GetRXClass().IsDerivedFrom(wc))
                    {
                        wipeouts.Add(id);
                    }
                }

                // Move the collected wipeouts to the bottom

                if (wipeouts.Count > 0)
                {
                    // Modify the draw order table, if we have wipepouts

                    var dot =
                      (DrawOrderTable)tr.GetObject(
                        btr.DrawOrderTableId, OpenMode.ForWrite
                      );
                    dot.MoveToBottom(wipeouts);

                    // Collect the block references to this block, to pass
                    // back to the calling function for updating

                    var btrBrIds = btr.GetBlockReferenceIds(false, false);
                    foreach (ObjectId btrBrId in btrBrIds)
                    {
                        brIds.Add(btrBrId);
                    }
                }
                else
                {
                    ids.Remove(btrId);
                }
            }
            return brIds;
        }
        /// <summary>
        /// 获取所有XClipBoundary
        /// </summary>
        public static void RetrieveXClipBoundary(bool drawBoundary=false)
        {
            List<Point2dCollection> point2DCollections = new List<Point2dCollection>();
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            try
            {
                if (ed.SelectImplied().Status != PromptStatus.OK) throw new System.Exception("Nothing has been pre-selected!");

                RXClass BlockReferenceRXClass = RXClass.GetClass(typeof(BlockReference));
                using (Transaction tr = Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId id in ed.SelectImplied().Value.GetObjectIds())
                    {
                        if (id.ObjectClass == BlockReferenceRXClass)
                        {
                            BlockReference blkRef = (BlockReference)tr.GetObject(id, OpenMode.ForRead);
                            if (blkRef.ExtensionDictionary != ObjectId.Null)
                            {
                                DBDictionary extdict = (DBDictionary)tr.GetObject(blkRef.ExtensionDictionary, OpenMode.ForRead);
                                if (extdict.Contains("ACAD_FILTER"))
                                {
                                    DBDictionary dict = (DBDictionary)tr.GetObject(extdict.GetAt("ACAD_FILTER"), OpenMode.ForRead);
                                    if (dict.Contains("SPATIAL"))
                                    {
                                        SpatialFilter filter = (SpatialFilter)tr.GetObject(dict.GetAt("SPATIAL"), OpenMode.ForRead);
                                        if(drawBoundary)
                                        {
                                            point2DCollections.Add(DrawPolygonPts(blkRef.Database, filter.Definition.Normal,
                                                filter.ClipSpaceToWorldCoordinateSystemTransform, filter.Definition.GetPoints()));
                                            DrawPolygon(blkRef.Database, filter.Definition.Normal, 
                                                filter.ClipSpaceToWorldCoordinateSystemTransform, filter.Definition.GetPoints());
                                        }                                        
                                    }
                                }
                            }
                        }
                    }

                    tr.Commit();
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage(Environment.NewLine + ex.Message);
            }
        }
        /// <summary>
        /// 获取Xclip的边界点
        /// </summary>
        /// <param name="blkRef"></param>
        /// <returns></returns>
        public static Point2dCollection RetrieveXClipBoundary(BlockReference blkRef)
        {
            Point2dCollection boundPts = new Point2dCollection();          
            try
            {
                RXClass BlockReferenceRXClass = RXClass.GetClass(typeof(BlockReference));
                using (Transaction tr = Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
                {
                    if (blkRef.ExtensionDictionary != ObjectId.Null)
                    {
                        DBDictionary extdict = (DBDictionary)tr.GetObject(blkRef.ExtensionDictionary, OpenMode.ForRead);
                        if (extdict.Contains("ACAD_FILTER"))
                        {
                            DBDictionary dict = (DBDictionary)tr.GetObject(extdict.GetAt("ACAD_FILTER"), OpenMode.ForRead);
                            if (dict.Contains("SPATIAL"))
                            {                               
                                SpatialFilter filter = (SpatialFilter)tr.GetObject(dict.GetAt("SPATIAL"), OpenMode.ForRead);
                                Point2dCollection point2DCollection = DrawPolygonPts(blkRef.Database, filter.Definition.Normal,
                                        filter.ClipSpaceToWorldCoordinateSystemTransform, filter.Definition.GetPoints());
                                if(point2DCollection != null && point2DCollection.Count>0)
                                {
                                    boundPts = point2DCollection;
                                }
                            }
                        }
                    }
                    tr.Commit();
                }
            }
            catch (System.Exception ex)
            {
                ThXClipUtils.WriteException(ex);
            }
            return boundPts;
        }
        public static List<Point2dCollection> RetrieveWipeOutBoundaryFromBlkRef(BlockReference blkRef,Transaction trans)
        {
            List<Point2dCollection> boundPts = new List<Point2dCollection>();
            RXClass BlockReferenceRXClass = RXClass.GetClass(typeof(BlockReference));
            BlockTableRecord btr = trans.GetObject(blkRef.BlockTableRecord,OpenMode.ForRead) as BlockTableRecord;
            foreach (var objId in btr)
            {
                DBObject dbObj = trans.GetObject(objId,OpenMode.ForRead);
                if(dbObj is Wipeout)
                {
                    Wipeout wipeout = dbObj as Wipeout;
                    Point2dCollection pts=wipeout.GetClipBoundary();
                    boundPts.Add(pts);
                }
                else if(dbObj is BlockReference)
                {
                    BlockReference newBr = dbObj as BlockReference;
                    List<Point2dCollection> newBrBoundPts = RetrieveWipeOutBoundaryFromBlkRef(newBr, trans);
                    if(newBrBoundPts!=null && newBrBoundPts.Count>0)
                    {
                        boundPts.AddRange(newBrBoundPts);
                    }
                }
            }
            return boundPts;
        }
        /// <summary>
        /// 绘制Polygon
        /// </summary>
        /// <param name="db"></param>
        /// <param name="normal"></param>
        /// <param name="mat"></param>
        /// <param name="vertices"></param>
        /// <returns></returns>
        public static ObjectId DrawPolygon(Database db, Vector3d normal, Matrix3d mat, Point2dCollection vertices)
        {
            ObjectId ret = ObjectId.Null;

            Transaction tr = db.TransactionManager.TopTransaction;
            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
            using (Polyline pl = new Polyline())
            {
                pl.SetDatabaseDefaults();
                pl.ColorIndex = 3;
                pl.Closed = true;
                for (int i = 0; i < vertices.Count; i++)
                {
                    pl.AddVertexAt(0, vertices[i], 0, 0, 0);
                }
                pl.TransformBy(mat);
                btr.AppendEntity(pl);
                tr.AddNewlyCreatedDBObject(pl, true);
                ret = pl.ObjectId;
            }
            return ret;
        }
        public static Point2dCollection DrawPolygonPts(Database db, Vector3d normal, Matrix3d mat, Point2dCollection vertices)
        {
            Point2dCollection pts = new Point2dCollection();
            Transaction tr = db.TransactionManager.TopTransaction;
            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
            using (Polyline pl = new Polyline())
            {
                pl.SetDatabaseDefaults();
                pl.Closed = true;
                for (int i = 0; i < vertices.Count; i++)
                {
                    pl.AddVertexAt(0, vertices[i], 0, 0, 0);
                }
                pl.TransformBy(mat);
                for (int i = 0; i < vertices.Count; i++)
                {
                    pts.Add(pl.GetPoint2dAt(i));
                }
            }
            return pts;
        }
        /// <summary>
        /// 判断点是否在多段上
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static bool IsPointOnPolyline(Polyline pl, Point3d pt)
        {
            bool isOn = false;
            for (int i = 0; i < pl.NumberOfVertices; i++)
            {
                Curve3d seg = null;
                SegmentType segType = pl.GetSegmentType(i);
                if (segType == SegmentType.Arc)
                    seg = pl.GetArcSegmentAt(i);
                else if (segType == SegmentType.Line)
                    seg = pl.GetLineSegmentAt(i);

                if (seg != null)
                {
                    isOn = seg.IsOn(pt);
                    if (isOn)
                        break;
                }
            }
            return isOn;
        }
        /// <summary>
        /// 获取点在Polyline哪一段
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static int GetPointOnPolylineSegment(Polyline pl, Point3d pt)
        {
            int index = -1;
            for (int i = 0; i < pl.NumberOfVertices; i++)
            {
                Curve3d seg = null;
                SegmentType segType = pl.GetSegmentType(i);
                if (segType == SegmentType.Arc)
                    seg = pl.GetArcSegmentAt(i);
                else if (segType == SegmentType.Line)
                    seg = pl.GetLineSegmentAt(i);

                if (seg != null)
                {
                    if(seg.IsOn(pt,ThCADCommon.Global_Tolerance))
                    {
                        index = i;
                        break;
                    }
                }
            }
            return index;
        }
        /// <summary>
        /// 判断点在一个密闭的多段线内
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static bool IsPointInPolyline(Point2dCollection pts,Point2d pt)
        {
            int i;
            int j;
            bool result = false;
            for (i = 0, j = pts.Count - 1; i < pts.Count; j = i++)
            {
                if ((pts[i].Y > pt.Y) != (pts[j].Y > pt.Y) &&
                    (pt.X < (pts[j].X - pts[i].X) * (pt.Y - pts[i].Y) / (pts[j].Y - pts[i].Y) + pts[i].X))
                {
                    result = !result;
                }
            }
            return result;
        }
        public static bool IsPointInPolyline(Point2dCollection pts, Point3d pt)
        {
            bool result = IsPointInPolyline(pts, new Point2d(pt.X, pt.Y));
            return result;
        }
        public static bool IsPointInPolyline(Point3dCollection pts, Point3d pt)
        {
            Point2dCollection newPts = new Point2dCollection();
            foreach(Point3d ptItem in pts)
            {
                newPts.Add(new Point2d(ptItem.X, ptItem.Y));
            }
            bool result = IsPointInPolyline(newPts, new Point2d(pt.X, pt.Y));
            return result;
        }
        /// <summary>
        /// 创建没有圆弧的多段线
        /// </summary>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static Polyline CreatePolyline(Point2dCollection pts,bool isClosed=true)
        {
            Polyline polyline = new Polyline();
            if(pts.Count==2)
            {
                Point2d minPt = pts[0];
                Point2d maxPt = pts[1];
                Vector2d vec= minPt.GetVectorTo(maxPt);
                if(vec.IsParallelTo(Vector2d.XAxis) || vec.IsParallelTo(Vector2d.YAxis))
                {
                    isClosed = false;                    
                }
                else
                {
                    double minX = Math.Min(pts[0].X, pts[1].X);
                    double minY = Math.Min(pts[0].Y, pts[1].Y);
                    double maxX = Math.Max(pts[0].X, pts[1].X);
                    double maxY = Math.Max(pts[0].Y, pts[1].Y);
                    pts = new Point2dCollection();
                    pts.Add(new Point2d(minX, minY));
                    pts.Add(new Point2d(maxX, minY));
                    pts.Add(new Point2d(maxX, maxY));
                    pts.Add(new Point2d(minX, maxY));
                }
            }
            for(int i=0;i<pts.Count;i++)
            {
                polyline.AddVertexAt(i, pts[i], 0, 0, 0);
            }
            if(isClosed)
            {
                polyline.Closed = true;
            }
            return polyline;
        }
        public static Polyline3d CreatePolyline3d(Point3dCollection pts)
        {
            Polyline3d polyline = new Polyline3d();
            if (pts.Count == 2)
            {
                Point3d minPt = pts[0];
                Point3d maxPt = pts[1];
                double minX = Math.Min(pts[0].X, pts[1].X);
                double minY = Math.Min(pts[0].Y, pts[1].Y);
                double maxX = Math.Max(pts[0].X, pts[1].X);
                double maxY = Math.Max(pts[0].Y, pts[1].Y);
                pts = new Point3dCollection();
                pts.Add(new Point3d(minX, minY,minPt.Z));
                pts.Add(new Point3d(maxX, minY, minPt.Z));
                pts.Add(new Point3d(maxX, maxY, minPt.Z));
                pts.Add(new Point3d(minX, maxY, minPt.Z));
            }
            polyline = new Polyline3d(Poly3dType.SimplePoly, pts, true);
            return polyline;
        }
        public static Polyline2d CreatePolyline2d(Point3dCollection pts)
        {
            Polyline2d polyline = new Polyline2d();
            if (pts.Count == 2)
            {
                Point3d minPt = pts[0];
                Point3d maxPt = pts[1];
                double minX = Math.Min(pts[0].X, pts[1].X);
                double minY = Math.Min(pts[0].Y, pts[1].Y);
                double maxX = Math.Max(pts[0].X, pts[1].X);
                double maxY = Math.Max(pts[0].Y, pts[1].Y);
                pts = new Point3dCollection();
                pts.Add(new Point3d(minX, minY, minPt.Z));
                pts.Add(new Point3d(maxX, minY, minPt.Z));
                pts.Add(new Point3d(maxX, maxY, minPt.Z));
                pts.Add(new Point3d(minX, maxY, minPt.Z));
            }
            DoubleCollection bulges = new DoubleCollection();
            for(int i=0;i<pts.Count;i++)
            {
                bulges.Add(0.0);
            }
            polyline = new Polyline2d(Poly2dType.SimplePoly, pts,0.0, true,0.0,0.0, bulges);
            return polyline;
        }
        /// <summary>
        /// 获取圆弧的凸度
        /// </summary>
        /// <param name="startAngle"></param>
        /// <param name="endAngle"></param>
        /// <returns></returns>
        public static double GetBulge(double startAngle,double endAngle)
        {
            double dAlfa = endAngle - startAngle;
            if (dAlfa < 0.0)//如果终点角度小于起点角度
            {
                dAlfa = 2 * Math.PI + dAlfa;
            }
            double dBulge = 0.0;
            dBulge = Math.Tan((dAlfa) / 4.0);
            return dBulge;
        }
        public static double GetBulge(Point3d cenPt,Point3d arcSp,Point3d arcEp)
        {
            Vector3d startVec = cenPt.GetVectorTo(arcSp);
            Vector3d endVec = cenPt.GetVectorTo(arcEp);
            double startAng = Vector3d.XAxis.GetAngleTo(startVec);
            double endAng = Vector3d.XAxis.GetAngleTo(endVec);
            double ang = endAng - startAng;
            double bulge= Math.Tan((ang) / 4.0); 
            return bulge;
        }
        public static double GetBulge(Point2d cenPt, Point2d arcSp, Point2d arcEp)
        {
            Vector2d startVec = cenPt.GetVectorTo(arcSp);
            Vector2d endVec = cenPt.GetVectorTo(arcEp);
            double ang = endVec.Angle - startVec.Angle;
            if(ang<0)
            {
                ang = 2 * Math.PI + ang;
            }
            bool res= JudgeTwoVectorIsAnticlockwise(startVec, endVec);
            if(!res) //逆时针
            {
                ang = ang-2*Math.PI;
            }           
            double bulge = Math.Tan((ang) / 4.0);
            return bulge;
        }
        /// <summary>
        /// 插入图块
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="blockName"></param>
        /// <param name="position"></param>
        /// <param name="scale"></param>
        /// <param name="rotateAngle"></param>
        /// <returns></returns>
        public static ObjectId InsertBlockReference(string layer, string blockName, Point3d position, Scale3d scale, double rotateAngle)
        {
            ObjectId blockRefId= ObjectId.Null;//存储要插入的块参照的Id
            Document doc = GetMdiActiveDocument();
            Database db = doc.Database;//获取数据库对象
            using (Transaction trans=db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId,OpenMode.ForRead) as BlockTable; //以读的方式打开块表
                if (bt.Has(blockName))
                {
                    //以写的方式打开空间（模型空间或图纸空间）
                    BlockTableRecord modelSpace = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace],OpenMode.ForWrite);
                    //创建一个块参照并设置插入点
                    BlockReference br = new BlockReference(position, bt[blockName]);
                    br.ScaleFactors = scale;//设置块参照的缩放比例
                    br.Layer = layer;//设置块参照的层名
                    br.Rotation = rotateAngle;//设置块参照的旋转角度
                    ObjectId btrId = bt[blockName];//获取块表记录的Id
                                                   //打开块表记录
                    BlockTableRecord record = (BlockTableRecord)trans.GetObject(btrId, OpenMode.ForRead);
                    //添加可缩放性支持
                    if (record.Annotative == AnnotativeStates.True)
                    {
                        ObjectContextCollection contextCollection = db.ObjectContextManager.GetContextCollection("ACDB_ANNOTATIONSCALES");
                        ObjectContexts.AddContext(br, contextCollection.GetContext("1:1"));
                    }
                    blockRefId = modelSpace.AppendEntity(br);//在空间中加入创建的块参照
                    trans.AddNewlyCreatedDBObject(br, true);//通知事务处理加入创建的块参照
                    modelSpace.DowngradeOpen();
                }
                trans.Commit();
            }
            return blockRefId;//返回添加的块参照的Id
        }
        public static List<ObjectId> AddToBlockTable(params Entity[] entities)
        {
            List<ObjectId> objIds = new List<ObjectId>();//存储要插入的块参照的Id
            Document doc = GetMdiActiveDocument();
            Database db = doc.Database;//获取数据库对象
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable; //以读的方式打开块表
                BlockTableRecord modelSpace = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                modelSpace.UpgradeOpen();
                for (int i=0;i<entities.Length;i++)
                {
                    objIds.Add(modelSpace.AppendEntity(entities[i]));
                    trans.AddNewlyCreatedDBObject(entities[i], true);
                }
                modelSpace.DowngradeOpen();
                trans.Commit();
            }            
            return objIds;
        }
        public static void EraseObjIds(params ObjectId[] objIds)
        {
            Document doc = GetMdiActiveDocument();
            Database db = doc.Database;//获取数据库对象
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                for (int i = 0; i < objIds.Length; i++)
                {
                    if(objIds[i].IsErased)
                    {
                        continue;
                    }
                    DBObject dbObj = trans.GetObject(objIds[i], OpenMode.ForWrite);
                    dbObj.Erase();
                }
                trans.Commit();
            }
        }
        public static Point3d GetArcTopPt(Point3d cenPt, Point3d arcSp, Point3d arcEp)
        {
            Point3d arcTopPt = Point3d.Origin;
            double startAngle = ThXClipCadOperation.AngleFromXAxis(cenPt, arcSp);
            double endAngle = ThXClipCadOperation.AngleFromXAxis(cenPt, arcEp);
            double jiaJiao = endAngle - startAngle;
            jiaJiao = (jiaJiao + Math.PI * 2.0) % (Math.PI * 2.0);
            double radius = cenPt.DistanceTo(arcSp);
            Point3d midPt = ThXClipCadOperation.GetMidPt(arcSp, arcEp);
            if (jiaJiao <= Math.PI)
            {
                arcTopPt = ThXClipCadOperation.GetExtentPt(cenPt, midPt, radius);
            }
            else
            {
                arcTopPt = ThXClipCadOperation.GetExtentPt(cenPt, midPt, -1.0 * radius);
            }
            return arcTopPt;
        }
        /// <summary>
        /// 炸块
        /// </summary>
        /// <param name="br">块</param>
        /// <param name="keepUnvisibleEnts">保留隐藏的物体</param>
        /// <returns></returns>
        public static List<Entity> Explode(BlockReference br,bool keepUnVisible=true)
        {
            List<Entity> entities = new List<Entity>();
            DBObjectCollection collection = new DBObjectCollection();
            br.Explode(collection);
            foreach (DBObject obj in collection)
            {
                if (obj is BlockReference)
                {                    
                    var newBr = obj as BlockReference;
                    if (!keepUnVisible && newBr.Visible==false)
                    {
                        continue;
                    }
                    var childEnts = Explode(newBr, keepUnVisible);
                    if (childEnts != null)
                    {
                        entities.AddRange(childEnts);
                    }
                }
                else if (obj is Entity)
                {
                    Entity ent = obj as Entity;
                    if(!keepUnVisible && ent.Visible == false)
                    {
                        continue;
                    }
                    entities.Add(obj as Entity);
                }
            }
            return entities;
        }
        /// <summary>
        /// 获取传入物体范围的对角点
        /// </summary>
        /// <param name="objIds"></param>
        /// <returns></returns>
        public static List<Point3d> GetObjBoundingPoints(List<ObjectId> objIds)
        {
            List<Point3d> boundingPoints = new List<Point3d>();
            Document doc = GetMdiActiveDocument();
            List<Point3d> minPtList = new List<Point3d>();
            List<Point3d> maxPtList = new List<Point3d>();
            using (Transaction trans=doc.TransactionManager.StartTransaction())
            {
                for(int i=0;i<objIds.Count;i++)
                {
                   DBObject dbObj=  trans.GetObject(objIds[i],OpenMode.ForRead);
                   if(dbObj.Bounds!=null && dbObj.Bounds.HasValue)
                    {
                        minPtList.Add(dbObj.Bounds.Value.MinPoint);
                        maxPtList.Add(dbObj.Bounds.Value.MaxPoint);
                    }
                }
                trans.Commit();
            }
            if(minPtList.Count>0 && maxPtList.Count>0)
            {
                double minX = minPtList.OrderBy(i => i.X).Select(i=>i.X).FirstOrDefault();
                double minY = minPtList.OrderBy(i => i.Y).Select(i => i.Y).FirstOrDefault();
                double minZ = minPtList.OrderBy(i => i.Z).Select(i => i.Z).FirstOrDefault();

                double maxX = maxPtList.OrderBy(i => i.X).Select(i => i.X).FirstOrDefault();
                double maxY = maxPtList.OrderBy(i => i.Y).Select(i => i.Y).FirstOrDefault();
                double maxZ = maxPtList.OrderBy(i => i.Z).Select(i => i.Z).FirstOrDefault();

                boundingPoints.Add(new Point3d(minX, minY, minZ));
                boundingPoints.Add(new Point3d(maxX, maxY, maxZ));
            }
            return boundingPoints;
        }
        /// <summary>
        /// 炸块到模型空间
        /// </summary>
        /// <param name="br"></param>
        /// <returns></returns>
        public static List<Entity> ExplodeToModelSpace(BlockReference br)
        {
            List<Entity> ents = new List<Entity>();
            DBObjectCollection dBObjectCollection = new DBObjectCollection();
            br.Explode(dBObjectCollection);
            foreach (DBObject dbObj in dBObjectCollection)
            {
                if (dbObj is BlockReference)
                {
                    BlockReference newBr = dbObj as BlockReference;
                    List<Entity> subEnts = ExplodeToModelSpace(newBr);
                    if (subEnts.Count > 0)
                    {
                        ents.AddRange(subEnts);
                    }
                }
                else if (dbObj is Entity)
                {
                    Entity ent = dbObj as Entity;
                    ents.Add(ent);
                }
            }
            return ents;
        }
        public static Point2dCollection GetWipeOutBoundaryPts(ObjectId wpId, bool needTransform = true)
        {
            Point2dCollection newPts = new Point2dCollection();
            Document doc = GetMdiActiveDocument();
            using (Transaction trans = doc.Database.TransactionManager.StartTransaction())
            {
                Wipeout wp = trans.GetObject(wpId, OpenMode.ForRead) as Wipeout;
                Point2dCollection pts = wp.GetClipBoundary();
                Matrix3d mt = wp.PixelToModelTransform;
                if (pts.Count == 2)
                {
                    double minX = Math.Min(pts[0].X, pts[1].X);
                    double maxX = Math.Max(pts[0].X, pts[1].X);
                    double minY = Math.Min(pts[0].Y, pts[1].Y);
                    double maxY = Math.Max(pts[0].Y, pts[1].Y);
                    newPts.Add(new Point2d(minX, minY));
                    newPts.Add(new Point2d(maxX, minY));
                    newPts.Add(new Point2d(maxX, maxY));
                    newPts.Add(new Point2d(minX, maxY));
                }
                else
                {
                    newPts = pts;
                }
                if (true)
                {
                    Point2dCollection tempPts = new Point2dCollection();
                    foreach (Point2d pt in newPts)
                    {
                        Point3d newPt = new Point3d(pt.X, pt.Y, 0.0);
                        newPt = newPt.TransformBy(mt);
                        tempPts.Add(new Point2d(newPt.X, newPt.Y));
                    }
                    newPts = tempPts;
                }
                trans.Commit();
            }
            return newPts;
        }
    }
    /// <summary>
    /// 线与直线的关系
    /// </summary>
    public enum PtAndLinePos
    {
        /// <summary>
        /// 起始端
        /// </summary>
        StartPort,
        /// <summary>
        /// 终点端
        /// </summary>
        EndPort,
        /// <summary>
        /// 线内
        /// </summary>
        In,
        /// <summary>
        /// 延伸
        /// </summary>
        Extend,
        /// <summary>
        /// 外部
        /// </summary>
        Outer
    }
}
