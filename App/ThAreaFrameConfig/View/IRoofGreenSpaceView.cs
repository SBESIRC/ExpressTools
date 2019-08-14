using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.Presenter;

namespace ThAreaFrameConfig.View
{
    public interface IRoofGreenSpaceView : IThAreaFrameView<IRoofGreenSpacePresenterCallback>
    {
        List<ThRoofGreenSpace> Spaces { get; set; }

        // 刷新
        void Reload();
    }
}
