using System;
using AcHelper;
using ThAreaFrameConfig.View;
using Autodesk.AutoCAD.EditorInput;

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

        public void Initialize()
        {
            roomView.Attach(this);
        }

        public void OnHandleAcadException(System.Exception e)
        {
            this.HandleAcadException(e);
        }

        public void OnPickAreaFrames(string name)
        {
            this.PickBuildingAreaFrames(name);
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
    }
}
