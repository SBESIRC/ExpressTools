using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.EditorInput;
using AcHelper;

namespace ThColumnInfo
{
    public class TopoService
    {
        public static List<Curve> TraceBoundary(Editor ed, Point3d cenPt,bool detectIsland=false)
        {
            List<Curve> curves = new List<Curve>();
            DBObjectCollection dbObjs = ed.TraceBoundary(cenPt, detectIsland);
            foreach(DBObject dbObj in dbObjs)
            {
                if(dbObj is Curve curve)
                {
                    curves.Add(curve);
                }
            }
            return curves;
        }
        public static void SuperBoundaryCmd(ObjectIdCollection objs)
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
    }
}
