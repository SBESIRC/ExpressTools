using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThAreaFrameConfig.Presenter
{
    public interface IThAreaFramePresenterCallback
    {
        void OnPickAreaFrames(string name);
    }
}
