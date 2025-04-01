using AutoMapper;
using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.IOServer.Manager;
using ChatServer.Main.Services;
using ChatServer.Main.Services.Helper;
using Google.Protobuf.WellKnownTypes;
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

            List<(FriendChatMessage,FriendChatMessage?)> friendChats = [];
            List<(GroupChatMessage,GroupChatMessage?)> chatGroups = [];


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

                        ChatPrivate? senderChatPrivate = null;
                        if(senderMessage != null && !string.IsNullOrWhiteSpace(senderMessage))
                        {
                            senderChatPrivate = new ChatPrivate
                            {
                                UserFromId = message.UserId,
                                UserTargetId = friend,
                                IsRetracted = false,
                                RetractTime = DateTime.MinValue,
                                Time = DateTime.Now,
                                Message = senderMessage
                            };
                        }

                        await chatPrivateRepository.InsertAsync(chatPrivate);
                        if (senderChatPrivate != null)
                            await chatPrivateRepository.InsertAsync(senderChatPrivate);
                        await unitOfWork.SaveChangesAsync();

                        // 添加待发送消息
                        var chatMessage = mapper.Map<FriendChatMessage>(chatPrivate);
                        FriendChatMessage? senderChatMessage = null;
                        if (chatMessage != null) 
                            senderChatMessage = mapper.Map<FriendChatMessage>(senderChatPrivate);
                        friendChats.Add((chatMessage!,senderChatMessage));
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

                        ChatGroup? senderChatGroup = null;
                        if (senderMessage != null && !string.IsNullOrWhiteSpace(senderMessage))
                        {
                            senderChatGroup = new ChatGroup
                            {
                                UserFromId = message.UserId,
                                GroupId = group,
                                IsRetracted = false,
                                RetractTime = DateTime.MinValue,
                                Time = DateTime.Now,
                                Message = senderMessage
                            };
                        }

                        await chatGroupRepository.InsertAsync(chatGroup);
                        if (senderChatGroup != null)
                            await chatGroupRepository.InsertAsync(senderChatGroup);
                        await unitOfWork.SaveChangesAsync();

                        var groupMessage = mapper.Map<GroupChatMessage>(chatGroup);
                        GroupChatMessage? senderGroupMessage = null;
                        if(senderChatGroup != null)
                            senderGroupMessage = mapper.Map<GroupChatMessage>(senderChatGroup);
                        chatGroups.Add((groupMessage!,senderGroupMessage));
                    }
                    catch { continue; }
                }
            }
        
            // -- 发送好友消息 -- // 
            foreach(var friendChat in  friendChats)
            {
                if (channel != null)
                {
                    await channel.WriteAndFlushProtobufAsync(friendChat.Item1);
                    if (friendChat.Item2 != null)
                    {
                        await Task.Delay(50);
                        await channel.WriteAndFlushProtobufAsync(friendChat.Item2);
                    }
                }

                var friend = clientChannelManager.GetClient(friendChat.Item1.UserTargetId);
                if (friend != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await friend.WriteAndFlushProtobufAsync(friendChat.Item1);
                        if (friendChat.Item2 != null)
                        {
                            await Task.Delay(50);
                            await friend.WriteAndFlushProtobufAsync(friendChat.Item2);
                        }
                    });
                }
            }

            // -- 发送群聊消息 -- //
            foreach(var groupChat in chatGroups)
            {
                if (channel != null)
                {
                    await channel.WriteAndFlushProtobufAsync(groupChat.Item1);
                    if (groupChat.Item2 != null)
                    {
                        await Task.Delay(50);
                        await channel.WriteAndFlushProtobufAsync(groupChat.Item2);
                    }
                }

                var memberIds = await groupService.GetGroupMembers(groupChat.Item1.GroupId);
                foreach (var memberId in memberIds)
                {
                    if(memberId == message.UserId) continue;

                    var member = clientChannelManager.GetClient(memberId);
                    if (member != null)
                    {
                        _ = Task.Run(async () =>
                        {
                            await member.WriteAndFlushProtobufAsync(groupChat.Item1);
                            if (groupChat.Item2 != null)
                            {
                                await Task.Delay(50);
                                await member.WriteAndFlushProtobufAsync(groupChat.Item2);
                            }
                        });
                    }
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
