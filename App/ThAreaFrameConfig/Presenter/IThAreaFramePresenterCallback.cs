using System;

namespace ThAreaFrameConfig.Presenter
{
    public interface IThAreaFramePresenterCallback
    {
        // 选取面积框线
        void OnPickAreaFrames(string name);

        // 重命名框线所在的图层
        void OnRenameAreaFrameLayer(string newName, IntPtr areaFrame);

        // 处理异常
        void OnHandleAcadException(Exception e);

        // 高亮面积框线
        void OnHighlightAreaFrame(IntPtr areaFrame);

        // 去高亮面积框线
        void OnUnhighlightAreaFrame(IntPtr areaFrame);
    }
}
