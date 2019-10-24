using System;
using System.Collections.Generic;
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

        public void OnHighlightAreaFrame(IntPtr areaFrame)
        {
            this.HighlightAreaFrame(areaFrame);
        }

        public void OnUnhighlightAreaFrame(IntPtr areaFrame)
        {
            this.UnhighlightAreaFrame(areaFrame);
        }

        public void OnHighlightAreaFrames(IntPtr[] areaFrames)
        {
            this.HighlightAreaFrames(areaFrames);
        }

        public void OnUnhighlightAreaFrames(IntPtr[] areaFrames)
        {
            this.UnhighlightAreaFrames(areaFrames);
        }

        public bool OnPickAreaFrames(string name)
        {
            return this.PickAreaFrames(name, ThResidentialRoomDbUtil.ConfigLayer);
        }

        public bool OnAdjustAreaFrames(Dictionary<string, string> parameters)
        {
            return this.AdjustAreaFrames(parameters, ThResidentialRoomDbUtil.ConfigLayer);
        }

        public void OnMoveAreaFrameToLayer(string newName, IntPtr areaFrame)
        {
            this.MoveAreaFrameToLayer(newName, areaFrame, ThResidentialRoomDbUtil.ConfigLayer);
        }

        public void OnRenameAreaFrameLayer(string newName, IntPtr[] areaFrames)
        {
            this.RenameAreaFrameLayer(newName, areaFrames);
        }

        public void OnDeleteAreaFrame(IntPtr areaFrame)
        {
            this.DeleteAreaFrame(areaFrame);
        }

        public void OnDeleteAreaFrames(IntPtr[] areaFrames)
        {
            throw new NotImplementedException();
        }

        public void OnDeleteAreaFrameLayer(string name)
        {
            this.DeleteAreaFrameLayer(name);
        }
    }
}
