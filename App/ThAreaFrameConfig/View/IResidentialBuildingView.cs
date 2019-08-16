using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ThAreaFrameConfig.Model;
using ThAreaFrameConfig.Presenter;

namespace ThAreaFrameConfig.View
{
    public interface IResidentialBuildingView : IThAreaFrameView<IResidentialBuildingPresenterCallback>
    {
        // 刷新
        void Reload();
    }
}
