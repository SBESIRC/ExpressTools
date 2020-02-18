using System;
using System.Linq;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using GeometryExtensions;

namespace ThStructure.Model
{
    /// <summary>
    /// 箍筋
    /// </summary>
    public class ThSDetailReinPoint : ThSComponent
    {
        private readonly ThSComponentCurveCollection geometries = new ThSComponentCurveCollection();
        private readonly ThSComponentParameterCollection parameters = new ThSComponentParameterCollection();

        public override ThSComponentCurveCollection Geometries
        {
            get
            {
                return geometries;
            }
        }

        public override Matrix3d ComponentTransform { get; set; }

        public override ThSComponentParameterCollection Parameters
        {
            get
            {
                return parameters;
            }
        }

        public override void GenerateLayout()
        {
            // 箍筋参数：
            //  1. 直径（diameter）
            //        y
            //        ^
            //        |
            //        #
            //        |
            //        |
            //        |
            //    #---0---#->x
            //        |
            //        |
            //        |
            //        #
            //        |
            var parameter = Parameters.Where(o => o.Key == "intCBarDia").First();

            double bulge = Math.Tan(Math.PI / 4.0);
            double diameter = (double)parameter.Value;
            PolylineSegmentCollection segments = new PolylineSegmentCollection();
            segments.Add(new PolylineSegment(new Point2d(-diameter / 2.0, 0.0), new Point2d(diameter / 2.0, 0.0), bulge));
            segments.Add(new PolylineSegment(new Point2d(diameter / 2.0, 0.0), new Point2d(-diameter / 2.0, 0.0), bulge));
            geometries.Add(new ThSComponentCurve(segments.ToPolyline()));
        }

        public override void Render(IThSComponentRenderTarget target)
        {
            target.Paint(this, ThSComponentDbStyleManager.Instance.Styles["DetailReinPoint"]);
        }
    }
}