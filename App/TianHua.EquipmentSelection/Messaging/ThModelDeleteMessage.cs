using Catel.Messaging;

namespace TianHua.FanSelection.Messaging
{
    public class ThModelDeleteMessage : MessageBase<ThModelDeleteMessage, FanDataModel>
    {
        public ThModelDeleteMessage()
        {
        }

        public ThModelDeleteMessage(FanDataModel model)
            : base(model)
        {
        }
    }
}
