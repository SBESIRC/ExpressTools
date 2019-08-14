using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.Presenter;

namespace ThAreaFrameConfig.View
{
    public interface IUnderGroundParkingView : IThAreaFrameView<IUnderGroundParkingPresenterCallback>
    {
        List<ThUnderGroundParking> Parkings { get; set; }

        // 刷新
        void Reload();
    }
}
