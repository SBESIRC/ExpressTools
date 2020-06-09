﻿using System;
using AcHelper;
using DotNetARX;
using Linq2Acad;
using System.Linq;
using Dreambuild.AutoCAD;
using GeometryExtensions;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace ThSitePlan
{
    public static class ThSitePlanEditorExtension
    {
        public static Extents2d ToPlotWindow(this Editor editor, Polyline polyline)
        {
            var extents = polyline.GeometricExtents;
            var minPoint = extents.MinPoint.Trans(CoordSystem.WCS, CoordSystem.DCS);
            var maxPoint = extents.MaxPoint.Trans(CoordSystem.WCS, CoordSystem.DCS);
            return new Extents2d(minPoint.ToPoint2d(), maxPoint.ToPoint2d());
        }

        public static DBObjectCollection TraceBoundaryEx(this Editor editor, Polyline polygon)
        {
            var offset = polygon.Offset(ThSitePlanCommon.seed_point_offset, ThPolylineExtension.OffsetSide.In);
            if (offset.Count() != 1)
            {
                return new DBObjectCollection();
            }

            //You need to bear in mind that this function ultimately has 
            //similar constraints to the BOUNDARY and BHATCH commands: 
            //it also works off AutoCAD’s display list and so the user 
            //will need to be appropriately ZOOMed into 
            //the geometry that is to be used for boundary detection.
            //editor.ZoomObject(polygon.ObjectId);

            // TraceBoundary()接受一个UCS的seed point
            var seedPtUcs = offset.First().StartPoint.Trans(CoordSystem.WCS, CoordSystem.UCS);
            return editor.TraceBoundary(seedPtUcs, true);
        }

        public static void CreateRegions(this Editor editor, ObjectIdCollection objs)
        {
            // 执行REGION命令
#if ACAD_ABOVE_2014
            Active.Editor.Command("_.REGION", 
                SelectionSet.FromObjectIds(objs.ToArray()),
                "");
#else
            ResultBuffer args = new ResultBuffer(
               new TypedValue((int)LispDataType.Text, "_.REGION"),
               new TypedValue((int)LispDataType.SelectionSet, SelectionSet.FromObjectIds(objs.ToArray())),
               new TypedValue((int)LispDataType.Text, "")
               );
            Active.Editor.AcedCmd(args);
#endif
        }

        public static void UnionRegions(this Editor editor, ObjectIdCollection objs)
        {
            // 执行UNION命令
#if ACAD_ABOVE_2014
            Active.Editor.Command("_.UNION",
                SelectionSet.FromObjectIds(objs.ToArray()),
                "");
#else
            ResultBuffer args = new ResultBuffer(
               new TypedValue((int)LispDataType.Text, "_.UNION"),
               new TypedValue((int)LispDataType.SelectionSet, SelectionSet.FromObjectIds(objs.ToArray())),
               new TypedValue((int)LispDataType.Text, "")
               );
            Active.Editor.AcedCmd(args);
#endif
        }

        public static void SubtractRegions(this Editor editor, ObjectIdCollection pObjs, ObjectIdCollection sObjs)
        {
#if ACAD_ABOVE_2014
            Active.Editor.Command("_.SUBTRACT",
                SelectionSet.FromObjectIds(pObjs.ToArray()),
                "",
                SelectionSet.FromObjectIds(sObjs.ToArray()),
                ""
                );
#else
            ResultBuffer args = new ResultBuffer(
               new TypedValue((int)LispDataType.Text, "_.SUBTRACT"),
               new TypedValue((int)LispDataType.SelectionSet, SelectionSet.FromObjectIds(pObjs.ToArray())),
               new TypedValue((int)LispDataType.Text, ""),
               new TypedValue((int)LispDataType.SelectionSet, SelectionSet.FromObjectIds(sObjs.ToArray())),
               new TypedValue((int)LispDataType.Text, "")
               );
            Active.Editor.AcedCmd(args);
#endif
        }

        public static void CreateHatchWithRegions(this Editor editor, ObjectIdCollection objs)
        {
            using (var hatchOV = new ThSitePlanHatchOverride())
            {
                // 执行HATCH命令
#if ACAD_ABOVE_2014
                Active.Editor.Command("_.-HATCH",
                    "_A",
                    "_H",
                    "_Y",
                    "",
                    "_S",
                    SelectionSet.FromObjectIds(objs.ToArray()),
                    "",
                    "");
#else
                ResultBuffer args = new ResultBuffer(
                   new TypedValue((int)LispDataType.Text, "_.-HATCH"),
                   new TypedValue((int)LispDataType.Text, "_A"),
                   new TypedValue((int)LispDataType.Text, "_H"),
                   new TypedValue((int)LispDataType.Text, "_Y"),
                   new TypedValue((int)LispDataType.Text, ""),
                   new TypedValue((int)LispDataType.Text, "_S"),
                   new TypedValue((int)LispDataType.SelectionSet, SelectionSet.FromObjectIds(objs.ToArray())),
                   new TypedValue((int)LispDataType.Text, ""),
                   new TypedValue((int)LispDataType.Text, "")
                   );
                Active.Editor.AcedCmd(args);
#endif
            }
        }

        public static void CreateHatchWithPoint(this Editor editor, Polyline framepl,Point3d ptin)
        {
            using (var hatchOV = new ThSitePlanHatchOverride())
            {
                // 执行HATCH命令
#if ACAD_ABOVE_2014
                Active.Editor.Command("_.-HATCH",
                    ptin,
                    "");
#else
                ResultBuffer args = new ResultBuffer(
                   new TypedValue((int)LispDataType.Text, "_.-HATCH"),
                   new TypedValue((int)LispDataType.Point3d, ptin),
                   new TypedValue((int)LispDataType.Text, "")
                   );
                Active.Editor.AcedCmd(args);
#endif
            }
        }


        public static void HatchBoundaryCmd(this Editor editor, ObjectIdCollection objs)
        {
            foreach (ObjectId obj in objs)
            {
#if ACAD_ABOVE_2014
                Active.Editor.Command("_.-HATCHEDIT",
                    obj,
                    "_B",
                    "_R",
                    "_Y");
#else
                ResultBuffer args = new ResultBuffer(
                   new TypedValue((int)LispDataType.Text, "_.-HATCHEDIT"),
                   new TypedValue((int)LispDataType.ObjectId, obj),
                   new TypedValue((int)LispDataType.Text, "_B"),
                   new TypedValue((int)LispDataType.Text, "_R"),
                   new TypedValue((int)LispDataType.Text, "_Y")
                   );
                Active.Editor.AcedCmd(args);
#endif
            }
        }

        public static void ExplodeCmd(this Editor editor, ObjectIdCollection objs)
        {
#if ACAD_ABOVE_2014
            // 由于未知原因，发送EXPLODE命令时提供一个选择集并不能正常工作
            // 这里采用一个Workaround，即使用循环为每个对象发送EXPLODE命令
            foreach(ObjectId obj in objs)
            {
                Active.Editor.Command("_.EXPLODE", obj);
            }
#else
            foreach(ObjectId obj in objs)
            {
                ResultBuffer args = new ResultBuffer(
                   new TypedValue((int)LispDataType.Text, "_.EXPLODE"),
                   new TypedValue((int)LispDataType.ObjectId, obj)
                   );
                Active.Editor.AcedCmd(args);
            }
#endif
        }

        public static void HatchDecomposeCmd(this Editor editor, ObjectIdCollection objs)
        {
            foreach (ObjectId obj in objs)
            {
#if ACAD_ABOVE_2014
                Active.Editor.Command("_.-HATCHEDIT", 
                    obj, 
                    "_H");
#else
                ResultBuffer args = new ResultBuffer(
                   new TypedValue((int)LispDataType.Text, "_.-HATCHEDIT"),
                   new TypedValue((int)LispDataType.ObjectId, obj),
                   new TypedValue((int)LispDataType.Text, "_H")
                   );
                Active.Editor.AcedCmd(args);
#endif
            }
        }

        public static void BoundaryCmd(this Editor editor, ObjectIdCollection objs, Point3d seedPt)
        {
#if ACAD_ABOVE_2014
            Active.Editor.Command("_.-BOUNDARY", 
                "_A",
                "_O",
                "_R",
                "",
                seedPt, 
                "");
#else
            ResultBuffer args = new ResultBuffer(
               new TypedValue((int)LispDataType.Text, "_.-BOUNDARY"),
               new TypedValue((int)LispDataType.Text, "_A"),
               new TypedValue((int)LispDataType.Text, "_O"),
               new TypedValue((int)LispDataType.Text, "_R"),
               new TypedValue((int)LispDataType.Text, ""),
               new TypedValue((int)LispDataType.Point3d, seedPt),
               new TypedValue((int)LispDataType.Text, "")
               );
            Active.Editor.AcedCmd(args);
#endif
        }

        public static void SuperBoundaryCmd(this Editor editor, ObjectIdCollection objs)
        {
#if ACAD_ABOVE_2014
            Active.Editor.Command("_._SBND_ALL",
                SelectionSet.FromObjectIds(objs.ToArray()),
                "");
#else
            ResultBuffer args = new ResultBuffer(
               new TypedValue((int)LispDataType.Text, "_._SBND_ALL"),
               new TypedValue((int)LispDataType.SelectionSet, SelectionSet.FromObjectIds(objs.ToArray())),
               new TypedValue((int)LispDataType.Text, "")
               );
            Active.Editor.AcedCmd(args);
#endif
        }

        /// <summary>
        /// 计算图元对象的包围框，
        /// 计算一个临时的包围框（比图元对象的范围框稍大一些10%）
        /// 在图元对象范围框和临时的包围框直接选一点作为“种子点”
        /// 通过使用BO命令获取轮廓面域
        /// </summary>
        /// <param name="editor"></param>
        /// <param name="objs"></param>
        public static void BoundaryCmdEx(this Editor editor, ObjectIdCollection objs)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            { 
                var extents = objs.Cast<ObjectId>().GetExtents();
                var frame = extents.Expand(1.1).CreatePolyline();
                var frameId = acadDatabase.CurrentSpace.Add(frame);
                var seedPt = extents.Expand(1.05).MinPoint;

                // 将新建的框作为最外面的边界框
                objs.Add(frameId);

                ObjectId outermost = ObjectId.Null;
                void handler(object s, ObjectEventArgs e)
                {
                    if (e.DBObject is Region region)
                    {
                        if (Math.Abs(frame.Length - region.Perimeter) <= Tolerance.Global.EqualVector)
                        {
                            outermost = e.DBObject.ObjectId;
                        }
                    }
                }
                acadDatabase.Database.ObjectAppended += handler;
#if ACAD_ABOVE_2014
                Active.Editor.Command("_.-BOUNDARY",
                    "_A",
                    "_B",
                    "_N",
                    SelectionSet.FromObjectIds(objs.ToArray()),
                    "",
                    "_O",
                    "_R",
                    "",
                    seedPt,
                    "");
#else
                ResultBuffer args = new ResultBuffer(
                   new TypedValue((int)LispDataType.Text, "_.-BOUNDARY"),
                   new TypedValue((int)LispDataType.Text, "_A"),
                   new TypedValue((int)LispDataType.Text, "_B"),
                   new TypedValue((int)LispDataType.Text, "_N"),
                   new TypedValue((int)LispDataType.SelectionSet, SelectionSet.FromObjectIds(objs.ToArray())),
                   new TypedValue((int)LispDataType.Text, ""),
                   new TypedValue((int)LispDataType.Text, "_O"),
                   new TypedValue((int)LispDataType.Text, "_R"),
                   new TypedValue((int)LispDataType.Text, ""),
                   new TypedValue((int)LispDataType.Point3d, seedPt),
                   new TypedValue((int)LispDataType.Text, "")
                   );
                Active.Editor.AcedCmd(args);
#endif
                acadDatabase.Database.ObjectAppended -= handler;

                // 删除最外面的边界框
                if (frameId.IsValid)
                {
                    acadDatabase.Element<Polyline>(frameId, true).Erase();
                }
                // 删除最外面的区域
                if (outermost.IsValid)
                {
                    acadDatabase.Element<Region>(outermost, true).Erase();
                }
            }
        }

        public static void PeditCmd(this Editor editor, ObjectIdCollection objs)
        {
#if ACAD_ABOVE_2014
            Active.Editor.Command("_.PEDIT", 
                "_M",
                SelectionSet.FromObjectIds(objs.ToArray()),
                "",
                "_Y",
                "_J",
                "",
                "");
#else
            ResultBuffer args = new ResultBuffer(
               new TypedValue((int)LispDataType.Text, "_.PEDIT"),
               new TypedValue((int)LispDataType.Text, "_M"),
               new TypedValue((int)LispDataType.SelectionSet, SelectionSet.FromObjectIds(objs.ToArray())),
               new TypedValue((int)LispDataType.Text, ""),
               new TypedValue((int)LispDataType.Text, "_Y"),
               new TypedValue((int)LispDataType.Text, "_J"),
               new TypedValue((int)LispDataType.Text, ""),
               new TypedValue((int)LispDataType.Text, "")
               );
            Active.Editor.AcedCmd(args);
#endif
        }

        public static void MeasureCmd(this Editor editor, ObjectId obj)
        {
#if ACAD_ABOVE_2014
            Active.Editor.Command("_.MEASURE",
                obj,
                Properties.Settings.Default.PlantRadius * 2,
                "");
#else
            ResultBuffer args = new ResultBuffer(
               new TypedValue((int)LispDataType.Text, "_.MEASURE"),
               new TypedValue((int)LispDataType.ObjectId, obj),
               new TypedValue((int)LispDataType.Double, Properties.Settings.Default.PlantRadius * 2),
               new TypedValue((int)LispDataType.Text, "")
               );
            Active.Editor.AcedCmd(args);
#endif
        }

        public static void OverkillCmd(this Editor editor, ObjectIdCollection objs)
        {
#if ACAD_ABOVE_2014
            Active.Editor.Command("_.-OVERKILL",
                SelectionSet.FromObjectIds(objs.ToArray()),
                "",
                "");
#else
            ResultBuffer args = new ResultBuffer(
               new TypedValue((int)LispDataType.Text, "_.-OVERKILL"),
               new TypedValue((int)LispDataType.SelectionSet, SelectionSet.FromObjectIds(objs.ToArray())),
               new TypedValue((int)LispDataType.Text, ""),
               new TypedValue((int)LispDataType.Text, "")
               );
            Active.Editor.AcedCmd(args);
#endif
        }

        public static void ThOverKillCmd(this Editor editor, ObjectIdCollection objs)
        {
#if ACAD_ABOVE_2014
            Active.Editor.Command("_.THOVERKILL", 
                SelectionSet.FromObjectIds(objs.ToArray()),
                "", 
                ThSitePlanCommon.overkill_tolerance);
#else
            ResultBuffer args = new ResultBuffer(
               new TypedValue((int)LispDataType.Text, "_.THOVERKILL"),
               new TypedValue((int)LispDataType.SelectionSet, SelectionSet.FromObjectIds(objs.ToArray())),
               new TypedValue((int)LispDataType.Text, ""),
               new TypedValue((int)LispDataType.Double, ThSitePlanCommon.overkill_tolerance)
               );
            Active.Editor.AcedCmd(args);
#endif
        }

        public static void EraseCmd(this Editor editor, ObjectIdCollection objs)
        {
#if ACAD_ABOVE_2014
            Active.Editor.Command("_.ERASE",
                SelectionSet.FromObjectIds(objs.ToArray()),
                "");
#else
            ResultBuffer args = new ResultBuffer(
               new TypedValue((int)LispDataType.Text, "_.ERASE"),
               new TypedValue((int)LispDataType.SelectionSet, SelectionSet.FromObjectIds(objs.ToArray())),
               new TypedValue((int)LispDataType.Text, "")
               );
            Active.Editor.AcedCmd(args);
#endif
        }

        public static void CircleCmd(this Editor editor, Point3d center, double radius)
        {
#if ACAD_ABOVE_2014
            Active.Editor.Command("_.CIRCLE", center, radius);
#else
            ResultBuffer args = new ResultBuffer(
               new TypedValue((int)LispDataType.Text, "_.CIRCLE"),
               new TypedValue((int)LispDataType.Point3d, center),
               new TypedValue((int)LispDataType.Double, radius)
               );
            Active.Editor.AcedCmd(args);
#endif
        }

        public static void TrimCmd(this Editor editor, Entity bdent)
        {
#if ACAD_ABOVE_2014
            Active.Editor.Command("Zoom",
                "W",
                new Point3d(bdent.GeometricExtents.MinPoint.X - 10, bdent.GeometricExtents.MinPoint.Y - 100, 0),
                new Point3d(bdent.GeometricExtents.MaxPoint.X + 10, bdent.GeometricExtents.MaxPoint.Y + 100, 0),
                "");
            Active.Editor.Command("_.TRIM",
                    SelectionSet.FromObjectIds(new ObjectId[] { bdent.ObjectId }),
                    "",
                    "F",
                    new Point3d(bdent.GeometricExtents.MinPoint.X - 1, bdent.GeometricExtents.MinPoint.Y - 1, 0),
                    new Point3d(bdent.GeometricExtents.MinPoint.X - 1, bdent.GeometricExtents.MaxPoint.Y + 1, 0),
                    new Point3d(bdent.GeometricExtents.MaxPoint.X + 1, bdent.GeometricExtents.MaxPoint.Y + 1, 0),
                    new Point3d(bdent.GeometricExtents.MaxPoint.X + 1, bdent.GeometricExtents.MinPoint.Y - 1, 0),
                    new Point3d(bdent.GeometricExtents.MinPoint.X - 2, bdent.GeometricExtents.MinPoint.Y - 1, 0),
                    "",
                    "");

#else
            ResultBuffer args1 = new ResultBuffer(
               new TypedValue((int)LispDataType.Text, "_.TRIM"),
               new TypedValue((int)LispDataType.SelectionSet, SelectionSet.FromObjectIds(new ObjectId[] { bdent.ObjectId })),
               new TypedValue((int)LispDataType.Text, ""),
               new TypedValue((int)LispDataType.Text, "_F"),
               new TypedValue((int)LispDataType.Point3d, new Point3d(bdent.GeometricExtents.MinPoint.X - 1, bdent.GeometricExtents.MinPoint.Y - 1, 0)),
               new TypedValue((int)LispDataType.Point3d, new Point3d(bdent.GeometricExtents.MinPoint.X - 1, bdent.GeometricExtents.MaxPoint.Y + 1, 0)),
               new TypedValue((int)LispDataType.Point3d, new Point3d(bdent.GeometricExtents.MaxPoint.X + 1, bdent.GeometricExtents.MaxPoint.Y + 1, 0)),
               new TypedValue((int)LispDataType.Point3d, new Point3d(bdent.GeometricExtents.MaxPoint.X + 1, bdent.GeometricExtents.MinPoint.Y - 1, 0)),
               new TypedValue((int)LispDataType.Point3d, new Point3d(bdent.GeometricExtents.MinPoint.X - 2, bdent.GeometricExtents.MinPoint.Y - 1, 0)),
               new TypedValue((int)LispDataType.Text, ""),
               new TypedValue((int)LispDataType.Text, "")
               );
            Active.Editor.AcedCmd(args1);
#endif
        }
    }
}
