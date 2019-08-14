using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
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
