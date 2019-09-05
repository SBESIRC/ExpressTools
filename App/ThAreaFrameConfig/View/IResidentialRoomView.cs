using System.Collections.Generic;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.Presenter;

namespace ThAreaFrameConfig.View
{
    public interface IResidentialRoomView : IThAreaFrameView
    {
        List<ThResidentialRoom> Rooms { get; }
    }
}
