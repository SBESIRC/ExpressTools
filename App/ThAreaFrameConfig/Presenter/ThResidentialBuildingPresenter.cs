using System;
using AcHelper;
using ThAreaFrameConfig.View;
using Autodesk.AutoCAD.EditorInput;
using System.Collections.Generic;

namespace ThAreaFrameConfig.Presenter
{
    public class ThResidentialBuildingPresenter : IThAreaFramePresenter, IResidentialBuildingPresenterCallback
    {
        private readonly IResidentialBuildingView roomView;

        public ThResidentialBuildingPresenter(IResidentialBuildingView view)
        {
            roomView = view;
        }

        public object UI => roomView;

        public void OnHandleAcadException(System.Exception e)
        {
            this.HandleAcadException(e);
        }

        public bool OnPickAreaFrames(string name)
        {
            return this.PickAreaFrames(name, ThResidentialRoomDbUtil.ConfigBuildingLayer);
        }

        public bool OnAdjustAreaFrames(Dictionary<string, string> parameters)
        {
            return this.AdjustAreaFrames(parameters, ThResidentialRoomDbUtil.ConfigBuildingLayer);
        }

        public void OnRenameAreaFrameLayer(string name, string newName)
        {
            using (Active.Document.LockDocument())
            {
                if (string.IsNullOrEmpty(name))
                {
                    Active.Editor.WriteLine("没有单体基底实体");
                    return;
                }

                try
                {
                    ThResidentialRoomDbUtil.RenameLayer(name, newName);
                    Active.Editor.WriteLine("修改成功");
                }
                catch
                {
                }
            }
        }

        public void OnMoveAreaFrameToLayer(string newName, IntPtr areaFrame)
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

        public void OnDeleteAreaFrames(IntPtr[] areaFrames)
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

        public void OnRenameAreaFrameLayer(string newName, IntPtr[] areaFrames)
        {
            //
        }
    }
}
