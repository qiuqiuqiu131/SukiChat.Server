using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.Common.Tool;
using ChatServer.Main.Entity;
using ChatServer.Main.IOServer.Manager;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;

namespace ChatServer.Main.MessageOperate.Processor.LoginProcessor;

/// <summary>
/// 处理目标:
/// LogoutRequest 客户端请求登出,在客户端关闭或者退出登录的时候发送
/// 
/// 需要发送的消息: 
/// CommonResponse 登出结果（没啥用）
/// </summary>
public class LogoutRequestProcessor : IProcessor<LogoutRequest>
{
    private readonly IClientChannelManager _clientChannelManager;

    public LogoutRequestProcessor(IClientChannelManager clientChannelManager)
    {
        _clientChannelManager = clientChannelManager;
    }

    public async Task Process(MessageUnit<LogoutRequest> unit)
    {
        unit.Channel.TryGetTarget(out IChannel? channel);
        if (channel == null) return;

        _clientChannelManager.ClientLogout(channel);

        var mess = new CommonResponse { State = true };
        await channel.WriteAndFlushProtobufAsync(mess);
    }
}