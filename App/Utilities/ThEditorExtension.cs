using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

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
            ed.Zoom(pline.GeometricExtents);
            if (mode == PolygonSelectionMode.Crossing)
                result = ed.SelectCrossingPolygon(polygon, new SelectionFilter(filter));
            else
                result = ed.SelectWindowPolygon(polygon, new SelectionFilter(filter));
            ed.SetCurrentView(view);
            return result;
        }

        public static void Zoom(this Editor ed, Extents3d extents)
        {
            using (ViewTableRecord view = ed.GetCurrentView())
            {
                Matrix3d worldToEye = 
                    Matrix3d.Rotation(-view.ViewTwist, view.ViewDirection, view.Target) *
                    Matrix3d.Displacement(view.Target - Point3d.Origin) *
                    Matrix3d.PlaneToWorld(view.ViewDirection)
                    .Inverse();
                extents.TransformBy(worldToEye);
                view.Width = extents.MaxPoint.X - extents.MinPoint.X;
                view.Height = extents.MaxPoint.Y - extents.MinPoint.Y;
                view.CenterPoint = new Point2d(
                    (extents.MaxPoint.X + extents.MinPoint.X) / 2.0,
                    (extents.MaxPoint.Y + extents.MinPoint.Y) / 2.0);
                ed.SetCurrentView(view);
            }
        }
    }
}