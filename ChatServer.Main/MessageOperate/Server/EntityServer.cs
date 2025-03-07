using ChatServer.Common.Protobuf;
using ChatServer.Main.ServerEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Server
{
    internal class EntityServer : BusinessServer
    {
        public EntityServer(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override void RegisteMessages(MessagesContainer messages)
        {
            messages.AddMessage<GetUserRequest>()
                .AddMessage<GroupMessageRequest>()
                .AddMessage<GroupMembersRequest>()
                .AddMessage<GroupMemberRequest>()
                .AddMessage<UpdateUserData>();
        }
    }
}
