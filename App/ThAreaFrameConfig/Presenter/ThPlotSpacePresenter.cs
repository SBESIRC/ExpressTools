using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThAreaFrameConfig.View;

namespace ThAreaFrameConfig.Presenter
{
    public class ThPlotSpacePresenter : IThAreaFramePresenter, IPlotSpaceCallback
    {
        private readonly IPlotSpaceView roomView;

        public ThPlotSpacePresenter(IPlotSpaceView view)
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

        public void OnPickAreaFrames(string name)
        {
            this.PickAreaFrames(name);
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
