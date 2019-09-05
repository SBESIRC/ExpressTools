using System.Collections.Generic;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.Presenter;

namespace ThAreaFrameConfig.View
{
    public interface IAOccupancyView : IThAreaFrameView
    {
        List<ThAOccupancy> AOccupancies { get; set; }
    }
}
