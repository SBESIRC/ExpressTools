using System;
using System.Collections.Generic;
using ThAreaFrameConfig.View;

namespace ThAreaFrameConfig.Presenter
{
    public class ThPublicGreenSpacePresenter : IThAreaFramePresenter, IPublicGreenSpacePresenterCallback
    {
        private readonly IPublicGreenSpaceView roomView;

        public ThPublicGreenSpacePresenter(IPublicGreenSpaceView view)
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
            return this.PickAreaFrames(name, ThResidentialRoomDbUtil.ConfigPublicGreenSpaceLayer);
        }

        public bool OnAdjustAreaFrames(Dictionary<string, string> parameters)
        {
            return this.AdjustAreaFrames(parameters, ThResidentialRoomDbUtil.ConfigPublicGreenSpaceLayer);
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
