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

        public bool OnPickAreaFrames(string name)
        {
            return this.PickAreaFrames(name, ThResidentialRoomDbUtil.ConfigLayer);
        }

        public void OnRenameAreaFrameLayer(string newName, IntPtr areaFrame)
        {
            this.RenameAreaFrameLayer(newName, areaFrame);
        }

        public void OnDeleteAreaFrameLayer(string name)
        {
            this.DeleteAreaFrameLayer(name);
        }

        public void OnHighlightAreaFrame(IntPtr areaFrame)
        {
            this.HighlightAreaFrame(areaFrame);
        }

        public void OnHighlightAreaFrames(IntPtr[] areaFrames)
        {
            this.HighlightAreaFrames(areaFrames);
        }

        public void OnUnhighlightAreaFrame(IntPtr areaFrame)
        {
            this.UnhighlightAreaFrame(areaFrame);
        }

        public void OnUnhighlightAreaFrames(IntPtr[] areaFrames)
        {
            this.UnhighlightAreaFrames(areaFrames);
        }

        public void OnDeleteAreaFrame(IntPtr areaFrame)
        {
            this.DeleteAreaFrame(areaFrame);
        }

        public void OnDeleteAreaFrames(IntPtr[] areaFrames)
        {
            this.DeleteAreaFrames(areaFrames);
        }
    }
}
