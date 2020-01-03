using System.Linq;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using DotNetARX;

namespace ThStructure.Model
{
    public class ThSDetailSection : ThSComponent
    {
        public override Matrix3d ComponentTransform { get; set; }
        public override ThSComponentCurveCollection Geometries { get; }
        public override ThSComponentParameterCollection Parameters { get; }

        public ThSDetailSection()
        {
            this.ComponentTransform = Matrix3d.Identity;
            this.Geometries = new ThSComponentCurveCollection();
            this.Parameters = new ThSComponentParameterCollection();
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
            double xLength = (double)Parameters.Where(o => o.Key == "Len_X").First().Value;
            double yLength = (double)Parameters.Where(o => o.Key == "Len_Y").First().Value;

            var section = new Polyline();
            section.CreateRectangle(new Point2d(-xLength / 2.0, -yLength / 2.0), new Point2d(xLength / 2.0, yLength / 2.0));
            this.Geometries.Add(new ThSComponentCurve(section));
        }

        public override void Render(IThSComponentRenderTarget target)
        {
            target.Paint(this, ThSComponentDbStyleManager.Instance.Styles["DetailSection"]);
        }
    }
}
