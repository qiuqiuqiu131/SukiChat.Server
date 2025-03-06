using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.Common.Tool;
using ChatServer.Main.Entity;
using ChatServer.Main.Services;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Processor.LoginProcessor;

/// <summary>
/// 处理目标：
/// RegisterRequest 注册账号请求
/// 
/// 数据库操作：
/// 1、User 添加用户
/// 
/// 需要发送的消息:
/// 1、CommonResponse 注册结果
/// </summary>
public class RegisteRequestProcessor : IProcessor<RegisteRequest>
{
    private readonly ILoginService loginHelper;

    public RegisteRequestProcessor(ILoginService loginHelper)
    {
        this.loginHelper = loginHelper;
    }

    public async Task Process(MessageUnit<RegisteRequest> unit)
    {
        unit.Channel.TryGetTarget(out IChannel? channel);
        if (channel == null) return;

        var message = unit.Message;
        bool result = await loginHelper.Registe(message.Name, message.Password);

        var response = new CommonResponse { State = result };
        if (result)
            response.Message = "注册成功";
        else
            response.Message = "注册失败";

        await channel.WriteAndFlushProtobufAsync(response);
    }
}
