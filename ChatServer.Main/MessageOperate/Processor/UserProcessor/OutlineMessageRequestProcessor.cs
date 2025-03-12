﻿using AutoMapper;
using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.Services;
using DotNetty.Transport.Channels;
using Google.Protobuf.Collections;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Processor.UserProcessor;

/// <summary>
/// 处理目标:
/// OutlineMessageRequest 获取客户离线时未接收到的消息，在客户上线时会发送此消息用于请求未接收到的消息
/// 
/// 需要发送的消息:
/// 1、OutlineMessageResponse 离线消息
/// </summary>
public class OutlineMessageRequestProcessor : IProcessor<OutlineMessageRequest>
{
    private readonly IServiceProvider serviceProvider;
    private readonly IUserService userService;
    private readonly IMapper mapper;

    public OutlineMessageRequestProcessor(IServiceProvider serviceProvider,
        IUserService userService, 
        IMapper mapper)
    {
        this.serviceProvider = serviceProvider;
        this.userService = userService;
        this.mapper = mapper;
    }

    public async Task Process(MessageUnit<OutlineMessageRequest> unit)
    {
        unit.Channel.TryGetTarget(out IChannel? channel);

        if (!await userService.IsUserExist(unit.Message.Id))
        {
            if (channel != null)
            {
                await channel.WriteAndFlushProtobufAsync(new OutlineMessageResponse { Id = unit.Message.Id });
            }
            return;
        }

        DateTime time = DateTime.Parse(unit.Message.LastLogoutTime);

        #region Step 1
        //-- 操作：获取离线后新朋友消息 --//
        var newFriendsTask = GetNewFriendMessages(unit.Message.Id, time);
        //-- 操作：获取离线后好友请求消息 --//
        var friendRequestsTask = GetFriendRequestMessages(unit.Message.Id, time);
        //-- 操作：获取离线后聊天消息 --//
        var friendChatsTask = GetFriendChatMessage(unit.Message.Id, time);
        //-- 操作：获取离线后的进群消息 --//
        var enterGroupsTask = GetEnterGroupMessage(unit.Message.Id, time);
        //-- 操作：获取离线后的群聊消息 --//
        var groupChatsTask = GetGroupChatMessage(unit.Message.Id, time);
        //-- 操作：获取离线后的群聊请求消息 --//
        var groupRequestTask = GetGroupRequestMessage(unit.Message.Id, time);

        await Task.WhenAll(newFriendsTask, friendRequestsTask, friendChatsTask,enterGroupsTask,groupChatsTask,groupRequestTask);

        var newFriends = newFriendsTask.Result;
        var friendRequests = friendRequestsTask.Result;
        var friendChats = friendChatsTask.Result;
        var enterGroups = enterGroupsTask.Result;
        var groupChats = groupChatsTask.Result;
        var groupRequest = groupRequestTask.Result;
        #endregion

        #region Step 2
        //-- 操作：构建离线消息体,并发送 --//
        OutlineMessageResponse response = new();
        response.Id = unit.Message.Id;
        response.NewFriends.AddRange(newFriends);
        response.FriendRequests.AddRange(friendRequests);
        response.FriendChats.AddRange(friendChats);
        response.EnterGroups.AddRange(enterGroups);
        response.GroupChats.AddRange(groupChats);
        response.GroupRequests.AddRange(groupRequest);

        if (channel != null)
            await channel.WriteAndFlushProtobufAsync(response);
        #endregion
    }


