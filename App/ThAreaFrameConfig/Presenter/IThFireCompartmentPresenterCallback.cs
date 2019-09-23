using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThAreaFrameConfig.Presenter
{
    public interface IThFireCompartmentPresenterCallback
    {
        // 选取面积框线
        bool OnPickAreaFrames(string name);
    }
}
