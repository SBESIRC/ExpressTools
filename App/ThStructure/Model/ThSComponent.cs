using Autodesk.AutoCAD.Geometry;

namespace ThStructure.Model
{
    // 组件类（抽象基类）
    public abstract class ThSComponent
    {
        // 生成组件构造
        public abstract void GenerateLayout();

        // 绘制组件构造
        public abstract void Render(IThSComponentRenderTarget target);

        // 组件几何信息
        public abstract ThSComponentCurveCollection Geometries { get; }

        // 组件位置信息
        public abstract Matrix3d ComponentTransform { get; set; }

        // 组件参数信息
        public abstract ThSComponentParameterCollection Parameters { get; }
    }
}