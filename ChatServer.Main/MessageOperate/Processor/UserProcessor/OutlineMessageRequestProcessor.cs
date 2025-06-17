using AutoMapper;
using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.Services;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.DependencyInjection;

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
        var message = unit.Message;

        if (!await userService.IsUserExist(unit.Message.Id))
        {
            if (channel != null)
            {
                await channel.WriteAndFlushProtobufAsync(new OutlineMessageResponse { Response = new CommonResponse { State = false } });
            }
            return;
        }

        var user = await userService.GetUser(message.Id);

        DateTime time = DateTime.Parse(unit.Message.LastLogoutTime);

        #region Step 1
        //-- 操作：获取离线后新朋友消息 --//
        var newFriendsTask = GetNewFriendMessages(unit.Message.Id, time);
        //-- 操作：获取离线后好友请求消息 --//
        var friendRequestsTask = GetFriendRequestMessages(unit.Message.Id, time, user.LastDeleteFriendMessageTime);
        //-- 操作：获取离线后好友删除消息
        var friendDeletesTask = GetFriendDeleteMessage(unit.Message.Id, time, user.LastDeleteFriendMessageTime);
        //-- 操作：获取离线后的进群消息 --//
        var enterGroupsTask = GetEnterGroupMessage(unit.Message.Id, time);
        //-- 操作：获取离线后的群聊请求消息 --//
        var groupRequestsTask = GetGroupRequestMessage(unit.Message.Id, time, user.LastDeleteGroupMessageTime);
        //-- 操作：获取离线后的删除成员消息 --//
        var groupDeletesTask = GetGroupDeleteMessage(unit.Message.Id, time, user.LastDeleteGroupMessageTime);
        //-- 操作：获取用户的分组信息 --//
        var userGroupsTask = GetUserGroupMessage(unit.Message.Id, time);

        await Task.WhenAll(newFriendsTask, friendRequestsTask,friendDeletesTask, enterGroupsTask, groupRequestsTask,groupDeletesTask,userGroupsTask);

        #endregion

        #region Step 2
        //-- 操作：构建离线消息体,并发送 --//
        OutlineMessageResponse response = new();
        response.Id = unit.Message.Id;
        response.NewFriends.AddRange(newFriendsTask.Result);
        response.FriendRequests.AddRange(friendRequestsTask.Result);
        response.FriendDeletes.AddRange(friendDeletesTask.Result);
        response.EnterGroups.AddRange(enterGroupsTask.Result);
        response.GroupRequests.AddRange(groupRequestsTask.Result);
        response.GroupDeletes.AddRange(groupDeletesTask.Result);
        response.UserGroups.AddRange(userGroupsTask.Result);
        response.Response = new CommonResponse { State = true };

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
        using (var scope = serviceProvider.CreateScope())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var friendRelationRepository = unitOfWork.GetRepository<FriendRelation>();
            var relations = await friendRelationRepository.GetAllAsync(
                predicate: x => x.User1Id.Equals(userId),
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
    private async Task<IEnumerable<FriendRequestMessage>> GetFriendRequestMessages(string userId, DateTime offlineTime,DateTime lastDeleteTime)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var friendRequestRepository = unitOfWork.GetRepository<FriendRequest>();
            var requests = await friendRequestRepository.GetAllAsync(
                predicate: x => (x.UserTargetId.Equals(userId) || x.UserFromId.Equals(userId)) && (x.RequestTime > offlineTime && x.RequestTime > lastDeleteTime || x.IsSolved && x.SolveTime > offlineTime && x.SolveTime > lastDeleteTime),
                orderBy: x => x.OrderBy(d => d.RequestTime));

            return requests.Select(mapper.Map<FriendRequestMessage>);
        }
    }

    /// <summary>
    /// 找到离线时间后所有好友删除消息
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="offlinetTime"></param>
    /// <returns></returns>
    private async Task<IEnumerable<FriendDeleteMessage>> GetFriendDeleteMessage(string userId, DateTime offlinetTime, DateTime lastDeleteTime)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var friendDeleteRepository = unitOfWork.GetRepository<FriendDelete>();
            var deletes = await friendDeleteRepository.GetAllAsync(
                predicate: x => (x.UserId1.Equals(userId) || x.UserId2.Equals(userId)) && x.Time > offlinetTime && x.Time > lastDeleteTime,
                orderBy: x => x.OrderBy(d => d.Time));

            return deletes.Select(mapper.Map<FriendDeleteMessage>);
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
                predicate: d => d.UserId.Equals(userId) && d.JoinTime > offlineTime);

            return relations.Select(mapper.Map<EnterGroupMessage>);
        }
    }

    /// <summary>
    /// 获取离线时的申请入群请求和响应
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="offlineTime"></param>
    /// <returns></returns>
    private async Task<IEnumerable<GroupRequestMessage>> GetGroupRequestMessage(string userId, DateTime offlineTime, DateTime lastDeleteTime)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var groupService = scope.ServiceProvider.GetRequiredService<IGroupService>();
            var groupIds = await groupService.GetGroupsOfManager(userId);

            var groupRequestRepository = unitOfWork.GetRepository<GroupRequest>();
            var requests = await groupRequestRepository.GetAllAsync(
                predicate: x => (groupIds.Contains(x.GroupId) || x.UserFromId.Equals(userId)) && (x.RequestTime > offlineTime && x.RequestTime > lastDeleteTime || x.IsSolved && x.SolveTime > offlineTime && x.SolveTime > lastDeleteTime),
                orderBy: x => x.OrderBy(d => d.RequestTime));

            return requests.Select(mapper.Map<GroupRequestMessage>);
        }
    }

    /// <summary>
    /// 找到离线时间后所有好友删除消息
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="offlinetTime"></param>
    /// <returns></returns>
    private async Task<IEnumerable<GroupDeleteMessage>> GetGroupDeleteMessage(string userId, DateTime offlinetTime, DateTime lastDeleteTime)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var friendDeleteRepository = unitOfWork.GetRepository<GroupDelete>();
            var deletes = await friendDeleteRepository.GetAllAsync(
                predicate: x =>  x.MemberId.Equals(userId) && x.Time > offlinetTime && x.Time > lastDeleteTime,
                orderBy: x => x.OrderBy(d => d.Time));

            return deletes.Select(mapper.Map<GroupDeleteMessage>);
        }
    }

    /// <summary>
    /// 找到离线时间后所有好友删除消息
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="offlinetTime"></param>
    /// <returns></returns>
    private async Task<IEnumerable<UserGroupMessage>> GetUserGroupMessage(string userId, DateTime offlinetTime)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var userGroupRepository = unitOfWork.GetRepository<UserGroup>();
            var userGroups = await userGroupRepository.GetAllAsync(
                predicate: x => (x.UserId.Equals(userId)));

            return userGroups.Select(mapper.Map<UserGroupMessage>);
        }
    }
}
