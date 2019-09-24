using DotNetARX;
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
            Polyline pline, 
            PolygonSelectionMode mode, 
            params TypedValue[] filter)
        {
            Point3dCollection polygon = new Point3dCollection();
            for (int i = 0; i < pline.NumberOfVertices; i++)
            {
                polygon.Add(pline.GetPoint3dAt(i));
            }
            PromptSelectionResult result;
            ViewTableRecord view = ed.GetCurrentView();
            ed.ZoomObject(pline.ObjectId);
            if (mode == PolygonSelectionMode.Crossing)
                result = ed.SelectCrossingPolygon(polygon, new SelectionFilter(filter));
            else
                result = ed.SelectWindowPolygon(polygon, new SelectionFilter(filter));
            ed.SetCurrentView(view);
            return result;
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