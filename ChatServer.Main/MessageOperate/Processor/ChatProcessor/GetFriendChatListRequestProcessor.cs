using AutoMapper;
using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Processor.ChatProcessor
{
    class GetFriendChatListRequestProcessor : IProcessor<GetFriendChatListRequest>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IUserService userService;
        private readonly IMapper mapper;
        private readonly IFriendService friendService;

        public GetFriendChatListRequestProcessor(IUnitOfWork unitOfWork, IUserService userService, IMapper mapper, IFriendService friendService)
        {
            this.unitOfWork = unitOfWork;
            this.userService = userService;
            this.mapper = mapper;
            this.friendService = friendService;
        }

        public async Task Process(MessageUnit<GetFriendChatListRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);
            var message = unit.Message;

            if (!await userService.IsUserExist(message.UserId))
            {
                if (channel != null)
                {
                    await channel.WriteAndFlushProtobufAsync(new GetFriendChatListResponse
                    {
                        Response = new CommonResponse { State = false, Message = "非法用户访问" }
                    });
                    return;
                }
            }

            if (!await friendService.IsFriend(message.UserId, message.FriendId))
            {
                if (channel != null)
                {
                    await channel.WriteAndFlushProtobufAsync(new GetFriendChatListResponse
                    {
                        Response = new CommonResponse { State = false, Message = "非好友关系，无权获取聊天记录" }
                    });
                    return;
                }
            }

            try
            {
                DateTime lastLoginTime = DateTime.Parse(message.LastLoginTime);

                var chatPirvateRepository = unitOfWork.GetRepository<ChatPrivate>();
                var chatList = await chatPirvateRepository.GetPagedListAsync(
                    predicate: d => ((d.UserFromId == message.UserId && d.UserTargetId == message.FriendId) || (d.UserFromId == message.FriendId && d.UserTargetId == message.UserId)) && (d.Time > lastLoginTime || (d.IsRetracted && d.RetractTime > lastLoginTime)),
                    orderBy: d => d.OrderByDescending(o => o.Time),
                    pageIndex: message.PageIndex,
                    pageSize: message.PageCount);

                var response = new GetFriendChatListResponse
                {
                    Response = new CommonResponse { State = true },
                    UserId = message.UserId,
                    FriendId = message.FriendId,
                    PageCount = chatList.PageSize,
                    PageIndex = chatList.PageIndex,
                    HasNext = chatList.HasNextPage,
                    Messages = { mapper.Map<FriendChatMessage>(chatList.Items) }
                };

                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(response);
            }
            catch (Exception ex)
            {
                if (channel != null)
                {
                    await channel.WriteAndFlushProtobufAsync(new GetFriendChatListResponse
                    {
                        Response = new CommonResponse { State = false, Message = "获取聊天记录失败: " + ex.Message }
                    });
                }
            }
        }
    }
}
