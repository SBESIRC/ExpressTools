using System;
using Autodesk.AutoCAD.Geometry;

namespace ThStructure.Model
{
    public class ThSComponentDecorator : ThSComponent
    {
        protected ThSComponent component;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// 
        public ThSComponentDecorator(ThSComponent component)
        {
            this.component = component;
        }

        public override ThSComponentCurveCollection Geometries
        {
            get
            {
                return component.Geometries;
            }
        }

        public override Matrix3d ComponentTransform
        {
            get
            {
                return component.ComponentTransform;
            }

            set
            {
                component.ComponentTransform = value;
            }
        }

        public override ThSComponentParameterCollection Parameters
        {
            get
            {
                return component.Parameters;
            }
        }

        public override void GenerateLayout()
        {
            component.GenerateLayout();
        }

        public override void Render(IThSComponentRenderTarget target)
        {
            component.Render(target);
        }
    }
}
