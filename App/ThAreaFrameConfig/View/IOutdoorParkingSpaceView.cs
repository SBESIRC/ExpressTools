using System.Collections.Generic;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.Presenter;

namespace ThAreaFrameConfig.View
{
    public interface IOutdoorParkingSpaceView : IThAreaFrameView
    {
        List<ThOutdoorParkingSpace> Spaces { get; set; }
    }
}
