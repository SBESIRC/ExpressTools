using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThAreaFrameConfig.View;

namespace ThAreaFrameConfig.Presenter
{
    public class ThAOccupanyPresenter : IThAreaFramePresenter, IAOccupanyPresenterCallback
    {
        private readonly IAOccupancyView roomView;

        public ThAOccupanyPresenter(IAOccupancyView view)
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
