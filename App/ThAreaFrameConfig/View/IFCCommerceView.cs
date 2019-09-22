using System.Collections.Generic;
using ThAreaFrameConfig.Model;

namespace ThAreaFrameConfig.View
{ 
    public interface IFCCommerceView : IThAreaFrameView
    {
        List<ThFireCompartment> Compartments { get; set; }
    }
}
