using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Linq2Acad;
using System.Text;
using TianHua.AutoCAD.Utility.ExtensionTools;
using System.IO;
using NLog;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace ThColumnInfo
{
    public class ThColumnInfoUtils
    {
        public static Tolerance tolerance = new Tolerance(1e-2, 1e-2);
        public static readonly string thColumnFrameRegAppName = "ThColumnFrame";
        public static List<Point3d> GetRetanglePts(List<Point3d> pts)
        {
            List<Point3d> recPts = new List<Point3d>();
            if(pts.Count<2)
            {
                return recPts;
            }
            double minX = pts.OrderBy(i => i.X).First().X;
            double minY = pts.OrderBy(i => i.Y).First().Y;
            double minZ = pts.OrderBy(i => i.Z).First().Z;

            double maxX = pts.OrderByDescending(i => i.X).First().X;
            double maxY = pts.OrderByDescending(i => i.Y).First().Y;
            double maxZ = pts.OrderByDescending(i => i.Z).First().Z;

            recPts.Add(new Point3d(minX, minY, minZ));
            recPts.Add(new Point3d(maxX, minY, minZ));
            recPts.Add(new Point3d(maxX, maxY, minZ));
            recPts.Add(new Point3d(minX, maxY, minZ));
            return recPts;
        }
        public static List<Point3d> GetPolylinePts(Curve curve)
        {
            List<Point3d> pts = new List<Point3d>();
            if(curve==null)
            {
                return pts;
            }
            if (curve is Polyline polyline)
            {
                for (int j = 0; j < polyline.NumberOfVertices; j++)
                {
                    pts.Add(polyline.GetPoint3dAt(j));
                }
            }
            else if (curve is Polyline2d polyline2d)
            {
                Point3dCollection allPts= polyline2d.GetAllGripPoints();
                foreach(Point3d ptItem in allPts)
                {
                    pts.Add(ptItem);
                }
            }
            else if(curve is Polyline2d polyline3d)
            {
                Point3dCollection allPts = polyline3d.GetAllGripPoints();
                foreach (Point3d ptItem in allPts)
                {
                    pts.Add(ptItem);
                }
            }
            return pts;
        }
        public static List<Point3d> GetPolylinePts(ObjectId polylineId)
        {
            List<Point3d> pts = new List<Point3d>();
            if (!CheckObjIdIsValid(polylineId))
            {
                return pts;
            }
            Document document = Application.DocumentManager.MdiActiveDocument;
            using (Transaction trans = document.TransactionManager.StartTransaction())
            {
                Polyline polyline = trans.GetObject(polylineId, OpenMode.ForRead) as Polyline;
                pts = GetPolylinePts(polyline);
                trans.Commit();
            }
            return pts;
        }
        public static bool CheckObjIdIsValid(ObjectId objId)
        {
            bool res = true;
            if (objId == ObjectId.Null || objId.IsErased || objId.IsValid == false)
            {
                res = false;
            }
            return res;
        }
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
                    isOn = seg.IsOn(pt,ThCADCommon.Global_Tolerance);
                    if (isOn)
                        break;
                }
            }
            return isOn;
        }
        /// <summary>
        /// 判断点在一个密闭的多段线内
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static bool IsPointInPolyline(Point2dCollection pts, Point2d pt)
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
        public static bool IsPointInPolyline(Point3dCollection pts, Point3d pt)
        {
            Point2dCollection newPts = new Point2dCollection();
            foreach (Point3d ptItem in pts)
            {
                newPts.Add(new Point2d(ptItem.X, ptItem.Y));
            }
            bool result = IsPointInPolyline(newPts, new Point2d(pt.X, pt.Y));
            return result;
        }
        /// <summary>
        /// 获取在一条下上的延伸点
        /// </summary>
        /// <param name="startPt"></param>
        /// <param name="endPt"></param>
        /// <param name="extendDis"></param>
        /// <returns></returns>
        public static Point3d GetExtendPt(Point3d startPt,Point3d endPt, double extendDis)
        {
            Vector3d vec = startPt.GetVectorTo(endPt);
            return startPt+vec.GetNormal().MultiplyBy(extendDis);
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
        /// 获取两点中点
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <returns></returns>
        public static Point3d GetMidPt(Point3d pt1, Point3d pt2)
        {
            return new Point3d((pt1.X + pt2.X) / 2.0, (pt1.Y + pt2.Y) / 2.0, (pt1.Z + pt2.Z) / 2.0);
        }
        /// <summary>
        /// 删除物体
        /// </summary>
        /// <param name="objIds"></param>
        public static void EraseObjIds(params ObjectId[] objIds)
        {
            Document doc = GetMdiActiveDocument();
            Database db = doc.Database;//获取数据库对象
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                for (int i = 0; i < objIds.Length; i++)
                {
                    if (objIds[i]==ObjectId.Null || objIds[i].IsErased || objIds[i].IsValid==false)
                    {
                        continue;
                    }
                    DBObject dbObj = trans.GetObject(objIds[i], OpenMode.ForWrite);
                    dbObj.Erase();
                }
                trans.Commit();
            }
        }
        public static Polyline CreatePolyline(Point3dCollection pts, bool isClosed = true,double lineWidth=0.0)
        {
            Point2dCollection p2dPts = new Point2dCollection();
            foreach(Point3d pt in pts)
            {
                p2dPts.Add(new Point2d(pt.X,pt.Y));
            }
            return CreatePolyline(p2dPts, isClosed, lineWidth);
        }
        public static Polyline CreateRectangle(Point3d minPt,Point3d maxPt)
        {
            double minX = Math.Min(minPt.X, maxPt.X);
            double minY = Math.Min(minPt.Y, maxPt.Y);
            double minZ = Math.Min(minPt.Z, maxPt.Z);

            double maxX = Math.Max(minPt.X, maxPt.X);
            double maxY = Math.Max(minPt.Y, maxPt.Y);
            double maxZ = Math.Max(minPt.Z, maxPt.Z);

            Point3d firstPt = new Point3d(minX, minY, minZ);
            Point3d thirdPt = new Point3d(maxX, maxY, minZ);
            Point3d secondPt = new Point3d(thirdPt.X, firstPt.Y, firstPt.Z);
            Point3d fourthPt = new Point3d(firstPt.X, thirdPt.Y, firstPt.Z);

            Point3dCollection pts = new Point3dCollection();
            pts.Add(firstPt);
            pts.Add(secondPt);
            pts.Add(thirdPt);
            pts.Add(fourthPt);

            return CreatePolyline(pts, true);
        }
        /// <summary>
        /// 创建没有圆弧的多段线
        /// </summary>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static Polyline CreatePolyline(Point2dCollection pts, bool isClosed = true,double lineWidth=0)
        {
            Polyline polyline = new Polyline();
            if (pts.Count == 2)
            {
                Point2d minPt = pts[0];
                Point2d maxPt = pts[1];
                Vector2d vec = minPt.GetVectorTo(maxPt);
                if (vec.IsParallelTo(Vector2d.XAxis) || vec.IsParallelTo(Vector2d.YAxis))
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
            for (int i = 0; i < pts.Count; i++)
            {
                polyline.AddVertexAt(i, pts[i], 0, lineWidth, lineWidth);
            }
            if (isClosed)
            {
                polyline.Closed = true;
            }
            return polyline;
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
                for (int i = 0; i < entities.Length; i++)
                {
                    objIds.Add(modelSpace.AppendEntity(entities[i]));
                    trans.AddNewlyCreatedDBObject(entities[i], true);
                }
                modelSpace.DowngradeOpen();
                trans.Commit();
            }
            return objIds;
        }
        public static ObjectId AddToBlockTable(Entity ent,bool visible)
        {
            ObjectId objId = ObjectId.Null;//存储要插入的块参照的Id
            Document doc = GetMdiActiveDocument();
            Database db = doc.Database;//获取数据库对象
            if(ent.ObjectId!= ObjectId.Null)
            {
                return objId;
            }
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable; //以读的方式打开块表
                BlockTableRecord modelSpace = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                modelSpace.UpgradeOpen();
                if(!visible)
                {
                    ent.Visible = false;
                }
                objId=modelSpace.AppendEntity(ent);
                trans.AddNewlyCreatedDBObject(ent, true);
                modelSpace.DowngradeOpen();
                trans.Commit();
            }
            return objId;
        }
        /// <summary>
        /// 创建三维多段线
        /// </summary>
        /// <param name="pts"></param>
        /// <returns></returns>
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
                pts.Add(new Point3d(minX, minY, minPt.Z));
                pts.Add(new Point3d(maxX, minY, minPt.Z));
                pts.Add(new Point3d(maxX, maxY, minPt.Z));
                pts.Add(new Point3d(minX, maxY, minPt.Z));
            }
            polyline = new Polyline3d(Poly3dType.SimplePoly, pts, true);
            return polyline;
        }
        /// <summary>
        /// 获取两点间的物体
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <param name="layer"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public static List<ObjectId> GetSelectionInBox(Point3d pt1, Point3d pt2, double offsetZ, string layer = "", string category = "")
        {
            List<ObjectId> objectIds = new List<ObjectId>();
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            TypedValue[] tvs = null;
            PromptSelectionResult psr = null;
            List<ObjectId> findObjIds = new List<ObjectId>();
            if (layer != "" && category != "")
            {
                tvs = new TypedValue[] { new TypedValue((int)DxfCode.LayerName, layer), new TypedValue((int)DxfCode.Start, category) };
                psr = Application.DocumentManager.MdiActiveDocument.Editor.SelectAll();
            }
            else if (layer != "")
            {
                tvs = new TypedValue[] { new TypedValue((int)DxfCode.LayerName, layer) };
                psr = Application.DocumentManager.MdiActiveDocument.Editor.SelectAll();
            }
            else if (category != "")
            {
                tvs = new TypedValue[] { new TypedValue((int)DxfCode.Start, category) };
                psr = Application.DocumentManager.MdiActiveDocument.Editor.SelectAll();
            }
            else
            {
                psr = Application.DocumentManager.MdiActiveDocument.Editor.SelectAll();
            }
            if (psr == null || psr.Status != PromptStatus.OK)
            {
                return objectIds;
            }
            else
            {
                findObjIds = psr.Value.GetObjectIds().ToList();
            }
            Point3d minPt = new Point3d(Math.Min(pt1.X, pt2.X), Math.Min(pt1.Y, pt2.Y), Math.Min(pt1.Z, pt2.Z) - offsetZ); //Box左下角点
            Point3d maxPt = new Point3d(Math.Max(pt1.X, pt2.X), Math.Max(pt1.Y, pt2.Y), Math.Max(pt1.Z, pt2.Z) + offsetZ); ////Box右上角点

            foreach (ObjectId objId in findObjIds)
            {
                Entity entity = ThColumnInfoDbUtils.GetEntity(db, objId);
                if (entity == null || entity.GeometricExtents == null) 
                {
                    continue;
                }
                Extents3d extents3D = entity.GeometricExtents;
                Point3d midPt = BaseFunction.GetMidPt(extents3D.MinPoint, extents3D.MaxPoint);
                if (BaseFunction.CheckPtInBox(extents3D.MinPoint, minPt, maxPt) ||
                   BaseFunction.CheckPtInBox(extents3D.MaxPoint, minPt, maxPt) ||
                   BaseFunction.CheckPtInBox(midPt, minPt, maxPt))
                {
                    objectIds.Add(objId);
                }
                else
                {
                    //如果一个图形包括当前矩形框
                    if (BaseFunction.CheckPtInBox(new Point3d(minPt.X, minPt.Y, extents3D.MinPoint.Z), extents3D.MinPoint, extents3D.MaxPoint) &&
                        BaseFunction.CheckPtInBox(new Point3d(maxPt.X, maxPt.Y, extents3D.MinPoint.Z), extents3D.MinPoint, extents3D.MaxPoint))
                    {
                        if (extents3D.MinPoint.Z >= minPt.Z && extents3D.MinPoint.Z <= maxPt.Z)
                        {
                            objectIds.Add(objId);
                        }
                    }
                }
            }
            return objectIds;
        }
        /// <summary>
        /// 获取两点间的物体
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <param name="layer"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public static List<ObjectId> GetSelectionInRectangle(Point3d pt1, Point3d pt2, string layer = "", string category = "")
        {
            List<ObjectId> objectIds = new List<ObjectId>();
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            TypedValue[] tvs = null;
            PromptSelectionResult psr = null;
            List<ObjectId> findObjIds = new List<ObjectId>();
            if (layer != "" && category != "")
            {
                tvs = new TypedValue[] { new TypedValue((int)DxfCode.LayerName, layer), new TypedValue((int)DxfCode.Start, category) };
                psr = Application.DocumentManager.MdiActiveDocument.Editor.SelectAll();
            }
            else if (layer != "")
            {
                tvs = new TypedValue[] { new TypedValue((int)DxfCode.LayerName, layer) };
                psr = Application.DocumentManager.MdiActiveDocument.Editor.SelectAll();
            }
            else if (category != "")
            {
                tvs = new TypedValue[] { new TypedValue((int)DxfCode.Start, category) };
                psr = Application.DocumentManager.MdiActiveDocument.Editor.SelectAll();
            }
            else
            {
                psr = Application.DocumentManager.MdiActiveDocument.Editor.SelectAll();
            }
            if (psr == null || psr.Status != PromptStatus.OK)
            {
                return objectIds;
            }
            else
            {
                findObjIds = psr.Value.GetObjectIds().ToList();
            }
            Point3d minPt = new Point3d(Math.Min(pt1.X, pt2.X), Math.Min(pt1.Y, pt2.Y), Math.Min(pt1.Z, pt2.Z)); //左下角点
            Point3d maxPt = new Point3d(Math.Max(pt1.X, pt2.X), Math.Max(pt1.Y, pt2.Y), Math.Max(pt1.Z, pt2.Z)); //右上角点

            foreach (ObjectId objId in findObjIds)
            {
                Entity entity = ThColumnInfoDbUtils.GetEntity(db, objId);
                if (entity == null || entity.GeometricExtents == null)
                {
                    continue;
                }
                Extents3d extents3D = entity.GeometricExtents;
                Point3d midPt = BaseFunction.GetMidPt(extents3D.MinPoint, extents3D.MaxPoint);
                if (BaseFunction.CheckPtRectangle(extents3D.MinPoint, minPt, maxPt) ||
                   BaseFunction.CheckPtRectangle(extents3D.MaxPoint, minPt, maxPt) ||
                   BaseFunction.CheckPtRectangle(midPt, minPt, maxPt))
                {
                    objectIds.Add(objId);
                }
                else
                {
                    //如果一个图形包括当前矩形框
                    if (BaseFunction.CheckPtRectangle(new Point3d(minPt.X, minPt.Y, extents3D.MinPoint.Z), extents3D.MinPoint, extents3D.MaxPoint) &&
                        BaseFunction.CheckPtRectangle(new Point3d(maxPt.X, maxPt.Y, extents3D.MinPoint.Z), extents3D.MinPoint, extents3D.MaxPoint))
                    {
                        objectIds.Add(objId);
                    }
                }
            }
            return objectIds;
        }
        /// <summary>
        /// Check if the OS is 32 or 64 bit
        /// </summary>
        public static bool is64bits
        {
            get
            {
                return (Application.GetSystemVariable
            ("PLATFORM").ToString().IndexOf("64") > 0);
            }
        }
        public static List<string> GetLayerNameList()
        {
            List<string> layerNames = new List<string>();
            using (AcadDatabase acadDatabase = AcadDatabase.Use(Application.DocumentManager.MdiActiveDocument.Database))
            {
                layerNames = acadDatabase.Layers.Select(i => i.Name).ToList();
            }
            return layerNames;
        }
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
        public static PromptSelectionResult SelectByPolyline(Editor ed,
           Polyline pline, PolygonSelectionMode mode,SelectionFilter sf)
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
                result = ed.SelectCrossingPolygon(polygon, sf);
            else
                result = ed.SelectWindowPolygon(polygon, sf);
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
        public static PromptSelectionResult SelectByPolyline(Editor ed, Point3dCollection polygon,
            PolygonSelectionMode mode, SelectionFilter filter)
        {
            PromptSelectionResult result;
            if (mode == PolygonSelectionMode.Crossing)
                result = ed.SelectCrossingPolygon(polygon, filter);
            else
                result = ed.SelectWindowPolygon(polygon, filter);
            return result;
        }
        /// <summary>
        /// 获取当前文档
        /// </summary>
        /// <returns></returns>
        public static Document GetMdiActiveDocument()
        {
            return Application.DocumentManager.MdiActiveDocument;
        }
        public static PromptSelectionResult SelectByRectangle(Editor ed, Point3d pt1, Point3d pt2, PolygonSelectionMode mode, SelectionFilter filter=null)
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
            if(filter==null)
            {
                if (mode == PolygonSelectionMode.Crossing)
                    result = ed.SelectCrossingPolygon(polygon);
                else
                    result = ed.SelectWindowPolygon(polygon);
            }
            else
            {
                if (mode == PolygonSelectionMode.Crossing)
                    result = ed.SelectCrossingPolygon(polygon, filter);
                else
                    result = ed.SelectWindowPolygon(polygon, filter);
            }
            return result;
        }
        /// <summary>
        /// 显示或隐藏物体
        /// </summary>
        /// <param name="objIds"> 传入的物体</param>
        /// <param name="isShow">true->显示，false->隐藏</param>
        public static void ShowObjIds(ObjectId[] objIds,bool isShow=true)
        {
            if(objIds==null && objIds.Length==0)
            {
                return;
            }
            var doc = GetMdiActiveDocument();
            using (Transaction trans=doc.TransactionManager.StartTransaction())
            {
                for (int i = 0; i < objIds.Length; i++)
                {
                    try
                    {
                        DBObject dbobj = trans.GetObject(objIds[i], OpenMode.ForRead);
                        if (dbobj is Entity)
                        {
                            Entity ent = dbobj as Entity;
                            if (isShow)
                            {
                                if (!ent.Visible)
                                {
                                    ent.UpgradeOpen();
                                    ent.Visible = true;
                                    ent.DowngradeOpen();
                                }
                            }
                            else
                            {
                                if (ent.Visible)
                                {
                                    ent.UpgradeOpen();
                                    ent.Visible = false;
                                    ent.DowngradeOpen();
                                }
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        ThColumnInfoUtils.WriteException(ex, "ShowObjIds");
                    }
                }
                trans.Commit();
            }
        }
        public static void ShowObjIds(List<ObjectId> objIds, bool isShow = true)
        {
            if(objIds!=null && objIds.Count>0)
            {
                ShowObjIds(objIds.ToArray(), isShow);
            }
        }
        public static void ShowObjId(ObjectId objId, bool isShow = true)
        {
            if (!CheckObjIdIsValid(objId))
            {
                return;
            }
            var doc = GetMdiActiveDocument();
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                try
                {
                    DBObject dbobj = trans.GetObject(objId, OpenMode.ForRead);
                    if (dbobj is Entity)
                    {
                        Entity ent = dbobj as Entity;
                        if (isShow)
                        {
                            if (!ent.Visible)
                            {
                                ent.UpgradeOpen();
                                ent.Visible = true;
                                ent.DowngradeOpen();
                            }
                        }
                        else
                        {
                            if (ent.Visible)
                            {
                                ent.UpgradeOpen();
                                ent.Visible = false;
                                ent.DowngradeOpen();
                            }
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    ThColumnInfoUtils.WriteException(ex, "ShowObjId");
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
            if (layerNameList == null || layerNameList.Count == 0)
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
        public static void WriteException(System.Exception exception, string specialText = "")
        {
            //string fileName = Guid.NewGuid() + "_" + DateTime.Now.ToString("s") + ".log";
            string fileName = Guid.NewGuid() + ".log";
            string basePath = System.IO.Path.GetTempPath();
            if (!Directory.Exists(basePath + "ThXlpLog"))
            {
                Directory.CreateDirectory(basePath + "ThXlpLog");
            }
            string text = string.Empty;
            if (exception != null)
            {
                Type exceptionType = exception.GetType();
                if (!string.IsNullOrEmpty(specialText))
                {
                    text = text + specialText + Environment.NewLine;
                }
                text += "Exception: " + exceptionType.Name + Environment.NewLine;
                text += "               " + "Message: " + exception.Message + Environment.NewLine;
                text += "               " + "Source: " + exception.Source + Environment.NewLine;
                text += "               " + "StackTrace: " + exception.StackTrace + Environment.NewLine;
            }
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            LogConfig(basePath + "ThXlpLog\\" + fileName);
            NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
            Logger.Error(text);
        }
        private static void LogConfig(string fileName)
        {
            var config = new NLog.Config.LoggingConfiguration();
            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = fileName };
            //var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            // Rules for mapping loggers to targets            
            //config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            // Apply config           
            NLog.LogManager.Configuration = config;
        }
        public static List<int> GetDatas(string str)
        {
            List<int> values = new List<int>();
            string pattern = "[-]?\\d+";
            MatchCollection matches= Regex.Matches(str, pattern);
            foreach(var match in matches)
            {
                if (!string.IsNullOrEmpty(match.ToString()))
                {
                    values.Add(Convert.ToInt32(match.ToString()));
                }
            }
            return values;
        }
        /// <summary>
        /// 获取特殊符号
        /// </summary>
        /// <param name="str"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static int IndexOfSpecialChar(string str,out string content)
        {
            int index = -1;
            content = "";
            index = str.IndexOf("%%130");
            if (index>=0)
            {
                content = "%%130";
                return index;
            }
            index = str.IndexOf("%%131");
            if (index >= 0)
            {
                content = "%%131";
                return index;
            }
            index = str.IndexOf("%%132");
            if (index >= 0)
            {
                content = "%%132";
                return index;
            }
            index = str.IndexOf("%%133");
            if (index >= 0)
            {
                content = "%%133";
                return index;
            }
            return index;
        }
        public static List<double> GetDoubleValues(string str)
        {
            string newStr = str;
            List<double> values = new List<double>();
            string content = "";
            while(IndexOfSpecialChar(newStr,out content)>=0)
            {
                int startIndex = newStr.IndexOf(content);
                newStr = newStr.Remove(startIndex, content.Length);
            }
            string pattern = "[-]?\\d+([.]{1}\\d+)?";
            MatchCollection matches = Regex.Matches(newStr, pattern);
            foreach (var match in matches)
            {
                if (!string.IsNullOrEmpty(match.ToString()))
                {
                    values.Add(Convert.ToDouble(match.ToString()));
                }
            }
            return values;
        }
        public static void ZoomObject(Editor ed, Point3dCollection pts)
        {
            Point3dCollection polygon = new Point3dCollection();
            foreach (Point3d pt in pts)
            {
                if (polygon.IndexOf(pt) < 0)
                {
                    polygon.Add(pt);
                }
            }
            if (polygon.Count < 2)
            {
                return;
            }
            if (polygon.Count == 2)
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
            List<ObjectId> objectIds = AddToBlockTable(polyline);
            ed.ZoomObject(objectIds[0]);
            EraseObjIds(objectIds[0]);
        }
        /// <summary>
        /// 根据两点Zoom
        /// </summary>
        /// <param name="ed"></param>
        /// <param name="minPt"></param>
        /// <param name="maxPt"></param>
        public static void ZoomObject(Editor ed, Point3d minPt, Point3d maxPt)
        {
            Point3dCollection pts = new Point3dCollection();
            pts.Add(minPt);
            pts.Add(maxPt);
            ZoomObject(ed, pts);
        }
        public static void ZoomWin(Editor ed, Point3d min, Point3d max)
        {
            Point2d min2d = new Point2d(min.X, min.Y);
            Point2d max2d = new Point2d(max.X, max.Y);

            ViewTableRecord view =
              new ViewTableRecord();

            view.CenterPoint =
              min2d + ((max2d - min2d) / 2.0);
            view.Height = max2d.Y - min2d.Y;
            view.Width = max2d.X - min2d.X;
            ed.SetCurrentView(view);
        }
        public static object GetSelectPoint(Editor ed, string message,Dictionary<string,string> keyMessage)
        {
            object value = null;
            PromptPointOptions ppo = new PromptPointOptions(message);
            ppo.AllowArbitraryInput = true;
            ppo.AllowNone = true;
            if (keyMessage!=null && keyMessage.Count>0)
            {
                keyMessage.ForEach(i => ppo.Keywords.Add(i.Key));
                ppo.AppendKeywordsToMessage = false;
            }
            bool isGoOn = true;
            do
            {
               PromptPointResult ppr= ed.GetPoint(ppo);
               if(ppr.Status==PromptStatus.OK)
                {
                    value = ppr.Value;
                }
               else if(ppr.Status==PromptStatus.Keyword)
                {
                    value = ppr.StringResult;
                }
               else
                {
                    isGoOn = false;
                }
            }
            while (isGoOn);
            return value;
        }
        public static object GetCornerPoint(Editor ed,Point3d basePt, string message, Dictionary<string, string> keyMessage)
        {
            object value = null;
            PromptCornerOptions ppo = new PromptCornerOptions(message, basePt);
            ppo.AllowArbitraryInput = true;
            ppo.AllowNone = true;
            if (keyMessage != null && keyMessage.Count > 0)
            {
                foreach (var item in keyMessage)
                {
                    ppo.Keywords.Add(item.Key, item.Key, item.Value);
                }
            }
            bool isGoOn = true;
            do
            {
                PromptPointResult ppr = ed.GetCorner(ppo);
                if (ppr.Status == PromptStatus.OK)
                {
                    value = ppr.Value;
                }
                else if (ppr.Status == PromptStatus.Keyword)
                {
                    value = ppr.Value;
                }
                else
                {
                    isGoOn = false;
                }
            }
            while (isGoOn);
            return value;
        }
        public static void AddXData(ObjectId objectId,string regAppName,List<TypedValue> tvs)
        {
            if(objectId== ObjectId.Null || string.IsNullOrEmpty(regAppName) || tvs.Count==0)
            {
                return;
            }
            Document doc = GetMdiActiveDocument();
            using (Transaction trans=doc.TransactionManager.StartTransaction())
            {
                RegAppTable regAppTable = trans.GetObject(doc.Database.RegAppTableId, OpenMode.ForRead) as RegAppTable;
                if(!regAppTable.Has(regAppName))
                {
                    regAppTable.UpgradeOpen();
                    RegAppTableRecord regAppTableRecord = new RegAppTableRecord();
                    regAppTableRecord.Name = regAppName;
                    regAppTable.Add(regAppTableRecord);
                    trans.AddNewlyCreatedDBObject(regAppTableRecord, true);
                    regAppTable.DowngradeOpen();
                }
                DBObject dbObj = trans.GetObject(objectId,OpenMode.ForWrite);
                tvs.Insert(0, new TypedValue((int)DxfCode.ExtendedDataRegAppName, regAppName));
                ResultBuffer rb = new ResultBuffer();
                tvs.ForEach(i => rb.Add(i));
                dbObj.XData = rb;
                dbObj.DowngradeOpen();
                trans.Commit();
            }
        }
        public static List<object> GetXData(ObjectId objId,string regAppName)
        {
            List<object> values = new List<object>();
            if(objId==ObjectId.Null || string.IsNullOrEmpty(regAppName))
            {
                return values;
            }
            Document doc = GetMdiActiveDocument();
            using (Transaction trans=doc.TransactionManager.StartTransaction())
            {
                DBObject dbObj= trans.GetObject(objId,OpenMode.ForRead);
                ResultBuffer rbs = dbObj.GetXDataForApplication(regAppName);
                if (rbs != null)
                {
                   foreach(var rb in rbs)
                    {
                        if (rb.TypeCode== (int)DxfCode.ExtendedDataAsciiString)
                        {
                            values.Add((string)rb.Value);
                        }
                        else if(rb.TypeCode == (int)DxfCode.ExtendedDataInteger32)
                        {
                            values.Add((int)rb.Value);
                        }
                        else if (rb.TypeCode == (int)DxfCode.ExtendedDataReal)
                        {
                            values.Add((double)rb.Value);
                        }
                        else if(rb.TypeCode == (int)DxfCode.ExtendedDataRegAppName)
                        {
                            values.Add((string)rb.Value);
                        }
                    }
                }
                trans.Commit();
            }
            return values;
        }
        public static void RemoveXData(ObjectId objectId, string regAppName)
        {
            if (objectId == ObjectId.Null || string.IsNullOrEmpty(regAppName))
            {
                return;
            }
            List<object> values= GetXData(objectId, regAppName);
            if(values.Count==0)
            {
                return;
            }
            Document doc = GetMdiActiveDocument();
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                DBObject dbObj = trans.GetObject(objectId, OpenMode.ForWrite);
                ResultBuffer rb = new ResultBuffer(new TypedValue(1001, regAppName));
                dbObj.XData = rb;
                dbObj.DowngradeOpen();
                trans.Commit();
            }
        }
        private void AddExtensionDictionary(ObjectId entityId,string dicKey,List<TypedValue> tvs)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            if (entityId == ObjectId.Null || string.IsNullOrEmpty(dicKey) || tvs.Count==0)
                return;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBObject dbObj = tr.GetObject(entityId, OpenMode.ForRead);
                ObjectId extId = dbObj.ExtensionDictionary;
                if (extId == ObjectId.Null)
                {
                    dbObj.UpgradeOpen();
                    dbObj.CreateExtensionDictionary();
                    extId = dbObj.ExtensionDictionary;
                }
                //now we will have extId...
                DBDictionary dbExt =
                        (DBDictionary)tr.GetObject(extId, OpenMode.ForRead);
                Xrecord xRec = new Xrecord();
                ResultBuffer rb = new ResultBuffer();
                tvs.ForEach(i => rb.Add(i));
                //set the data
                xRec.Data = rb;
                //if not present add the data
                if (!dbExt.Contains(dicKey))
                {
                    dbExt.UpgradeOpen();
                    dbExt.SetAt(dicKey, xRec);
                    tr.AddNewlyCreatedDBObject(xRec, true);
                }
                else
                {
                    ObjectId xrecId = dbExt.GetAt(dicKey);
                    tr.GetObject(xrecId, OpenMode.ForWrite).Erase();
                    dbExt[dicKey] = xRec;
                    tr.AddNewlyCreatedDBObject(xRec, true);
                }
                tr.Commit();
            }
        }
        public static ObjectId AddXrecord(ObjectId objId,string searchKey,List<TypedValue> tvs)
        {
            ObjectId idXrec = ObjectId.Null;
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            if (objId == ObjectId.Null || string.IsNullOrEmpty(searchKey) || tvs.Count == 0)
                return idXrec;
            using (Transaction trans=db.TransactionManager.StartTransaction())
            {
                DBObject dbObj = trans.GetObject(objId,OpenMode.ForRead);
                if(dbObj.ExtensionDictionary==ObjectId.Null)
                {
                    dbObj.UpgradeOpen(); //切换对象为写的状态
                    dbObj.CreateExtensionDictionary(); //为对象创建扩展字典
                    dbObj.DowngradeOpen(); //为了安全，将对象切换成读的状态
                }
                //打开对象的扩展字典
                DBDictionary dict = (DBDictionary)trans.GetObject(dbObj.ExtensionDictionary,OpenMode.ForRead);
                Xrecord xrec = new Xrecord(); //
                ResultBuffer rb = new ResultBuffer();
                tvs.ForEach(i => rb.Add(i));
                xrec.Data=rb;
                dict.UpgradeOpen();
                if (!dict.Contains(searchKey))
                {
                    dict.SetAt(searchKey, xrec);
                    trans.AddNewlyCreatedDBObject(xrec, true);
                }
                else
                {
                    ObjectId oldXRecId = dict.GetAt(searchKey);
                    trans.GetObject(oldXRecId,OpenMode.ForWrite).Erase();
                    dict[searchKey] = xrec;
                    trans.AddNewlyCreatedDBObject(xrec, true);
                }
                idXrec=dict.GetAt(searchKey);
                dict.DowngradeOpen();
                trans.Commit();
            } 
            return idXrec;
        }
        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="id"></param>
        /// <param name="searchKey"></param>
        /// <returns></returns>
        public static List<TypedValue> GetXRecord(ObjectId id,string searchKey,Database db=null)
        {
            List<TypedValue> tvs = new List<TypedValue>();
            try
            {
                if(db==null)
                {
                    db = Application.DocumentManager.MdiActiveDocument.Database;
                }
                using (Transaction trans=db.TransactionManager.StartTransaction())
                {
                    DBObject obj = trans.GetObject(id, OpenMode.ForRead);
                    ObjectId dictId = obj.ExtensionDictionary;
                    if (dictId == ObjectId.Null)
                    {
                        return tvs;
                    }
                    DBDictionary dict = trans.GetObject(dictId, OpenMode.ForRead) as DBDictionary;
                    if (!dict.Contains(searchKey))
                    {
                        return tvs;
                    }
                    ObjectId xrecordId = dict.GetAt(searchKey);
                    if (xrecordId == ObjectId.Null)
                    {
                        return tvs;
                    }
                    Xrecord xrecord = trans.GetObject(xrecordId, OpenMode.ForRead) as Xrecord;
                    ResultBuffer rb = xrecord.Data;
                    tvs = rb.AsArray().ToList();
                    trans.Commit();
                }
            }
            catch(System.Exception ex)
            {
                ThColumnInfoUtils.WriteException(ex, "GetXRecord");
            }
            return tvs;
        }
        public static List<Entity> Explode(BlockReference br, bool keepUnVisible = true)
        {
            List<Entity> entities = new List<Entity>();
            DBObjectCollection collection = new DBObjectCollection();
            br.Explode(collection);
            foreach (DBObject obj in collection)
            {
                if (obj is BlockReference)
                {
                    var newBr = obj as BlockReference;
                    if (!keepUnVisible && newBr.Visible == false)
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
                    if (!keepUnVisible && ent.Visible == false)
                    {
                        continue;
                    }
                    entities.Add(obj as Entity);
                }
            }
            return entities;
        }
        public static List<string> GetLayerList(string contain = "")
        {
            List<string> layers = new List<string>();
            Document doc = GetMdiActiveDocument();
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                LayerTable lt = trans.GetObject(doc.Database.LayerTableId, OpenMode.ForRead) as LayerTable;
                foreach (var id in lt)
                {
                    LayerTableRecord ltr = trans.GetObject(id, OpenMode.ForRead) as LayerTableRecord;
                    if (string.IsNullOrEmpty(contain))
                    {
                        layers.Add(ltr.Name);
                    }
                    else
                    {
                        if (ltr.Name.ToUpper().Contains(contain.ToUpper()))
                        {
                            layers.Add(ltr.Name);
                        }
                    }
                }
                trans.Commit();
            }
            return layers;
        }
        /// <summary>
        /// 添加有名字典
        /// </summary>
        /// <param name="db"></param>
        /// <param name="searchKey"></param>
        /// <returns></returns>
        public static ObjectId AddNamedDictionary(Database db, string searchKey)
        {
            ObjectId dictId = ObjectId.Null;
            if(string.IsNullOrEmpty(searchKey))
            {
                return dictId;
            }
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                DBDictionary dicts = trans.GetObject(db.NamedObjectsDictionaryId,OpenMode.ForRead) as DBDictionary;
                if(!dicts.Contains(searchKey))
                {
                    DBDictionary dict = new DBDictionary();
                    dicts.UpgradeOpen();
                    dictId = dicts.SetAt(searchKey, dict);
                    dicts.DowngradeOpen();
                    trans.AddNewlyCreatedDBObject(dict, true);
                }
                else
                {
                    dictId = dicts.GetAt(searchKey);
                }
                trans.Commit();
            }
            return dictId;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="curve1"></param>
        /// <param name="curve2"></param>
        /// <returns></returns>
        public static bool JudgeTwoCurveIsOverLap(Curve curve1,Curve curve2)
        {
            bool result = false;
            try
            {
                DBObjectCollection dbObjCo11 = new DBObjectCollection();
                dbObjCo11.Add(curve1);
                DBObjectCollection dbObjCo12 = new DBObjectCollection();
                dbObjCo12.Add(curve2);

                Region firstRegion = (Region)Region.CreateFromCurves(dbObjCo11)[0];
                Region secondRegion = (Region)Region.CreateFromCurves(dbObjCo12)[0];

                firstRegion.BooleanOperation(BooleanOperationType.BoolIntersect, secondRegion);
                if (firstRegion.Area > 0.0)
                {
                    result = true;
                }
                firstRegion.Dispose();
                secondRegion.Dispose();
            }
            catch(System.Exception ex)
            {
                WriteException(ex, "JudgeTwoCurveIsOverLap,Parameter->Curve,Curve");
            }      
            return result;
        }
        public static bool JudgeTwoCurveIsOverLap(List<Point3d> firstCurvePts, List<Point3d> secondCurvePts)
        {
            bool result = false;
            try
            {
                Point3dCollection firstPts = new Point3dCollection();
                firstCurvePts.ForEach(i => firstPts.Add(i));
                Point3dCollection secondPts = new Point3dCollection();
                secondCurvePts.ForEach(i => secondPts.Add(i));

                Polyline firstPolyline = CreatePolyline(firstPts);
                Polyline seocndPolyline = CreatePolyline(secondPts);
                result = JudgeTwoCurveIsOverLap(firstPolyline, seocndPolyline);
                firstPolyline.Dispose();
                seocndPolyline.Dispose();
                firstPts.Dispose();
                secondPts.Dispose();
            }
            catch(System.Exception ex)
            {
                WriteException(ex, "JudgeTwoCurveIsOverLap,Parameter->List<Point3d>,List<Point3d>");
            }
            return result;
        }
        public static ObjectId DrawOffsetColumn(List<Point3d> polylinePts, double offsetDisScale = 2.5,
            bool visible = false, double lineWeight=200,bool setLayer=true)
        {
            ObjectId frameId = ObjectId.Null;
            if (polylinePts.Count < 2)
            {
                return frameId;
            }
            List<Point3d>  ucsPolylinePts=polylinePts.Select(i => ThColumnInfoUtils.TransPtFromWcsToUcs(i)).ToList();
            double minX = ucsPolylinePts.OrderBy(i => i.X).First().X;
            double minY = ucsPolylinePts.OrderBy(i => i.Y).First().Y;
            double minZ = ucsPolylinePts.OrderBy(i => i.Z).First().Z;
            double maxX = ucsPolylinePts.OrderByDescending(i => i.X).First().X;
            double maxY = ucsPolylinePts.OrderByDescending(i => i.Y).First().Y;
            double maxZ = ucsPolylinePts.OrderByDescending(i => i.Z).First().Z;

            Point3d leftDownPt = new Point3d(minX, minY, minZ);
            Point3d rightUpPt = new Point3d(maxX, maxY, minZ);
            Point3d cenPt = ThColumnInfoUtils.GetMidPt(leftDownPt, rightUpPt);
            double columnLen = (maxX - minX) * offsetDisScale;
            double columnHeight = (maxY - minY) * offsetDisScale;
            rightUpPt = new Point3d(cenPt.X + columnLen / 2.0, cenPt.Y + columnHeight / 2.0, minZ);
            leftDownPt = new Point3d(cenPt.X - columnLen / 2.0, cenPt.Y - columnHeight / 2.0, minZ);

            Point3dCollection wcsRecPts = new Point3dCollection();
            wcsRecPts.Add(TransPtFromUcsToWcs(new Point3d(leftDownPt.X, leftDownPt.Y,0)));
            wcsRecPts.Add(TransPtFromUcsToWcs(new Point3d(rightUpPt.X, leftDownPt.Y, 0)));
            wcsRecPts.Add(TransPtFromUcsToWcs(new Point3d(rightUpPt.X, rightUpPt.Y,0)));
            wcsRecPts.Add(TransPtFromUcsToWcs(new Point3d(leftDownPt.X, rightUpPt.Y, 0)));

            Polyline polyline = ThColumnInfoUtils.CreatePolyline(wcsRecPts, true, lineWeight);
            frameId = ThColumnInfoUtils.AddToBlockTable(polyline, visible);
            if (setLayer)
            {
                ObjectId layerId = BaseFunction.CreateColumnLayer();
                ThColumnInfoUtils.SetLayer(frameId, layerId);
            }
            TypedValue tv = new TypedValue((int)DxfCode.ExtendedDataAsciiString, "*");
            AddXData(frameId, ThColumnInfoUtils.thColumnFrameRegAppName, new List<TypedValue>() { tv }); 
            return frameId;
        }
        public static void ChangeColor(ObjectId objId, short colorIndex)
        {
            if (objId == ObjectId.Null)
            {
                return;
            }
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                Entity ent = trans.GetObject(objId, OpenMode.ForRead) as Entity;
                ent.UpgradeOpen();
                ent.ColorIndex = colorIndex;
                ent.DowngradeOpen();
                trans.Commit();
            }
        }
        public static void ChangeText(ObjectId textId, string content)
        {
            if (!CheckObjIdIsValid(textId))
            {
                return;
            }
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                DBObject dbObj = trans.GetObject(textId, OpenMode.ForRead) as Entity;
                if(dbObj is DBText dbText)
                {
                    dbText.UpgradeOpen();
                    dbText.TextString = content;
                    dbText.DowngradeOpen();
                }
                else if(dbObj is MText mText)
                {
                    mText.UpgradeOpen();
                    mText.Contents = content;
                    mText.DowngradeOpen();
                }
                trans.Commit();
            }
        }
        public static Brush SysColorConvertBrush(System.Drawing.Color sysColor)
        {
            Brush brush = new SolidColorBrush(Color.FromArgb(sysColor.A, sysColor.R, sysColor.G, sysColor.B));
            return brush;
        }
        /// <summary>
        /// Cad颜色转系统颜色
        /// </summary>
        /// <param name="cadColor"></param>
        /// <returns></returns>
        public static System.Drawing.Color AcadColorToSystemColor(Autodesk.AutoCAD.Colors.Color cadColor)
        {
            System.Drawing.Color color = System.Drawing.Color.FromArgb(
                 cadColor.ColorValue.R, cadColor.ColorValue.G, cadColor.ColorValue.B);
            return color;
        }
        /// <summary>
        /// 系统颜色转Cad颜色
        /// </summary>
        /// <param name="systemColor"></param>
        /// <returns></returns>
        public static Autodesk.AutoCAD.Colors.Color SystemColorToAcadColor(System.Drawing.Color systemColor)
        {
            Autodesk.AutoCAD.Colors.Color color = Autodesk.AutoCAD.Colors.Color.FromColor(systemColor); 
            return color;
        }
        public static bool JudgeTwoEntityIntersect(Entity firstEnt,Entity secondEnt)
        {
            bool inters = false;
            Plane zeroPlane = new Plane(Point3d.Origin, Vector3d.ZAxis);
            Point3dCollection pts = new Point3dCollection();
            try
            {
                firstEnt.IntersectWith(secondEnt, Intersect.OnBothOperands, zeroPlane, pts, IntPtr.Zero, IntPtr.Zero);
                if(pts.Count>0)
                {
                    inters = true;
                }
            }
            catch(System.Exception ex)
            {
                WriteException(ex, "JudgeTwoEntityInters");
            }
            finally
            {
                zeroPlane.Dispose();
                pts.Dispose();
            }
            return inters;
        }
        /// <summary>
        /// Gets the transformation matrix from the current User Coordinate System (UCS)
        /// to the World Coordinate System (WCS).
        /// </summary>
        /// <param name="ed">The instance to which this method applies.</param>
        /// <returns>The UCS to WCS transformation matrix.</returns>
        public static Matrix3d UCS2WCS()
        {
            var doc = GetMdiActiveDocument();
            return doc.Editor.CurrentUserCoordinateSystem;
        }

        /// <summary>
        /// Gets the transformation matrix from the World Coordinate System (WCS)
        /// to the current User Coordinate System (UCS).
        /// </summary>
        /// <param name="ed">The instance to which this method applies.</param>
        /// <returns>The WCS to UCS transformation matrix.</returns>
        public static Matrix3d WCS2UCS()
        {
            var doc = GetMdiActiveDocument();
            return doc.Editor.CurrentUserCoordinateSystem.Inverse();
        }
        /// <summary>
        /// 把坐标点从世界坐标系转到用户坐标系
        /// </summary>
        /// <param name="pt">Wcs Point3d</param>
        /// <returns></returns>
        public static Point3d TransPtFromWcsToUcs(Point3d pt)
        {
            Matrix3d mt = WCS2UCS();
            return pt.TransformBy(mt);
        }
        /// <summary>
        /// 把坐标点从用户坐标系转到世界坐标系
        /// </summary>
        /// <param name="pt">Ucs Point3d</param>
        /// <returns></returns>
        public static Point3d TransPtFromUcsToWcs(Point3d pt)
        {
            Matrix3d mt = UCS2WCS();
            return pt.TransformBy(mt);
        }
        /// <summary>
        /// 获取Entity Ucs Bound
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static Extents3d GeometricExtentsImpl(Entity entity)
        {
            var wcs2Ucs = WCS2UCS();
            using (var clone = entity.GetTransformedCopy(wcs2Ucs))
            {
                return clone.GeometricExtents;
            }
        }
        /// <summary>
        /// 设置图层
        /// </summary>
        /// <param name="objId"></param>
        /// <param name="layerId"></param>
        public static void SetLayer(ObjectId objId,ObjectId layerId)
        {
            if(objId==ObjectId.Null || objId.IsErased || !objId.IsValid)
            {
                return;
            }
            if (layerId == ObjectId.Null || layerId.IsErased || !layerId.IsValid)
            {
                return;
            }
            Document doc = GetMdiActiveDocument();
            using (Transaction trans=doc.Database.TransactionManager.StartTransaction())
            {
                if(trans.GetObject(objId,OpenMode.ForRead) is Entity ent)
                {
                    ent.UpgradeOpen();
                    ent.LayerId = layerId;
                    ent.DowngradeOpen();
                }
                trans.Commit();
            }
        }
    }
}
