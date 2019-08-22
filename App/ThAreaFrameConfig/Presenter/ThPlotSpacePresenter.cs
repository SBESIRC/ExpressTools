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
            throw new NotImplementedException();
        }

        public void OnDeleteAreaFrameLayer(string name)
        {
            throw new NotImplementedException();
        }

        public void OnDeleteAreaFrames(IntPtr[] areaFrames)
        {
            throw new NotImplementedException();
        }

        public void OnHandleAcadException(Exception e)
        {
            throw new NotImplementedException();
        }

        public void OnHighlightAreaFrame(IntPtr areaFrame)
        {
            throw new NotImplementedException();
        }

        public void OnHighlightAreaFrames(IntPtr[] areaFrames)
        {
            throw new NotImplementedException();
        }

        public void OnPickAreaFrames(string name)
        {
            throw new NotImplementedException();
        }

        public void OnRenameAreaFrameLayer(string newName, IntPtr areaFrame)
        {
            throw new NotImplementedException();
        }

        public void OnUnhighlightAreaFrame(IntPtr areaFrame)
        {
            throw new NotImplementedException();
        }

        public void OnUnhighlightAreaFrames(IntPtr[] areaFrames)
        {
            throw new NotImplementedException();
        }
    }
}
