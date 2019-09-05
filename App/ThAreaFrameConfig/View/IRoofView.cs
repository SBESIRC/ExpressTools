using System.Collections.Generic;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.Presenter;

namespace ThAreaFrameConfig.View
{
    public interface IRoofView : IThAreaFrameView
    {
        List<ThRoof> Roofs { get; set; }
    }
}
