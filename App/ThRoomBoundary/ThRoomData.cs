using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Linq2Acad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThRoomBoundary.topo;

namespace ThRoomBoundary
{
    /// <summary>
    /// 门和相关墙数据的映射
    /// </summary>
    class DoorRelatedData
    {
        public List<LineSegment2d> DoorBound
        {
            get;
            set;
        }

        public List<LineSegment2d> WallLines
        {
            get;
            set;
        }

        public DoorRelatedData(List<LineSegment2d> doorBound, List<LineSegment2d> wallLines = null)
        {
            DoorBound = doorBound;
            WallLines = wallLines;
        }
    }

    class CollinearData
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
    class ThRoomUtils
    {
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
                // 相关数据
                var relatedCurves = BoundRelatedBoundCurves(doorBound, wallCurves);
                // 插入数据
                if (relatedCurves == null || relatedCurves.Count == 0)
                    continue;

                var insertCurves = InsertConnectCurves(relatedCurves, layer);
                if (insertCurves != null && insertCurves.Count != 0)
                {
                    curves.AddRange(insertCurves);
                }
            }

            return curves;
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

            //生成共线数据
            var curves = new List<Curve>();
            using (var db = AcadDatabase.Active())
            {
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
            }


            return curves;
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

            //跟Y轴平行
            if (collinearData.Line.Direction.IsParallelTo(new Vector2d(0, 1)))
            {
                ptLst.Sort((p1, p2) => { return p1.Y.CompareTo(p2.Y); });
            }
            else
            {
                ptLst.Sort((p1, p2) => { return p1.X.CompareTo(p2.X); });
            }

            var ptS = ptLst.First();
            var ptE = ptLst.Last();
            var line = new Line(new Point3d(ptS.X, ptS.Y, 0), new Point3d(ptE.X, ptE.Y, 0));
            return line;
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
        /// 绘制
        /// </summary>
        /// <param name="curves"></param>
        public static void DrawCurves(List<Curve> curves)
        {
            using (AcadDatabase acad = AcadDatabase.Active())
            {
                IntegerCollection intCol = new IntegerCollection();
                foreach (var curve in curves)
                {
                    curve.Color = Color.FromRgb(0, 255, 0);
                    Autodesk.AutoCAD.GraphicsInterface.TransientManager tm = Autodesk.AutoCAD.GraphicsInterface.TransientManager.CurrentTransientManager;

                    tm.AddTransient(curve, Autodesk.AutoCAD.GraphicsInterface.TransientDrawingMode.Highlight, 128, intCol);
                }
            }
        }

