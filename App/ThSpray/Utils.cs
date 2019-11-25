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
        public static void DrawLinesWithTransaction(List<TopoEdge> edges)
        {
            if (edges == null || edges.Count == 0)
                return;
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
    }
}
