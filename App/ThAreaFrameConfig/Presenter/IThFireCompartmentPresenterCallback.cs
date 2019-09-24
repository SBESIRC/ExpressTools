using System;
using ThAreaFrameConfig.Model;

namespace ThAreaFrameConfig.Presenter
{
    public interface IThFireCompartmentPresenterCallback
    {
        // 选取面积框线
        bool OnPickAreaFrames(ThFireCompartment compartment, string name);

        // 修改防火分区
        bool OnModifyFireCompartment(ThFireCompartment compartment);
    }
}
