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
    class GroupChatMessageGroupProcessor(IUnitOfWork unitOfWork, 
        IUserService userService, IGroupService groupService, IMapper mapper) : IProcessor<GetGroupChatListRequest>
    {
        public async Task Process(MessageUnit<GetGroupChatListRequest> unit)
        {
            var message = unit.Message;
            unit.Channel.TryGetTarget(out var channel);
            if (channel == null) return;

            if (await userService.IsUserExist(message.UserId))
            {
                await channel.WriteAndFlushProtobufAsync(new GetGroupChatListResponse
                {
                    Response = new CommonResponse { State = false, Message = "非法用户访问" }
                });
                return;
            }

            if(!await groupService.IsGroupExist(message.GroupId) || !await groupService.IsGroupMember(message.UserId, message.GroupId))
            {
                await channel.WriteAndFlushProtobufAsync(new GetGroupChatListResponse
                {
                    Response = new CommonResponse { State = false, Message = "非此群聊成员，无权获取聊天记录" }
                });
                return;
            }

            DateTime lastLoginTime = DateTime.Parse(message.LastLoginTime);

            var repository = unitOfWork.GetRepository<ChatGroup>();
            var res = await repository.GetPagedListAsync(
                predicate: d => d.GroupId == message.GroupId && (d.Time < lastLoginTime || d.RetractTime < lastLoginTime),
                orderBy: d => d.OrderByDescending(o => o.Time),
                pageIndex: message.PageIndex,
                pageSize: message.PageCount);
            var response = new GetGroupChatListResponse
            {
                Response = new CommonResponse { State = true },
                UserId = message.UserId,
                GroupId = message.GroupId,
                PageCount = res.PageSize,
                PageIndex = res.PageIndex,
                HasNext = res.HasNextPage
            };
            response.Messages.AddRange(mapper.Map<List<GroupChatMessage>>(res.Items));

            await channel.WriteAndFlushProtobufAsync(response);
        }
    }
}
