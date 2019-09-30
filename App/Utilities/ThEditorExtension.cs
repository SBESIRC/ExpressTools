using DotNetARX;
using Linq2Acad;
using AcHelper;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using TianHua.AutoCAD.Utility.ExtensionTools;

namespace Autodesk.AutoCAD.EditorInput
{
    public enum PolygonSelectionMode
    {
        Crossing,
        Window
    }

    public static class ThEditorExtension
    {
        // Select object inside a polyline
        //  https://forums.autodesk.com/t5/net/select-object-inside-a-polyline/td-p/6018866
        public static PromptSelectionResult SelectByPolyline(this Editor ed, 
            ObjectId plineObjId, 
            PolygonSelectionMode mode,
            SelectionFilter filter)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Use(plineObjId.Database))
            {
                // 保存当前view
                ViewTableRecord view = ed.GetCurrentView();

                // zoom到pline
                Active.Editor.ZoomObject(plineObjId);

                // 计算选择范围
                var pline = acadDatabase.Element<Polyline>(plineObjId);
                Point3dCollection points = pline.Vertices();

                // 选择
                PromptSelectionResult result;
                if (mode == PolygonSelectionMode.Crossing)
                    result = ed.SelectCrossingPolygon(points, filter);
                else
                    result = ed.SelectWindowPolygon(points, filter);

                // 恢复view
                ed.SetCurrentView(view);
                return result;
            }
        }

        public static void ZoomObject(this Editor ed, ObjectId entId)
        {
            Database db = ed.Document.Database;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //获取实体对象
                Entity ent = trans.GetObject(entId, OpenMode.ForRead) as Entity;
                if (ent == null) return;
                //根据实体的范围对视图进行缩放
                Extents3d ext = ent.GeometricExtents;
                ext.TransformBy(ed.CurrentUserCoordinateSystem.Inverse());
                COMTool.ZoomWindow(ext.MinPoint, ext.MaxPoint);
                trans.Commit();
            }
        }
    }
}