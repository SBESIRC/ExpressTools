using System.Collections.Generic;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.Presenter;

namespace ThAreaFrameConfig.View
{
    public interface IPlotSpaceView : IThAreaFrameView
    {
        List<ThPlotSpace> Spaces { get; set; }
    }
}
