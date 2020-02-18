using Autodesk.AutoCAD.DatabaseServices;
using Linq2Acad;

namespace ThStructure.Model
{
    public class ThSComponentDbRender : IThSComponentRenderTarget
    {
        public void Paint(ThSComponent component, IThSComponentRenderStyle style)
        {
            using (AcadDatabase acadDatabase = AcadDatabase.Active())
            {
                foreach(var curve in component.Geometries)
                {
                    curve.Geometry.TransformBy(component.ComponentTransform);
                    acadDatabase.ModelSpace.Add(curve.Geometry);
                    curve.Geometry.ConstantWidth = (double)style.Value(nameof(Polyline.ConstantWidth));
                }
            }
        }
    }
}
