using GeoAPI.Geometries;
using Autodesk.AutoCAD.Geometry;

namespace ThCADCore.NTS
{
    public static class ThSitePlanNTSGeExtension
    {
        public static Point3d ToAcGePoint3d(this Coordinate coordinate)
        {
            if (!double.IsNaN(coordinate.Z))
            {
                return new Point3d(
                    ThSitePlanNTSService.Instance.PrecisionModel.MakePrecise(coordinate.X),
                    ThSitePlanNTSService.Instance.PrecisionModel.MakePrecise(coordinate.Y),
                    ThSitePlanNTSService.Instance.PrecisionModel.MakePrecise(coordinate.Z)
                    );
            }
            else
            {
                return new Point3d(
                    ThSitePlanNTSService.Instance.PrecisionModel.MakePrecise(coordinate.X),
                    ThSitePlanNTSService.Instance.PrecisionModel.MakePrecise(coordinate.Y),
                    0);
            }
        }

        public static Point3d ToAcGePoint3d(this IPoint point)
        {
            return point.Coordinate.ToAcGePoint3d();
        }

        public static Point2d ToAcGePoint2d(this Coordinate coordinate)
        {
            return new Point2d(
                    ThSitePlanNTSService.Instance.PrecisionModel.MakePrecise(coordinate.X),
                    ThSitePlanNTSService.Instance.PrecisionModel.MakePrecise(coordinate.Y)
                );
        }

        public static Point2d ToAcGePoint2d(this IPoint point)
        {
            return point.Coordinate.ToAcGePoint2d();
        }

        public static Coordinate ToNTSCoordinate(this Point3d point)
        {
            return new Coordinate(
                    ThSitePlanNTSService.Instance.PrecisionModel.MakePrecise(point.X),
                    ThSitePlanNTSService.Instance.PrecisionModel.MakePrecise(point.Y),
                    ThSitePlanNTSService.Instance.PrecisionModel.MakePrecise(point.Z)
                    );

        }

        public static Coordinate ToNTSCoordinate(this Point2d point)
        {
            return new Coordinate(
                ThSitePlanNTSService.Instance.PrecisionModel.MakePrecise(point.X),
                ThSitePlanNTSService.Instance.PrecisionModel.MakePrecise(point.Y)
                );
        }
    }
}
