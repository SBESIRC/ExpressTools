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

namespace ThSpray
{
    public class ConnectedNode
    {
        public ConnectedNode(List<LineSegment2d> relatedCurvesNode)
        {
            m_curNode = relatedCurvesNode;
        }

        public bool m_bUse = false;
        public List<LineSegment2d> m_curNode = null;
        public List<ConnectedNode> m_relatedConnectNodes = new List<ConnectedNode>();
    }

    /// <summary>
    /// 连通区域的外包框
    /// </summary>
    public class ConnectedBoxBound
    {
        private List<List<LineSegment2d>> m_curvesLst;
        private List<List<LineSegment2d>> m_outBounds = new List<List<LineSegment2d>>();
        public List<List<LineSegment2d>> OutBounds
        {
            get { return m_outBounds; }
        }

        private List<ConnectedNode> m_ConnectedNodes = new List<ConnectedNode>();
        public ConnectedBoxBound(List<List<LineSegment2d>> curvesLst)
        {
            m_curvesLst = curvesLst;
        }

        /// <summary>
        /// 生成相邻区域外包轮廓
        /// </summary>
        /// <param name="curvesLst"></param>
        /// <returns></returns>
        public static List<List<LineSegment2d>> MakeRelatedBoundary(List<List<LineSegment2d>> curvesLst)
        {
            var boxBound = new ConnectedBoxBound(curvesLst);
            var relatedCurves = boxBound.Do();
            boxBound.CalculateBounds(relatedCurves);
            return boxBound.OutBounds;
        }

        public static List<List<LineSegment2d>> MakeRelatedCurves(List<List<LineSegment2d>> curvesLst)
        {
            var boxBound = new ConnectedBoxBound(curvesLst);
            return boxBound.Do();
        }

        public List<List<LineSegment2d>> Do()
        {
            for (int i = 0; i < m_curvesLst.Count; i++)
            {
                m_ConnectedNodes.Add(new ConnectedNode(m_curvesLst[i]));
            }

            for (int i = 0; i < m_ConnectedNodes.Count; i++)
            {
                var curProfile = m_ConnectedNodes[i];
                for (int j = i + 1; j < m_ConnectedNodes.Count; j++)
                {
                    var nextProfile = m_ConnectedNodes[j];
                    if (IsAreaConnectArea(curProfile.m_curNode, nextProfile.m_curNode))
                    {
                        curProfile.m_relatedConnectNodes.Add(nextProfile);
                        nextProfile.m_relatedConnectNodes.Add(curProfile);
                    }
                }
            }

            var connectedAreaLst = new List<List<LineSegment2d>>();
            // 相邻区域收集
            for (int j = 0; j < m_ConnectedNodes.Count; j++)
            {
                if (m_ConnectedNodes[j].m_bUse)
                    continue;

                if (m_ConnectedNodes[j].m_relatedConnectNodes.Count == 0)
                {
                    m_ConnectedNodes[j].m_bUse = true;
                    connectedAreaLst.Add(m_ConnectedNodes[j].m_curNode);
                }
                else
                {
                    var loops = SearchFromOneArea(m_ConnectedNodes[j]);
                    connectedAreaLst.Add(loops);
                }
            }

            return connectedAreaLst;
        }

        /// <summary>
        /// 计算相邻区域的外包框
        /// </summary>
        /// <param name="connectedAreaLst"></param>
        public void CalculateBounds(List<List<LineSegment2d>> connectedAreaLst)
        {
            foreach (var connectCurves in connectedAreaLst)
            {
                // 经过外扩的
                var boundLines = GetBoundFromLine2ds(connectCurves);
                if (boundLines != null && boundLines.Count != 0)
                {
                    m_outBounds.Add(boundLines);
                }
            }
        }

        /// <summary>
        ///  计算外包边界
        /// </summary>
        /// <param name="line2ds"></param>
        /// <returns></returns>
        public static List<LineSegment2d> GetBoundFromLine2ds(List<LineSegment2d> line2ds)
        {
            if (line2ds == null || line2ds.Count == 0)
                return null;

            var boundLines = new List<LineSegment2d>();
            var ptLst = new List<XY>();
            foreach (var line in line2ds)
            {
                var pxHead = line.StartPoint;
                var pxEnd = line.EndPoint;
                var ptS = new XY(pxHead.X, pxHead.Y);
                var ptE = new XY(pxEnd.X, pxEnd.Y);
                ptLst.Add(ptS);
                ptLst.Add(ptE);
            }

            var leftBottom = new XY(ptLst[0].X, ptLst[0].Y);
            var rightTop = new XY(ptLst[0].X, ptLst[0].Y);
            for (int i = 1; i < ptLst.Count; i++)
            {
                if (leftBottom.X > ptLst[i].X)
                    leftBottom.X = ptLst[i].X;
                if (leftBottom.Y > ptLst[i].Y)
                    leftBottom.Y = ptLst[i].Y;

                if (rightTop.X < ptLst[i].X)
                    rightTop.X = ptLst[i].X;
                if (rightTop.Y < ptLst[i].Y)
                    rightTop.Y = ptLst[i].Y;
            }

            leftBottom = leftBottom - new XY(10, 10);
            rightTop = rightTop + new XY(10, 10);
            var pt1 = new Point2d(leftBottom.X, leftBottom.Y);
            var pt3 = new Point2d(rightTop.X, rightTop.Y);
            var pt2 = new Point2d(pt3.X, pt1.Y);
            var pt4 = new Point2d(pt1.X, pt3.Y);

            var line1 = new LineSegment2d(pt1, pt2);
            var line2 = new LineSegment2d(pt2, pt3);
            var line3 = new LineSegment2d(pt3, pt4);
            var line4 = new LineSegment2d(pt4, pt1);
            boundLines.Add(line1);
            boundLines.Add(line2);
            boundLines.Add(line3);
            boundLines.Add(line4);
            return boundLines;
        }

        // 从一块区域开始搜索
        private List<LineSegment2d> SearchFromOneArea(ConnectedNode Searchprofile)
        {
            var line2ds = new List<LineSegment2d>();
            var InOutNodes = new List<ConnectedNode>();
            InOutNodes.Add(Searchprofile);
            while (InOutNodes.Count != 0)
            {
                var curProfile = InOutNodes.First();
                line2ds.AddRange(curProfile.m_curNode);
                curProfile.m_bUse = true;
                InOutNodes.RemoveAt(0);
                var childConnectNodes = curProfile.m_relatedConnectNodes;
                foreach (var connectedNode in childConnectNodes)
                {
                    if (!connectedNode.m_bUse && !InOutNodes.Contains(connectedNode))
                        InOutNodes.Add(connectedNode);
                }
            }

            return line2ds;
        }

        /// <summary>
        /// 重叠
        /// </summary>
        /// <param name="lineFir"></param>
        /// <param name="lineSec"></param>
        /// <returns></returns>
        private bool IsOverlap(LineSegment2d lineFir, LineSegment2d lineSec)
        {
            var ptFirS = lineFir.StartPoint;
            var ptFirE = lineFir.EndPoint;
            var ptSecS = lineSec.StartPoint;
            var ptSecE = lineSec.EndPoint;
            if (CommonUtils.IsPointOnSegment(ptFirS, lineSec) || CommonUtils.IsPointOnSegment(ptFirE, lineSec)
               || CommonUtils.IsPointOnSegment(ptSecS, lineFir) || CommonUtils.IsPointOnSegment(ptSecE, lineFir))
                return true;
            return false;
        }

        // 区域之间有连通的
        private bool IsAreaConnectArea(List<LineSegment2d> curvesFir, List<LineSegment2d> curvesSec)
        {
            foreach (var curveFir in curvesFir)
            {
                foreach (var curveSec in curvesSec)
                {
                    var intersectPts = curveFir.IntersectWith(curveSec);
                    if (intersectPts != null && intersectPts.Count() == 1)
                    {
                        // intersect
                        return true;
                    }
                    else
                    {
                        // not intersect
                        if (IsOverlap(curveFir, curveSec))
                            return true;
                    }
                }
            }

            return false;
        }
    }
    class Utils
    {
        public static int INDEX = 0;
        /// <summary>
        /// 获取所有curves
        /// </summary>
        /// <returns></returns>
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

