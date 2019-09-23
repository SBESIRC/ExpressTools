using System;
using ThAreaFrameConfig.Model;

namespace ThAreaFrameConfig.Presenter
{
    public interface IThFireCompartmentPresenterCallback
    {
        // 选取面积框线
        bool OnPickAreaFrames(ThFireCompartment compartment, string name);
    }
}
