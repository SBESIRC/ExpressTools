using System.Collections.Generic;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.Presenter;

namespace ThAreaFrameConfig.View
{
    public interface IRoofGreenSpaceView : IThAreaFrameView
    {
        List<ThRoofGreenSpace> Spaces { get; set; }
    }
}
