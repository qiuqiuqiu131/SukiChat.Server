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
    internal class GrouMembersRequestProcessor : IProcessor<GroupMembersRequest>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IUserService userService;
        private readonly IGroupService groupService;

        public GrouMembersRequestProcessor(IUnitOfWork unitOfWork,
            IUserService userService,
            IGroupService groupService)
        {
            this.unitOfWork = unitOfWork;
            this.userService = userService;
            this.groupService = groupService;
        }

        public async Task Process(MessageUnit<GroupMembersRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);
            var message = unit.Message;

            // 验证是否为本群成员
            if(! await groupService.IsGroupMember(message.UserId,message.GroupId))
            {
                if(channel != null)
                {
                    await channel.WriteAndFlushProtobufAsync(new GroupMembersMessage
                    {
                        Response = new CommonResponse { State = false }
                    });
                }
                return; 
            }

            var groupRelationRepository = unitOfWork.GetRepository<GroupRelation>();
            var members = await groupRelationRepository.GetAllAsync(predicate:d => d.Group.Equals(message.GroupId));

            var response = new GroupMembersMessage
            {
                GroupId = message.GroupId
            };

            foreach (var member in members)
            {
                var lastSpeakTime = await groupService.MemberLastSpeakTime(member.UserId, member.GroupId);
                var user = await userService.GetUser(member.UserId);

                var memberMessage = new GroupMember
                {
                    GroupId = member.GroupId,
                    JoinTime = member.JoinTime.ToString(),
                    LastSpeakTime = lastSpeakTime.ToString(),
                    Nickname = member.NickName,
                    UserId = member.UserId,
                    Status = member.Status,
                    HeadIndex = user.HeadIndex
                };
                response.Members.Add(memberMessage);
            }

            if (channel != null)
                await channel.WriteAndFlushProtobufAsync(response);
        }
    }
}