        public static List<Curve> Line2d2Curve(List<LineSegment2d> line2ds)
        {
            var curves = new List<Curve>();
            foreach (var line2d in line2ds)
            {
                var ptS = line2d.StartPoint;
                var ptE = line2d.EndPoint;
                var pt3S = new Point3d(ptS.X, ptS.Y, 0);
                var pt3E = new Point3d(ptE.X, ptE.Y, 0);
                var line = new Line(pt3S, pt3E);
                curves.Add(line);
            }

            return curves;
        }

        public static void CreateGroup(List<List<TopoEdge>> topoEdges, string groupName, string showName)
        {

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            Utils.CreateLayer(showName, Color.FromRgb(255, 0, 0));

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // Get the group dictionary from the drawing
                DBDictionary gd = (DBDictionary)tr.GetObject(db.GroupDictionaryId, OpenMode.ForRead);

                foreach (var loop in topoEdges)
                {
                    var loopcurves = new List<Curve>();
                    foreach (var edge in loop)
                    {
                        loopcurves.Add(edge.SrcCurve);
                    }

                    //Random random = new Random();
                    //int value = random.Next(100000000);
                    Utils.INDEX++;
                    string grpName = groupName + Utils.INDEX;
                    //do
                    //{
                    //    inputGrpName = groupName + value;
                    //    try
                    //    {
                    //        if (gd.Contains(inputGrpName))
                    //            ed.WriteMessage("\nA group with this name already exists.");
                    //        else
                    //            grpName = inputGrpName;
                    //    }
                    //    catch
                    //    {
                    //        ed.WriteMessage("\nInvalid group name.");
                    //    }
                    //} while (grpName == "");

                    // Create our new group...
                    Group grp = new Group("Profile group", true);
                    var color = Color.FromRgb(255, 255, 0);
                    grp.SetColor(color);
                    // Add the new group to the dictionary
                    gd.UpgradeOpen();
                    ObjectId grpId = gd.SetAt(grpName, grp);
                    tr.AddNewlyCreatedDBObject(grp, true);
                    // Open the model-space
                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                    // Add some lines to the group to form a square
                    // (the entities belong to the model-space)

                    ObjectIdCollection ids = new ObjectIdCollection();
                    foreach (Entity ent in loopcurves)
                    {
                        ent.Layer = showName;
                        ObjectId id = ms.AppendEntity(ent);
                        ids.Add(id);
                        tr.AddNewlyCreatedDBObject(ent, true);
                    }

                    grp.InsertAt(0, ids);
                }

                tr.Commit();
            }
        }

        public static void DrawPreviewPoint(DBObjectCollection objCol, Point3d pt, double length = 500)
        {
            var dbCollection = new DBObjectCollection();
            var half = length * 0.5;
            var curveFirStart = pt + new Vector3d(1, 1, 0).GetNormal() * half;
            var curveFirEnd = curveFirStart - new Vector3d(1, 1, 0).GetNormal() * length;
            var curFir = new Line(curveFirStart, curveFirEnd);
            objCol.Add(curFir);

            var curveSecStart = pt + new Vector3d(-1, 1, 0).GetNormal() * half;
            var curveSecEnd = curveSecStart - new Vector3d(-1, 1, 0).GetNormal() * length;
            var curveSec = new Line(curveSecStart, curveSecEnd);
            objCol.Add(curveSec);
            IntegerCollection intCol = new IntegerCollection();
            using (AcadDatabase acad = AcadDatabase.Active())
            {
                Autodesk.AutoCAD.GraphicsInterface.TransientManager tm = Autodesk.AutoCAD.GraphicsInterface.TransientManager.CurrentTransientManager;
                tm.AddTransient(curFir, Autodesk.AutoCAD.GraphicsInterface.TransientDrawingMode.DirectTopmost, 128, intCol);
                tm.AddTransient(curveSec, Autodesk.AutoCAD.GraphicsInterface.TransientDrawingMode.DirectTopmost, 128, intCol);
            }
        }

        public static void ErasePreviewPoint(DBObjectCollection dbCol)
        {
            using (AcadDatabase acad = AcadDatabase.Active())
            {
                Autodesk.AutoCAD.GraphicsInterface.TransientManager tm = Autodesk.AutoCAD.GraphicsInterface.TransientManager.CurrentTransientManager;
                foreach (DBObject obj in dbCol)
                {
                    tm.EraseTransient(obj, new IntegerCollection());
                    obj.Dispose();
                }
            }
        }

        /// <summary>
        /// 创建新的图层
        /// </summary>
        /// <param name="allLayers"></param>
        /// <param name="aimLayer"></param>
        public static void CreateLayer(string aimLayer, Color color)
        {
            LayerTableRecord layerRecord = null;
            using (var db = AcadDatabase.Active())
            {
                foreach (var layer in db.Layers)
                {
                    if (layer.Name.Equals(aimLayer))
                    {
                        layerRecord = db.Layers.Element(aimLayer);
                        break;
                    }
                }

                // 创建新的图层
                if (layerRecord == null)
                {
                    layerRecord = db.Layers.Create(aimLayer);
                    layerRecord.Color = color;
                    layerRecord.IsPlottable = false;
                }
            }
        }

        /// <summary>
        /// 显示
        /// </summary>
        /// <param name="curvesLst"></param>
        public static void ShowCurves(List<List<Curve>> curvesLst)
        {
            var polylines = Convert2Polyline(curvesLst);
            DisplayBoundaryProfile(polylines, "polyline");
        }

        /// <summary>
        /// 界面显示
        /// </summary>
        /// <param name="roomDataPolylines"></param>
        /// <param name="layerName"></param>
        public static void DisplayBoundaryProfile(List<Polyline> boundaryPolylines, string boundaryLayerName)
        {
            if (boundaryPolylines == null || boundaryPolylines.Count == 0)
                return;

            using (var db = AcadDatabase.Active())
            {
                CreateLayer(boundaryLayerName, Color.FromRgb(255, 0, 0));

                foreach (var polyline in boundaryPolylines)
                {
                    var objectPolylineId = db.ModelSpace.Add(polyline);
                    db.ModelSpace.Element(objectPolylineId, true).Layer = boundaryLayerName;
                }
            }
        }

        /// <summary>
        /// 界面显示
        /// </summary>
        /// <param name="roomDataPolylines"></param>
        /// <param name="layerName"></param>
        public static void DrawProfile(List<Curve> curves, string LayerName, Color color = null)
        {
            if (curves == null || curves.Count == 0)
                return;

            using (var db = AcadDatabase.Active())
            {
                if (color == null)
                    CreateLayer(LayerName, Color.FromRgb(255, 0, 0));
                else
                    CreateLayer(LayerName, color);

                foreach (var curve in curves)
                {
                    var objectCurveId = db.ModelSpace.Add(curve.Clone() as Curve);
                    db.ModelSpace.Element(objectCurveId, true).Layer = LayerName;
                }
            }
        }

