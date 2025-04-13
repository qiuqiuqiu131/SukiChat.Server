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

public class CallResponseProcessor : IProcessor<CallResponse>
{
    private readonly IClientChannelManager _channelManager;

    public CallResponseProcessor(
        IClientChannelManager channelManager)
    {
        _channelManager = channelManager;
    }

    public async Task Process(MessageUnit<CallResponse> unit)
    {
        if (!unit.Channel.TryGetTarget(out var channel))
            return;

        var response = unit.Message;

        // 将响应转发给呼叫方
        var callerChannel = _channelManager.GetClient(response.Caller);
        if (callerChannel != null)
        {
            await callerChannel.WriteAndFlushProtobufAsync(response);
        }
    }
}
