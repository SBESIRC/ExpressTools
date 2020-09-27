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
    }
}
