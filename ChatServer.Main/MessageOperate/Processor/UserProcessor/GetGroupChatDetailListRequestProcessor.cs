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

namespace ChatServer.Main.MessageOperate.Processor.UserProcessor
{
    class GetGroupChatDetailListRequestProcessor : IProcessor<GetGroupChatDetailListRequest>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IUserService userService;
        private readonly IMapper mapper;

        public GetGroupChatDetailListRequestProcessor(IUnitOfWork unitOfWork, IUserService userService, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.userService = userService;
            this.mapper = mapper;
        }

        public async Task Process(MessageUnit<GetGroupChatDetailListRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);
            var message = unit.Message;

            if (!await userService.IsUserExist(message.UserId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new GetGroupChatDetailListResponse
                    {
                        Response = new CommonResponse { State = false, Message = "用户不存在" }
                    });
                return;
            }

            try
            {
                var lastLoginTime = DateTime.Parse(message.LastLoginTime);
            
                var groupRepository = unitOfWork.GetRepository<ChatGroupDetail>();
                var chatList = await groupRepository.GetPagedListAsync(
                    predicate: d => d.UserId == message.UserId && d.Time > lastLoginTime,
                    orderBy: o => o.OrderByDescending(d => d.ChatGroupId).ThenBy(d => d.Time),
                    pageIndex: message.PageIndex,
                    pageSize: message.PageCount);

                var response = new GetGroupChatDetailListResponse
                {
                    Response = new CommonResponse { State = true, Message = "获取群聊详细记录成功" },
                    HasNext = chatList.HasNextPage,
                    PageCount = chatList.PageSize,
                    PageIndex = chatList.PageIndex,
                    UserId = message.UserId,
                    Messages = { mapper.Map<List<ChatGroupDetailMessage>>(chatList.Items) },
                };

                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(response);
            }
            catch
            {
                if(channel != null)
                    await channel.WriteAndFlushProtobufAsync(new GetGroupChatDetailListResponse
                    {
                        Response = new CommonResponse { State = false, Message = "获取群聊详细记录失败" }
                    });
            }

        }
    }
}
