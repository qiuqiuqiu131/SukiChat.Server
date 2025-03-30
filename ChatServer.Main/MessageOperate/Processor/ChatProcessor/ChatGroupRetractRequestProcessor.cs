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
    class ChatGroupRetractRequestProcessor : IProcessor<ChatGroupRetractRequest>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IGroupService groupService;
        private readonly IUserService userService;
        private readonly IClientChannelManager clientChannelManager;

        public ChatGroupRetractRequestProcessor(IUnitOfWork unitOfWork,IGroupService groupService,IUserService userService,IClientChannelManager clientChannelManager)
        {
            this.unitOfWork = unitOfWork;
            this.groupService = groupService;
            this.userService = userService;
            this.clientChannelManager = clientChannelManager;
        }

        public async Task Process(MessageUnit<ChatGroupRetractRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);

            var message = unit.Message;

            if(!await userService.IsUserExist(message.UserId))
            {
                if(channel != null)
                {
                    await channel.WriteAndFlushProtobufAsync(new ChatGroupRetractMessage
                    {
                        Response = new CommonResponse { State = false ,Message = "非法账号"}
                    });
                }
                return; 
            }

            bool result = false;
            string groupId = "";

            try
            {
                var repository = unitOfWork.GetRepository<ChatGroup>();
                var chatGroup = await repository.GetFirstOrDefaultAsync(predicate:d => d.Id.Equals(message.ChatGroupId),disableTracking:false);

                // 如果存在此聊天消息，并且是发送者
                if (chatGroup != null && chatGroup.UserFromId.Equals(message.UserId) && DateTime.Now - chatGroup.Time < TimeSpan.FromMinutes(2))
                {
                    chatGroup.IsRetracted = true;
                    chatGroup.RetractTime = DateTime.Now;
                   
                    result = true;
                    groupId = chatGroup.GroupId;
                }

                await unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if(channel != null)
                {
                    await channel.WriteAndFlushProtobufAsync(new ChatGroupRetractMessage
                    {
                        Response = new CommonResponse { State = false, Message = "服务器出错" }
                    });
                }
                return;
            }

            if(!result)
            {
                if(channel != null)
                {
                    await channel.WriteAndFlushProtobufAsync(new ChatGroupRetractMessage
                    {
                        Response = new CommonResponse { State = false, Message = "消息处理错误" }
                    });
                }
                return;
            }


            // 对群成员发送撤回消息
            var response = new ChatGroupRetractMessage
            {
                Response = new CommonResponse { State = true },
                UserId = message.UserId,
                ChatGroupId = message.ChatGroupId,
            };

            var groupMembers = await groupService.GetGroupMembers(groupId);
            foreach (var member in groupMembers)
            {
                var client = clientChannelManager.GetClient(member);
                if(client != null)
                    await client.WriteAndFlushProtobufAsync(response);
            }
        }
    }
}
