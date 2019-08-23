using System.Collections.Generic;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.Presenter;

namespace ThAreaFrameConfig.View
{
    public interface IResidentialRoomView : IThAreaFrameView<IThAreaFramePresenterCallback>
    {
        List<ThResidentialRoom> Rooms { get; }

        // 刷新
        void Reload();
    }
}
