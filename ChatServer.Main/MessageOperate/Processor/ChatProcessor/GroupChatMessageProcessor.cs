using ChatServer.Common;
using ChatServer.Common.Helper;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.IOServer.Manager;
using ChatServer.Main.Services;
using ChatServer.Main.Services.Helper;
using DotNetty.Transport.Channels;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Processor.ChatProcessor
{
    /// <summary>
    /// 处理目标：
    /// GroupChatMessage 群聊消息
    /// 
    /// 数据库操作：
    /// 1、ChatGroup 将群聊消息保存到数据库
    /// 
    /// 需要发送的消息：
    /// 1、GroupChatMessage (To:群成员) 将聊天消息发送给群所有在线成员（除发送者）
    /// 2、GroupChatMessageResponse (To:发送方) 用于通知发送方消息是否发送成功
    /// </summary>
    public class GroupChatMessageProcessor : IProcessor<GroupChatMessage>
    {
        private readonly IClientChannelManager channelManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly IGroupService groupService;
        private readonly ILogger logger;

        public GroupChatMessageProcessor(
            IClientChannelManager channelManager,
            IUnitOfWork unitOfWork,
            IGroupService groupService,
            ILogger logger)
        {
            this.channelManager = channelManager;
            this.unitOfWork = unitOfWork;
            this.groupService = groupService;
            this.logger = logger;
        }

        public async Task Process(MessageUnit<GroupChatMessage> unit)
        {
            unit.Channel.TryGetTarget(out IChannel? channel);

            // 发送过来的GroupChatMessage不需要有Time属性，以服务器接收到的时间为准。
            var time = DateTime.Now;
            var message = unit.Message;

            // 检查群组是否存在
            if (!await groupService.IsGroupExist(message.GroupId))
            {
                if (channel != null)
                {
                    await channel.WriteAndFlushProtobufAsync(new GroupChatMessageResponse
                    {
                        Response = new CommonResponse { State = false, Message = "群组不存在" }
                    });
                }
                return;
            }

            // 检查发送者是否为群成员
            if (!await groupService.IsGroupMember(message.UserFromId, message.GroupId))
            {
                if (channel != null)
                {
                    await channel.WriteAndFlushProtobufAsync(new GroupChatMessageResponse
                    {
                        Response = new CommonResponse { State = false, Message = "您不是该群成员" }
                    });
                }
                return;
            }

            ChatGroup chatGroup = new ChatGroup
            {
                UserFromId = message.UserFromId,
                GroupId = message.GroupId,
                Time = time,
                Message = ChatMessageHelper.EncruptChatMessage(message.Messages)
            };

            //-- 操作：保存消息到数据库 --//
            try
            {
                var repository = unitOfWork.GetRepository<ChatGroup>();
                await repository.InsertAsync(chatGroup);

                var relationRepository = unitOfWork.GetRepository<GroupRelation>();
                var relstions = await relationRepository.GetAllAsync(predicate:d => d.GroupId.Equals(message.GroupId),disableTracking:false);
                foreach (var rel in relstions) 
                    rel.IsChatting = true;

                await unitOfWork.SaveChangesAsync();
            }
            catch
            {
                // 如果数据库操作失败，返回发送失败
                if (channel != null)
                {
                    await channel.WriteAndFlushProtobufAsync(new GroupChatMessageResponse
                    {
                        Response = new CommonResponse { State = false, Message = "服务器出错" }
                    });
                }
                return;
            }

            message.Time = time.ToInvariantString();
            message.Id = chatGroup.Id;

            //-- 操作：成功保存消息，返回发送成功 --//
            if (channel != null)
            {
                await channel.WriteAndFlushProtobufAsync(new GroupChatMessageResponse
                {
                    Response = new CommonResponse { State = true, Message = "发送成功" },
                    Time = time.ToInvariantString(),
                    Id = chatGroup.Id
                });
            }

            //-- 操作：获取群成员列表，将消息发送给所有在线成员（除了发送者） --//
            var groupMembers = await groupService.GetGroupMembers(message.GroupId);
            if (groupMembers != null && groupMembers.Any())
            {
                foreach (var member in groupMembers)
                {
                    // 不给发送者再发送一份
                    if (member.Equals(message.UserFromId))
                        continue;

                    var memberChannel = channelManager.GetClient(member);
                    if (memberChannel != null)
                    {
                        await memberChannel.WriteAndFlushProtobufAsync(message);
                    }
                }
            }
        }
    }
}
