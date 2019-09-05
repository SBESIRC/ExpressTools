using System.Collections.Generic;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.Presenter;

namespace ThAreaFrameConfig.View
{
    public interface IPublicGreenSpaceView : IThAreaFrameView
    {
        List<ThPublicGreenSpace> Spaces { get; set; }
    }
}
