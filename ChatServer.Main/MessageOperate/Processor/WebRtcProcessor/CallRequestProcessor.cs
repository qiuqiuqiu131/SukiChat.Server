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

public class CallRequestProcessor : IProcessor<CallRequest>
{
    private readonly IClientChannelManager _channelManager;

    public CallRequestProcessor(
        IClientChannelManager channelManager)
    {
        _channelManager = channelManager;
    }

    public async Task Process(MessageUnit<CallRequest> unit)
    {
        if (!unit.Channel.TryGetTarget(out var channel))
            return;

        var request = unit.Message;

        // 检查被叫方是否在线
        var calleeChannel = _channelManager.GetClient(request.Callee);
        if (calleeChannel == null)
        {
            await channel.WriteAndFlushProtobufAsync(new CallResponse
            {
                Response = new CommonResponse { State = false, Message = "用户不在线" },
                Caller = request.Caller,
                Callee = request.Callee,
                Accept = false
            });
            return;
        }

        // 转发呼叫请求给被叫方
        await calleeChannel.WriteAndFlushProtobufAsync(request);
    }
}
