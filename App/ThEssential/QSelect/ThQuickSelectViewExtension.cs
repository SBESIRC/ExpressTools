using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using AcHelper;
using DotNetARX;
using System;
using System.Collections.Generic;
using Linq2Acad;
using System.Linq;

namespace ThEssential.QSelect
{
    public static class ThQuickSelectViewExtension
    { 
        /// <summary>
        /// 视图在UCS的范围
        /// </summary>
        /// <param name="vtr"></param>
        /// <returns></returns>
        public static Extents3d UCSExtent(this AbstractViewTableRecord vtr)
        {
            // 视图在UCS的范围
            double viewHeight = vtr.Height;
            double viewWidth = vtr.Width;
            Point2d viewCenter = vtr.CenterPoint;
            Point2d topLeft = viewCenter + new Vector2d(-viewWidth / 2, viewHeight / 2);
            Point2d topRight = viewCenter + new Vector2d(viewWidth / 2, viewHeight / 2);
            Point2d bottomLeft = viewCenter + new Vector2d(-viewWidth / 2, -viewHeight / 2);
            Point2d bottomRight = viewCenter + new Vector2d(viewWidth / 2, -viewHeight / 2);

            // 选择窗口左下角(UCS)
            Point3d pt1 = new Point3d(bottomLeft.X, bottomLeft.Y, 0.0);
            Point3d corner1 = pt1.TranslateCoordinates(UCSTools.CoordSystem.DCS, UCSTools.CoordSystem.UCS);

            // 选择窗口右上角(UCS)
            Point3d pt2 = new Point3d(topRight.X, topRight.Y, 0.0);
            Point3d corner2 = pt2.TranslateCoordinates(UCSTools.CoordSystem.DCS, UCSTools.CoordSystem.UCS);

            // 返回UCS下的范围
            double minX = Math.Min(corner1.X, corner2.X);
            double minY = Math.Min(corner1.Y, corner2.Y);
            double minZ = Math.Min(corner1.Z, corner2.Z);
            double maxX = Math.Max(corner1.X, corner2.X);
            double maxY = Math.Max(corner1.Y, corner2.Y);
            double maxZ = Math.Max(corner1.Z, corner2.Z); 
            return new Extents3d(new Point3d(minX, minY, minZ), new Point3d(maxX, maxY, maxZ));
        }

        /// <summary>
        /// 快速选择
        /// </summary>
        /// <param name="vtr"></param>
        /// <param name="entity"></param>
        /// <param name="filterType"></param>
        public static void QSelect(this AbstractViewTableRecord vtr, Entity entity, QSelectFilterType filterType)
        {
            Extents3d extents = vtr.UCSExtent();
            if (filterType == QSelectFilterType.QSelectFilterColor)
            {
                List<ObjectId> sameColorObjIds = QSelectColor(extents, entity);
                if (sameColorObjIds.Count > 0)
                {
                    Active.Editor.SetImpliedSelection(sameColorObjIds.ToArray());
                }
            }
            else
            {
                var result = Active.Editor.SelectWindow(
                extents.MinPoint,
                extents.MaxPoint,
                entity.QSelectFilter(filterType));
                if (result.Status == PromptStatus.OK)
                {
                    Active.Editor.SetImpliedSelection(result.Value);
                }
            }
        }
        private static List<ObjectId> QSelectColor(Extents3d extents,Entity entity)
        {
            List<ObjectId> sameColorObjIds = new List<ObjectId>();
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                var result = Active.Editor.SelectWindow(
               extents.MinPoint,
               extents.MaxPoint);
                Autodesk.AutoCAD.Colors.Color entColor = entity.Color;
                if (entColor.ColorMethod == Autodesk.AutoCAD.Colors.ColorMethod.ByLayer)
                {
                    entColor = ThQuickSelect.GetByLayerColor(acadDatabase.Database, entity);
                }
                if (result.Status == PromptStatus.OK)
                {
                    ObjectId[] findObjIds = result.Value.GetObjectIds();
                    foreach (ObjectId objId in findObjIds)
                    {
                        Entity currentEnt = acadDatabase.Element<Entity>(objId);
                        Autodesk.AutoCAD.Colors.Color currentColor = currentEnt.Color;
                        if (currentColor.ColorMethod == Autodesk.AutoCAD.Colors.ColorMethod.ByLayer)
                        {
                            currentColor = ThQuickSelect.GetByLayerColor(acadDatabase.Database, currentEnt);
                        }
                        if (currentColor.Equals(entColor))
                        {
                            sameColorObjIds.Add(objId);
                        }
                    }
                }
            }
            return sameColorObjIds;
        }
        /// <summary>
        /// 快速选择
        /// </summary>
        /// <param name="vtr"></param>
        /// <param name="xClass"></param>
        public static void QSelect(this AbstractViewTableRecord vtr, string dxfName)
        {
            Extents3d extents = vtr.UCSExtent();
            var result = Active.Editor.SelectWindow(
                extents.MinPoint,
                extents.MaxPoint,
                dxfName.QSelectFilter());
            if (result.Status == PromptStatus.OK)
            {
                Active.Editor.SetImpliedSelection(result.Value);
            }
        }
    }
}
