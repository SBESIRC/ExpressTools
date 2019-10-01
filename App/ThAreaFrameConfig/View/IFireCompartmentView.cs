using System.Collections.Generic;
using ThAreaFrameConfig.Model;

namespace ThAreaFrameConfig.View
{ 
    public interface IFireCompartmentView : IThAreaFrameView
    {
        List<ThFireCompartment> Compartments { get; set; }
    }
}
