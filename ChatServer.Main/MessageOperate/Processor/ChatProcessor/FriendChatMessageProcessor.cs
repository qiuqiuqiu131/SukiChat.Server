using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.IOServer.Manager;
using ChatServer.Main.Services.Helper;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Processor.ChatProcessor
{
    /// <summary>
    /// 处理目标：
    /// FriendChatMessage 好友之间的聊天消息
    /// 
    /// 需要发送的消息：
    /// 1、FriendChatMessage (To:接受方) 将聊天消息发送给接受方
    /// 2、FriendChatMessageResponse (To:发送方) 用于通知发送方消息是否发送成功
    /// </summary>
    public class FriendChatMessageProcessor : IProcessor<FriendChatMessage>
    {
        private readonly IClientChannelManager channelManager;
        private readonly IUnitOfWork unitOfWork;

        public FriendChatMessageProcessor(IClientChannelManager channelManager,
            IUnitOfWork unitOfWork)
        {
            this.channelManager = channelManager;
            this.unitOfWork = unitOfWork;
        }

        public async Task Process(MessageUnit<FriendChatMessage> unit)
        {
            unit.Channel.TryGetTarget(out IChannel? channel);

            // 发送过来的FriendChatMessage不需要有Time属性，以服务器接受到的时间为准。
            var time = DateTime.Now;

            var message = unit.Message;
            ChatPrivate chatPrivate = new ChatPrivate
            {
                UserFromId = message.UserFromId,
                UserTargetId = message.UserTargetId,
                Time = time,
                Message = ChatMessageHelper.EncruptChatMessage(message.Messages)
            };

            //-- 操作：保存消息到数据库 --//
            try
            {
                var repository = unitOfWork.GetRepository<ChatPrivate>();
                repository.Update(chatPrivate);

                var relationRepository = unitOfWork.GetRepository<FriendRelation>();
                var relation1 = await relationRepository.GetFirstOrDefaultAsync(predicate:d => d.User1Id.Equals(message.UserFromId) && d.User2Id.Equals(message.UserTargetId),disableTracking:false);
                if(relation1 != null)
                    relation1.IsChatting = true;

                var relation2 = await relationRepository.GetFirstOrDefaultAsync(predicate:d => d.User1Id.Equals(message.UserTargetId) && d.User2Id.Equals(message.UserFromId),disableTracking:false);
                if(relation2 != null)
                    relation2.IsChatting = true;

                await unitOfWork.SaveChangesAsync();
            }
            catch
            {
                // 如果数据库操作失败，返回发送失败
                if (channel != null)
                {
                    await channel.WriteAndFlushProtobufAsync(new FriendChatMessageResponse
                    {
                        Response = new CommonResponse { State = false, Message = "服务器出错" }
                    });
                }
                return;
            }

            message.Time = time.ToString();
            message.Id = chatPrivate.Id;

            //-- 操作：成功保存消息，返回发送成功 --//
            if (channel != null)
            {
                await channel.WriteAndFlushProtobufAsync(new FriendChatMessageResponse
                {
                    Response = new CommonResponse { State = true, Message = "发送成功" },
                    Time = time.ToString(),
                    Id = chatPrivate.Id
                });
            }

            //-- 操作：将消息发送给接受方 --//
            var targetChannel = channelManager.GetClient(chatPrivate.UserTargetId);
            if (targetChannel != null)
            {
                await targetChannel.WriteAndFlushProtobufAsync(message);
            }

            // 如果为语音消息，在发送给发送方
            if(message.Messages.Count == 1 && message.Messages[0].ContentCase == ChatMessage.ContentOneofCase.CallMess)
            {
                await channel.WriteAndFlushProtobufAsync(message);
            }
        }
    }
}
