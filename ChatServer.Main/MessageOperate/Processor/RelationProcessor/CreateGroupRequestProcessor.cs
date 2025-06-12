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
using ChatServer.Main.Services.Helper;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.DependencyInjection;

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
        private readonly IFriendService friendService;
        private readonly IUserService userService;
        private readonly IClientChannelManager channelManager;
        private readonly IIdGeneratorManager idGeneratorManager;

        public CreateGroupRequestProcessor(
            IUnitOfWork unitOfWork,
            IServiceProvider serviceProvider,
            IFriendService friendService,
            IUserService userService,
            IClientChannelManager channelManager,
            IIdGeneratorManager idGeneratorManager)
        {
            this.unitOfWork = unitOfWork;
            this.friendService = friendService;
            this.userService = userService;
            this.channelManager = channelManager;
            this.idGeneratorManager = idGeneratorManager;
        }

        public async Task Process(MessageUnit<CreateGroupRequest> unit)
        {
            unit.Channel.TryGetTarget(out IChannel? channel);

            var message = unit.Message;

            // 验证用户是否存在
            var isUser = await userService.IsUserExist(message.UserId);
            if (!isUser)
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
            var rand = new Random();

            // 使用IdGeneratorService生成群组ID
            string groupId = idGeneratorManager.GenerateGroupId();
            var group = new Group
            {
                Id = groupId,
                CreateTime = DateTime.Now,
                HeadIndex = rand.Next(1,10)
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
            var user = await userService.GetUser(message.UserId);
            groupName += user.Name;

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
                    var friend = await userService.GetUser(friendId);
                    groupName += ",";
                    groupName += friend.Name;
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
                var friendChannel = channelManager.GetClient(friendRelation.UserId);
                if(friendChannel == null) continue;
                await friendChannel.WriteAndFlushProtobufAsync(new PullGroupMessage
                {
                    GroupId = friendRelation.GroupId,
                    UserIdFrom = message.UserId,
                    UserIdTarget = friendRelation.UserId,
                    Grouping = "默认分组",
                    Time = group.CreateTime.ToString(),
                    Status = 2
                });
            }


            // 发送被拉入群的系统消息， TODO： 可批量化处理
            List<GroupChatMessage> messages = new List<GroupChatMessage>();
            foreach (var friendId in message.FriendId)
            {
                // 构建消息并保存
                var friend = await userService.GetUser(friendId);
                var chatMessage = new GroupChatMessage
                {
                    GroupId = groupId,
                    UserFromId = "System",
                    Time = group.CreateTime.ToString(),
                };

                chatMessage.Messages.Add(new ChatMessage
                {
                    SystemMessage = new SystemMessage
                    {
                        Blocks =
                        {
                            new SystemMessageBlock{Text = user.Name,Bold = true},
                            new SystemMessageBlock{Text = "邀请"},
                            new SystemMessageBlock{Text = friend.Name,Bold = true},
                            new SystemMessageBlock{Text = "加入群聊"}
                        }
                    }
                });

                // 保存到数据库
                ChatGroup chatGroup = new ChatGroup
                {
                    UserFromId = "System",
                    GroupId = groupId,
                    Message = ChatMessageHelper.EncruptChatMessage(chatMessage.Messages),
                    Time = group.CreateTime,
                };

                try
                {
                    var respository = unitOfWork.GetRepository<ChatGroup>();
                    await respository.InsertAsync(chatGroup);
                    await unitOfWork.SaveChangesAsync();
                }
                catch { continue; }

                chatMessage.Id = chatGroup.Id;

                messages.Add(chatMessage);
            }

            await Task.Delay(500);

            // 构建群聊消息列表
            var chatMessageList = new GroupChatMessageList
            {
                GroupId = groupId,
                Messages = { messages },
                Time = DateTime.Now.ToString()
            };

            // 发送消息
            if(channel != null)
            {
                chatMessageList.UserId = message.UserId;
                await channel.WriteAndFlushProtobufAsync(chatMessageList);
            }
            foreach (var friendId in message.FriendId)
            {
                var friendChannel = channelManager.GetClient(friendId);
                if (friendChannel == null) continue;
                
                chatMessageList.UserId = friendId;
                await friendChannel.WriteAndFlushProtobufAsync(chatMessageList);
            }
        }
    }
}