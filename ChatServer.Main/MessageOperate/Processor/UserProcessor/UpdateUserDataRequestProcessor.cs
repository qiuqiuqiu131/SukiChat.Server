﻿using AutoMapper;
using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.IOServer.Manager;
using ChatServer.Main.Services;
using ChatServer.Main.Services.Helper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Processor.UserProcessor;

/// <summary>
/// 处理目标：
/// UpdateUserData 更新用户信息请求
/// 
/// 数据库操作：
/// 1、User 更新用户信息
/// </summary>
public class UpdateUserDataRequestProcessor : IProcessor<UpdateUserDataRequest>
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IMapper mapper;
    private readonly IClientChannelManager channelManager;
    private readonly IFriendService friendService;
    private readonly ICipherHelper cipherHelper;

    public UpdateUserDataRequestProcessor(IUnitOfWork unitOfWork,
        IMapper mapper, 
        IClientChannelManager channelManager,
        IFriendService friendService,
        ICipherHelper cipherHelper)
    {
        this.unitOfWork = unitOfWork;
        this.mapper = mapper;
        this.channelManager = channelManager;
        this.friendService = friendService;
        this.cipherHelper = cipherHelper;
    }

    public async Task Process(MessageUnit<UpdateUserDataRequest> unit)
    {
        unit.Channel.TryGetTarget(out var channel);

        bool success = true;
        try
        {
            var repository = unitOfWork.GetRepository<User>();
            UserMessage userMess = unit.Message.User;
            User user = mapper.Map<User>(userMess);
            user.Password = cipherHelper.Encrypt(userMess.Password);
            repository.Update(user);
            await unitOfWork.SaveChangesAsync();
        }
        catch
        {
            success = false;
        }

        if(channel != null)
        {
            await channel.WriteAndFlushProtobufAsync(new UpdateUserData 
            { 
                Response = new CommonResponse { State = success },
                UserId = unit.Message.UserId,
            });
        }

        if(success)
        {
            var message = new UpdateUserData
            {
                UserId = unit.Message.UserId,
            };

            var friendIds = await friendService.GetFriendsId(unit.Message.UserId);

            foreach (var friendId in friendIds)
            {
                var friend = channelManager.GetClient(friendId);
                if(friend != null)
                    _ = friend.WriteAndFlushProtobufAsync(message);
            }
        }
    }
}
