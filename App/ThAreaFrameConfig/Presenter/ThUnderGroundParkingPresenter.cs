using System;
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

        public void OnHandleAcadException(Exception e)
        {
            this.HandleAcadException(e);
        }

        public void OnPickAreaFrames(string name)
        {
            this.PickAreaFrames(name);
        }

        public void OnRenameAreaFrameLayer(string newName, IntPtr areaFrame)
        {
            //
        }

        public void OnHighlightAreaFrame(IntPtr areaFrame)
        {
            //
        }

        public void OnUnhighlightAreaFrame(IntPtr areaFrame)
        {
            //
        }

        public void OnDeleteAreaFrame(IntPtr areaFrame)
        {
            //
        }

        public void OnDeleteAreaFrameLayer(string name)
        {
            //
        }

        public void OnHighlightAreaFrames(IntPtr[] areaFrames)
        {
            //
        }

        public void OnUnhighlightAreaFrames(IntPtr[] areaFrames)
        {
            //
        }

        public void OnDeleteAreaFrames(IntPtr[] areaFrames)
        {
            //
        }

        public void OnDeleteAreaFrames(IntPtr areaFrame)
        {
            //
        }
    }
}
