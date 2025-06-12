using AutoMapper;
using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChatServer.Main.MessageOperate.Processor.GroupProcessor
{
    internal class GrouMemberListRequestProcessor : IProcessor<GroupMemberListRequest>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IUserService userService;
        private readonly IGroupService groupService;

        public GrouMemberListRequestProcessor(IUnitOfWork unitOfWork,
            IMapper mapper,
            IUserService userService,
            IGroupService groupService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.userService = userService;
            this.groupService = groupService;
        }

        public async Task Process(MessageUnit<GroupMemberListRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);
            var message = unit.Message;

            if (!await userService.IsUserExist(message.UserId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new GroupMemberListResponse
                    {
                        Response = new CommonResponse { State = false, Message = "用户不存在" }
                    });
                return;
            }

            if (!await groupService.IsGroupExist(message.GroupId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new GroupMemberListResponse
                    {
                        Response = new CommonResponse { State = false, Message = "群组不存在" }
                    });
                return;
            }

            var groupRelationRepository = unitOfWork.GetRepository<GroupRelation>();
            var entity = await groupRelationRepository.GetPagedListAsync(
                predicate: d => d.GroupId == message.GroupId,
                orderBy: o => o.OrderBy(d => d.UserId),
                pageIndex: message.PageIndex,
                pageSize: message.PageCount,
                include: i => i.Include(d => d.User));
        
            var lists = entity.Items.AsParallel().Select(d =>
            {
                var memberMessage = new GroupMemberMessage
                {
                    GroupId = d.GroupId,
                    JoinTime = d.JoinTime.ToString(),
                    LastSpeakTime = "",
                    Nickname = d.NickName ?? d.User.Name,
                    UserId = d.UserId,
                    Status = d.Status,
                    HeadIndex = d.User.HeadCount == 0 ? -1 : d.User.HeadIndex
                };
                return memberMessage;
            });

            foreach (var item in lists)
            {
                var lastSpeakTime = await groupService.MemberLastSpeakTime(item.UserId, item.GroupId);
                item.LastSpeakTime = lastSpeakTime.ToString();
            }

            var response = new GroupMemberListResponse
            {
                Response = new CommonResponse { State = true },
                GroupId = message.GroupId,
                UserId = message.UserId,
                PageIndex = entity.PageIndex,
                PageCount = entity.PageSize,
                HasNext = entity.HasNextPage,
                Members = { lists }
            };
            if (channel != null)
                await channel.WriteAndFlushProtobufAsync(response);
        }
    }
}
