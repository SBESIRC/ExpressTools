using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using DotNetARX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TopoNode
{
    public class ThirdUtils
    {
        public static List<Curve> MakeProfileFromPoint(List<Curve> srcCurves, Point3d pt)
        {
            var profileCalcu = new CalcuContainPointProfile(srcCurves, pt);
            var relatedCurves = profileCalcu.DoCalRelatedCurves();
            if (relatedCurves == null)
                return null;

            var profile = TopoSearch.MakeSrcProfileLoopsFromPoint(relatedCurves, pt);
            return profile;
        }
    }
}
