using System;

namespace ThAreaFrameConfig.Presenter
{
    public interface IThAreaFramePresenterCallback
    {
        void OnPickAreaFrames(string name);

        void OnHandleAcadException(Exception e);
    }
}
