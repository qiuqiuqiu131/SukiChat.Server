using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.IOServer.Manager;
using ChatServer.Main.Manager;
using ChatServer.Main.Services;
using DotNetty.Transport.Channels;

namespace ChatServer.Main.MessageOperate.Processor.RelationProcessor
{
    /// <summary>
    /// 处理目标：
    /// CreateGroupRequest 创建群组请求
    /// 
    /// 数据库操作：
    /// 1、Group 创建群组
    /// 2、GroupRelation 保存群主关系
    /// 
    /// 需要发送的消息：
    /// 1、CreateGroupResponse (To:请求者) 通知请求者群组是否创建成功
    /// </summary>
    public class CreateGroupRequestProcessor : IProcessor<CreateGroupRequest>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IUserService userService;
        private readonly IFriendService friendService;
        private readonly IClientChannelManager channelManager;
        private readonly IIdGeneratorManager idGeneratorManager;

        public CreateGroupRequestProcessor(
            IUnitOfWork unitOfWork,
            IUserService userService,
            IFriendService friendService,
            IClientChannelManager channelManager,
            IIdGeneratorManager idGeneratorManager)
        {
            this.unitOfWork = unitOfWork;
            this.userService = userService;
            this.friendService = friendService;
            this.channelManager = channelManager;
            this.idGeneratorManager = idGeneratorManager;
        }

        public async Task Process(MessageUnit<CreateGroupRequest> unit)
        {
            unit.Channel.TryGetTarget(out IChannel? channel);

            var message = unit.Message;

            // 验证用户是否存在
            var isUser = userService.IsUserExist(message.UserId);
            if (isUser == null)
            {
                if (channel != null)
                {
                    await channel.WriteAndFlushProtobufAsync(new CreateGroupResponse
                    {
                        Response = new CommonResponse { State = false, Message = "用户不存在" }
                    });
                }
                return;
            }

            #region 生成Entity
            // 使用IdGeneratorService生成群组ID
            string groupId = idGeneratorManager.GenerateGroupId();
            var group = new Group
            {
                Id = groupId,
                CreateTime = DateTime.Now,
                HeadPath = "-1"
            };

            string groupName = string.Empty;

            // 添加群主关系
            var groupRelation = new GroupRelation
            {
                GroupId = groupId,
                JoinTime = group.CreateTime,
                Status = 0,
                UserId = message.UserId,
                Grouping = "默认分组"
            };
            groupName += (await userService.GetUser(message.UserId)).Name;

            // 添加组员关系
            var friendRelations = new List<GroupRelation>();
            foreach(string friendId in message.FriendId)
            {
                if(await friendService.IsFriend(message.UserId,friendId))
                {
                    friendRelations.Add(new GroupRelation
                    {
                        GroupId = groupId,
                        JoinTime = group.CreateTime,
                        Grouping = "默认分组",
                        Status = 2,
                        UserId = friendId
                    });
                    groupName += ",";
                    groupName += (await userService.GetUser(friendId)).Name;
                }
            }

            if(groupName.Length > 16)
            {
                groupName = groupName.Substring(0, 14);
                groupName += "..";
            }
            group.Name = groupName;
            #endregion

            // 数据库保存
            try
            {
                var groupRepository = unitOfWork.GetRepository<Group>();
                await groupRepository.InsertAsync(group);
                await unitOfWork.SaveChangesAsync();
                var groupRelationRepository = unitOfWork.GetRepository<GroupRelation>();
                groupRelationRepository.Update(groupRelation);
                foreach (var friendRelation in friendRelations)
                    groupRelationRepository.Update(friendRelation);
                await unitOfWork.SaveChangesAsync();
            }
            catch
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new CreateGroupResponse
                    {
                        Response = new CommonResponse { State = false },
                    });
                return;
            }

            // 数据库存入成功
            // 通知建群者
            if (channel != null)
            {
                await channel.WriteAndFlushProtobufAsync(new CreateGroupResponse
                {
                    Response = new CommonResponse { State = true },
                    GroupName = groupName,
                    GroupId = groupId,
                    Time = group.CreateTime.ToString()
                });
            }

            // 通知被拉入群者
            foreach (var friendRelation in friendRelations)
            {
                var friendChannel = channelManager.GetClient(friendRelation.GroupId);
                if(friendChannel == null) continue;
                _ = friendChannel.WriteAndFlushProtobufAsync(new PullGroupMessage
                {
                    GroupId = friendRelation.GroupId,
                    UserIdFrom = message.UserId,
                    UserIdTarget = friendRelation.UserId,
                    Grouping = "默认分组",
                    Time = group.CreateTime.ToString(),
                    Status = 2
                });
            }
        }
    }
}