using AcHelper;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using DotNetARX;

namespace ThEssential.Copy
{
    public static class ThCopyEditorExtension
    {
        public static void Copy(this Editor editor, ObjectIdCollection objs, Point3d basePt, Point3d secondPt)
        {
#if ACAD_ABOVE_2014
            Active.Editor.Command("_.COPY",
                SelectionSet.FromObjectIds(objs.ToArray()),
                "",
                basePt,
                secondPt,
                "");
#else
            ResultBuffer args = new ResultBuffer(
               new TypedValue((int)LispDataType.Text, "_.COPY"),
               new TypedValue((int)LispDataType.SelectionSet, SelectionSet.FromObjectIds(objs.ToArray())),
               new TypedValue((int)LispDataType.Text, ""),
               new TypedValue((int)LispDataType.Point3d, basePt),
               new TypedValue((int)LispDataType.Point3d, secondPt),
               new TypedValue((int)LispDataType.Text, "")
               );
            Active.Editor.AcedCmd(args);
#endif
        }

        public static void CopyWithArray(this Editor editor, ObjectIdCollection objs, Point3d basePt, Point3d secondPt, uint copies)
        {
#if ACAD_ABOVE_2014
            Active.Editor.Command("_.COPY",
                SelectionSet.FromObjectIds(objs.ToArray()),
                "",
                basePt,
                "_A",
                copies.ToString(),
                secondPt,
                "");
#else
            ResultBuffer args = new ResultBuffer(
               new TypedValue((int)LispDataType.Text, "_.COPY"),
               new TypedValue((int)LispDataType.SelectionSet, SelectionSet.FromObjectIds(objs.ToArray())),
               new TypedValue((int)LispDataType.Text, ""),
               new TypedValue((int)LispDataType.Point3d, basePt),
               new TypedValue((int)LispDataType.Text, "_A"),
               new TypedValue((int)LispDataType.Int32, copies),
               new TypedValue((int)LispDataType.Point3d, secondPt),
               new TypedValue((int)LispDataType.Text, "")
               );
            Active.Editor.AcedCmd(args);
#endif

        }

        public static void CopyWithFit(this Editor editor, ObjectIdCollection objs, Point3d basePt, Point3d secondPt, uint copies)
        {
#if ACAD_ABOVE_2014
            Active.Editor.Command("_.COPY",
                SelectionSet.FromObjectIds(objs.ToArray()),
                "",
                basePt,
                "_A",
                copies.ToString(),
                "_F",
                secondPt,
                "");
#else
            ResultBuffer args = new ResultBuffer(
               new TypedValue((int)LispDataType.Text, "_.COPY"),
               new TypedValue((int)LispDataType.SelectionSet, SelectionSet.FromObjectIds(objs.ToArray())),
               new TypedValue((int)LispDataType.Text, ""),
               new TypedValue((int)LispDataType.Point3d, basePt),
               new TypedValue((int)LispDataType.Text, "_A"),
               new TypedValue((int)LispDataType.Int32, copies),
               new TypedValue((int)LispDataType.Text, "_F"),
               new TypedValue((int)LispDataType.Point3d, secondPt),
               new TypedValue((int)LispDataType.Text, "")
               );
            Active.Editor.AcedCmd(args);
#endif
        }
    }
}
