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
    class RemoveMemberRequestProcessor : IProcessor<RemoveMemberRequest>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IUserService userService;
        private readonly IGroupService groupService;
        private readonly IClientChannelManager clientChannelManager;

        public RemoveMemberRequestProcessor(
            IUnitOfWork unitOfWork,
            IUserService userService,
            IGroupService groupService,
            IClientChannelManager clientChannelManager)
        {
            this.unitOfWork = unitOfWork;
            this.userService = userService;
            this.groupService = groupService;
            this.clientChannelManager = clientChannelManager;
        }

        public async Task Process(MessageUnit<RemoveMemberRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);
            var message = unit.Message;

            // 验证操作者、被移除成员和群组是否存在
            if (!await userService.IsUserExist(message.UserId) ||
                !await userService.IsUserExist(message.MemberId) ||
                !await groupService.IsGroupExist(message.GroupId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new RemoveMemberMessage
                    {
                        Response = new CommonResponse { State = false, Message = "用户或群组不存在" }
                    });
                return;
            }

            // 验证操作者是否为群主或管理员
            bool isOwner = await groupService.IsGroupOwner(message.UserId, message.GroupId);
            bool isManager = await groupService.IsGroupManager(message.UserId, message.GroupId);

            if (!isOwner && !isManager)
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new RemoveMemberMessage
                    {
                        Response = new CommonResponse { State = false, Message = "您没有权限执行此操作" }
                    });
                return;
            }

            // 验证被移除成员是否在群组中
            if (!await groupService.IsGroupMember(message.MemberId, message.GroupId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new RemoveMemberMessage
                    {
                        Response = new CommonResponse { State = false, Message = "该成员不在群组中" }
                    });
                return;
            }

            // 验证被移除成员不是群主
            if (await groupService.IsGroupOwner(message.MemberId, message.GroupId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new RemoveMemberMessage
                    {
                        Response = new CommonResponse { State = false, Message = "不能移除群主" }
                    });
                return;
            }

            // 验证管理员不能移除其他管理员
            if (isManager && !isOwner && await groupService.IsGroupManager(message.MemberId, message.GroupId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new RemoveMemberMessage
                    {
                        Response = new CommonResponse { State = false, Message = "管理员不能移除其他管理员" }
                    });
                return;
            }

            int deleteId = 0;
            DateTime time = DateTime.Now;
            try
            {
                // 从GroupRelation表中删除成员与群组的关系
                var groupRelationRepository = unitOfWork.GetRepository<GroupRelation>();
                var relation = await groupRelationRepository.GetFirstOrDefaultAsync(
                    predicate: x => x.UserId.Equals(message.MemberId) && x.GroupId.Equals(message.GroupId));

                if (relation != null)
                    groupRelationRepository.Delete(relation);

                // 添加移除记录到GroupDelete表
                var groupDeleteRepository = unitOfWork.GetRepository<GroupDelete>();
                var groupDelete = new GroupDelete
                {
                    GroupId = message.GroupId,
                    MemberId = message.MemberId,
                    DeleteMethod = 1, // 1表示被管理员移除
                    OperateUserId = message.UserId, // 操作者ID
                    Time = time
                };

                await groupDeleteRepository.InsertAsync(groupDelete);
                await unitOfWork.SaveChangesAsync();
                deleteId = groupDelete.Id;
            }
            catch (Exception ex)
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new RemoveMemberMessage
                    {
                        Response = new CommonResponse { State = false, Message = "移除成员失败，数据库错误" }
                    });
                return;
            }

            // 向操作者发送成功响应
            if (channel != null)
                await channel.WriteAndFlushProtobufAsync(new RemoveMemberMessage
                {
                    Response = new CommonResponse { State = true, Message = "成功移除群成员" },
                    UserId = message.UserId,
                    GroupId = message.GroupId,
                    MemberId = message.MemberId,
                    RemoveId = deleteId,
                    Time = time.ToInvariantString()
                });

            // 向被移除的成员发送通知
            var memberChannel = clientChannelManager.GetClient(message.MemberId);
            if (memberChannel != null)
            {
                await memberChannel.WriteAndFlushProtobufAsync(new RemoveMemberMessage
                {
                    Response = new CommonResponse { State = true, Message = "您已被移出群聊" },
                    UserId = message.UserId,
                    GroupId = message.GroupId,
                    MemberId = message.MemberId,
                    RemoveId = deleteId,
                    Time = time.ToInvariantString()
                });
            }

            // 向群内其他成员广播通知
            var groupMemeberRemovedMessage = new GroupMemeberRemovedMessage
            {
                GroupId = message.GroupId,
                MemberId = message.MemberId,
                Time = time.ToInvariantString()
            };

            var memberIds = await groupService.GetGroupMembers(message.GroupId);
            foreach (var memberId in memberIds.Where(id => id != message.MemberId))
            {
                var memberCh = clientChannelManager.GetClient(memberId);
                if (memberCh != null)
                    _ = memberCh.WriteAndFlushProtobufAsync(groupMemeberRemovedMessage);
            }
        }
    }
}
