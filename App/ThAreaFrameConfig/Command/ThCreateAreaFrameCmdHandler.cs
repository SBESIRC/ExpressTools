using AcHelper;
using AcHelper.Commands;

namespace ThAreaFrameConfig.Command
{
    public class ThCreateAreaFrameCmdHandler : CommandHandlerBase
    {
        public static IAreaFrameCommand Handler { get; set; }

        public static string LayerName
        {
            get
            {
                return (string)Active.Document.UserData["LayerName"];
            }

            set
            {
                Active.Document.UserData["LayerName"] = value;
            }
        }
    }
}