        /// <summary>
        /// 绘制
        /// </summary>
        /// <param name="curves"></param>
        public static void DrawCurvesAdd(List<Curve> curves)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            foreach (var curve in curves)
            {
                curve.Color = Color.FromRgb(0, 255, 255);
                // 添加到modelSpace中
                AcHelper.DocumentExtensions.AddEntity<Curve>(doc, curve);
            }
        }

        /// <summary>
        /// 绘制
        /// </summary>
        /// <param name="curves"></param>
        public static void DrawCurvesAdd(List<LineSegment2d> line2ds)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            var curves = new List<Curve>();
            foreach (var line2d in line2ds)
            {
                var start = line2d.StartPoint;
                var endPt = line2d.EndPoint;
                var startPoint = new Point3d(start.X, start.Y, 0);
                var endPoint = new Point3d(endPt.X, endPt.Y, 0);
                var line = new Line(startPoint, endPoint);
                curves.Add(line);
            }

            foreach (var curve in curves)
            {
                curve.Color = Color.FromRgb(0, 255, 255);
                // 添加到modelSpace中
                AcHelper.DocumentExtensions.AddEntity<Curve>(doc, curve);
            }
        }

        // 绘图，用于测试点的位置
        public static void DrawLinesWithTransaction(List<TopoEdge> edges)
        {
            using (AcadDatabase acad = AcadDatabase.Active())
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                foreach (var edge in edges)
                {
                    var ptS = edge.Start;
                    var ptE = edge.End;
                    var line = new Line(new Point3d(ptS.X, ptS.Y, 0), new Point3d(ptE.X, ptE.Y, 0));
                    line.Color = Color.FromRgb(0, 255, 255);
                    // 添加到modelSpace中
                    AcHelper.DocumentExtensions.AddEntity<Line>(doc, line);
                }
            }
        }

        // 绘图，用于测试点的位置
        public static void DrawLinesWithTransaction(List<LineSegment2d> lines)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            foreach (var line2d in lines)
            {
                var ptS = line2d.StartPoint;
                var ptE = line2d.EndPoint;
                var line = new Line(new Point3d(ptS.X, ptS.Y, 0), new Point3d(ptE.X, ptE.Y, 0));
                line.Color = Color.FromRgb(0, 255, 255);
                // 添加到modelSpace中
                AcHelper.DocumentExtensions.AddEntity<Line>(doc, line);
            }
        }

        /// <summary>
        /// 获取字体
        /// </summary>
        /// <returns></returns>
        public static ObjectId GetIdFromSymbolTable()
        {
            Database dbH = HostApplicationServices.WorkingDatabase;
            using (Transaction trans = dbH.TransactionManager.StartTransaction())
            {
                TextStyleTable textTableStyle = (TextStyleTable)trans.GetObject(dbH.TextStyleTableId, OpenMode.ForWrite);

                if (textTableStyle.Has("黑体"))
                {
                    ObjectId idres = textTableStyle["黑体"];
                    if (!idres.IsErased)
                        return idres;
                }
            }

            return ObjectId.Null;
        }

        /// <summary>
        /// 绘制
        /// </summary>
        /// <param name="curves"></param>
        public static void DrawTopoEdges(List<TopoEdge> curves)
        {
            using (AcadDatabase acad = AcadDatabase.Active())
            {
                IntegerCollection intCol = new IntegerCollection();
                foreach (var curve in curves)
                {
                    var ptS = curve.Start;
                    var ptE = curve.End;
                    var line = new Line(new Point3d(ptS.X, ptS.Y, 0), new Point3d(ptE.X, ptE.Y, 0));
                    line.Color = Color.FromRgb(0, 255, 0);
                    Autodesk.AutoCAD.GraphicsInterface.TransientManager tm = Autodesk.AutoCAD.GraphicsInterface.TransientManager.CurrentTransientManager;

                    tm.AddTransient(line, Autodesk.AutoCAD.GraphicsInterface.TransientDrawingMode.Highlight, 128, intCol);
                }
            }
        }

        // 绘图，用于测试点的位置
        public static void DrawPointWithTransaction(Point2d pt)
        {
            var drawPoint = new Point3d(pt.X, pt.Y, 0);
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Line lineX = new Line(drawPoint, drawPoint + new Vector3d(1, 0, 0) * 10000);
            Line lineY = new Line(drawPoint, drawPoint + new Vector3d(0, 1, 0) * 10000);

            // 添加到modelSpace中
            AcHelper.DocumentExtensions.AddEntity<Line>(doc, lineX);
            AcHelper.DocumentExtensions.AddEntity<Line>(doc, lineY);
        }

        /// <summary>
        /// 获取指定图层中的所有曲线
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static List<Curve> GetAllCurvesFromLayerName(string layerName)
        {
            var curves = new List<Curve>();
            var curveIds = new List<ObjectId>();
            using (var db = AcadDatabase.Active())
            {
                var res = db.ModelSpace.OfType<Curve>().Where(p => p.Layer == layerName);
                foreach (var text in res)
                {
                    curveIds.Add(text.ObjectId);
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

                if (curves.Count == 0)
                    return null;
            }

            return curves;
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
        /// 获取指定图层块中的所有曲线
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static List<List<LineSegment2d>> GetBoundsFromLayerBlocks(List<string> layerNames, List<LineSegment2d> rectLines)
        {
            var blockBounds = new List<List<LineSegment2d>>();
            using (var db = AcadDatabase.Active())
            {
                var blocks = db.ModelSpace.OfType<BlockReference>();
                foreach (var block in blocks)
                {
                    if (ValidBlock(block, layerNames))
                    {
                        var bound = block.Bounds.Value;
                        var lines = GetRectFromBound(bound);
                        if (CommonUtils.OutLoopContainsInnerLoop(rectLines, lines))
                            blockBounds.Add(lines);
                    }
                }
            }

            return blockBounds;
        }

        /// <summary>
        /// 获取block中的所有曲线数据
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public static List<Curve> GetCurvesFromBlock(BlockReference block)
        {
            DBObjectCollection collection = new DBObjectCollection();
            block.Explode(collection);
            var blockCurves = new List<Curve>();
            foreach (var obj in collection)
            {
                if (obj is Curve)
                {
                    var curve = obj as Curve;
                    curve.Layer = block.Layer;
                    blockCurves.Add(curve);
                }
                else if (obj is BlockReference)
                {
                    var blockReference = obj as BlockReference;
                    blockReference.Layer = block.Layer;
                    var childCurves = GetCurvesFromBlock(blockReference);
                    if (childCurves.Count != 0)
                        blockCurves.AddRange(childCurves);
                }
            }

            return blockCurves;
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
        /// 获取文本
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static List<DBText> GetAllTextFromLayerName(string layerName)
        {
            var texts = new List<DBText>();
            var textIds = new List<ObjectId>();
            using (var db = AcadDatabase.Active())
            {
                var res = db.ModelSpace.OfType<DBText>().Where(p => p.Layer == layerName);
                foreach (var text in res)
                {
                    textIds.Add(text.ObjectId);
                }
            }

            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database dataBase = doc.Database;

            using (Transaction dataGetTrans = dataBase.TransactionManager.StartTransaction())
            {
                // 图片多段线
                foreach (var textId in textIds)
                {
                    DBText curve = (DBText)dataGetTrans.GetObject(textId, OpenMode.ForRead);

                    texts.Add((DBText)curve.Clone());
                }

                if (texts.Count == 0)
                    return null;
            }

            return texts;
        }

        /// <summary>
        /// 获取框选范围的线的集合
        /// </summary>
        /// <param name="ptFir"></param>
        /// <param name="ptSec"></param>
        /// <returns></returns>
        public static List<LineSegment2d> GetRectangleFromPoint(Point3d ptFir, Point3d ptSec)
        {
            var ptLst = new List<XY>();
            var ptFir2d = new XY(ptFir.X, ptFir.Y);
            var ptSec2d = new XY(ptSec.X, ptSec.Y);
            ptLst.Add(ptFir2d);
            ptLst.Add(ptSec2d);
            var leftBottom = new XY(ptFir2d.X, ptFir2d.Y);
            var rightTop = new XY(ptFir2d.X, ptFir2d.Y);
            if (leftBottom.X > ptSec2d.X)
                leftBottom.X = ptSec2d.X;
            if (leftBottom.Y > ptSec2d.Y)
                leftBottom.Y = ptSec2d.Y;

            if (rightTop.X < ptSec2d.X)
                rightTop.X = ptSec2d.X;
            if (rightTop.Y < ptSec2d.Y)
                rightTop.Y = ptSec2d.Y;
            if (leftBottom.IsEqualTo(rightTop))
                return null;
            var recPt1 = new Point2d(leftBottom.X, leftBottom.Y);
            var recPt2 = new Point2d(rightTop.X, leftBottom.Y);
            var recPt3 = new Point2d(rightTop.X, rightTop.Y);
            var recPt4 = new Point2d(leftBottom.X, rightTop.Y);
            var lines = new List<LineSegment2d>();
            lines.Add(new LineSegment2d(recPt1, recPt2));
            lines.Add(new LineSegment2d(recPt2, recPt3));
            lines.Add(new LineSegment2d(recPt3, recPt4));
            lines.Add(new LineSegment2d(recPt4, recPt1));
            return lines;
        }

        /// <summary>
        /// 矩形框选择后的轮廓面积线
        /// </summary>
        /// <param name="roomDatas"></param>
        /// <param name="rectLines"></param>
        /// <returns></returns>
        public static List<RoomDataPolyline> MakeValidProfilesFromSelectRect(List<RoomData> roomDatas, List<LineSegment2d> rectLines)
        {
            if (roomDatas == null || roomDatas.Count == 0)
                return null;

            if (rectLines == null || rectLines.Count == 0)
                return null;

            var validRoomDatas = new List<RoomData>();
            foreach (var room in roomDatas)
            {
                if (room.Lines != null && room.Lines.Count != 0)
                {
                    if (ProfileInRectLines(room.Lines, rectLines))
                    {
                        validRoomDatas.Add(room);
                    }
                }
            }

            if (validRoomDatas.Count == 0)
                return null;

            var roomDataPolylines = TopoUtils.Convert2RoomDataPolyline(validRoomDatas);
            return roomDataPolylines;
        }

        public static bool ProfileInRectLines(List<Line> lines, List<LineSegment2d> rectLines)
        {
            foreach (var line in lines)
            {
                var startPoint3d = line.StartPoint;
                var startPoint2d = new Point2d(startPoint3d.X, startPoint3d.Y);
                if (!CommonUtils.PointInnerEntity(rectLines, startPoint2d))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 界面显示
        /// </summary>
        /// <param name="roomDataPolylines"></param>
        /// <param name="layerName"></param>
        public static void DisplayRoomProfile(List<RoomDataPolyline> roomDataPolylines, string layerName)
        {
            // 设置字体样式
            var textId = ThRoomUtils.GetIdFromSymbolTable();
            using (var db = AcadDatabase.Active())
            {
                var layers = db.Layers;
                LayerTableRecord layerRecord = null;
                foreach (var layer in layers)
                {
                    if (layer.Name.Equals(layerName))
                    {
                        layerRecord = db.Layers.Element(layerName);
                        break;
                    }
                }

                // 创建新的图层
                if (layerRecord == null)
                {
                    layerRecord = db.Layers.Create(layerName);
                    layerRecord.Color = Color.FromRgb(255, 0, 0);
                    layerRecord.IsPlottable = false;
                }

                foreach (var room in roomDataPolylines)
                {
                    if (room.ValidData)
                    {
                        //var area = (int)room.Area;
                        //var pos = room.Pos;
                        //var dbText = new DBText();
                        //if (textId != ObjectId.Null)
                        //    dbText.TextStyleId = textId;
                        //dbText.TextString = area.ToString();
                        //dbText.Position = pos;
                        //dbText.Height = 200;
                        //dbText.Thickness = 1;
                        //dbText.WidthFactor = 1;
                        //var objectTextId = db.ModelSpace.Add(dbText);
                        //db.ModelSpace.Element(objectTextId, true).Layer = layerName;
                        var objectPolylineId = db.ModelSpace.Add(room.RoomPolyline);
                        db.ModelSpace.Element(objectPolylineId, true).Layer = layerName;
                    }
                }
            }
        }

        /// <summary>
        /// 获取选择矩形框线
        /// </summary>
        /// <returns></returns>
        public static List<LineSegment2d> GetSelectRectLines()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            PromptPointOptions ppo = new PromptPointOptions("请框选需要生成房间线的范围");
            ppo.AllowNone = false;
            PromptPointResult ppr = ed.GetPoint(ppo);
            if (ppr.Status != PromptStatus.OK)
                return null;

            Point3d first = ppr.Value;
            PromptCornerOptions pco = new PromptCornerOptions(string.Empty, first);
            ppr = ed.GetCorner(pco);
            if (ppr.Status != PromptStatus.OK)
                return null;
            Point3d second = ppr.Value;

            //框线范围
            var rectLines = ThRoomUtils.GetRectangleFromPoint(first, second);
            return rectLines;
        }

        /// <summary>
        /// 获取矩形框截取后的curves
        /// </summary>
        /// <param name="allCurves"></param>
        /// <param name="rectLines"></param>
        /// <returns></returns>
        public static List<Curve> GetValidCurvesFromSelectArea(List<Curve> allCurves, List<LineSegment2d> rectLines)
        {
            if (allCurves == null || allCurves.Count == 0)
                return null;

            var validCurves = new List<Curve>();
            foreach (var srcCurve in allCurves)
            {
                if (IsValidCurve(srcCurve, rectLines))
                    validCurves.Add(srcCurve);
            }

            return validCurves;
        }

        public static bool IsValidCurve(Curve curve, List<LineSegment2d> rectLines)
        {
            var bound = curve.Bounds.Value;
            if (bound == null)
                return false;

            var srcLine2ds = GetRectFromBound(bound);
            if (IsLinesIntersectWithLines(srcLine2ds, rectLines) || CommonUtils.OutLoopContainsInnerLoop(rectLines, srcLine2ds))
                return true;

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

        public static List<LineSegment2d> GetRectFromBound(Extents3d bound)
        {
            var minPoint = bound.MinPoint - new Vector3d(300, 300, 0);
            var maxPoint = bound.MaxPoint + new Vector3d(300, 300, 0);
            var recPt1 = new Point2d(minPoint.X, minPoint.Y);
            var recPt2 = new Point2d(maxPoint.X, minPoint.Y);
            var recPt3 = new Point2d(maxPoint.X, maxPoint.Y);
            var recPt4 = new Point2d(minPoint.X, maxPoint.Y);
            var lines = new List<LineSegment2d>();
            lines.Add(new LineSegment2d(recPt1, recPt2));
            lines.Add(new LineSegment2d(recPt2, recPt3));
            lines.Add(new LineSegment2d(recPt3, recPt4));
            lines.Add(new LineSegment2d(recPt4, recPt1));
            return lines;
        }
        /// <summary>
        /// 打开需要显示的图层
        /// </summary>
        public static List<string> ShowLayers()
        {
            var aimLayers = new List<string>();
            using (var db = AcadDatabase.Active())
            {
                var layers = db.Layers;
                var closeLayerNames = new List<string>();
                foreach (var layer in layers)
                {
                    if (!layer.Name.Contains("WALL") && !layer.Name.Contains("AE-DOOR") && !layer.Name.Contains("AD-NAME") && !layer.Name.Contains("S_COLU")
                         && !layer.Name.Contains("WINDOW") && !layer.Name.Contains("天华面积框线"))
                    {
                        closeLayerNames.Add(layer.Name);
                    }

                    if (layer.Name.Contains("WALL") || layer.Name.Contains("AE-DOOR") || layer.Name.Contains("AD-NAME") || layer.Name.Contains("S_COLU")
                         || layer.Name.Contains("WINDOW"))
                    {
                        aimLayers.Add(layer.Name);
                    }
                }

                foreach (var lName in closeLayerNames)
                {
                    db.Layers.Element(lName, true).IsOff = true;
                }
            }

            return aimLayers;
        }

        /// <summary>
        /// 打开需要显示的图层
        /// </summary>
        public static List<string> ShowThLayers(out List<string> wallLayers, out List<string> doorLayers, out List<string> windLayers)
        {
            var allCurveLayers = new List<string>();
            wallLayers = new List<string>();
            doorLayers = new List<string>();
            windLayers = new List<string>();
            using (var db = AcadDatabase.Active())
            {
                var layers = db.Layers;
                var closeLayerNames = new List<string>();
                foreach (var layer in layers)
                {
                    if (!layer.Name.Contains("AE-DOOR-INSD") && !layer.Name.Contains("AE-WALL") && !layer.Name.Contains("AE-WIND") && !layer.Name.Contains("AD-NAME-ROOM")
                         && !layer.Name.Contains("AE-STRU") && !layer.Name.Contains("S_COLU") && !layer.Name.Contains("天华面积框线"))
                    {
                        closeLayerNames.Add(layer.Name);
                    }

                    if (layer.Name.Contains("AE-WALL")|| layer.Name.Contains("AD-NAME-ROOM")
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

                foreach (var lName in closeLayerNames)
                {
                    db.Layers.Element(lName, true).IsOff = true;
                }
            }

            return allCurveLayers;
        }

        /// <summary>
        /// 扣减墙本生的轮廓
        /// </summary>
        /// <param name="allRoomDataPolylines"></param>
        /// <param name="wallRoomDataPolyines"></param>
        /// <returns></returns>
        public static void MakeValidRoomDataPolylines(List<RoomDataPolyline> allRoomDataPolylines, List<RoomDataPolyline> wallRoomDataPolyines)
        {
            if (wallRoomDataPolyines == null || wallRoomDataPolyines.Count == 0)
                return;

            foreach (var curAllRoomData in allRoomDataPolylines)
            {
                if (curAllRoomData.ValidData)
                {
                    foreach (var curWallRoomData in wallRoomDataPolyines)
                    {
                        if (IsRoomDataSame(curAllRoomData, curWallRoomData))
                            break;
                    }
                }

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="srcRoomData"> 所有的轮廓中的一个</param>
        /// <param name="desRoomData"> 墙的轮廓中的一个</param>
        /// <returns></returns>
        public static bool IsRoomDataSame(RoomDataPolyline srcRoomData, RoomDataPolyline desRoomData)
        {
            if (desRoomData == null)
                return false;

            if (CommonUtils.IsAlmostNearZero(Math.Abs(srcRoomData.Area) - Math.Abs(desRoomData.Area), 100)
                && CommonUtils.IsAlmostNearZero(srcRoomData.RoomPolyline.Length - desRoomData.RoomPolyline.Length, 2))
            {
                srcRoomData.ValidData = false;
                return true;
            }

            return false;
        }

        public static List<Curve> MoveCurves(List<Curve> curves, double moveValue = -40000)
        {
            var moveCurves = new List<Curve>();
            foreach (var curve in curves)
            {
                if (curve is Line)
                {
                    var line = curve as Line;
                    var ptS = line.StartPoint + new Vector3d(0, moveValue, 0);
                    var ptE = line.EndPoint + new Vector3d(0, moveValue, 0);
                    moveCurves.Add(new Line(ptS, ptE));
                }
            }

            return moveCurves;
        }
    }
}
