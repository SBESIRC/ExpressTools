using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using ThStructureCheck.Common.Interface;

namespace ThStructureCheck.Common
{
    public static class CadTool
    {
        public static Tolerance tolerance = new Tolerance(1e-6, 1e-6);
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
        /// <summary>
        /// 获取多段线点集合
        /// </summary>
        /// <param name="curve"></param>
        /// <returns></returns>
        public static List<Point3d> GetPolylinePts(Curve curve)
        {
            List<Point3d> pts = new List<Point3d>();
            if (curve == null)
            {
                return pts;
            }
            Document doc = GetMdiActiveDocument();
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                if (curve is Polyline polyline)
                {
                    for (int j = 0; j < polyline.NumberOfVertices; j++)
                    {
                        pts.Add(polyline.GetPoint3dAt(j));
                    }
                }
                else if (curve is Polyline2d polyline2d)
                {
                    foreach (ObjectId item in polyline2d)
                    {
                        Vertex2d itemEnt = trans.GetObject(item, OpenMode.ForRead) as Vertex2d;
                        pts.Add(polyline2d.VertexPosition(itemEnt));
                    }
                }
                else if (curve is Polyline3d polyline3d)
                {
                    foreach (ObjectId item in polyline3d)
                    {
                        var rs = trans.GetObject(item, OpenMode.ForRead) as PolylineVertex3d;
                        pts.Add(rs.Position);
                    }
                }
                trans.Commit();
            }
            return pts;
        }
        /// <summary>
        /// 获取任意实体的所有端点和夹点
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ent"></param>
        /// <returns></returns>
        public static Point3dCollection GetAllGripPoints<T>(this T ent) where T : Entity
        {
            Point3dCollection pts = new Point3dCollection();
            IntegerCollection inters = new IntegerCollection();

            ent.GetGripPoints(pts, inters, inters);
            return pts;
        }
        /// <summary>
        /// 检查ObjectId是否有效
        /// </summary>
        /// <param name="objId"></param>
        /// <returns></returns>
        public static bool CheckObjIdIsValid(ObjectId objId)
        {
            bool res = true;
            if (objId == ObjectId.Null || objId.IsErased || objId.IsValid == false)
            {
                res = false;
            }
            return res;
        }
        /// <summary>
        /// 判断点是否在多段线边界上
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
                    isOn = seg.IsOn(pt, tolerance);
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
        /// <summary>
        /// 获取在一条下上的延伸点
        /// </summary>
        /// <param name="startPt"></param>
        /// <param name="endPt"></param>
        /// <param name="extendDis"></param>
        /// <returns></returns>
        public static Point3d GetExtendPt(Point3d startPt, Point3d endPt, double extendDis)
        {
            Vector3d vec = startPt.GetVectorTo(endPt);
            return startPt + vec.GetNormal().MultiplyBy(extendDis);
        }
        /// <summary>
        /// 获取当前文档
        /// </summary>
        /// <returns></returns>
        public static Document GetMdiActiveDocument()
        {
            return Application.DocumentManager.MdiActiveDocument;
        }
        public static T GetEntity<T>(ObjectId objectId) where T :Entity
        {
            T entity=null;
            if (objectId.IsValid)
            {
                Document doc = GetMdiActiveDocument();
                using (Transaction trans=doc.TransactionManager.StartTransaction())
                {
                    DBObject dbObj = objectId.GetObject(OpenMode.ForRead);
                    entity = (T)dbObj;
                    trans.Commit();
                }
            }
            return entity;
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
                    if (objIds[i] == ObjectId.Null || objIds[i].IsErased || objIds[i].IsValid == false)
                    {
                        continue;
                    }
                    DBObject dbObj = trans.GetObject(objIds[i], OpenMode.ForWrite);
                    dbObj.Erase();
                }
                trans.Commit();
            }
        }
        /// <summary>
        /// 创建多段线
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="isClosed"></param>
        /// <param name="lineWidth"></param>
        /// <returns></returns>
        public static Polyline CreatePolyline(Point3dCollection pts, bool isClosed = true, double lineWidth = 0.0)
        {
            Point2dCollection p2dPts = new Point2dCollection();
            foreach (Point3d pt in pts)
            {
                p2dPts.Add(new Point2d(pt.X, pt.Y));
            }
            return CreatePolyline(p2dPts, isClosed, lineWidth);
        }
        /// <summary>
        /// 创建没有圆弧的多段线
        /// </summary>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static Polyline CreatePolyline(Point2dCollection pts, bool isClosed = true, double lineWidth = 0)
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
        /// <summary>
        /// 加多个物体添加到Database
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
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
        /// 把单个物体添加到Database
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="visible"></param>
        /// <returns></returns>
        public static ObjectId AddToBlockTable(Entity ent, bool visible)
        {
            ObjectId objId = ObjectId.Null;//存储要插入的块参照的Id
            Document doc = GetMdiActiveDocument();
            Database db = doc.Database;//获取数据库对象
            if (ent.ObjectId != ObjectId.Null)
            {
                return objId;
            }
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable; //以读的方式打开块表
                BlockTableRecord modelSpace = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                modelSpace.UpgradeOpen();
                if (!visible)
                {
                    ent.Visible = false;
                }
                objId = modelSpace.AppendEntity(ent);
                trans.AddNewlyCreatedDBObject(ent, true);
                modelSpace.DowngradeOpen();
                trans.Commit();
            }
            return objId;
        }
        /// <summary>
        /// 选择Polyline内或相交的物体
        /// </summary>
        /// <param name="ed"></param>
        /// <param name="polygon"></param>
        /// <param name="mode"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
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
        /// 按矩形框选择
        /// </summary>
        /// <param name="ed"></param>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <param name="mode"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static PromptSelectionResult SelectByRectangle(Editor ed, Point3d pt1, Point3d pt2, 
            PolygonSelectionMode mode, SelectionFilter filter = null)
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
            if (filter == null)
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
        /// 显示或隐藏多个物体
        /// </summary>
        /// <param name="objIds"> 传入的物体</param>
        /// <param name="isShow">true->显示，false->隐藏</param>
        public static void ShowObjIds(ObjectId[] objIds, bool isShow = true)
        {
            if (objIds == null && objIds.Length == 0)
            {
                return;
            }
            var doc = GetMdiActiveDocument();
            using (Transaction trans = doc.TransactionManager.StartTransaction())
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
                        Utils.WriteException(ex, "ShowObjIds");
                    }
                }
                trans.Commit();
            }
        }
        /// <summary>
        /// 显示或隐藏多个物体
        /// </summary>
        /// <param name="objIds"></param>
        /// <param name="isShow"></param>
        public static void ShowObjIds(List<ObjectId> objIds, bool isShow = true)
        {
            if (objIds != null && objIds.Count > 0)
            {
                ShowObjIds(objIds.ToArray(), isShow);
            }
        }
        /// <summary>
        /// 显示或隐藏单个物体
        /// </summary>
        /// <param name="objId"></param>
        /// <param name="isShow"></param>
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
                    Utils.WriteException(ex, "ShowObjId");
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
        /// <summary>
        /// 锁定层
        /// </summary>
        /// <param name="layerNameList"></param>
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
        /// <summary>
        /// 炸块
        /// </summary>
        /// <param name="br"></param>
        /// <param name="keepUnVisible"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 获取所有图层中包括制定文字的图层集合
        /// </summary>
        /// <param name="contain"></param>
        /// <returns></returns>
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
        /// 判断两个曲线是否相交
        /// </summary>
        /// <param name="curve1"></param>
        /// <param name="curve2"></param>
        /// <returns></returns>
        public static bool JudgeTwoCurveIsOverLap(Curve curve1, Curve curve2)
        {
            bool result = false;
            try
            {
                if(curve1==null || curve2==null)
                {
                    return result;
                }
                if(curve1.IsDisposed || curve2.IsDisposed)
                {
                    result = false
                }
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
            catch (System.Exception ex)
            {
                Utils.WriteException(ex, "JudgeTwoCurveIsOverLap,Parameter->Curve,Curve");
            }
            return result;
        }
        /// <summary>
        /// 判断两个曲线是否相交
        /// </summary>
        /// <param name="firstCurvePts"></param>
        /// <param name="secondCurvePts"></param>
        /// <returns></returns>
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
            catch (System.Exception ex)
            {
                Utils.WriteException(ex, "JudgeTwoCurveIsOverLap,Parameter->List<Point3d>,List<Point3d>");
            }
            return result;
        }
        /// <summary>
        /// 改变颜色
        /// </summary>
        /// <param name="objId"></param>
        /// <param name="colorIndex"></param>
        public static void ChangeColor(ObjectId objId, short colorIndex)
        {
            if (objId == ObjectId.Null)
            {
                return;
            }
            Document doc = GetMdiActiveDocument();
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                Entity ent = trans.GetObject(objId, OpenMode.ForRead) as Entity;
                ent.UpgradeOpen();
                ent.ColorIndex = colorIndex;
                ent.DowngradeOpen();
                trans.Commit();
            }
        }
        /// <summary>
        /// 改变文字内容
        /// </summary>
        /// <param name="textId"></param>
        /// <param name="content"></param>
        public static void ChangeText(ObjectId textId, string content)
        {
            if (!CheckObjIdIsValid(textId))
            {
                return;
            }
            Document doc = GetMdiActiveDocument();
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                DBObject dbObj = trans.GetObject(textId, OpenMode.ForRead) as Entity;
                if (dbObj is DBText dbText)
                {
                    dbText.UpgradeOpen();
                    dbText.TextString = content;
                    dbText.DowngradeOpen();
                }
                else if (dbObj is MText mText)
                {
                    mText.UpgradeOpen();
                    mText.Contents = content;
                    mText.DowngradeOpen();
                }
                trans.Commit();
            }
        }
        /// <summary>
        /// 系统颜色转变为画刷
        /// </summary>
        /// <param name="sysColor"></param>
        /// <returns></returns>
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
        public static void SetLayer(ObjectId objId, ObjectId layerId)
        {
            if (objId == ObjectId.Null || objId.IsErased || !objId.IsValid)
            {
                return;
            }
            if (layerId == ObjectId.Null || layerId.IsErased || !layerId.IsValid)
            {
                return;
            }
            Document doc = GetMdiActiveDocument();
            using (Transaction trans = doc.Database.TransactionManager.StartTransaction())
            {
                if (trans.GetObject(objId, OpenMode.ForRead) is Entity ent)
                {
                    ent.UpgradeOpen();
                    ent.LayerId = layerId;
                    ent.DowngradeOpen();
                }
                trans.Commit();
            }
        }
        public static Point3d PointToPoint3d(IPoint point)
        {
            return new Point3d(point.X, point.Y, point.Z);
        }
        public static bool ThreePointsCollinear(IPoint firstPt,IPoint secondPt,IPoint pt,double range=0.0)
        {
            Point3d lineSp = PointToPoint3d(firstPt);
            Point3d lineEp = PointToPoint3d(secondPt);
            Point3d outerPt = PointToPoint3d(pt);
            return ThreePointsCollinear(lineSp, lineEp, outerPt, range);
        }
        public static bool ThreePointsCollinear(Point3d lineSp, Point3d lineEp, Point3d outerPt, double range = 0.0)
        {
            Vector3d line1Vec = lineSp.GetVectorTo(lineEp);
            Vector3d line2Vec = lineSp.GetVectorTo(outerPt);
            double angle = line1Vec.GetAngleTo(line2Vec);
            double dis = Math.Sin(angle) * line2Vec.Length;
            if (dis <= range)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 三点共线
        /// </summary>
        /// <param name="lineSp"></param>
        /// <param name="lineEp"></param>
        /// <param name="outerPt"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static bool ThreePointsCollinear1(Point3d lineSp, Point3d lineEp, Point3d outerPt, double range=0.0)
        {
            // 结果测试是对的，几何意义不理解
            Vector3d line1Vec = lineSp.GetVectorTo(lineEp);
            Vector3d line2Vec = lineSp.GetVectorTo(outerPt);
            Vector3d crossProduct = line1Vec.CrossProduct(line2Vec);
            double dis = crossProduct.Length / line1Vec.Length;
            if (dis <= range)
            {
                return true;
            }
            return false;
        }
        public static bool IsCollinear(Vector3d firstVec, Vector3d secondVec)
        {
            firstVec = firstVec.GetNormal();
            secondVec = secondVec.GetNormal();

            double x1 = firstVec.X;
            double y1 = firstVec.Y;
            double z1 = firstVec.Z;

            double x2 = secondVec.X;
            double y2 = secondVec.Y;
            double z2 = secondVec.Z;

            if(x1*y2==x2*y1 && y1*z2==z1*y2 && x1*z2==x2*z1)
            {
                return true;
            }
            return false;
        }
        
    }
    public enum PolygonSelectionMode
    {
        Crossing,
        Window
    }
}
