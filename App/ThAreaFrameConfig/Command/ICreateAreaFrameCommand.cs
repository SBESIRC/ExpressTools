using AcHelper.Commands;

namespace ThAreaFrameConfig.Command
{
    // 扩展IAcadCommand以支持参数
    public interface ICreateAreaFrameCommand : IAcadCommand
    {
        bool Success { get; }
        void Execute(object[] parameters);
    }
}
