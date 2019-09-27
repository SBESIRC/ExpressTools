using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices.Filters;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThXClip
{
    public class CadOperation
    {
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
            if (res <= 0)
            {
                return true;
            }
            else
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
            if(angle1< angle2)
            {
                arc.StartAngle = angle1;
                arc.StartAngle = angle2;
            }
            else
            {
                arc.StartAngle = angle2;
                arc.StartAngle = angle1;
            }
            return arc;
        }
        public static double AngleFromXAxis(Point3d pt1, Point3d pt2)
        {
            Vector2d vector = new Vector2d(pt1.X-pt2.X,pt1.Y-pt2.Y);
            return vector.Angle;
        }
        public static double AngleFromXAxisOther(Point3d pt1, Point3d pt2)
        {
            Point2d newPt1 = new Point2d(pt1.X, pt1.Y);
            Point2d newPt2 = new Point2d(pt2.X, pt2.Y);
            Vector2d vec = newPt1.GetVectorTo(newPt2);
            return vec.Angle;
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
                    if(seg.IsOn(pt))
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
        public static Polyline CreatePolyline(Point2dCollection pts)
        {
            Polyline polyline = new Polyline();
            if(pts.Count==2)
            {
                Point2d minPt = pts[0];
                Point2d maxPt = pts[1];
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
            for(int i=0;i<pts.Count;i++)
            {
                polyline.AddVertexAt(i, pts[i], 0, 0, 0);
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
    }
}
