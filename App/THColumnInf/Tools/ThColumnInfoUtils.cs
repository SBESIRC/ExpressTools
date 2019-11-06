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

namespace ThColumnInfo
{
    public class ThColumnInfoUtils
    {
        public static Tolerance tolerance = new Tolerance(1e-2, 1e-2);
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
        public static bool IsNumeric(string str) //接收一个string类型的参数,保存到str里
        {
            if (str == null || str.Length == 0)    //验证这个参数是否为空
                return false;                           //是，就返回False
            ASCIIEncoding ascii = new ASCIIEncoding();//new ASCIIEncoding 的实例
            byte[] bytestr = ascii.GetBytes(str);         //把string类型的参数保存到数组里
            foreach (byte c in bytestr)                   //遍历这个数组里的内容
            {
                if (c < 48 || c > 57)                          //判断是否为数字
                {
                    return false;                              //不是，就返回False
                }
            }
            return true;                                        //是，就返回True
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
        public static void EraseObjIds(params ObjectId[] objIds)
        {
            Document doc = GetMdiActiveDocument();
            Database db = doc.Database;//获取数据库对象
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                for (int i = 0; i < objIds.Length; i++)
                {
                    if (objIds[i].IsErased)
                    {
                        continue;
                    }
                    DBObject dbObj = trans.GetObject(objIds[i], OpenMode.ForWrite);
                    dbObj.Erase();
                }
                trans.Commit();
            }
        }
        public static Polyline CreatePolyline(Point3dCollection pts, bool isClosed = true)
        {
            Point2dCollection p2dPts = new Point2dCollection();
            foreach(Point3d pt in pts)
            {
                p2dPts.Add(new Point2d(pt.X,pt.Y));
            }
            return CreatePolyline(p2dPts, isClosed);
        }
        /// <summary>
        /// 创建没有圆弧的多段线
        /// </summary>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static Polyline CreatePolyline(Point2dCollection pts, bool isClosed = true)
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
                polyline.AddVertexAt(i, pts[i], 0, 0, 0);
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
                for(int i=0;i< objIds.Length;i++)
                {
                    DBObject dbobj = trans.GetObject(objIds[i],OpenMode.ForRead);
                    if(dbobj is Entity)
                    {
                        Entity ent = dbobj as Entity;
                        if (isShow)
                        {
                            if(!ent.Visible)
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
    }
}