        // 绘图，用于测试点的位置
        public static void DrawLinesWithTransaction(List<TopoEdge> edges, string layerName = null)
        {
            if (edges == null || edges.Count == 0)
                return;
            using (AcadDatabase acad = AcadDatabase.Active())
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                if (layerName != null)
                    CreateLayer(layerName, Color.FromRgb(255, 0, 0));

                foreach (var edge in edges)
                {
                    var ptS = edge.Start;
                    var ptE = edge.End;
                    var line = new Line(new Point3d(ptS.X, ptS.Y, 0), new Point3d(ptE.X, ptE.Y, 0));
                    line.Color = Color.FromRgb(0, 255, 255);
                    if (layerName != null)
                        line.Layer = layerName;
                    // 添加到modelSpace中
                    AcHelper.DocumentExtensions.AddEntity<Line>(doc, line);
                }
            }
        }

        /// <summary>
        /// 格式转换
        /// </summary>
        /// <param name="curvesLst"></param>
        /// <returns></returns>
        public static List<Polyline> Convert2Polyline(List<List<Curve>> curvesLst)
        {
            if (curvesLst == null || curvesLst.Count == 0)
                return null;

            var polylineLst = new List<Polyline>();
            foreach (var curves in curvesLst)
            {
                var polyline = new Polyline();
                polyline.Closed = true;

                for (int i = 0; i < curves.Count; i++)
                {
                    var point = curves[i].StartPoint;
                    polyline.AddVertexAt(i, new Point2d(point.X, point.Y), 0, 0, 0);
                }

                polylineLst.Add(polyline);
            }

            return polylineLst;
        }

        /// <summary>
        /// 获取block中的所有曲线数据
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public static List<Curve> GetCurvesFromBlock(BlockReference block, bool bSetLayer = true)
        {
            DBObjectCollection collection = new DBObjectCollection();
            block.Explode(collection);
            var blockCurves = new List<Curve>();
            using (var db = AcadDatabase.Active())
            {
                foreach (var obj in collection)
                {
                    if (obj is Curve)
                    {
                        var curve = obj as Curve;
                        if (bSetLayer)
                            curve.Layer = block.Layer;
                        blockCurves.Add(curve);
                    }
                    else if (obj is BlockReference)
                    {
                        var blockReference = obj as BlockReference;
                        if (bSetLayer)
                            blockReference.Layer = block.Layer;
                        var childCurves = GetCurvesFromBlock(blockReference, bSetLayer);
                        if (childCurves.Count != 0)
                            blockCurves.AddRange(childCurves);
                    }
                }
            }

            return blockCurves;
        }

        /// <summary>
        /// 获取指定图层中的所有曲线
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static List<Curve> GetAllCurvesFromLayerNames(List<string> layerNames)
        {
            var curves = new List<Curve>();
            var curveIds = new List<ObjectId>();
            var blockCurves = new List<Curve>();
            using (var db = AcadDatabase.Active())
            {

                // curve 读取
                var res = db.ModelSpace.OfType<Curve>().Where(p =>
                {
                    foreach (var layerName in layerNames)
                    {
                        if (p.Layer == layerName)
                            return true;
                    }

                    return false;
                });

                foreach (var text in res)
                {
                    curveIds.Add(text.ObjectId);
                }

                var blocks = db.ModelSpace.OfType<BlockReference>();
                foreach (var block in blocks)
                {
                    if (ValidBlock(block, layerNames))
                    {
                        var blockRelatedCurves = GetCurvesFromBlock(block);
                        if (blockRelatedCurves.Count != 0)
                            blockCurves.AddRange(blockRelatedCurves);
                    }
                    else
                    {
                        DBObjectCollection collection = new DBObjectCollection();

                        try
                        {
                            block.Explode(collection);
                            foreach (var obj in collection)
                            {
                                if (obj is Curve)
                                {
                                    var curve = obj as Curve;
                                    if (ValidLayer(curve.Layer, layerNames))
                                        blockCurves.Add(curve);
                                }
                            }
                        }
                        catch
                        {
                            // 有些block炸开的时候会抛出eCannotScaleNonUniformly异常
                            // 如果这个block 不能炸开，不作处理
                        }
                    }
                }
            }

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database dataBase = doc.Database;

            using (Transaction dataGetTrans = dataBase.TransactionManager.StartTransaction())
            {
                // 图片多段线
                foreach (var curveId in curveIds)
                {
                    Curve curve = (Curve)dataGetTrans.GetObject(curveId, OpenMode.ForRead);
                    curves.Add((Curve)curve.Clone());
                }
            }

            if (blockCurves.Count != 0)
                curves.AddRange(blockCurves);
            return curves;
        }

        /// <summary>
        /// 是否是有效的curve
        /// </summary>
        /// <param name="curveLayer"></param>
        /// <param name="layerNames"></param>
        /// <returns></returns>
        public static bool ValidLayer(string curveLayer, List<string> layerNames)
        {
            foreach (var layerName in layerNames)
            {
                if (curveLayer == layerName)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 是否是有效的块
        /// </summary>
        /// <param name="block"></param>
        /// <param name="layerNames"></param>
        /// <returns></returns>
        public static bool ValidBlock(BlockReference block, List<string> layerNames)
        {
            foreach (var layerName in layerNames)
            {
                if (block.Layer == layerName)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 数据打撒成直线段
        /// </summary>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static List<LineSegment2d> TesslateCurve2Lines(List<Curve> curves)
        {
            var lines = new List<LineSegment2d>();
            foreach (var curve in curves)
            {
                if (curve is Line)
                {
                    var line = curve as Line;
                    var ptS = line.StartPoint;
                    var ptE = line.EndPoint;
                    var lineSegment2d = new LineSegment2d(new Point2d(ptS.X, ptS.Y), new Point2d(ptE.X, ptE.Y));
                    lines.Add(lineSegment2d);
                }
                else if (curve is Arc)
                {
                    var arc = curve as Arc;
                    var polyline = arc.Spline.ToPolyline();
                    var lineNodes = Polyline2Lines(polyline as Polyline);
                    if (lineNodes != null)
                        lines.AddRange(lineNodes);
                }
                else if (curve is Circle)
                {
                    var circle = curve as Circle;
                    var spline = circle.Spline;
                    var polyline = spline.ToPolyline();
                    var lineNodes = Polyline2Lines(polyline as Polyline);
                    if (lineNodes != null)
                        lines.AddRange(lineNodes);
                }
                else if (curve is Ellipse)
                {
                    var ellipse = curve as Ellipse;
                    var polyline = ellipse.Spline.ToPolyline();
                    var lineNodes = Polyline2Lines(polyline as Polyline);
                    if (lineNodes != null)
                        lines.AddRange(lineNodes);
                }
                else if (curve is Polyline)
                {
                    var lineNodes = Polyline2Lines(curve as Polyline);
                    if (lineNodes != null)
                        lines.AddRange(lineNodes);
                }
                else if (curve is Spline)
                {
                    var polyline = (curve as Spline).ToPolyline();
                    if (polyline is Polyline)
                    {
                        var lineNodes = Polyline2Lines(polyline as Polyline);
                        if (lineNodes != null)
                            lines.AddRange(lineNodes);
                    }
                }
            }

            return lines;
        }

        /// <summary>
        /// 多段线转换为直线
        /// </summary>
        /// <param name="polyline"></param>
        /// <returns></returns>
        public static List<LineSegment2d> GetLineFromPolyline(Polyline polyline)
        {
            if (polyline == null)
                return null;

            var lines = new List<LineSegment2d>();
            if (polyline.Closed)
            {
                for (int i = 0; i < polyline.NumberOfVertices; i++)
                {
                    var bulge = polyline.GetBulgeAt(i);
                    if (CommonUtils.IsAlmostNearZero(bulge))
                    {
                        var line2d = polyline.GetLineSegment2dAt(i);
                        lines.Add(line2d);
                    }
                }
            }
            else
            {
                for (int j = 0; j < polyline.NumberOfVertices - 1; j++)
                {
                    var bulge = polyline.GetBulgeAt(j);
                    if (CommonUtils.IsAlmostNearZero(bulge))
                    {
                        var line2d = polyline.GetLineSegment2dAt(j);
                        lines.Add(line2d);
                    }
                }
            }

            return lines;
        }

        /// <summary>
        /// 多段线转换为直线
        /// </summary>
        /// <param name="polyline"></param>
        /// <returns></returns>
        public static List<LineSegment2d> Polyline2Lines(Polyline polyline)
        {
            if (polyline == null)
                return null;

            var lines = new List<LineSegment2d>();
            if (polyline.Closed)
            {
                for (int i = 0; i < polyline.NumberOfVertices; i++)
                {
                    var bulge = polyline.GetBulgeAt(i);
                    if (CommonUtils.IsAlmostNearZero(bulge))
                    {
                        var line2d = polyline.GetLineSegment2dAt(i);
                        lines.Add(line2d);
                    }
                    else
                    {
                        var type = polyline.GetSegmentType(i);
                        if (type == SegmentType.Arc)
                        {
                            var arc3d = polyline.GetArcSegmentAt(i);
                            var normal = arc3d.Normal;
                            var axisZ = Vector3d.ZAxis;
                            var arc = new Arc();
                            if (normal.IsEqualTo(Vector3d.ZAxis.Negate()))
                                arc.CreateArcSCE(arc3d.EndPoint, arc3d.Center, arc3d.StartPoint);
                            else
                                arc.CreateArcSCE(arc3d.StartPoint, arc3d.Center, arc3d.EndPoint);
                            var pline = arc.Spline.ToPolyline();
                            var lineNodes = Polyline2Lines(pline as Polyline);
                            if (lineNodes != null)
                                lines.AddRange(lineNodes);
                        }
                    }
                }
            }
            else
            {
                for (int j = 0; j < polyline.NumberOfVertices - 1; j++)
                {
                    var bulge = polyline.GetBulgeAt(j);
                    if (CommonUtils.IsAlmostNearZero(bulge))
                    {
                        var line2d = polyline.GetLineSegment2dAt(j);
                        lines.Add(line2d);
                    }
                    else
                    {
                        var type = polyline.GetSegmentType(j);
                        if (type == SegmentType.Arc)
                        {
                            var arc3d = polyline.GetArcSegmentAt(j);
                            var normal = arc3d.Normal;
                            var axisZ = Vector3d.ZAxis;
                            var arc = new Arc();
                            if (normal.IsEqualTo(Vector3d.ZAxis.Negate()))
                                arc.CreateArcSCE(arc3d.EndPoint, arc3d.Center, arc3d.StartPoint);
                            else
                                arc.CreateArcSCE(arc3d.StartPoint, arc3d.Center, arc3d.EndPoint);
                            var pline = arc.Spline.ToPolyline();
                            var lineNodes = Polyline2Lines(pline as Polyline);
                            if (lineNodes != null)
                                lines.AddRange(lineNodes);
                        }
                    }
                }
            }

            return lines;
        }

        /// <summary>
        ///  计算curves的外包边界
        /// </summary>
        /// <param name="curves"></param>
        /// <returns></returns>
        public static List<LineSegment2d> GetBoundFromCurves(List<Curve> curves)
        {
            if (curves == null || curves.Count == 0)
                return null;

            var boundLines = new List<LineSegment2d>();
            var lineNodes = Utils.TesslateCurve2Lines(curves);
            var ptLst = new List<XY>();
            foreach (var line in lineNodes)
            {
                var pxHead = line.StartPoint;
                var pxEnd = line.EndPoint;
                var ptS = new XY(pxHead.X, pxHead.Y);
                var ptE = new XY(pxEnd.X, pxEnd.Y);
                ptLst.Add(ptS);
                ptLst.Add(ptE);
            }

            var leftBottom = new XY(ptLst[0].X, ptLst[0].Y);
            var rightTop = new XY(ptLst[0].X, ptLst[0].Y);
            for (int i = 1; i < ptLst.Count; i++)
            {
                if (leftBottom.X > ptLst[i].X)
                    leftBottom.X = ptLst[i].X;
                if (leftBottom.Y > ptLst[i].Y)
                    leftBottom.Y = ptLst[i].Y;

                if (rightTop.X < ptLst[i].X)
                    rightTop.X = ptLst[i].X;
                if (rightTop.Y < ptLst[i].Y)
                    rightTop.Y = ptLst[i].Y;
            }

            var pt1 = new Point2d(leftBottom.X, leftBottom.Y);
            var pt3 = new Point2d(rightTop.X, rightTop.Y);
            var pt2 = new Point2d(pt3.X, pt1.Y);
            var pt4 = new Point2d(pt1.X, pt3.Y);

            var line1 = new LineSegment2d(pt1, pt2);
            var line2 = new LineSegment2d(pt2, pt3);
            var line3 = new LineSegment2d(pt3, pt4);
            var line4 = new LineSegment2d(pt4, pt1);
            boundLines.Add(line1);
            boundLines.Add(line2);
            boundLines.Add(line3);
            boundLines.Add(line4);
            return boundLines;
        }

        /// <summary>
        /// 获取指定图层中连通区域的包围盒边界处理
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static List<List<LineSegment2d>> GetBoundsFromLayerBlocksAndCurves(List<string> layerNames)
        {
            var blockBounds = new List<List<LineSegment2d>>();
            using (var db = AcadDatabase.Active())
            {
                // 块轮廓
                var blocks = db.ModelSpace.OfType<BlockReference>();
                foreach (var block in blocks)
                {
                    if (ValidBlock(block, layerNames))
                    {
                        var blockCurves = GetCurvesFromBlock(block);
                        var lines = GetBoundFromCurves(blockCurves);
                        if (lines == null || lines.Count == 0)
                            continue;

                        blockBounds.Add(lines);
                    }
                }

                var layerCurves = db.ModelSpace.OfType<Curve>().Where(l =>
                {
                    foreach (var layerName in layerNames)
                    {
                        if (l.Layer == layerName)
                            return true;
                    }

                    return false;
                }).ToList<Curve>();

                // 线数据处理
                if (layerCurves != null && layerCurves.Count != 0)
                {
                    foreach (var curve in layerCurves)
                    {
                        if (curve is Line)
                        {
                            var line = curve as Line;
                            var ptS = new Point2d(line.StartPoint.X, line.StartPoint.Y);
                            var ptE = new Point2d(line.EndPoint.X, line.EndPoint.Y);
                            blockBounds.Add(new List<LineSegment2d>() { new LineSegment2d(ptS, ptE) });
                        }
                        else
                        {
                            var curveBounds = Utils.GetRectFromBound(curve.Bounds.Value);
                            blockBounds.Add(curveBounds);
                        }
                    }
                }
            }

            // 生成相邻框的数据，框边缘是外扩的， 上面的框边缘都是原始的，不经过外扩的数据
            var connectBounds = ConnectedBoxBound.MakeRelatedBoundary(blockBounds);
            return connectBounds;
        }

        /// <summary>
        /// 线是否与曲线集相交
        /// </summary>
        /// <param name="srcLines"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public static bool IsLinesIntersectWithLine(List<LineSegment2d> srcLines, LineSegment2d line)
        {
            foreach (var srcline in srcLines)
            {
                var intersectPts = srcline.IntersectWith(line);
                if (intersectPts != null && intersectPts.Count() != 0)
                    return true;
            }

            return false;
        }


        // 直线处理
        public static List<LineSegment2d> BoundRelatedBoundCurves(List<LineSegment2d> doorBound, List<Curve> wallCurves)
        {
            var relatedLines = new List<LineSegment2d>();
            foreach (var curve in wallCurves)
            {
                if (curve is Line)
                {
                    var line = curve as Line;
                    var line2d = new LineSegment2d(new Point2d(line.StartPoint.X, line.StartPoint.Y), new Point2d(line.EndPoint.X, line.EndPoint.Y));
                    if (IsLinesIntersectWithLine(doorBound, line2d) || (CommonUtils.PointInnerEntity(doorBound, line2d.StartPoint) && CommonUtils.PointInnerEntity(doorBound, line2d.EndPoint)))
                    {
                        relatedLines.Add(line2d);
                    }
                }
            }

            return relatedLines;
        }

        /// <summary>
        /// 共线处理
        /// </summary>
        /// <param name="lineFir"></param>
        /// <param name="lineSec"></param>
        /// <returns></returns>
        public static bool IsCollinearLines(LineSegment2d lineFir, LineSegment2d lineSec)
        {
            var angle1 = lineFir.Direction.GetAngleTo(lineSec.Direction);
            var angle2 = lineFir.Direction.GetAngleTo(lineSec.Direction.Negate());
            if (CommonUtils.IsAlmostNearZero(Math.Abs(angle1), 1e-5) || CommonUtils.IsAlmostNearZero(Math.Abs(angle2), 1e-5))
            {
                var ptFirS = lineFir.StartPoint;
                var ptSecS = lineSec.StartPoint;
                var vector = ptFirS - ptSecS;
                var angle3 = lineFir.Direction.GetAngleTo(vector);
                var angle4 = lineFir.Direction.GetAngleTo(vector.Negate());
                if (CommonUtils.IsAlmostNearZero(Math.Abs(angle3), 1e-5) || CommonUtils.IsAlmostNearZero(Math.Abs(angle4), 1e-5))
                {
                    // 共线直线之间的距离不能太小
                    var ptFirE = lineFir.EndPoint;
                    var ptSecE = lineSec.EndPoint;
                    if (ptSecS.GetDistanceTo(ptFirS) > 10 && ptSecS.GetDistanceTo(ptFirE) > 10 && ptSecE.GetDistanceTo(ptFirS) > 10 && ptSecE.GetDistanceTo(ptFirE) > 10)
                        return true;
                }

            }

            return false;
        }

        /// <summary>
        /// 是否有平行共线的数据，如果没有，则进行相交处理
        /// </summary>
        /// <param name="relatedDatas"></param>
        /// <returns> 没有平行共线的处理则返回true</returns>
        public static bool IsNotHaveCollinearDatas(List<CollinearData> relatedDatas)
        {
            foreach (var relatedData in relatedDatas)
            {
                // 只要有false则说明有共线的处理
                if (!relatedData.Valid)
                    return false;
            }

            return true;
        }

        //共线数据生成
        public static List<Curve> InsertConnectCurves(List<LineSegment2d> line2ds, string layer = null)
        {
            var relatedDatas = new List<CollinearData>();
            foreach (var line in line2ds)
            {
                relatedDatas.Add(new CollinearData(line));
            }

            for (int i = 0; i < relatedDatas.Count; i++)
            {
                if (!relatedDatas[i].Valid)
                    continue;

                for (int j = i + 1; j < relatedDatas.Count; j++)
                {
                    if (!relatedDatas[j].Valid)
                        continue;

                    //共线
                    if (IsCollinearLines(relatedDatas[i].Line, relatedDatas[j].Line))
                    {
                        relatedDatas[i].RelatedCollinearLines.Add(((LineSegment2d)relatedDatas[j].Line.Clone()));
                        relatedDatas[j].Valid = false;
                    }
                }
            }

            // 如果没有平行线，则做相交处理
            if (IsNotHaveCollinearDatas(relatedDatas))
            {
                var innerLinesLst = new List<List<LineSegment2d>>();
                relatedDatas.ForEach(p =>
                {
                    innerLinesLst.Add(new List<LineSegment2d>() { p.Line });
                });

                var relatedCurvesLst = ConnectedBoxBound.MakeRelatedCurves(innerLinesLst);
                if (relatedCurvesLst.Count == 2)
                {
                    return ConnectLinesWithsLines(relatedCurvesLst.First(), relatedCurvesLst.Last(), layer);
                }
                else
                    return null;
            }
            else
            {
                // 有共线则生成共线数据
                var curves = new List<Curve>();
                foreach (var data in relatedDatas)
                {
                    if (data.Valid)
                    {
                        var line = MakeLineFromCollinears(data);
                        if (line != null)
                        {
                            if (layer != null)
                                line.Layer = layer;
                            curves.Add(line);
                        }
                    }
                }

                return curves;
            }
        }

        public static List<Entity> PreProcessCurDwg(List<string> validLayers)
        {
            var resEntityLst = new List<Entity>();
            double progressPos = 5;
            // 本图纸数据块处理
            using (var db = AcadDatabase.Active())
            {
                var blockRefs = db.CurrentSpace.OfType<BlockReference>().Where(p => p.Visible).ToList();
                var incre = 10.0 / blockRefs.Count;
                foreach (var blockReference in blockRefs)
                {
                    var layerId = blockReference.LayerId;
                    if (layerId == null || !layerId.IsValid)
                        continue;

                    LayerTableRecord layerTableRecord = db.Element<LayerTableRecord>(layerId);
                    if (layerTableRecord.IsOff)
                        continue;

                    var blockId = blockReference.BlockTableRecord;
                    var blockRefRecord = db.Element<BlockTableRecord>(blockId);
                    if (blockRefRecord.IsFromExternalReference)
                        continue;
                    progressPos += 0.4;
                    progressPos += incre;
                    var entityLst = GetEntityFromBlock(blockReference);
                    if (entityLst != null && entityLst.Count > 1)
                    {
                        foreach (var entity in entityLst)
                        {
                            // 除了块，其余的可以用图层确定是否收集，块的图层内部数据也可能符合要求
                            if (!entity.Equals(blockReference) && IsValidLayer(entity, validLayers))
                            {
                                db.CurrentSpace.Add(entity);
                                resEntityLst.Add(entity);
                            }
                        }
                    }
                }
            }

            return resEntityLst;
        }

        public static bool IsValidLayer(Entity entity, List<string> validLayers)
        {
            if (validLayers.Count == 0)
                return false;
            if (entity is BlockReference)
                return true;

            foreach (var layer in validLayers)
            {
                if (entity.Layer.Contains(layer))
                    return true;
            }
            return false;
        }

        public static bool IsLinesIntersectWithLines(List<LineSegment2d> srcLines, List<LineSegment2d> rectLines)
        {
            foreach (var rectLine in rectLines)
            {
                foreach (var line in srcLines)
                {
                    var intersectPts = line.IntersectWith(rectLine);
                    if (intersectPts != null && intersectPts.Count() != 0)
                        return true;
                }
            }

            return false;
        }

        public static void PostProcess(List<Entity> removeEntityLst)
        {
            using (var db = AcadDatabase.Active())
            {
                if (removeEntityLst != null && removeEntityLst.Count != 0)
                {
                    foreach (var entity in removeEntityLst)
                    {
                        var openEntity = db.Element<Entity>(entity.Id, true);
                        openEntity.Erase(true);
                    }
                }
            }
        }

        public static bool IsValidBlockReference(BlockReference block, List<LineSegment2d> rectLines)
        {
            var blockCurves = GetCurvesFromBlock(block, false);
            var lines = GetBoundFromCurves(blockCurves);
            if (lines == null || lines.Count == 0)
                return false;

            if (CommonUtils.OutLoopContainsInnerLoop(rectLines, lines) || IsLinesIntersectWithLines(lines, rectLines))
                return true;

            return false;
        }

        /// <summary>
        /// 从block单个数据
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public static List<Entity> GetEntityFromBlock(BlockReference block)
        {
            if (block == null || !block.Visible)
                return null;

            var entityLst = new List<Entity>();
            var blockReferences = new List<BlockReference>();
            blockReferences.Add(block);
            while (blockReferences.Count > 0)
            {
                var curBlock = blockReferences.First();
                blockReferences.RemoveAt(0);

                if (curBlock.Visible)
                {
                    var entitysInBlock = FromSingleBlock(curBlock, ref blockReferences);
                    if (entitysInBlock != null && entitysInBlock.Count != 0)
                    {
                        entityLst.AddRange(entitysInBlock);
                    }
                }
            }

            return entityLst;
        }

        /// <summary>
        /// 判断单个能否炸开，以及返回的数据
        /// </summary>
        /// <param name="block"></param>
        /// <param name="childReferences"></param>
        /// <returns></returns>
        public static List<Entity> FromSingleBlock(BlockReference block, ref List<BlockReference> childReferences)
        {
            if (block == null)
                return null;

            var entityLst = new List<Entity>();
            try
            {
                var dbCollection = new DBObjectCollection();
                block.Explode(dbCollection);

                if (IsHasBlockReference(dbCollection)) // 内部包含块
                {
                    foreach (var obj in dbCollection)
                    {
                        if (obj is Entity)
                        {
                            if (obj is BlockReference)
                            {
                                var childBlock = obj as BlockReference;
                                if (childBlock.Visible)
                                    childReferences.Add(childBlock);
                            }
                            else
                            {
                                var entity = obj as Entity;
                                if (entity.Visible)
                                    entityLst.Add(entity);
                            }
                        }
                    }
                }
                else if (IsCanExplode(dbCollection)) // 内部不包含块且曲线所在的图层名包含3个以上
                {
                    foreach (var obj in dbCollection)
                    {
                        if (obj is Entity)
                        {
                            var entity = obj as Entity;
                            if (entity.Visible)
                                entityLst.Add(entity);
                        }
                    }
                }
                else // 内部不包含块且曲线所在的图层名小于3个图层
                {
                    // 此时这个块不被炸开作为一个整体。
                    if (block.Visible)
                        entityLst.Add(block);
                }
            }
            catch
            {
                return null;
            }

            return entityLst;
        }

        /// <summary>
        /// 是否含有块
        /// </summary>
        /// <param name="dbCollection"></param>
        /// <returns></returns>
        public static bool IsHasBlockReference(DBObjectCollection dbCollection)
        {
            if (dbCollection == null || dbCollection.Count == 0)
                return false;

            foreach (var obj in dbCollection)
            {
                if (obj is BlockReference)
                {
                    var curBlock = obj as BlockReference;
                    if (curBlock.Visible)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 一个块中是否有多3个图层
        /// </summary>
        /// <param name="dbCollection"></param>
        /// <returns></returns>
        public static bool IsCanExplode(DBObjectCollection dbCollection)
        {
            var layerNames = new List<string>();
            foreach (var obj in dbCollection)
            {
                if (obj is Curve)
                {
                    var curve = obj as Curve;
                    if (curve.Visible)
                    {
                        var layerName = curve.Layer;
                        if (!layerNames.Contains(layerName))
                        {
                            layerNames.Add(layerName);
                            if (layerNames.Count > 3)
                                return true;
                        }
                    }
                }
            }

            return false;
        }

        public static List<Entity> PreProcessXREF(List<string> validLayers)
        {
            var resEntityLst = new List<Entity>();
            // 外部参照
            double progressPos = 17;
            using (var db = AcadDatabase.Active())
            {
                var refs = db.XRefs;
                var incre = 10.0 / refs.Count();

                foreach (var xblock in refs)
                {
                    if (xblock.Block.XrefStatus == XrefStatus.Resolved)
                    {
                        ObjectIdCollection idCollection = new ObjectIdCollection();
                        BlockTableRecord blockTableRecord = xblock.Block;
                        List<BlockReference> blockReferences = blockTableRecord.GetAllBlockReferences(true, true).ToList();
                        for (int i = 0; i < blockReferences.Count(); i++)
                        {
                            var blockReference = blockReferences[i];
                            progressPos += 0.4;
                            progressPos += incre;
                            var entityLst = GetEntityFromBlock(blockReference);
                            if (entityLst != null && entityLst.Count != 0)
                            {
                                foreach (var entity in entityLst)
                                {
                                    if (!entity.Equals(blockReference))
                                    {
                                        try
                                        {
                                            if (entity is DBText || entity is MText)
                                            {
                                                continue;
                                            }
                                            if (entity is BlockReference)
                                            {
                                                // 块里面的数据可能是有效单元， 剔除文字的影响
                                                var reference = entity as BlockReference;
                                                var dbCollection = new DBObjectCollection();
                                                reference.Explode(dbCollection);
                                                foreach (var part in dbCollection)
                                                {
                                                    if (part is DBText || part is MText)
                                                    {
                                                        continue;
                                                    }
                                                    else if (part is Entity)
                                                    {
                                                        var partEntity = part as Entity;
                                                        partEntity.Layer = reference.Layer;
                                                        db.CurrentSpace.Add(partEntity);
                                                        resEntityLst.Add(partEntity);
                                                    }
                                                }
                                            }
                                            else if (IsValidLayer(entity, validLayers))
                                            {
                                                db.CurrentSpace.Add(entity);
                                                resEntityLst.Add(entity);
                                            }
                                        }
                                        catch
                                        { }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return resEntityLst;
        }

        /// <summary>
        /// 图纸预处理
        /// </summary>
        public static List<Entity> PreProcess(List<string> validLayers)
        {
            var resEntityLst = new List<Entity>();
            var curEntityLst = PreProcessCurDwg(validLayers);

            if (curEntityLst.Count != 0)
                resEntityLst.AddRange(curEntityLst);

            var xRefEntityLst = PreProcessXREF(validLayers);
            if (xRefEntityLst.Count != 0)
                resEntityLst.AddRange(xRefEntityLst);
            return resEntityLst;
        }

        /// <summary>
        /// 由共线数据集生成新的直线
        /// </summary>
        /// <param name="collinearLines"></param>
        /// <returns></returns>
        public static Line MakeLineFromCollinears(CollinearData collinearData)
        {
            if (collinearData.RelatedCollinearLines == null || collinearData.RelatedCollinearLines.Count == 0)
                return null;

            var ptLst = new List<Point2d>();
            foreach (var line2d in collinearData.RelatedCollinearLines)
            {
                ptLst.Add(line2d.StartPoint);
                ptLst.Add(line2d.EndPoint);
            }

            var curLine = collinearData.Line;
            ptLst.Add(curLine.StartPoint);
            ptLst.Add(curLine.EndPoint);

            Tolerance tolerance = new Tolerance(1e-4, 0);
            //跟Y轴平行
            if (collinearData.Line.Direction.IsParallelTo(new Vector2d(0, 1), tolerance))
            {
                ptLst.Sort((p1, p2) => { return p1.Y.CompareTo(p2.Y); });
            }
            else
            {
                ptLst.Sort((p1, p2) => { return p1.X.CompareTo(p2.X); });
            }

            Point2d ptS;
            Point2d ptE;
            if (ptLst.Count > 3)
            {
                ptS = ptLst[1];
                ptE = ptLst[2];
            }
            else
            {
                ptS = ptLst.First();
                ptE = ptLst.Last();
            }

            var line = new Line(new Point3d(ptS.X, ptS.Y, 0), new Point3d(ptE.X, ptE.Y, 0));
            return line;
        }

        public static bool IsParallel(LineSegment2d lineFir, LineSegment2d lineSec)
        {
            var angle1 = lineFir.Direction.GetAngleTo(lineSec.Direction);
            var angle2 = lineFir.Direction.GetAngleTo(lineSec.Direction.Negate());
            if (CommonUtils.IsAlmostNearZero(Math.Abs(angle1), 1e-5) || CommonUtils.IsAlmostNearZero(Math.Abs(angle2), 1e-5))
                return true;
            return false;
        }

        public static LineSegment2d ExtendLine(LineSegment2d line)
        {
            var ptS = line.StartPoint;
            var ptE = line.EndPoint;
            ptS = ptS - line.Direction * 1000000;
            ptE = ptE + line.Direction * 1000000;
            return new LineSegment2d(ptS, ptE);
        }

        /// <summary>
        /// 一块区域和另一块区域的连接处理
        /// </summary>
        /// <param name="line2dsFir"></param>
        /// <param name="line2dsSec"></param>
        /// <returns></returns>
        public static List<Curve> ConnectLinesWithsLines(List<LineSegment2d> line2dsFir, List<LineSegment2d> line2dsSec, string layerName = null)
        {
            //DrawCurves(line2dsFir);
            //DrawCurves(line2dsSec);
            var curves = new List<Curve>();
            foreach (var firLine in line2dsFir)
            {
                foreach (var secLine in line2dsSec)
                {
                    if (IsParallel(firLine, secLine))
                        continue;

                    // 非平行
                    var firLineExtend = ExtendLine(firLine);
                    var secLineExtend = ExtendLine(secLine);
                    var intersectPoints = firLineExtend.IntersectWith(secLineExtend);

                    if (intersectPoints != null && intersectPoints.Count() == 1)
                    {
                        Point2d pt = intersectPoints.First();
                        var ptFirHead = firLine.StartPoint;
                        var ptFirEnd = firLine.EndPoint;
                        var ptSecHead = secLine.StartPoint;
                        var ptSecEnd = secLine.EndPoint;
                        Line line = null;
                        if (CommonUtils.IsPointOnSegment(pt, firLine, 1e-1))
                        {
                            if (pt.GetDistanceTo(ptSecHead) < pt.GetDistanceTo(ptSecEnd))
                                line = new Line(new Point3d(pt.X, pt.Y, 0), new Point3d(ptSecHead.X, ptSecHead.Y, 0));
                            else
                                line = new Line(new Point3d(pt.X, pt.Y, 0), new Point3d(ptSecEnd.X, ptSecEnd.Y, 0));
                        }
                        else if (CommonUtils.IsPointOnSegment(pt, secLine, 1e-1))
                        {
                            if (pt.GetDistanceTo(ptFirHead) < pt.GetDistanceTo(ptFirEnd))
                                line = new Line(new Point3d(pt.X, pt.Y, 0), new Point3d(ptFirHead.X, ptFirHead.Y, 0));
                            else
                                line = new Line(new Point3d(pt.X, pt.Y, 0), new Point3d(ptFirEnd.X, ptFirEnd.Y, 0));
                        }

                        if (line != null)
                        {
                            if (layerName != null)
                                line.Layer = layerName;

                            // 剔除重复的增加线
                            if (!IsInValidLine(curves, line))
                                curves.Add(line);
                        }
                    }

                }
            }

            return curves;
        }

        public static bool IsInValidLine(List<Curve> curves, Line line)
        {
            if (curves.Count == 0)
                return false;

            foreach (var curve in curves)
            {
                if (curve is Line)
                {
                    var addLine = curve as Line;
                    if (CommonUtils.IsAlmostNearZero(addLine.Length - line.Length, 1))
                    {
                        var ptAddS = addLine.StartPoint;
                        var ptAddE = addLine.EndPoint;
                        var ptS = line.StartPoint;
                        var ptE = line.EndPoint;
                        if ((CommonUtils.Point3dIsEqualPoint3d(ptAddS, ptS, 0.1) && CommonUtils.Point3dIsEqualPoint3d(ptAddE, ptE, 0.1))
                            || (CommonUtils.Point3dIsEqualPoint3d(ptAddS, ptE, 0.1) && CommonUtils.Point3dIsEqualPoint3d(ptAddE, ptS, 0.1)))
                            return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 门和墙的曲线连接
        /// </summary>
        /// <param name="doorBounds"></param>
        /// <param name="wallCurves"></param>
        public static List<Curve> InsertDoorRelatedCurveDatas(List<List<LineSegment2d>> doorBounds, List<Curve> wallCurves, string layer)
        {
            if ((doorBounds == null || doorBounds.Count == 0) || (wallCurves == null || wallCurves.Count == 0))
                return null;

            var curves = new List<Curve>();
            foreach (var doorBound in doorBounds)
            {
                // 相关数据内部或者相交
                var relatedCurves = BoundRelatedBoundCurves(doorBound, wallCurves);
                if (relatedCurves == null || relatedCurves.Count == 0)
                    continue;

                // 根据相关数据进行连接处理
                // 收尾相连且共线的线段进行合并直线处理
                var combineCurves = CombineCollinearLines(relatedCurves);
                var insertCurves = InsertConnectCurves(combineCurves, layer);
                if (insertCurves != null && insertCurves.Count != 0)
                {
                    curves.AddRange(insertCurves);
                }
            }

            return curves;
        }

        public class CollinearData
        {
            public LineSegment2d Line
            {
                get;
                set;
            }

            public bool Valid
            {
                get;
                set;
            }

            public List<LineSegment2d> RelatedCollinearLines
            {
                get;
                set;
            }

            public CollinearData(LineSegment2d line)
            {
                Line = line;
                Valid = true;
                RelatedCollinearLines = new List<LineSegment2d>();
            }

        }

        class PathNearSearch
        {
            class PathNode
            {
                public PathNode(LineSegment2d line)
                {
                    m_curLine = line;
                }

                public bool m_bUse = false;
                public LineSegment2d m_curLine = null;
                public List<PathNode> m_relatedConnectNodes = new List<PathNode>();
            }

            private List<LineSegment2d> m_lines;
            private List<LineSegment2d> m_outLines = new List<LineSegment2d>();
            public List<LineSegment2d> OutLines
            {
                get { return m_outLines; }
            }

            private List<PathNode> m_ConnectedNodes = new List<PathNode>();
            public PathNearSearch(List<LineSegment2d> lines)
            {
                m_lines = lines;
            }

            public static List<LineSegment2d> MakeMergeLines(List<LineSegment2d> lines)
            {
                var pathSearch = new PathNearSearch(lines);
                pathSearch.Do();
                return pathSearch.OutLines;
            }

            public void Do()
            {
                for (int i = 0; i < m_lines.Count; i++)
                {
                    m_ConnectedNodes.Add(new PathNode(m_lines[i]));
                }

                for (int i = 0; i < m_ConnectedNodes.Count; i++)
                {
                    var curProfile = m_ConnectedNodes[i];
                    for (int j = i + 1; j < m_ConnectedNodes.Count; j++)
                    {
                        var nextProfile = m_ConnectedNodes[j];
                        if (IsLineNear(curProfile.m_curLine, nextProfile.m_curLine))
                        {
                            curProfile.m_relatedConnectNodes.Add(nextProfile);
                            nextProfile.m_relatedConnectNodes.Add(curProfile);
                        }
                    }
                }

                // 相邻区域收集
                for (int j = 0; j < m_ConnectedNodes.Count; j++)
                {
                    if (m_ConnectedNodes[j].m_bUse)
                        continue;

                    if (m_ConnectedNodes[j].m_relatedConnectNodes.Count == 0)
                    {
                        m_ConnectedNodes[j].m_bUse = true;
                        m_outLines.Add(m_ConnectedNodes[j].m_curLine);
                    }
                    else
                    {
                        var nearLines = SearchFromOneLine(m_ConnectedNodes[j]);
                        var mergeLine = MergeLines(nearLines);
                        if (mergeLine != null)
                            m_outLines.Add(mergeLine);
                    }
                }
            }

            private LineSegment2d MergeLines(List<LineSegment2d> lines)
            {
                if (lines == null || lines.Count == 0)
                    return null;
                var ptLst = new List<Point2d>();
                foreach (var line in lines)
                {
                    ptLst.Add(line.StartPoint);
                    ptLst.Add(line.EndPoint);
                }

                //跟Y轴平行
                if (lines[0].Direction.IsParallelTo(new Vector2d(0, 1)))
                {
                    ptLst.Sort((p1, p2) => { return p1.Y.CompareTo(p2.Y); });
                }
                else
                {
                    ptLst.Sort((p1, p2) => { return p1.X.CompareTo(p2.X); });
                }

                Point2d ptS = ptLst.First();
                Point2d ptE = ptLst.Last();
                var outLine = new LineSegment2d(ptS, ptE);
                return outLine;
            }

            // 从一块区域开始搜索
            private List<LineSegment2d> SearchFromOneLine(PathNode SearchLine)
            {
                var line2ds = new List<LineSegment2d>();
                var InOutNodes = new List<PathNode>();
                InOutNodes.Add(SearchLine);
                while (InOutNodes.Count != 0)
                {
                    var curProfile = InOutNodes.First();
                    line2ds.Add(curProfile.m_curLine);
                    curProfile.m_bUse = true;
                    InOutNodes.RemoveAt(0);
                    var childConnectNodes = curProfile.m_relatedConnectNodes;
                    foreach (var connectedNode in childConnectNodes)
                    {
                        if (!connectedNode.m_bUse && !InOutNodes.Contains(connectedNode))
                            InOutNodes.Add(connectedNode);
                    }
                }

                return line2ds;
            }

            // 线与线之间是连接的
            private bool IsLineNear(LineSegment2d lineFir, LineSegment2d lineSec)
            {
                var ptFirHead = lineFir.StartPoint;
                var ptFirTail = lineFir.EndPoint;
                var ptSecHead = lineSec.StartPoint;
                var ptSecTail = lineSec.EndPoint;

                if (CommonUtils.IsAlmostNearZero(ptFirHead.GetDistanceTo(ptSecHead), 1e-2) || CommonUtils.IsAlmostNearZero(ptFirHead.GetDistanceTo(ptSecTail), 1e-2)
                    || CommonUtils.IsAlmostNearZero(ptFirTail.GetDistanceTo(ptSecHead), 1e-2) || CommonUtils.IsAlmostNearZero(ptFirTail.GetDistanceTo(ptSecTail), 1e-2))
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        ///  合并相连的直线
        /// </summary>
        class CombineLine
        {
            public List<LineSegment2d> CombineLines
            {
                get;
                set;
            }

            public List<LineSegment2d> OutLines
            {
                get;
                set;
            }

            public CombineLine(List<LineSegment2d> srcLines)
            {
                CombineLines = srcLines;
                OutLines = new List<LineSegment2d>();
            }

            public static List<LineSegment2d> MakeCombineLines(List<LineSegment2d> srcLines)
            {
                var combine = new CombineLine(srcLines);
                combine.Do();
                return combine.OutLines;
            }

            private bool IsCollinearLines(LineSegment2d lineFir, LineSegment2d lineSec)
            {
                var angle1 = lineFir.Direction.GetAngleTo(lineSec.Direction);
                var angle2 = lineFir.Direction.GetAngleTo(lineSec.Direction.Negate());
                if (CommonUtils.IsAlmostNearZero(Math.Abs(angle1), 1e-5) || CommonUtils.IsAlmostNearZero(Math.Abs(angle2), 1e-5))
                {
                    var ptFirS = lineFir.StartPoint;
                    var ptSecS = lineSec.StartPoint;
                    var vector = ptFirS - ptSecS;
                    var angle3 = lineFir.Direction.GetAngleTo(vector);
                    var angle4 = lineFir.Direction.GetAngleTo(vector.Negate());
                    if (CommonUtils.IsAlmostNearZero(Math.Abs(angle3), 1e-5) || CommonUtils.IsAlmostNearZero(Math.Abs(angle4), 1e-5))
                    {
                        return true;
                    }

                }

                return false;
            }

            public void Do()
            {
                var relatedDatas = new List<CollinearData>();
                foreach (var line in CombineLines)
                {
                    relatedDatas.Add(new CollinearData(line));
                }

                for (int i = 0; i < relatedDatas.Count; i++)
                {
                    if (!relatedDatas[i].Valid)
                        continue;

                    for (int j = i + 1; j < relatedDatas.Count; j++)
                    {
                        if (!relatedDatas[j].Valid)
                            continue;

                        //共线
                        if (IsCollinearLines(relatedDatas[i].Line, relatedDatas[j].Line))
                        {
                            relatedDatas[i].RelatedCollinearLines.Add(((LineSegment2d)relatedDatas[j].Line.Clone()));
                            relatedDatas[j].Valid = false;
                        }
                    }
                }

                // 有共线则生成共线数据
                foreach (var data in relatedDatas)
                {
                    if (data.Valid)
                    {
                        if (data.RelatedCollinearLines.Count == 0)
                        {
                            OutLines.Add(data.Line);
                        }
                        else
                        {
                            var lines = CombineData(data);
                            if (lines.Count != 0)
                                OutLines.AddRange(lines);
                        }
                    }
                }
            }

            /// <summary>
            /// 合并相邻的数据
            /// </summary>
            /// <param name="CollinearData"></param>
            /// <returns></returns>
            private List<LineSegment2d> CombineData(CollinearData data)
            {
                var lines = new List<LineSegment2d>();
                lines.Add(data.Line);
                lines.AddRange(data.RelatedCollinearLines);
                var outLines = PathNearSearch.MakeMergeLines(lines);
                return outLines;
            }
        }

        /// <summary>
        /// 合并直线处理
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static List<LineSegment2d> CombineCollinearLines(List<LineSegment2d> lines)
        {
            if (lines == null || lines.Count == 0)
                return null;

            var combines = CombineLine.MakeCombineLines(lines);
            return combines;
        }

        /// <summary>
        /// 可扩展的矩形范围， vector 为空值时， 则是原始的范围
        /// </summary>
        /// <param name="bound"></param>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static List<LineSegment2d> GetRectFromBound(Extents3d bound, Vector3d? vector = null)
        {
            Point3d minPoint;
            Point3d maxPoint;
            if (vector != null)
            {
                minPoint = bound.MinPoint - vector.Value;
                maxPoint = bound.MaxPoint + vector.Value;
            }
            else
            {
                minPoint = bound.MinPoint;
                maxPoint = bound.MaxPoint;
            }

            var recPt1 = new Point2d(minPoint.X, minPoint.Y);
            var recPt2 = new Point2d(maxPoint.X, minPoint.Y);
            var recPt3 = new Point2d(maxPoint.X, maxPoint.Y);
            var recPt4 = new Point2d(minPoint.X, maxPoint.Y);
            var lines = new List<LineSegment2d>();
            if (!CommonUtils.IsAlmostNearZero(recPt1.GetDistanceTo(recPt2), 1e-4))
                lines.Add(new LineSegment2d(recPt1, recPt2));
            if (!CommonUtils.IsAlmostNearZero(recPt2.GetDistanceTo(recPt3), 1e-4))
                lines.Add(new LineSegment2d(recPt2, recPt3));
            if (!CommonUtils.IsAlmostNearZero(recPt3.GetDistanceTo(recPt4), 1e-4))
                lines.Add(new LineSegment2d(recPt3, recPt4));
            if (!CommonUtils.IsAlmostNearZero(recPt4.GetDistanceTo(recPt1), 1e-4))
                lines.Add(new LineSegment2d(recPt4, recPt1));
            return lines;
        }

        /// <summary>
        /// 打开需要显示的图层
        /// </summary>
        public static List<string> ShowThLayers(out List<string> wallLayers, out List<string> doorLayers, out List<string> windLayers, out List<string> allValidLayers)
        {
            var allCurveLayers = new List<string>();
            wallLayers = new List<string>();
            doorLayers = new List<string>();
            windLayers = new List<string>();
            allValidLayers = new List<string>();
            using (var db = AcadDatabase.Active())
            {
                var layers = db.Layers;
                var closeLayerNames = new List<string>();
                foreach (var layer in layers)
                {
                    //if (!layer.Name.Contains("AE-DOOR-INSD") && !layer.Name.Contains("AE-WALL") && !layer.Name.Contains("AE-WIND") && !layer.Name.Contains("AD-NAME-ROOM")
                    //     && !layer.Name.Contains("AE-STRU") && !layer.Name.Contains("S_COLU") && !layer.Name.Contains("天华面积框线"))
                    //{
                    //    closeLayerNames.Add(layer.Name);
                    //}

                    if (layer.Name.Contains("AE-WALL") || layer.Name.Contains("AD-NAME-ROOM")
                         || layer.Name.Contains("AE-STRU") || layer.Name.Contains("S_COLU"))
                    {
                        allCurveLayers.Add(layer.Name);
                    }

                    if (layer.Name.Contains("AE-WALL"))
                        wallLayers.Add(layer.Name);

                    if (layer.Name.Contains("AE-DOOR-INSD"))
                        doorLayers.Add(layer.Name);

                    if (layer.Name.Contains("AE-WIND"))
                        windLayers.Add(layer.Name);
                }

                allValidLayers.Add("AE-WALL");
                allValidLayers.Add("AD-NAME-ROOM");
                allValidLayers.Add("AE-STRU");
                allValidLayers.Add("S_COLU");
                allValidLayers.Add("AE-DOOR-INSD");
                allValidLayers.Add("AE-WIND");

                foreach (var lName in closeLayerNames)
                {
                    db.Layers.Element(lName, true).IsOff = true;
                }
            }

            return allCurveLayers;
        }
    }
}
