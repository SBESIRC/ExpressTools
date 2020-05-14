using Linq2Acad;
using System.Linq;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;
using ThEssential.Equipment;
using System;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThEssential.Overkill
{
    public static class ThOverkillDbExtension
    {
        public static DBObjectCollection RemoveDuplicateCurves(this DBObjectCollection curves, Tolerance tolerance)
        {
            DBObjectCollection resCurves = new DBObjectCollection();

            while (curves.Count > 0)
            {
                Curve firCurve = curves[0] as Curve;
                resCurves.Add(firCurve);
                curves.Remove(firCurve);
                Point3d endPoint = firCurve.EndPoint;
                Point3d startPoint = firCurve.StartPoint;

                foreach (Curve cuv in curves)
                {
                    if (endPoint.IsEqualTo(cuv.StartPoint, tolerance) && startPoint.IsEqualTo(cuv.EndPoint, tolerance) ||
                        endPoint.IsEqualTo(cuv.EndPoint, tolerance) && startPoint.IsEqualTo(cuv.StartPoint, tolerance))
                    {
                        curves.Remove(cuv);
                    }
                }
            }

            return resCurves;
        }

        public static DBObjectCollection MergeOverlappingCurves(this DBObjectCollection curves, Tolerance tolerance)
        {
            DBObjectCollection resCurves = new DBObjectCollection();
            using (AcadDatabase acdb = AcadDatabase.Active())
            {
                while (curves.Count > 0)
                {
                    var firCurve = curves[0];
                    curves.Remove(firCurve);

                    for (int i = 0; i < curves.Count; i++)
                    {
                        Line firLine = acdb.Element<Line>(firCurve.Id, true);
                        Line tempLine = acdb.Element<Line>(curves[i].Id, true);
                        LinearEntity3d firCuv3d = firLine.ToGeLine() as LinearEntity3d;
                        LinearEntity3d tempCuv3d = tempLine.ToGeLine() as LinearEntity3d;
                        LinearEntity3d overlopCuv = firCuv3d.Overlap(tempCuv3d, tolerance);
                        if (overlopCuv == null)
                        {
                            continue;
                        }

                        Line colLine = firLine.MoveToCollinear(tempLine, tolerance);
                        if (colLine == null)
                        {
                            continue;
                        }
                        tempLine.Erase();

                        curves.Remove(curves[i]);
                        firCurve = colLine;
                        i = -1;
                    }
                    resCurves.Add(firCurve);
                }
                return resCurves;
            }
        }

        /// <summary>
        /// 合并线(如果不共线将短的线移动到长的线再合并)
        /// </summary>
        /// <param name="firLine"></param>
        /// <param name="secLine"></param>
        public static Line MoveToCollinear(this Line firLine, Line secLine, Tolerance tol)
        {
            Line longerLine = firLine;
            Line shorterLine = secLine;
            if (firLine.Length < secLine.Length)
            {
                longerLine = secLine;
                shorterLine = firLine;
            }

            Vector3d normal = (longerLine.EndPoint - longerLine.StartPoint).GetNormal().CrossProduct(new Vector3d(0, 0, 1));
            Plane proPlane = new Plane(longerLine.StartPoint, normal);

            List<Point3d> points = new List<Point3d>();
            points.Add(shorterLine.StartPoint.OrthoProject(proPlane));
            points.Add(shorterLine.EndPoint.OrthoProject(proPlane));
            points.Add(longerLine.StartPoint);
            points.Add(longerLine.EndPoint);
            points = points.OrderBy(x => x.X + x.Y).ToList();
            Point3d sp = points.First();
            Point3d ep = points.Last();
            if (sp.DistanceTo(ep) > firLine.Length + secLine.Length + tol.EqualPoint)
            {
                return null;
            }
            firLine.StartPoint = sp;
            firLine.EndPoint = ep;
            return firLine;
        }

        #region 非代码勿提交，在这里用来测试的
        [CommandMethod("TIANHUACAD", "THSPTest", CommandFlags.Modal)]
        public static void THDbTest()
        {
            TypedValue[] filList = new TypedValue[] { new TypedValue((int)DxfCode.Start, "Polyline") };
            SelectionFilter sfilter = new SelectionFilter(filList);
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            PromptSelectionResult ProSset = ed.SelectAll();

            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                DBObjectCollection cuvCollec = new DBObjectCollection();

                if (ProSset.Status == PromptStatus.OK)
                {
                    List<Line> lines = new List<Line>();
                    ObjectId[] oids = ProSset.Value.GetObjectIds();
                    foreach (ObjectId objId in oids)
                    {
                        Polyline line = trans.GetObject(objId, OpenMode.ForWrite) as Polyline;
                        cuvCollec.Add(line);
                    }

                    GetPolyLineBounding(cuvCollec, Tolerance.Global);
                }

                trans.Commit();
            }
        }

        public static DBObjectCollection GetPolyLineBounding(DBObjectCollection dBObjects, Tolerance tolerance)
        {
            DBObjectCollection resBounding = new DBObjectCollection();
            using (AcadDatabase acdb = AcadDatabase.Active())
            {
                foreach (var dbObj in dBObjects)
                {
                    if (dbObj is Polyline)
                    {
                        Polyline polyline = dbObj as Polyline;
                        if (polyline.NumberOfVertices < 4)
                        {
                            continue;
                        }

                        List<Point2d> points = new List<Point2d>();
                        for (int i = 0; i < polyline.NumberOfVertices; i++)
                        {
                            if (points.Where(x => x.IsEqualTo(polyline.GetPoint2dAt(i), tolerance)).Count() <= 0)
                            {
                                points.Add(polyline.GetPoint2dAt(i));
                            }
                        }

                        Polyline resPolyline = new Polyline(points.Count);
                        Point2d thisP = points.First();
                        int index = 0;
                        resPolyline.AddVertexAt(index, thisP, 0, 0, 0);
                        points.Remove(thisP);
                        while (points.Count > 0)
                        {
                            thisP = points.OrderBy(x => x.GetDistanceTo(thisP)).First();
                            index++;
                            resPolyline.AddVertexAt(index, thisP, 0, 0, 0);
                            points.Remove(thisP);
                        }
                        resPolyline.Closed = true;

                        acdb.ModelSpace.Add(resPolyline);
                        polyline.Erase();
                    }
                }
            }

            return null;
        }
        #endregion
    }
}
