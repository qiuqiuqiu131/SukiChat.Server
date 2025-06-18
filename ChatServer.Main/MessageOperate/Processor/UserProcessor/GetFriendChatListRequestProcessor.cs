using AutoMapper;
using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Processor.UserProcessor
{
    class GetFriendChatListRequestProcessor : IProcessor<GetFriendChatListRequest>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IUserService userService;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public GetFriendChatListRequestProcessor(IUnitOfWork unitOfWork, IUserService userService, IMapper mapper, ILogger logger)
        {
            this.unitOfWork = unitOfWork;
            this.userService = userService;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task Process(MessageUnit<GetFriendChatListRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);
            var message = unit.Message;

            if (!await userService.IsUserExist(message.UserId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new GetFriendChatListResponse
                    {
                        Response = new CommonResponse { State = false, Message = "用户不存在" }
                    });
                return;
            }

            try
            {
                DateTime lastLoginTime = DateTime.Parse(message.LastLoginTime);

                var chatRepository = unitOfWork.GetRepository<ChatPrivate>();
                var chatList = await chatRepository.GetPagedListAsync(
                    predicate: d => (d.UserFromId == message.UserId || d.UserTargetId == message.UserId) && (d.Time > lastLoginTime || d.IsRetracted && d.RetractTime > lastLoginTime),
                    orderBy: o => o.OrderByDescending(d => d.Id).ThenBy(d => d.Time),
                    pageIndex: message.PageIndex,
                    pageSize: message.PageCount);

                var response = new GetFriendChatListResponse
                {
                    Response = new CommonResponse { State = true, Message = "获取好友聊天记录成功" },
                    HasNext = chatList.HasNextPage,
                    PageCount = chatList.PageSize,
                    PageIndex = chatList.PageIndex,
                    UserId = message.UserId,
                    Messages = { mapper.Map<List<FriendChatMessage>>(chatList.Items) }
                };

                logger.Information($"Friend Chat Message Request -> User Id: {response.UserId}, Page Index: {response.PageIndex}, Total Page: {chatList.TotalPages}, Total Count: {chatList.TotalCount}");

                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(response);
            }
            catch
            {
                if(channel != null)
                    await channel.WriteAndFlushProtobufAsync(new GetFriendChatListResponse
                    {
                        Response = new CommonResponse { State = false, Message = "获取好友聊天记录失败" }
                    });
            }
        }
    }
}
