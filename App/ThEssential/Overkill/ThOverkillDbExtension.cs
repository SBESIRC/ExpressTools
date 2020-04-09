﻿using Linq2Acad;
using System.Linq;
using ThEssential.Equipment;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;


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

                        Line colLine = firLine.MoveToCollinear(tempLine);
                        acdb.ModelSpace.Add(colLine);
                        firLine.Erase();
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
        public static Line MoveToCollinear(this Line firLine, Line secLine)
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
            return new Line(sp, ep);
        }
    }
}
