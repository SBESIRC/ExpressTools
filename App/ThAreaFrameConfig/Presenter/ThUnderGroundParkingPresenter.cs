using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThAreaFrameConfig.View;

namespace ThAreaFrameConfig.Presenter
{
    public class ThUnderGroundParkingPresenter : IThAreaFramePresenter, IUnderGroundParkingPresenterCallback
    {
        private readonly IUnderGroundParkingView roomView;

        public ThUnderGroundParkingPresenter(IUnderGroundParkingView view)
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
