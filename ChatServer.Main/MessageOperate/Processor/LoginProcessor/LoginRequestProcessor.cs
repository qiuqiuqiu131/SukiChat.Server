using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.Common.Tool;
using ChatServer.Main.Entity;
using ChatServer.Main.IOServer.Manager;
using ChatServer.Main.Services;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;

namespace ChatServer.Main.MessageOperate.Processor.LoginProcessor;

/// <summary>
/// 处理目标：
/// LoginRequest 登录请求
/// 
/// 需要发送的消息: 
/// CommonResponse 登录结果
/// </summary>
public class LoginRequestProcessor : IProcessor<LoginRequest>
{
    private readonly ILoginService loginService;
    private readonly IClientChannelManager clientChannelManager;

    public LoginRequestProcessor(ILoginService loginService,
        IClientChannelManager clientChannelManager)
    {
        this.loginService = loginService;
        this.clientChannelManager = clientChannelManager;
    }

    public async Task Process(MessageUnit<LoginRequest> unit)
    {
        unit.Channel.TryGetTarget(out IChannel? channel);
        if (channel == null) return;

        var message = unit.Message;
        string? userName = await loginService.Login(message.Id, message.Password);

        var response = new CommonResponse { State = userName != null };
        if (userName != null)
        {
            //登录成功
            response.Message = "登录成功";
            clientChannelManager.ClientLogin(channel, unit.Message.Id);
        }
        else
            response.Message = "登录失败";

        await channel.WriteAndFlushProtobufAsync(new LoginResponse { 
            Response = response, Id = message.Id});
    }
}
