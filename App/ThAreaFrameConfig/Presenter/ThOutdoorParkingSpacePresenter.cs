using System;
using System.Collections.Generic;
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

        public bool OnAdjustAreaFrames(Dictionary<string, string> parameters)
        {
            return this.AdjustAreaFrames(parameters, ThResidentialRoomDbUtil.ConfigOutdoorParkingSpaceLayer);
        }

        public void OnMoveAreaFrameToLayer(string newName, IntPtr areaFrame)
        {
            this.MoveAreaFrameToLayer(newName, areaFrame, ThResidentialRoomDbUtil.ConfigOutdoorParkingSpaceLayer);
        }

        public void OnRenameAreaFrameLayer(string newName, IntPtr[] areaFrames)
        {
            this.RenameAreaFrameLayer(newName, areaFrames);
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
