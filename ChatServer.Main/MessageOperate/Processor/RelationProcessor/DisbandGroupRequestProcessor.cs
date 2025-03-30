
using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.IOServer.Manager;
using ChatServer.Main.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Processor.RelationProcessor
{
    class DisbandGroupRequestProcessor : IProcessor<DisbandGroupRequest>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IClientChannelManager clientChannelManager;
        private readonly IUserService userService;
        private readonly IGroupService groupService;

        public DisbandGroupRequestProcessor(IUnitOfWork unitOfWork,
            IClientChannelManager clientChannelManager,
            IUserService userService,
            IGroupService groupService)
        {
            this.unitOfWork = unitOfWork;
            this.clientChannelManager = clientChannelManager;
            this.userService = userService;
            this.groupService = groupService;
        }

        public async Task Process(MessageUnit<DisbandGroupRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);
            var message = unit.Message;

            // 验证用户和群组是否存在
            if (!await userService.IsUserExist(message.UserId) ||
                !await groupService.IsGroupExist(message.GroupId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new DisbandGroupMessage
                    {
                        Response = new CommonResponse { State = false, Message = "用户或群组不存在" }
                    });
                return;
            }

            // 使用GroupService的IsGroupOwner验证用户是否为群主
            if (!await groupService.IsGroupOwner(message.UserId, message.GroupId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new DisbandGroupMessage
                    {
                        Response = new CommonResponse { State = false, Message = "只有群主可以解散群聊" }
                    });
                return;
            }

            // 获取群成员列表，用于后续通知
            List<string> memberIds = await groupService.GetGroupMembers(message.GroupId);
            Dictionary<string, int> memberDeleteIds = new Dictionary<string, int>();
            DateTime time = DateTime.Now;
            try
            {
                // 从GroupRelation表中删除所有群组关系
                var groupRelationRepository = unitOfWork.GetRepository<GroupRelation>();

                await groupRelationRepository.GetAll(predicate: d => d.GroupId.Equals(message.GroupId)).ExecuteDeleteAsync();

                // 为每个成员添加GroupDelete记录并跟踪其ID
                var groupDeleteRepository = unitOfWork.GetRepository<GroupDelete>();

                foreach (var memberId in memberIds)
                {
                    var groupDelete = new GroupDelete
                    {
                        GroupId = message.GroupId,
                        MemberId = memberId,
                        DeleteMethod = 2, // 群主解散群聊
                        OperateUserId = message.UserId, // 操作者是群主
                        Time = time
                    };

                    await groupDeleteRepository.InsertAsync(groupDelete);

                    // 将实体状态更改为Detached，并存储ID以供后续使用
                    memberDeleteIds[memberId] = groupDelete.Id;
                }

                var groupReposotory = unitOfWork.GetRepository<Group>();
                var group = await groupReposotory.GetFirstOrDefaultAsync(predicate:d => d.Id.Equals(message.GroupId),disableTracking:false);
                group.IsDisband = true;

                await unitOfWork.SaveChangesAsync(); // 每次插入后保存，以获取ID
            }
            catch (Exception ex)
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new DisbandGroupMessage
                    {
                        Response = new CommonResponse { State = false, Message = "解散群组失败，数据库错误" }
                    });
                return;
            }

            // 向群主发送解散成功响应
            if (channel != null && memberDeleteIds.ContainsKey(message.UserId))
            {
                await channel.WriteAndFlushProtobufAsync(new DisbandGroupMessage
                {
                    Response = new CommonResponse { State = true, Message = "成功解散群组" },
                    UserId = message.UserId,
                    GroupId = message.GroupId,
                    MemberId = message.UserId,
                    DisBandId = memberDeleteIds[message.UserId],
                    Time = time.ToString()
                });
            }

            // 向所有群成员发送通知，每个成员使用自己的deleteId
            try
            {
                foreach (var memberId in memberIds)
                {
                    if (memberId != message.UserId) // 不发给群主自己
                    {
                        var memberChannel = clientChannelManager.GetClient(memberId);
                        if (memberChannel != null && memberDeleteIds.ContainsKey(memberId))
                        {
                            await memberChannel.WriteAndFlushProtobufAsync(new DisbandGroupMessage
                            {
                                Response = new CommonResponse { State = true, Message = "您所在的群组已被群主解散" },
                                UserId = message.UserId, // 群主ID
                                GroupId = message.GroupId,
                                MemberId = memberId,
                                DisBandId = memberDeleteIds[memberId], // 使用该成员对应的删除记录ID
                                Time = time.ToString()
                            });
                        }
                    }
                }
            }
            catch
            {
                // 通知成员失败不影响主流程
            }
        }
    }
}
