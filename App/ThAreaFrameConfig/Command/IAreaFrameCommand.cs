using AcHelper.Commands;

namespace ThAreaFrameConfig.Command
{
    // 扩展IAcadCommand以支持参数
    public interface IAreaFrameCommand : IAcadCommand
    {
        bool Success { get; }
    }
}
