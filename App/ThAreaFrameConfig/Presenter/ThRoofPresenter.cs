using System;
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

        public void OnHandleAcadException(Exception e)
        {
            this.HandleAcadException(e);
        }

        public void OnPickAreaFrames(string name)
        {
            this.PickRoofAreaFrames(name);
        }
    }
}
