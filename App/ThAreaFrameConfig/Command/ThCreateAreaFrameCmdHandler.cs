using AcHelper.Commands;

namespace ThAreaFrameConfig.Command
{
    public class ThCreateAreaFrameCmdHandler : CommandHandlerBase
    {
        public static string LayerName { get; set; }
        public static ICreateAreaFrameCommand Handler { get; set; }
    }
}
