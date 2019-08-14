using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.Presenter;

namespace ThAreaFrameConfig.View
{
    public interface IAOccupancyView : IThAreaFrameView<IAOccupanyPresenterCallback>
    {
        List<ThAOccupancy> AOccupancies { get; set; }

        // 刷新
        void Reload();
    }
}
