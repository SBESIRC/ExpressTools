using System;
using System.Linq;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using GeometryExtensions;

namespace ThStructure.Model
{
    public enum HookAttachmentPoint
    {
        /// <summary>
        /// 左上角
        /// </summary>
        HookAttachmentTopLeft,
        /// <summary>
        /// 右上角
        /// </summary>
        HookAttachmentTopRight,
        /// <summary>
        /// 左下角
        /// </summary>
        HookAttachmentBottomLeft,
        /// <summary>
        /// 右下角
        /// </summary>
        HookAttachmentBottomRight
    };

    /// <summary>
    /// 纵筋
    /// </summary>
    public class ThSDetailRein : ThSComponent
    {
        // 倒角半径
        private static readonly double fillet_radius = 19.0;
        private static readonly double hook_x_length = 26.0;
        private static readonly double hook_y_length = 72.0;
        private static readonly double point_distance = 22.0;
        private static readonly double section_distance = 40.0;
        private readonly ThSDetailReinPointCollection points = new ThSDetailReinPointCollection();
        private readonly ThSComponentCurveCollection segments = new ThSComponentCurveCollection();
        private readonly ThSComponentParameterCollection parameters = new ThSComponentParameterCollection();

        public override ThSComponentCurveCollection Geometries
        {
            get
            {
                return segments;
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

        public HookAttachmentPoint HookAttachment { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ThSDetailRein()
        {
            this.ComponentTransform = Matrix3d.Identity;
            this.HookAttachment = HookAttachmentPoint.HookAttachmentTopLeft;
        }

        public override void GenerateLayout()
        {
            // 参数列表：
            //  Len_X：柱截面X方向大小
            //  Len_Y：柱截面Y方向大小
            //  intCBarCount：角筋根数
            //  intCBarDia：角筋直径
            //  intXBarCount：X方向纵筋根数
            //  intXBarDia：X方向纵筋直径
            //  intYBarCount：Y方向纵筋根数
            //  intYBarDia：Y方向纵筋直径
            ConstructPoints();
            ConstructAngularRein();
        }

        public override void Render(IThSComponentRenderTarget target)
        {
            target.Paint(this, ThSComponentDbStyleManager.Instance.Styles["DetailRein"]);
            foreach(var point in points)
            {
                point.Render(target);
            }
        }

        private Polyline ConstructHookCurve(double angle)
        {
            var hookSegments = new PolylineSegmentCollection();

            var start = new Point2d(0.0, 0.0);
            var length = hook_x_length + Math.Tan(Math.PI / 8) * fillet_radius;
            var end = new Point2d(length, 0.0);
            hookSegments.Add(new PolylineSegment(start, end));

            start = end;
            length = hook_y_length + Math.Tan(Math.PI / 8) * fillet_radius;
            end = start.Add(Vector2d.XAxis.RotateBy(angle).MultiplyBy(length));
            hookSegments.Add(new PolylineSegment(start, end));

            var pLine = hookSegments.ToPolyline();
            pLine.FilletAll(fillet_radius);

            return pLine;
        }

        private void ConstructPoints()
        {
            double xLength = (double)Parameters.Where(o => o.Key == "Len_X").First().Value;
            double yLength = (double)Parameters.Where(o => o.Key == "Len_Y").First().Value;
            double cBarDia = (double)Parameters.Where(o => o.Key == "intCBarDia").First().Value;
            int cBarCount = (int)Parameters.Where(o => o.Key == "intCBarCount").First().Value;
            if (HookAttachment == HookAttachmentPoint.HookAttachmentTopLeft)
            {
                double xOffset = section_distance + fillet_radius + Math.Cos(Math.PI / 4) * point_distance;
                double yOffset = section_distance + fillet_radius + Math.Cos(Math.PI / 4) * point_distance;

                // 左上角
                var point = new ThSDetailReinPoint();
                point.Parameters.Add(new ThSComponentParameter("intCBarDia", cBarDia));
                point.ComponentTransform = Matrix3d.Displacement(new Vector3d(-xLength / 2.0 + xOffset, yLength / 2.0 - yOffset, 0.0));
                point.GenerateLayout();
                points.Add(point);

                // 右上角
                point = new ThSDetailReinPoint();
                point.Parameters.Add(new ThSComponentParameter("intCBarDia", cBarDia));
                point.ComponentTransform = Matrix3d.Displacement(new Vector3d(xLength / 2.0 - xOffset, yLength / 2.0 - yOffset, 0.0));
                point.GenerateLayout();
                points.Add(point);

                // 右下角
                point = new ThSDetailReinPoint();
                point.Parameters.Add(new ThSComponentParameter("intCBarDia", cBarDia));
                point.ComponentTransform = Matrix3d.Displacement(new Vector3d(xLength / 2.0 - xOffset, -yLength / 2.0 + yOffset, 0.0));
                point.GenerateLayout();
                points.Add(point);

                // 左下角
                point = new ThSDetailReinPoint();
                point.Parameters.Add(new ThSComponentParameter("intCBarDia", cBarDia));
                point.ComponentTransform = Matrix3d.Displacement(new Vector3d(-xLength / 2.0 + xOffset, -yLength / 2.0 + yOffset, 0.0));
                point.GenerateLayout();
                points.Add(point);
            }
        }

        private void ConstructAngularRein()
        {
            double xLength = (double)Parameters.Where(o => o.Key == "Len_X").First().Value;
            double yLength = (double)Parameters.Where(o => o.Key == "Len_Y").First().Value;

            if (HookAttachment == HookAttachmentPoint.HookAttachmentTopLeft)
            {
                var pLine = new Polyline()
                {
                    Closed = true
                };
                pLine.AddVertexAt(0, new Point2d(-xLength / 2.0, yLength / 2.0), 0, 0, 0);
                pLine.AddVertexAt(1, new Point2d(xLength / 2.0, yLength / 2.0), 0, 0, 0);
                pLine.AddVertexAt(2, new Point2d(xLength / 2.0, -yLength / 2.0), 0, 0, 0);
                pLine.AddVertexAt(3, new Point2d(-xLength / 2.0, -yLength / 2.0), 0, 0, 0);
                var rein = pLine.GetOffsetCurves(section_distance)[0] as Polyline;
                rein.FilletAll(fillet_radius);

                // 垂直弯钩
                var vHook = ConstructHookCurve(Math.PI / 4);
                Matrix3d scaleMat = Matrix3d.Identity;
                Matrix3d rotationMat = Matrix3d.Rotation(-Math.PI / 2, Vector3d.ZAxis, Point3d.Origin);
                Matrix3d movementMat = Matrix3d.Displacement(rein.GetPoint3dAt(0).GetAsVector());
                vHook.TransformBy(scaleMat.PreMultiplyBy(rotationMat).PreMultiplyBy(movementMat));
                vHook.ReverseCurve();

                // 水平弯钩
                var hHook = ConstructHookCurve(-Math.PI / 4);
                scaleMat = Matrix3d.Identity;
                rotationMat = Matrix3d.Identity;
                movementMat = Matrix3d.Displacement(rein.GetPoint3dAt(1).GetAsVector());
                hHook.TransformBy(scaleMat.PreMultiplyBy(rotationMat).PreMultiplyBy(movementMat));

                // 组装
                var components = new PolylineSegmentCollection();
                components.AddRange(new PolylineSegmentCollection(vHook));
                components.AddRange(new PolylineSegmentCollection(rein));
                components.Add(new PolylineSegment(rein.GetArcSegment2dAt(0)));
                components.AddRange(new PolylineSegmentCollection(hHook));

                // 保存
                segments.Add(new ThSComponentCurve(components.ToPolyline()));
            }
        }
    }
}
