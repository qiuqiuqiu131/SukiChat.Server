using ChatServer.Common;
using ChatServer.Common.Helper;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.IOServer.Manager;
using ChatServer.Main.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Processor.RelationProcessor
{
    class QuitGroupRequestProcessor : IProcessor<QuitGroupRequest>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IUserService userService;
        private readonly IGroupService groupService;
        private readonly IClientChannelManager clientChannelManager;

        public QuitGroupRequestProcessor(IUnitOfWork unitOfWork,
            IUserService userService,
            IGroupService groupService,
            IClientChannelManager clientChannelManager)
        {
            this.unitOfWork = unitOfWork;
            this.userService = userService;
            this.groupService = groupService;
            this.clientChannelManager = clientChannelManager;
        }

        public async Task Process(MessageUnit<QuitGroupRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);
            var message = unit.Message;

            // 验证用户和群组是否存在
            if (!await userService.IsUserExist(message.UserId) ||
                !await groupService.IsGroupExist(message.GroupId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new QuitGroupMessage
                    {
                        Response = new CommonResponse { State = false, Message = "用户或群组不存在" }
                    });
                return;
            }

            // 验证用户是否在群组中
            if (!await groupService.IsGroupMember(message.UserId, message.GroupId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new QuitGroupMessage
                    {
                        Response = new CommonResponse { State = false, Message = "您不在该群组中" }
                    });
                return;
            }

            int deleteId = 0;
            DateTime time = DateTime.Now;

            try
            {
                // 从GroupRelation表中删除用户与群组的关系
                var groupRelationRepository = unitOfWork.GetRepository<GroupRelation>();
                var relation = await groupRelationRepository.GetFirstOrDefaultAsync(
                    predicate: x => x.UserId.Equals(message.UserId) && x.GroupId.Equals(message.GroupId));

                if (relation != null)
                    groupRelationRepository.Delete(relation);

                // 添加退出记录到GroupDelete表
                var groupDeleteRepository = unitOfWork.GetRepository<GroupDelete>();
                var groupDelete = new GroupDelete
                {
                    GroupId = message.GroupId,
                    MemberId = message.UserId,
                    DeleteMethod = 0, // 成员主动退出
                    OperateUserId = message.UserId, // 操作者就是退出的用户自己
                    Time = time
                };

                await groupDeleteRepository.InsertAsync(groupDelete);
                await unitOfWork.SaveChangesAsync();
                deleteId = groupDelete.Id;
            }
            catch (Exception ex)
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new QuitGroupMessage
                    {
                        Response = new CommonResponse { State = false, Message = "退出群组失败，数据库错误" }
                    });
                return;
            }

            // 向退出的用户发送成功响应
            if (channel != null)
                await channel.WriteAndFlushProtobufAsync(new QuitGroupMessage
                {
                    Response = new CommonResponse { State = true, Message = "成功退出群组" },
                    UserId = message.UserId,
                    GroupId = message.GroupId,
                    QuitId = deleteId,
                    Time = time.ToInvariantString()
                });

            var memberRemoveRequest = new GroupMemeberRemovedMessage
            {
                GroupId = message.GroupId,
                MemberId = message.UserId,
                Time = DateTime.Now.ToString()
            };

            var groupMemberIds = await groupService.GetGroupMembers(message.GroupId);
            foreach (var member in groupMemberIds)
            {
                if (member.Equals(message.UserId)) continue;
                var client = clientChannelManager.GetClient(member);
                if(client != null)
                    _ = client.WriteAndFlushProtobufAsync(memberRemoveRequest);
            }
        }
    }
}
