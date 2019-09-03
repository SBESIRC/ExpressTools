using System;
using ThAreaFrameConfig.View;

namespace ThAreaFrameConfig.Presenter
{
    public class ThOutdoorParkingSpacePresenter : IThAreaFramePresenter, IOutdoorParkingSpacePresenterCallback
    {
        private readonly IOutdoorParkingSpaceView roomView;

        public ThOutdoorParkingSpacePresenter(IOutdoorParkingSpaceView view)
        {
            roomView = view;
        }

        public object UI => roomView;

        public void Initialize()
        {
            roomView.Attach(this);
        }

        public void OnDeleteAreaFrame(IntPtr areaFrame)
        {
            this.DeleteAreaFrame(areaFrame);
        }

        public void OnDeleteAreaFrameLayer(string name)
        {
            this.DeleteAreaFrameLayer(name);
        }

        public void OnDeleteAreaFrames(IntPtr[] areaFrames)
        {
            this.DeleteAreaFrames(areaFrames);
        }

        public void OnHandleAcadException(Exception e)
        {
            this.HandleAcadException(e);
        }

        public void OnHighlightAreaFrame(IntPtr areaFrame)
        {
            this.HighlightAreaFrame(areaFrame);
        }

        public void OnHighlightAreaFrames(IntPtr[] areaFrames)
        {
            this.HighlightAreaFrames(areaFrames);
        }

        public bool OnPickAreaFrames(string name)
        {
            return this.PickAreaFrames(name, ThResidentialRoomDbUtil.ConfigOutdoorParkingSpaceLayer);
        }

        public void OnRenameAreaFrameLayer(string newName, IntPtr areaFrame)
        {
            this.RenameAreaFrameLayer(newName, areaFrame);
        }

        public void OnUnhighlightAreaFrame(IntPtr areaFrame)
        {
            this.UnhighlightAreaFrame(areaFrame);
        }

        public void OnUnhighlightAreaFrames(IntPtr[] areaFrames)
        {
            this.UnhighlightAreaFrames(areaFrames);
        }
    }
}
