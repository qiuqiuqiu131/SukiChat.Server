using AutoMapper;
using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.IOServer.Manager;
using ChatServer.Main.Services;
using ChatServer.Main.Services.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Processor.ChatProcessor
{
    class ChatShareMessageRequestProcessor : IProcessor<ChatShareMessageRequest>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IUserService userService;
        private readonly IMapper mapper;
        private readonly IFriendService friendService;
        private readonly IGroupService groupService;
        private readonly IClientChannelManager clientChannelManager;

        public ChatShareMessageRequestProcessor(IUnitOfWork unitOfWork,IUserService userService,
            IMapper mapper,
            IFriendService friendService,
            IGroupService groupService,
            IClientChannelManager clientChannelManager)
        {
            this.unitOfWork = unitOfWork;
            this.userService = userService;
            this.mapper = mapper;
            this.friendService = friendService;
            this.groupService = groupService;
            this.clientChannelManager = clientChannelManager;
        }

        public async Task Process(MessageUnit<ChatShareMessageRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);
            var message = unit.Message;

            if (!await userService.IsUserExist(message.UserId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new ChatShareMessageResponse
                    {
                        Response = new CommonResponse { State = false, Message = "非法账号" }
                    });
                return;
            }

            if (message.Messages.ContentCase is ChatMessage.ContentOneofCase.FileMess)
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new ChatShareMessageResponse
                    {
                        Response = new CommonResponse { State = false, Message = "不支持转发文件消息" }
                    });
            }

            List<FriendChatMessage> friendChats = [];
            List<GroupChatMessage> chatGroups = [];


            string messages = ChatMessageHelper.EncruptChatMessage([message.Messages]);
            string? senderMessage = null;

            if (!string.IsNullOrEmpty(message.SenderMessage))
                senderMessage = ChatMessageHelper.EncruptChatMessage([new ChatMessage { TextMess = new TextMess { Text = message.SenderMessage } }]);

            var friends = message.TargetIds.Where(d => d.IsUser).Select(d => d.Id).ToList();
            var groups = message.TargetIds.Where(d => !d.IsUser).Select(d => d.Id).ToList();

            // -- 保存数据库 -- //
            if (friends.Any())
            {
                var chatPrivateRepository = unitOfWork.GetRepository<ChatPrivate>();
                foreach (var friend in friends)
                {
                    if (!await friendService.IsFriend(message.UserId, friend))
                        continue;

                    try
                    {
                        var chatPrivate = new ChatPrivate
                        {
                            UserFromId = message.UserId,
                            UserTargetId = friend,
                            IsRetracted = false,
                            RetractTime = DateTime.MinValue,
                            Time = DateTime.Now,
                            Message = messages
                        };

                        await chatPrivateRepository.InsertAsync(chatPrivate);
                        await unitOfWork.SaveChangesAsync();

                        // 添加待发送消息
                        var chatMessage = mapper.Map<FriendChatMessage>(chatPrivate);
                        friendChats.Add(chatMessage);
                    }
                    catch { continue; }
                }
            }

            if (groups.Any())
            {
                var chatGroupRepository = unitOfWork.GetRepository<ChatGroup>();
                foreach (var group in groups)
                {
                    if (!await groupService.IsGroupMember(message.UserId, group))
                        continue;

                    try
                    {
                        var chatGroup = new ChatGroup
                        {
                            UserFromId = message.UserId,
                            GroupId = group,
                            IsRetracted = false,
                            RetractTime = DateTime.MinValue,
                            Time = DateTime.Now,
                            Message = messages
                        };

                        await chatGroupRepository.InsertAsync(chatGroup);
                        await unitOfWork.SaveChangesAsync();

                        var groupMessage = mapper.Map<GroupChatMessage>(chatGroup);
                        chatGroups.Add(groupMessage);
                    }
                    catch { continue; }
                }
            }
        
            // -- 发送好友消息 -- // 
            foreach(var friendChat in  friendChats)
            {
                if(channel != null)
                    await channel.WriteAndFlushProtobufAsync(friendChat);

                var friend = clientChannelManager.GetClient(friendChat.UserTargetId);
                if (friend != null)
                    _ = friend.WriteAndFlushProtobufAsync(friendChat);
            }

            // -- 发送群聊消息 -- //
            foreach(var groupChat in chatGroups)
            {
                if(channel != null)
                    await channel.WriteAndFlushProtobufAsync(groupChat);

                var memberIds = await groupService.GetGroupMembers(groupChat.GroupId);
                foreach (var memberId in memberIds)
                {
                    if(memberId == message.UserId) continue;

                    var member = clientChannelManager.GetClient(memberId);
                    if (member != null)
                       _ = member.WriteAndFlushProtobufAsync(groupChat);
                }
            }

            if (channel != null)
                await channel.WriteAndFlushProtobufAsync(new ChatShareMessageResponse
                {
                    Response = new CommonResponse { State = true }
                });
        }
    }
}
