using System.Collections.Generic;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.Presenter;

namespace ThAreaFrameConfig.View
{
    public interface IPublicGreenSpaceView : IThAreaFrameView<IPublicGreenSpacePresenterCallback>
    {
        List<ThPublicGreenSpace> Spaces { get; set; }

        // 刷新
        void Reload();
    }
}
