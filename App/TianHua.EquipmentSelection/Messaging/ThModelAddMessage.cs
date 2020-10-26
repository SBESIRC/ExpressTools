using Catel.Messaging;

namespace TianHua.FanSelection.Messaging
{
    public class ThModelAddMessage : MessageBase<ThModelAddMessage, FanDataModel>
    {
        public ThModelAddMessage()
        {
        }

        public ThModelAddMessage(FanDataModel model)
            : base(model)
        {
        }
    }
}
