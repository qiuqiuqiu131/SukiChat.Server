using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Serilog;
using ChatServer.Common.Tool;
using Google.Protobuf;
using DotNetty.Common.Utilities;
using ChatServer.Main.MessageOperate;

namespace ChatServer.Main.IOServer.ServerHandler
{
    /// <summary>
    /// 处理业务逻辑
    /// </summary>
    public class EchoServerHandler : SimpleChannelInboundHandler<IMessage>
    {
        private readonly IProtobufDispatcher dispatcher;

        public EchoServerHandler(IProtobufDispatcher dispatcher) : base()
        {
            this.dispatcher = dispatcher;
        }

        /// <summary>
        /// socket接收消息方法具体的实现
        /// </summary>
        /// <param name="context">当前频道的句柄，可使用发送和接收方法</param>
        /// <param name="message">接收到的客户端发送的内容</param>
        protected override void ChannelRead0(IChannelHandlerContext context, IMessage message)
        {
            if (message != null)
            {
                // 通过ProtobufDispatcher分发消息
                dispatcher.SendMessage(context.Channel, message);
            }

            ReferenceCountUtil.Release(message);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            context.Channel?.Pipeline.Remove(this);
            base.ChannelInactive(context);
        }

    }
}
