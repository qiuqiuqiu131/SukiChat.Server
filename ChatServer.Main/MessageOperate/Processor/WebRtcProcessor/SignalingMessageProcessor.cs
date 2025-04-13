using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.Main.Entity;
using ChatServer.Main.IOServer.Manager;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Processor.WebRtcProcessor;

public class SignalingMessageProcessor : IProcessor<SignalingMessage>
{
    private readonly IClientChannelManager _channelManager;

    public SignalingMessageProcessor(
        IClientChannelManager channelManager)
    {
        _channelManager = channelManager;
    }

    public async Task Process(MessageUnit<SignalingMessage> unit)
    {
        if (!unit.Channel.TryGetTarget(out var channel))
            return;

        var message = unit.Message;

        // 验证双方是否在线
        var targetChannel = _channelManager.GetClient(message.To);
        if (targetChannel == null)
        {
            return;
        }

        // 转发信令消息
        await targetChannel.WriteAndFlushProtobufAsync(message);
    }
}
