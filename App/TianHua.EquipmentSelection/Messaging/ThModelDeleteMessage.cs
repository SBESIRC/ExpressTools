using Catel.Messaging;

namespace TianHua.FanSelection.Messaging
{
    public class ThModelDeleteMessageArgs : ThModelMessageArgs
    {
        public string Model { get; set; }
    }

    public class ThModelDeleteMessage : MessageBase<ThModelDeleteMessage, ThModelDeleteMessageArgs>
    {
        public ThModelDeleteMessage()
        {
        }

        public ThModelDeleteMessage(ThModelDeleteMessageArgs args)
            : base(args)
        {
        }
    }
}
