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
    class GetGroupChatListRequestProcessor : IProcessor<GetGroupChatListRequest>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IUserService userService;
        private readonly IGroupService groupService;
        private readonly IMapper mapper;

        public GetGroupChatListRequestProcessor(IUnitOfWork unitOfWork, IUserService userService, IGroupService groupService, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.userService = userService;
            this.groupService = groupService;
            this.mapper = mapper;
        }

        public async Task Process(MessageUnit<GetGroupChatListRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);
            var message = unit.Message;

            if (!await userService.IsUserExist(message.UserId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new GetGroupChatListResponse
                    {
                        Response = new CommonResponse { State = false, Message = "用户不存在" }
                    });
                return;
            }

            try
            {
                DateTime lastLoginTime = DateTime.Parse(message.LastLoginTime);

                var groupList = await groupService.GetGroupsOfUser(message.UserId);

                var groupRepository = unitOfWork.GetRepository<ChatGroup>();
                var chatList = await groupRepository.GetPagedListAsync(
                    predicate: d => groupList.Contains(d.GroupId) && (d.Time > lastLoginTime || d.IsRetracted && d.RetractTime > lastLoginTime),
                    orderBy: o => o.OrderByDescending(d => d.Id).ThenBy(d => d.Time),
                    pageIndex: message.PageIndex,
                    pageSize: message.PageCount);

                var response = new GetGroupChatListResponse
                {
                    Response = new CommonResponse { State = true, Message = "获取群聊记录成功" },
                    HasNext = chatList.HasNextPage,
                    PageCount = chatList.PageSize,
                    PageIndex = chatList.PageIndex,
                    UserId = message.UserId,
                    Messages = { mapper.Map<List<GroupChatMessage>>(chatList.Items) }
                };

                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(response);
            }
            catch
            {
                if(channel != null)
                    await channel.WriteAndFlushProtobufAsync(new GetGroupChatListResponse
                    {
                        Response = new CommonResponse { State = false, Message = "获取群聊记录失败" }
                    });
            }
        }
    }
}
