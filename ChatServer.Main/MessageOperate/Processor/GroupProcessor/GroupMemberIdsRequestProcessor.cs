using ChatServer.Common;
using ChatServer.Common.Protobuf;
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
    internal class GroupMemberIdsRequestProcessor : IProcessor<GroupMemberIdsRequest>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IUserService userService;
        private readonly IGroupService groupService;

        public GroupMemberIdsRequestProcessor(IUnitOfWork unitOfWork,
            IUserService userService,
            IGroupService groupService)
        {
            this.unitOfWork = unitOfWork;
            this.userService = userService;
            this.groupService = groupService;
        }

        public async Task Process(MessageUnit<GroupMemberIdsRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);
            var message = unit.Message;

            // 验证是否为本群成员
            if (!await groupService.IsGroupMember(message.UserId, message.GroupId))
            {
                if (channel != null)
                {
                    await channel.WriteAndFlushProtobufAsync(new GroupMemberIds { GroupId = message.GroupId });
                }
                return;
            }

            var memberIds = await groupService.GetGroupMembers(message.GroupId);
            GroupMemberIds groupMemberIds = new GroupMemberIds { GroupId = message.GroupId };
            groupMemberIds.MemberIds.AddRange(memberIds);

            if (channel != null)
                await channel.WriteAndFlushProtobufAsync(groupMemberIds);
        }
    }
}