    /// <summary>
    /// 找到离线时间后所有被处理的消息
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="offlineTime"></param>
    /// <returns></returns>
    private async Task<IEnumerable<NewFriendMessage>> GetNewFriendMessages(string userId, DateTime offlineTime)
    {
        using(var scope = serviceProvider.CreateScope())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var friendRelationRepository = unitOfWork.GetRepository<FriendRelation>();
            var relations = await friendRelationRepository.GetAllAsync(
                predicate: x => x.User1Id.Equals(userId) && x.GroupTime > offlineTime,
                orderBy: x => x.OrderBy(d => d.GroupTime));

            return relations.Select(mapper.Map<NewFriendMessage>);
        }
    }

    /// <summary>
    /// 找到离线时间后所有好友请求的消息
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="offlineTime"></param>
    /// <returns></returns>
    private async Task<IEnumerable<FriendRequestMessage>> GetFriendRequestMessages(string userId, DateTime offlineTime)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var friendRequestRepository = unitOfWork.GetRepository<FriendRequest>();
            var requests = await friendRequestRepository.GetAllAsync(
                predicate: x => (x.UserTargetId.Equals(userId) || x.UserFromId.Equals(userId)) && (x.RequestTime > offlineTime || x.IsSolved && x.SolveTime > offlineTime),
                orderBy: x => x.OrderBy(d => d.RequestTime));

            return requests.Select(mapper.Map<FriendRequestMessage>);
        }
    }

    /// <summary>
    /// 找到离线时间后所有聊天消息
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="offlineTime"></param>
    /// <returns></returns>
    private async Task<IEnumerable<FriendChatMessage>> GetFriendChatMessage(string userId, DateTime offlineTime)
    {
        using (var scope = serviceProvider.CreateScope()) 
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var chatPrivateRepository = unitOfWork.GetRepository<ChatPrivate>();
            var chats = await chatPrivateRepository.GetAllAsync(
                predicate: x => (x.UserFromId.Equals(userId) || x.UserTargetId.Equals(userId)) && x.Time > offlineTime,
                orderBy: x => x.OrderBy(d => d.Time));

            return chats.Select(mapper.Map<FriendChatMessage>);
        }
    }

    /// <summary>
    /// 获取群聊聊天记录
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="offlineTime"></param>
    /// <returns></returns>
    private async Task<IEnumerable<GroupChatMessage>> GetGroupChatMessage(string userId, DateTime offlineTime)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // 获取所有用户加入的群聊
            var groupService = scope.ServiceProvider.GetRequiredService<IGroupService>();
            var groupIds = await groupService.GetGroupsOfUser(userId);

            // 获取所有群聊的消息
            var chatGroupRepository = unitOfWork.GetRepository<ChatGroup>();
            var chats = await chatGroupRepository.GetAllAsync(
                predicate: d => groupIds.Contains(d.GroupId) && d.Time > offlineTime,
                orderBy: d => d.OrderBy(d => d.Time));

            return chats.Select(mapper.Map<GroupChatMessage>);
        }
    }

    /// <summary>
    ///  获取加入的群聊
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="offlineTime"></param>
    /// <returns></returns>
    private async Task<IEnumerable<EnterGroupMessage>> GetEnterGroupMessage(string userId, DateTime offlineTime)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // 在GroupRelation中找离线时加入的群聊关系
            var groupRelationRepository = unitOfWork.GetRepository<GroupRelation>();
            var relations = await groupRelationRepository.GetAllAsync(
                predicate: d=> d.UserId.Equals(userId) && d.JoinTime > offlineTime);

            return relations.Select(mapper.Map<EnterGroupMessage>);
        }
    }

    /// <summary>
    /// 获取离线时的申请入群请求和响应
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="offlineTime"></param>
    /// <returns></returns>
    private async Task<IEnumerable<GroupRequestMessage>> GetGroupRequestMessage(string userId, DateTime offlineTime)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var groupService = scope.ServiceProvider.GetRequiredService<IGroupService>();
            var groupIds = await groupService.GetGroupsOfManager(userId);

            var groupRequestRepository = unitOfWork.GetRepository<GroupRequest>();
            var requests = await groupRequestRepository.GetAllAsync(
                predicate: x => (groupIds.Contains(x.GroupId) || x.UserFromId.Equals(userId)) && (x.RequestTime > offlineTime || x.IsSolved && x.SolveTime > offlineTime),
                orderBy: x => x.OrderBy(d => d.RequestTime));

            return requests.Select(mapper.Map<GroupRequestMessage>);
        }
    }
}
