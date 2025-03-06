using ChatServer.Common.Protobuf;
using ChatServer.Main.MessageOperate.Processor;
using ChatServer.Main.ServerEntity;
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
                .AddMessage<FriendChatMessage>()
                .AddMessage<FriendWritingMessage>()
                .AddMessage<GroupChatMessage>();
        }
    }
}
