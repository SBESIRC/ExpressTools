using Autodesk.AutoCAD.DatabaseServices;

namespace ThStructure.BeamInfo.Utils
{
    public static class CalBeamIntersect
    {
        public static bool JudgeBeamIntersect(Polyline beamBoundary, Polyline boundary, double offset = 0)
        {
            bool result = false;
            try
            {
                if (offset != 0)
                {
                    boundary = GetObjectUtils.ExpansionPolyline(boundary, offset);
                }

                DBObjectCollection dbObjCo11 = new DBObjectCollection();
                dbObjCo11.Add(beamBoundary);
                DBObjectCollection dbObjCo12 = new DBObjectCollection();
                dbObjCo12.Add(boundary);

                Region firstRegion = (Region)Region.CreateFromCurves(dbObjCo11)[0];
                Region secondRegion = (Region)Region.CreateFromCurves(dbObjCo12)[0];
                firstRegion.BooleanOperation(BooleanOperationType.BoolIntersect, secondRegion);
                if (firstRegion.Area > 0.0)
                {
                    result = true;
                }
                firstRegion.Dispose();
                secondRegion.Dispose();
            }
            catch
            {
                return false;
            }
            return result;
        }
    }
}
