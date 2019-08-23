using System.Collections.Generic;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.Presenter;

namespace ThAreaFrameConfig.View
{
    public interface IPlotSpaceView : IThAreaFrameView<IPlotSpacePresenterCallback>
    {
        List<ThPlotSpace> Spaces { get; set; }

        // 刷新
        void Reload();
    }
}
