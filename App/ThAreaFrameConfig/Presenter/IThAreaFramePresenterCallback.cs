using System;

namespace ThAreaFrameConfig.Presenter
{
    public interface IThAreaFramePresenterCallback
    {
        // 选取面积框线
        void OnPickAreaFrames(string name);

        // 删除面积框线
        void OnDeleteAreaFrame(IntPtr areaFrame);

        // 重命名面积框线所在的图层
        void OnRenameAreaFrameLayer(string newName, IntPtr areaFrame);

        // 删除面积框线图层
        void OnDeleteAreaFrameLayer(string name);

        // 处理异常
        void OnHandleAcadException(Exception e);

        // 高亮面积框线
        void OnHighlightAreaFrame(IntPtr areaFrame);

        // 去高亮面积框线
        void OnUnhighlightAreaFrame(IntPtr areaFrame);
    }
}
