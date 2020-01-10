using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using AcHelper;
using DotNetARX;

namespace ThEssential
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
            return new Extents3d(corner1, corner2);
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
            var result = Active.Editor.SelectWindow(
                extents.MinPoint, 
                extents.MaxPoint, 
                entity.QSelectFilter(filterType));
            if (result.Status == PromptStatus.OK)
            {
                Active.Editor.SetImpliedSelection(result.Value);
            }
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
