using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.Main.Entity;
using ChatServer.Common;
using ChatServer.Main.IOServer.Manager;
using AutoMapper;
using DotNetty.Transport.Channels;
using ChatServer.Main.Services;

namespace ChatServer.Main.MessageOperate.Processor.RelationProcessor;

/// <summary>
/// 处理目标：
/// FriendRequestFromClient 来自某个客户的对另一个客户的好友请求
/// 
/// 数据库操作：
/// 1、FriendRequest 将好友请求保存到数据库
/// 
/// 需要发送的消息: 
/// 1、FriendRequestFromClientResponse (To:发送方) 用于通知发送方好友请求是否发送成功
/// 2、FriendRequestFromServer (To:请求方) 用于通知请求方有人向他发送了好友请求
/// </summary>
public class FriendRequestProcessor : IProcessor<FriendRequestFromClient>
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IUserService userService;
    private readonly IFriendService friendService;
    private readonly IClientChannelManager channelManager;
    private readonly IMapper mapper;

    public FriendRequestProcessor(IUnitOfWork unitOfWork,
        IUserService userService,
        IFriendService friendService,
        IClientChannelManager channelManager,
        IMapper mapper)
    {
        this.unitOfWork = unitOfWork;
        this.userService = userService;
        this.friendService = friendService;
        this.channelManager = channelManager;
        this.mapper = mapper;
    }

    public async Task Process(MessageUnit<FriendRequestFromClient> unit)
    {
        unit.Channel.TryGetTarget(out IChannel? channel);
        var message = unit.Message;



        //-- 判断：不是用户
        if(!await userService.IsUserExist(unit.Message.UserTargetId) 
            || !await userService.IsUserExist(unit.Message.UserFromId))
        {
            if (channel != null)
                await channel.WriteAndFlushProtobufAsync(new FriendRequestFromClientResponse
                {
                    Response = new CommonResponse { State = false, Message = "消息源错误" }
                });
            return;
        }

        //-- 判断：不能添加自己为好友 --//
        if (message.UserTargetId == message.UserFromId)
        {
            if (channel != null)
                await channel.WriteAndFlushProtobufAsync(new FriendRequestFromClientResponse
                {
                    Response = new CommonResponse { State = false, Message = "不能添加自己为好友" }
                });
            return;
        }

        //-- 判断：不能向已经成为好友的对象发送好友请求 --//
        if (await friendService.IsFriend(message.UserTargetId, message.UserFromId))
        {
            if (channel != null)
                await channel.WriteAndFlushProtobufAsync(new FriendRequestFromClientResponse
                {
                    Response = new CommonResponse { State = false, Message = "你们已经是好友了" }
                });
            return;
        }


        var request = mapper.Map<FriendRequest>(unit.Message);

        try
        {
            //-- 执行：将好友请求保存到数据库 --//
            var repository = unitOfWork.GetRepository<FriendRequest>();

            // 如果存在相同的好友请求，则覆盖之前相同的好友请求
            var entity = await repository.GetFirstOrDefaultAsync(
                predicate: d => d.UserFromId == message.UserFromId && d.UserTargetId == message.UserTargetId && !d.IsSolved,
                orderBy: o => o.OrderByDescending(d => d.RequestTime), disableTracking: true);
            if (entity != null)
                request.Id = entity.Id;

            repository.Update(request);
            await unitOfWork.SaveChangesAsync();
        }
        catch(Exception e)
        {
            if (channel != null)
                await channel.WriteAndFlushProtobufAsync(new FriendRequestFromClientResponse
                {
                    Response = new CommonResponse { State = false, Message = "服务器繁忙" }
                });
            return;
        }


        //-- 执行：发送好友请求成功的响应 To：发送方 --//
        if (channel != null)
            await channel.WriteAndFlushProtobufAsync(new FriendRequestFromClientResponse
            {
                Response = new CommonResponse { State = true, Message = "添加好友请求发送成功" },
                RequestId = request.Id,
                RequestTime = request.RequestTime.ToString(),
                Request = message
            });


        //-- 执行：发送好友请求 To：请求方 --//
        var targetOnline = channelManager.GetClient(unit.Message.UserTargetId);
        if (targetOnline == null) return;
        FriendRequestFromServer requestMess = mapper.Map<FriendRequestFromServer>(request);
        await targetOnline.WriteAndFlushProtobufAsync(requestMess);
    }
}
