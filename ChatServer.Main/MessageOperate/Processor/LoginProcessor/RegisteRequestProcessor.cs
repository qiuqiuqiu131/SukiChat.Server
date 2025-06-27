using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.Common.Tool;
using ChatServer.Main.Entity;
using ChatServer.Main.IOServer.Manager;
using ChatServer.Main.Services;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Configuration;
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
    private readonly IClientChannelManager clientChannelManager;
    private readonly IConfigurationRoot configurationRoot;

    public RegisteRequestProcessor(ILoginService loginHelper,
        IClientChannelManager clientChannelManager,
        IConfigurationRoot configurationRoot)
    {
        this.loginHelper = loginHelper;
        this.clientChannelManager = clientChannelManager;
        this.configurationRoot = configurationRoot;
    }

    public async Task Process(MessageUnit<RegisteRequest> unit)
    {
        unit.Channel.TryGetTarget(out IChannel? channel);
        if (channel == null) return;

        var message = unit.Message;
        var (status,id) = await loginHelper.Registe(message.Name, message.Password,message.Email,message.Phone);

        var response = new CommonResponse { State = status };
        if (status)
            response.Message = "注册成功";
        else
            response.Message = "注册失败";

        await channel.WriteAndFlushProtobufAsync(new RegisteResponse
        {
            Response = response,
            Id = id
        });

        if(status)
        {
            var robotId = configurationRoot.GetValue("Robot:Id", "1310000001");
            var robot = clientChannelManager.GetClient(robotId);
            if(robot == null) return;

            var newFriendMessage = new NewFriendMessage
            {
                UserId = robotId,
                FrinedId = id,
                Grouping = "默认分组",
            };
            await robot.WriteAndFlushProtobufAsync(newFriendMessage);
        }
    }
}
