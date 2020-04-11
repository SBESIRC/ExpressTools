﻿using System.Linq;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Dreambuild.AutoCAD;
using GeometryExtensions;
using TianHua.AutoCAD.Utility.ExtensionTools;
using AcHelper;

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
            editor.ZoomObject(polygon.ObjectId);

            // TraceBoundary()接受一个UCS的seed point
            var seedPtUcs = offset.First().StartPoint.Trans(CoordSystem.WCS, CoordSystem.UCS);
            return editor.TraceBoundary(seedPtUcs, true);
        }

        public static ObjectIdCollection CreateRegions(this Editor editor, ObjectIdCollection objs)
        {
            // 执行REGION命令
            Active.Editor.Command("_.REGION", 
                SelectionSet.FromObjectIds(objs.ToArray()),
                "");

            // 获取REGION对象
            PromptSelectionResult selRes = Active.Editor.SelectLast();
            if (selRes.Status == PromptStatus.OK)
            {
                return new ObjectIdCollection(selRes.Value.GetObjectIds());
            }
            return new ObjectIdCollection();
        }

        public static ObjectIdCollection UnionRegions(this Editor editor, ObjectIdCollection objs)
        {
            // 执行UNION命令
            Active.Editor.Command("_.UNION",
                SelectionSet.FromObjectIds(objs.ToArray()),
                "");

            // 获取REGION对象
            PromptSelectionResult selRes = Active.Editor.SelectLast();
            if (selRes.Status != PromptStatus.OK)
            {
                return new ObjectIdCollection();
            }

            // 执行EXPLODE命令
            Active.Editor.Command("_.EXPLODE",
                SelectionSet.FromObjectIds(selRes.Value.GetObjectIds()),
                "");

            // 获取UNION后的REGION对象
            selRes = Active.Editor.SelectLast();
            if (selRes.Status != PromptStatus.OK)
            {
                return new ObjectIdCollection();
            }

            return new ObjectIdCollection(selRes.Value.GetObjectIds());
        }

        public static ObjectIdCollection CreateHatchWithRegions(this Editor editor, ObjectIdCollection objs)
        {
            using (var hatchOV = new ThSitePlanHatchOverride())
            {
                // 执行HATCH命令
                Active.Editor.Command("_.HATCH",
                    "_S",
                    SelectionSet.FromObjectIds(objs.ToArray()),
                    "");

                // 获取HATCH对象
                PromptSelectionResult selRes = Active.Editor.SelectLast();
                if (selRes.Status != PromptStatus.OK)
                {
                    return new ObjectIdCollection();
                }

                return new ObjectIdCollection(selRes.Value.GetObjectIds());
            }
        }
    }
}
