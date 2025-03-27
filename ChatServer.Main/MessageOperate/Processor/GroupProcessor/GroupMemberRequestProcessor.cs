using AutoMapper.Execution;
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

namespace ChatServer.Main.MessageOperate.Processor.GroupProcessor
{
    internal class GroupMemberRequestProcessor : IProcessor<GroupMemberRequest>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IUserService userService;
        private readonly IGroupService groupService;

        public GroupMemberRequestProcessor(IUnitOfWork unitOfWork,
            IUserService userService,
            IGroupService groupService)
        {
            this.unitOfWork = unitOfWork;
            this.userService = userService;
            this.groupService = groupService;
        }

        public async Task Process(MessageUnit<GroupMemberRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);
            var message = unit.Message;

            var groupRelationRepository = unitOfWork.GetRepository<GroupRelation>();
            var groupRelation = await groupRelationRepository.GetFirstOrDefaultAsync(
                predicate: d => d.UserId.Equals(message.MemberId) && d.GroupId.Equals(message.GroupId));

            // 不存在relation，可能为次成员退出了群聊
            if (groupRelation == null)
            {
                var _user = await userService.GetUser(message.MemberId);
                var _memberMessage = new GroupMemberMessage
                {
                    GroupId = message.GroupId,
                    JoinTime = string.Empty,
                    LastSpeakTime = string.Empty,
                    Nickname = _user.Name,
                    UserId = _user.Id,
                    Status = 3,
                    HeadIndex = _user.HeadCount == 0 ? -1 : _user.HeadIndex
                };

                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(_memberMessage);
                return;
            }

            var lastSpeakTime = await groupService.MemberLastSpeakTime(message.MemberId, message.GroupId);
            var user = await userService.GetUser(message.MemberId);

            var memberMessage = new GroupMemberMessage
            {
                GroupId = groupRelation.GroupId,
                JoinTime = groupRelation.JoinTime.ToString(),
                LastSpeakTime = lastSpeakTime.ToString(),
                Nickname = groupRelation.NickName ?? user.Name,
                UserId = groupRelation.UserId,
                Status = groupRelation.Status,
                HeadIndex = user.HeadCount == 0 ? -1 : user.HeadIndex
            };

            if (channel != null)
                await channel.WriteAndFlushProtobufAsync(memberMessage);
        }
    }
}
