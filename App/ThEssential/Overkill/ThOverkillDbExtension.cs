using System;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThEssential.Overkill
{
    public static class ThOverkillDbExtension
    {
        public static DBObjectCollection RemoveDuplicateCurves(this DBObjectCollection curves, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }

        public static DBObjectCollection MergeOverlappingCurves(this DBObjectCollection curves, Tolerance tolerance)
        {
            throw new NotImplementedException();
        }
    }
}
