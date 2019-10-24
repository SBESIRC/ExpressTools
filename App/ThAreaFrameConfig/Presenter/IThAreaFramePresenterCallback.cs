using System;
using System.Collections.Generic;

namespace ThAreaFrameConfig.Presenter
{
    public interface IThAreaFramePresenterCallback
    {
        // 选取面积框线
        bool OnPickAreaFrames(string name);

        // 规整面积框线
        bool OnAdjustAreaFrames(Dictionary<string, string> parameters);

        // 删除面积框线
        void OnDeleteAreaFrame(IntPtr areaFrame);
        void OnDeleteAreaFrames(IntPtr[] areaFrames);

        // 重命名面积框线所在的图层
        void OnMoveAreaFrameToLayer(string newName, IntPtr areaFrame);
        void OnRenameAreaFrameLayer(string newName, IntPtr[] areaFrames);

        // 删除面积框线图层
        void OnDeleteAreaFrameLayer(string name);

        // 高亮面积框线
        void OnHighlightAreaFrame(IntPtr areaFrame);
        void OnHighlightAreaFrames(IntPtr[] areaFrames);

        // 去高亮面积框线
        void OnUnhighlightAreaFrame(IntPtr areaFrame);
        void OnUnhighlightAreaFrames(IntPtr[] areaFrames);
    }
}
