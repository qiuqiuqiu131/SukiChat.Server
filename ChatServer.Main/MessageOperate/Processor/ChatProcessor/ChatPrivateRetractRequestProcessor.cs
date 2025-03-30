using ChatServer.Common;
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

namespace ChatServer.Main.MessageOperate.Processor.ChatProcessor
{
    class ChatPrivateRetractRequestProcessor : IProcessor<ChatPrivateRetractRequest>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IUserService userService;
        private readonly IClientChannelManager clientChannelManager;

        public ChatPrivateRetractRequestProcessor(IUnitOfWork unitOfWork,IUserService userService,IClientChannelManager clientChannelManager)
        {
            this.unitOfWork = unitOfWork;
            this.userService = userService;
            this.clientChannelManager = clientChannelManager;
        }

        public async Task Process(MessageUnit<ChatPrivateRetractRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);

            var message = unit.Message;

            if (!await userService.IsUserExist(message.UserId))
            {
                if (channel != null)
                {
                    await channel.WriteAndFlushProtobufAsync(new ChatPrivateRetractMessage
                    {
                        Response = new CommonResponse { State = false, Message = "非法账号" }
                    });
                }
                return;
            }

            bool result = false;
            string friendId = "";

            try
            {
                var repository = unitOfWork.GetRepository<ChatPrivate>();
                var chatPrivate = await repository.GetFirstOrDefaultAsync(predicate: d => d.Id.Equals(message.ChatPrivateId), disableTracking: false);

                // 如果存在此聊天消息，并且是发送者
                if (chatPrivate != null && chatPrivate.UserFromId.Equals(message.UserId) && DateTime.Now - chatPrivate.Time < TimeSpan.FromMinutes(2))
                {
                    chatPrivate.IsRetracted = true;
                    chatPrivate.RetractTime = DateTime.Now;

                    result = true;
                    friendId = chatPrivate.UserTargetId;
                }

                await unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (channel != null)
                {
                    await channel.WriteAndFlushProtobufAsync(new ChatPrivateRetractMessage
                    {
                        Response = new CommonResponse { State = false, Message = "服务器出错" }
                    });
                }
                return;
            }

            if (!result)
            {
                if (channel != null)
                {
                    await channel.WriteAndFlushProtobufAsync(new ChatPrivateRetractMessage
                    {
                        Response = new CommonResponse { State = false, Message = "消息处理错误" }
                    });
                }
                return;
            }


            // 对双方发送撤回消息
            var response = new ChatPrivateRetractMessage
            {
                Response = new CommonResponse { State = true },
                UserId = message.UserId,
                ChatPrivateId = message.ChatPrivateId,
            };

            if (channel != null)
                await channel.WriteAndFlushProtobufAsync(response);

            var friend = clientChannelManager.GetClient(friendId);
            if(friend != null)
                await friend.WriteAndFlushProtobufAsync(response);
        }
    }
}
