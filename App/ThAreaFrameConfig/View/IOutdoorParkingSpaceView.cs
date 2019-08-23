﻿using System.Collections.Generic;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.Presenter;

namespace ThAreaFrameConfig.View
{
    public interface IOutdoorParkingSpaceView : IThAreaFrameView<IOutdoorParkingSpacePresenterCallback>
    {
        List<ThOutdoorParkingSpace> Spaces { get; set; }

        // 刷新
        void Reload();
    }
}
