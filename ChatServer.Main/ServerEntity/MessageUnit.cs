using DotNetty.Transport.Channels;
using Google.Protobuf;

namespace ChatServer.Main.Entity
{
    public class MessageUnit<T> where T :IMessage
    {
        public T Message { get; init;}
        public WeakReference<IChannel> Channel { get; init; }
        public DateTime DateTime { get; init; }

        public MessageUnit(T message,IChannel channel)
        {
            Message = message;
            Channel = new WeakReference<IChannel>(channel);
            DateTime = DateTime.Now;
        }
    }

    public class MessageUnit
    {
        public IMessage Message { get; init;}
        public WeakReference<IChannel> Channel { get; init; }
        public DateTime DateTime { get; init; }

        public MessageUnit(IMessage message,IChannel channel)
        {
            Message = message;
            Channel = new WeakReference<IChannel>(channel);
            DateTime = DateTime.Now;
        }
    }
}
