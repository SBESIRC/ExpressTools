using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThAreaFrameConfig.View;

namespace ThAreaFrameConfig.Presenter
{
    public class ThRoofPresenter : IThAreaFramePresenter, IRoofPresenterCallback
    {
        private readonly IRoofView roomView;

        public ThRoofPresenter(IRoofView view)
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
