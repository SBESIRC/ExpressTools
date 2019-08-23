using System.Collections.Generic;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.Presenter;

namespace ThAreaFrameConfig.View
{
    public interface IRoofView : IThAreaFrameView<IRoofPresenterCallback>
    {
        List<ThRoof> Roofs { get; set; }

        // 刷新
        void Reload();
    }
}
