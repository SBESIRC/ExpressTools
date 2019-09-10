using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
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
    }
}
