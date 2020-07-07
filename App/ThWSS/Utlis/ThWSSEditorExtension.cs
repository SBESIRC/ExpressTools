using AcHelper;
using System.Linq;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThWSS.Utlis
{
    public static class ThWSSEditorExtension
    {
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
    }
}
