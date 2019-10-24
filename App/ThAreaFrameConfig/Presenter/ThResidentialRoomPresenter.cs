using System;
using System.Collections.Generic;
using ThAreaFrameConfig.View;

namespace ThAreaFrameConfig.Presenter
{
    public class ThResidentialRoomPresenter : IThAreaFramePresenter, IThAreaFramePresenterCallback
    {
        private readonly IResidentialRoomView roomView;

        public ThResidentialRoomPresenter(IResidentialRoomView view)
        {
            roomView = view;
        }

        public object UI => roomView;

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

        public void OnDeleteAreaFrame(IntPtr areaFrame)
        {
            this.DeleteAreaFrame(areaFrame);
        }

        public void OnDeleteAreaFrames(IntPtr[] areaFrames)
        {
            this.DeleteAreaFrames(areaFrames);
        }

        public void OnDeleteAreaFrameLayer(string name)
        {
            this.DeleteAreaFrameLayer(name);
        }

        public void OnRemoveStorey(string[] names)
        {
            foreach(var name in names)
            {
                ThResidentialRoomDbUtil.RemoveLayer(name);
            }
        }
    }
}