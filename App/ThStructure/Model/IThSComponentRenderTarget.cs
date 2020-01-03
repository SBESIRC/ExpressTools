using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThStructure.Model
{
    public interface IThSComponentRenderTarget
    {
        void Paint(ThSComponent component, IThSComponentRenderStyle style);
    }
}
