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
    class ThRoomData
    {

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
            var textId = ThRoomData.GetIdFromSymbolTable();
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
                    var area = (int)room.Area;
                    var pos = room.Pos;
                    var dbText = new DBText();
                    if (textId != ObjectId.Null)
                        dbText.TextStyleId = textId;
                    dbText.TextString = area.ToString();
                    dbText.Position = pos;
                    dbText.Height = 200;
                    dbText.Thickness = 1;
                    dbText.WidthFactor = 1;
                    var objectTextId = db.ModelSpace.Add(dbText);
                    db.ModelSpace.Element(objectTextId, true).Layer = layerName;
                    var objectPolylineId = db.ModelSpace.Add(room.RoomPolyline);
                    db.ModelSpace.Element(objectPolylineId, true).Layer = layerName;
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
            var rectLines = ThRoomData.GetRectangleFromPoint(first, second);
            return rectLines;
        }

        /// <summary>
        /// 打开需要显示的图层
        /// </summary>
        public static void ShowLayers()
        {
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
                }

                foreach (var lName in closeLayerNames)
                {
                    db.Layers.Element(lName, true).IsOff = true;
                }
            }
        }
    }
}
