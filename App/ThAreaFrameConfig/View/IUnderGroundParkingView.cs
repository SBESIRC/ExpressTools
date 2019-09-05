using System.Collections.Generic;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.Presenter;

namespace ThAreaFrameConfig.View
{
    public interface IUnderGroundParkingView : IThAreaFrameView
    {
        List<ThUnderGroundParking> Parkings { get; set; }
    }
}
