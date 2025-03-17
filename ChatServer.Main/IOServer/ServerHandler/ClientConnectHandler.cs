using ChatServer.Common.Protobuf;
using ChatServer.Common.Tool;
using ChatServer.Main.IOServer.Manager;
using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using Google.Protobuf;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.IOServer.ServerHandler
{
    /// <summary>
    /// 处理客户端连接
    /// </summary>
    internal class ClientConnectHandler : ChannelHandlerAdapter
    {
        private readonly IClientChannelManager userManager;
        private readonly ILogger logger;
        private readonly int readIdleTolerate;

        private int readIdleTimes = 0; // 空闲计数

        public ClientConnectHandler(IClientChannelManager userManager, ILogger logger,IConfigurationRoot configuration)
        {
            this.userManager = userManager;
            this.logger = logger;

            readIdleTolerate = int.Parse(configuration["Heartbeat:ReadIdleTolerate"]!);
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            base.ChannelActive(context);
            // logger.Information("New Client connected: " + context.Channel.RemoteAddress);
            userManager.AddClient(context.Channel);
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var buffer = message as IByteBuffer;

            if (buffer == null)
                return;

            var readableBytes = new byte[buffer.ReadableBytes];
            buffer.GetBytes(buffer.ReaderIndex, readableBytes);
            buffer.Release();

            IMessage mess = ProtobufHelper.ParseFrom(readableBytes);

            // 心跳包
            if (mess is HeartBeat)
            {
                readIdleTimes = 0;
                return;
            }
            base.ChannelRead(context, mess);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            base.ExceptionCaught(context, exception);
            context.CloseAsync();
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            // logger.Information("Client disconnected: " + context.Channel?.RemoteAddress);
            userManager.RemoveClient(context.Channel);
            
            context.Channel?.Pipeline.Remove(this);
            base.ChannelInactive(context);
        }

        public override async void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            if (evt is not IdleStateEvent e) return;

            switch (e.State)
            {
                //长期没收到客户端发来数据
                //发送心跳包，客户接收到心跳包后，会回复一个心跳包
                //如果客户端没有回复心跳包，说明客户端已经断开连接
                case IdleState.ReaderIdle:
                    readIdleTimes++;
                    if (readIdleTimes > readIdleTolerate)
                    {
                        await context.CloseAsync();
                        break;
                    }
                    byte[] messageBytes = ProtobufHelper.Serialize(new HeartBeat());
                    try
                    {
                        await context.Channel.WriteAndFlushAsync(Unpooled.CopiedBuffer(messageBytes));
                    }
                    catch { }
                    break;
            }
        }
    }
}
