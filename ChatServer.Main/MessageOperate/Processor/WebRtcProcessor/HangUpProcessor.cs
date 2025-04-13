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

public class HangUpProcessor : IProcessor<HangUp>
{
    private readonly IClientChannelManager _channelManager;

    public HangUpProcessor(
        IClientChannelManager channelManager)
    {
        _channelManager = channelManager;
    }

    public async Task Process(MessageUnit<HangUp> unit)
    {
        if (!unit.Channel.TryGetTarget(out var channel))
            return;

        var hangUp = unit.Message;

        // 转发挂断消息给对方
        var targetChannel = _channelManager.GetClient(hangUp.To);
        if (targetChannel != null)
        {
            await targetChannel.WriteAndFlushProtobufAsync(hangUp);
        }
    }
}