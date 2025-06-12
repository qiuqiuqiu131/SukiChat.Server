using AutoMapper;
using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.IOServer.Manager;
using ChatServer.Main.Services.Helper;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Processor.UserProcessor;

/// <summary>
/// 处理目标：
/// GetUserRequest 获取用户信息
/// 
/// 需要发送的消息: 
/// 1、UserMessage 用户信息
/// </summary>
public class GetUserRequestProcessor : IProcessor<GetUserRequest>
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IMapper mapper;
    private readonly IClientChannelManager clientChannelManager;

    public GetUserRequestProcessor(IUnitOfWork unitOfWork,
        IMapper mapper,
        IClientChannelManager clientChannelManager)
    {
        this.unitOfWork = unitOfWork;
        this.mapper = mapper;
        this.clientChannelManager = clientChannelManager;
    }

    public async Task Process(MessageUnit<GetUserRequest> unit)
    {
        unit.Channel.TryGetTarget(out IChannel? channel);
        if (channel == null) return;

        var userRepository = unitOfWork.GetRepository<User>();
        var user = await userRepository.GetFirstOrDefaultAsync(predicate: d => d.Id.Equals(unit.Message.Id));
        if (user != null)
        {
            UserMessage userMessage = mapper.Map<UserMessage>(user);
            //查看用户是否在线
            userMessage.IsOnline = clientChannelManager.ClientOnline(user.Id);
            await channel.WriteAndFlushProtobufAsync(new GetUserResponse
            {
                Response = new CommonResponse { State = true },
                User = userMessage
            });
        }
        else
        {
            await channel.WriteAndFlushProtobufAsync(new GetUserResponse
            {
                Response = new CommonResponse { State = false, Message = "用户不存在" }
            });
        }
    }
}
