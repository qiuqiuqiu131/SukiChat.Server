using ChatServer.Common.Protobuf;
using ChatServer.Main.MessageOperate.Processor;
using ChatServer.Main.ServerEntity;
using File.Protobuf;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Server
{
    public class CommunicateServer : BusinessServer
    {
        public CommunicateServer(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override void RegisteMessages(MessagesContainer messages)
        {
            messages.AddMessage<FriendRequestFromClient>()
                .AddMessage<FriendResponseFromClient>()
                .AddMessage<JoinGroupRequestFromClient>()
                .AddMessage<JoinGroupResponseFromClient>()
                .AddMessage<FriendChatMessage>()
                .AddMessage<FriendWritingMessage>()
                .AddMessage<GroupChatMessage>()
                .AddMessage<UpdateFriendLastChatIdRequest>()
                .AddMessage<UpdateGroupLastChatIdRequest>()
                .AddMessage<FileRequest>()

                .AddMessage<GetFriendChatListRequest>()
                .AddMessage<GetGroupChatListRequest>()

                .AddMessage<GetFriendChatDetailListRequest>()
                .AddMessage<GetGroupChatDetailListRequest>()

                .AddMessage<ChatGroupDeleteRequest>()
                .AddMessage<ChatPrivateDeleteRequest>()
                .AddMessage<ChatGroupRetractRequest>()
                .AddMessage<ChatPrivateRetractRequest>()

                .AddMessage<ChatShareMessageRequest>();
        }
    }
}
