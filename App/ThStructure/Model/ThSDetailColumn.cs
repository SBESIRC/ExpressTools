using System.Linq;
using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;

namespace ThStructure.Model
{
    public class ThSDetailColumn : ThSComponent
    {
        public override Matrix3d ComponentTransform { get; set; }
        public override ThSComponentCurveCollection Geometries { get;  }
        public override ThSComponentParameterCollection Parameters { get; }

        private ThSDetailSection Section { get; set; }
        private Dictionary<string, ThSDetailRein> Reins { get;  }

        public ThSDetailColumn()
        {
            this.Section = null;
            this.ComponentTransform = Matrix3d.Identity;
            this.Reins = new Dictionary<string, ThSDetailRein>();
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
            double cBarDia = (double)Parameters.Where(o => o.Key == "intCBarDia").First().Value;
            int cBarCount = (int)Parameters.Where(o => o.Key == "intCBarCount").First().Value;

            // 截面
            this.Section = new ThSDetailSection();
            this.Section.Parameters.Add(new ThSComponentParameter("Len_X", xLength));
            this.Section.Parameters.Add(new ThSComponentParameter("Len_Y", xLength));
            this.Section.GenerateLayout();

            // 角筋
            var rein = new ThSDetailRein();
            rein.Parameters.Add(new ThSComponentParameter("Len_X", xLength));
            rein.Parameters.Add(new ThSComponentParameter("Len_Y", yLength));
            rein.Parameters.Add(new ThSComponentParameter("intCBarDia", cBarDia));
            rein.Parameters.Add(new ThSComponentParameter("intCBarCount", cBarCount));
            rein.GenerateLayout();
            this.Reins.Add("AngularRein", rein);
        }

        public override void Render(IThSComponentRenderTarget target)
        {
            this.Section.Render(target);
            foreach(var rein in Reins)
            {
                rein.Value.Render(target);
            }
        }
    }
}
