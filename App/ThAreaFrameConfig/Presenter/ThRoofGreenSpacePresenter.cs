using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThAreaFrameConfig.View;

namespace ThAreaFrameConfig.Presenter
{
    public class ThRoofGreenSpacePresenter : IThAreaFramePresenter, IRoofGreenSpacePresenterCallback
    {
        private readonly IRoofGreenSpaceView roomView;

        public ThRoofGreenSpacePresenter(IRoofGreenSpaceView view)
        {
            roomView = view;
        }

        public object UI => roomView;

        public void Initialize()
        {
            roomView.Attach(this);
        }
    }
}
